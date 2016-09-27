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

using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using System;
using System.Net;
using System.Net.Sockets;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Channels.Tcp
{
    /// <summary>
    /// This class is used to communicate with a remote application over TCP/IP protocol.
    /// </summary>
    public class TcpCommunicationChannel : CommunicationChannelBase
    {
        #region Members

        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int ReceiveBufferSize = 4 * 1024;

        /// <summary>
        /// This buffer is used to receive bytes
        /// </summary>
        private readonly byte[] _buffer;

        //4KB
        /// <summary>
        /// Socket object to send/reveice messages.
        /// </summary>
        private readonly Socket _clientSocket;

        private readonly ScsTcpEndPoint _remoteEndPoint;

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        private readonly object _syncLock;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new TcpCommunicationChannel object.
        /// </summary>
        /// <param name="clientSocket">A connected Socket object that is
        /// used to communicate over network</param>
        public TcpCommunicationChannel(Socket clientSocket)
        {
            _clientSocket = clientSocket;
            _clientSocket.NoDelay = true;

            var ipEndPoint = (IPEndPoint)_clientSocket.RemoteEndPoint;
            _remoteEndPoint = new ScsTcpEndPoint(ipEndPoint.Address.ToString(), ipEndPoint.Port);

            _buffer = new byte[ReceiveBufferSize];
            _syncLock = new object();
        }

        #endregion

        #region Properties

        ///<summary>
        /// Gets the endpoint of remote application.
        ///</summary>
        public override ScsEndPoint RemoteEndPoint
        {
            get
            {
                return _remoteEndPoint;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disconnects from remote application and closes channel.
        /// </summary>
        public override void Disconnect()
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            _running = false;
            try
            {
                if (_clientSocket.Connected)
                {
                    _clientSocket.Close();
                }

                _clientSocket.Dispose();
            }
            catch
            {
            }

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();
        }

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected override void SendMessagepublic(IScsMessage message)
        {
            //Send message
            var totalSent = 0;
            lock (_syncLock)
            {
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);
                //Send all bytes to the remote application
                while (totalSent < messageBytes.Length)
                {
                    if (_clientSocket.Connected)
                    {
                        var sent = 0;
                        try
                        {
                            sent = _clientSocket.Send(messageBytes, totalSent, messageBytes.Length - totalSent, SocketFlags.None);
                        }
                        catch (Exception)
                        {
                            //annoying bug
                            Logger.Log.Warn("A packet would have been sent to a disconnected client. IGNORE THIS.");
                        }

                        if (sent <= 0)
                        {
                            throw new CommunicationException("Message could not be sent via TCP socket. Only " + totalSent + " bytes of " + messageBytes.Length + " bytes are sent.");
                        }

                        totalSent += sent;
                    }
                    else
                    {
                        Logger.Log.Warn("A packet would have been sent to a disconnected client. IGNORE THIS.");
                        break;
                    }
                }

                LastSentMessageTime = DateTime.Now;
                OnMessageSent(message);
            }
        }

        /// <summary>
        /// Starts the thread to receive messages from socket.
        /// </summary>
        protected override void Startpublic()
        {
            _running = true;
            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
        }

        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!_running)
            {
                return;
            }

            try
            {
                var bytesRead = -1;

                //Get received bytes count
                bytesRead = _clientSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    LastReceivedMessageTime = DateTime.Now;
                    //Copy received bytes to a new byte array
                    var receivedBytes = new byte[bytesRead];
                    Array.Copy(_buffer, receivedBytes, bytesRead);
                    //Read messages according to current wire protocol
                    var messages = WireProtocol.CreateMessages(receivedBytes);
                    //Raise MessageReceived event for all received messages
                    foreach (var message in messages)
                    {
                        OnMessageReceived(message);
                    }
                }
                else
                {
                    Logger.Log.Warn(Language.Instance.GetMessageFromKey("CLIENT_DISCONNECTED"));
                    Disconnect();
                }

                //Read more bytes if still running
                if (_running)
                {
                    _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, 0, new AsyncCallback(ReceiveCallback), null);
                }
            }
            catch
            {
                Disconnect();
            }
        }

        #endregion
    }
}