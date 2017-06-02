/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;
using OpenNos.Core.Threading;
using System;
using System.Threading.Tasks;

namespace OpenNos.Core.Networking.Communication.Scs.Client
{
    /// <summary>
    /// This class provides base functionality for client Classs.
    /// </summary>
    public abstract class ScsClientBase : IScsClient
    {
        #region Members

        /// <summary>
        /// Default timeout value for connecting a server.
        /// </summary>
        private const int DefaultConnectionAttemptTimeout = 15000;

        /// <summary>
        /// This timer is used to send PingMessage messages to server periodically.
        /// </summary>
        private readonly Timer _pingTimer;

        /// <summary>
        /// The communication channel that is used by client to send and receive messages.
        /// </summary>
        private ICommunicationChannel _communicationChannel;

        private bool _disposed;
        private IScsWireProtocol _wireProtocol;

        #endregion

        #region Instantiation

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ScsClientBase()
        {
            _pingTimer = new Timer(30000);
            _pingTimer.Elapsed += PingTimer_Elapsed;
            ConnectTimeout = DefaultConnectionAttemptTimeout;
            WireProtocol = WireProtocolManager.GetDefaultWireProtocol();
        }

        #endregion

        #region Events

        /// <summary>
        /// This event is raised when communication channel closed.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// This event is raised when a new message is received.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// This event is raised when a new message is sent without any error. It does not guaranties
        /// that message is properly handled and processed by remote application.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the communication state of the Client.
        /// </summary>
        public CommunicationStates CommunicationState
        {
            get
            {
                return _communicationChannel != null
                           ? _communicationChannel.CommunicationState
                           : CommunicationStates.Disconnected;
            }
        }

        /// <summary>
        /// Timeout for connecting to a server (as milliseconds). Default value: 15 seconds (15000 ms).
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastReceivedMessageTime
        {
            get
            {
                return _communicationChannel != null
                           ? _communicationChannel.LastReceivedMessageTime
                           : DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastSentMessageTime
        {
            get
            {
                return _communicationChannel != null
                           ? _communicationChannel.LastSentMessageTime
                           : DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets/sets wire protocol that is used while reading and writing messages.
        /// </summary>
        public IScsWireProtocol WireProtocol
        {
            get
            {
                return _wireProtocol;
            }

            set
            {
                if (CommunicationState == CommunicationStates.Connected)
                {
                    throw new ApplicationException("Wire protocol can not be changed while connected to server.");
                }

                _wireProtocol = value;
            }
        }

        #endregion

        #region Methods

        public async Task ClearLowPriorityQueue()
        {
            await _communicationChannel.ClearLowPriorityQueue();
        }

        /// <summary>
        /// Connects to server.
        /// </summary>
        public void Connect()
        {
            WireProtocol.Reset();
            _communicationChannel = CreateCommunicationChannel();
            _communicationChannel.WireProtocol = WireProtocol;
            _communicationChannel.Disconnected += CommunicationChannel_Disconnected;
            _communicationChannel.MessageReceived += CommunicationChannel_MessageReceived;
            _communicationChannel.MessageSent += CommunicationChannel_MessageSent;
            _communicationChannel.Start();
            _pingTimer.Start();
            OnConnected();
        }

        /// <summary>
        /// Disconnects from server. Does nothing if already disconnected.
        /// </summary>
        public void Disconnect()
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            _communicationChannel.Disconnect();
        }

        /// <summary>
        /// Disposes this object and closes underlying connection.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <exception cref="CommunicationStateException">
        /// Throws a CommunicationStateException if client is not connected to the server.
        /// </exception>
        public void SendMessage(IScsMessage message, byte priority)
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                throw new CommunicationStateException("Client is not connected to the server.");
            }

            _communicationChannel.SendMessage(message, priority);
        }

        /// <summary>
        /// This method is implemented by derived Classs to create appropriate communication channel.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected abstract ICommunicationChannel CreateCommunicationChannel();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect();
                _pingTimer.Dispose();
            }
        }

        /// <summary>
        /// Raises Connected event.
        /// </summary>
        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises Disconnected event.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises MessageReceived event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageReceived(IScsMessage message)
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(message, DateTime.Now));
        }

        /// <summary>
        /// Raises MessageSent event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageSent(IScsMessage message)
        {
            MessageSent?.Invoke(this, new MessageEventArgs(message, DateTime.Now));
        }

        /// <summary>
        /// Handles Disconnected event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_Disconnected(object sender, EventArgs e)
        {
            _pingTimer.Stop();
            OnDisconnected();
        }

        /// <summary>
        /// Handles MessageReceived event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is ScsPingMessage)
            {
                return;
            }

            OnMessageReceived(e.Message);
        }

        /// <summary>
        /// Handles MessageSent event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageSent(object sender, MessageEventArgs e)
        {
            OnMessageSent(e.Message);
        }

        /// <summary>
        /// Handles Elapsed event of _pingTimer to send PingMessage messages to server.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void PingTimer_Elapsed(object sender, EventArgs e)
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            try
            {
                var lastMinute = DateTime.Now.AddMinutes(-1);
                if (_communicationChannel.LastReceivedMessageTime > lastMinute || _communicationChannel.LastSentMessageTime > lastMinute)
                {
                    return;
                }

                _communicationChannel.SendMessage(new ScsPingMessage(), 10);
            }
            catch
            {
            }
        }

        #endregion
    }
}