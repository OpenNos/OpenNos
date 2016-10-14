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

using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Messengers
{
    /// <summary>
    /// This class is a wrapper for IMessenger and is used to synchronize message receiving
    /// operation. It extends RequestReplyMessenger. It is suitable to use in applications those want
    /// to receive messages by synchronized method calls instead of asynchronous MessageReceived event.
    /// </summary>
    public class SynchronizedMessenger<T> : RequestReplyMessenger<T> where T : IMessenger
    {
        #region Members

        /// <summary>
        /// This object is used to synchronize/wait threads.
        /// </summary>
        private readonly ManualResetEventSlim _receiveWaiter;

        /// <summary>
        /// A queue that is used to store receiving messages until Receive(...) method is called to
        /// get them.
        /// </summary>
        private readonly Queue<IScsMessage> _receivingMessageQueue;

        /// <summary>
        /// This boolean value indicates the running state of this class.
        /// </summary>
        private volatile bool _running;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new SynchronizedMessenger object.
        /// </summary>
        /// <param name="messenger">A IMessenger object to be used to send/receive messages</param>
        public SynchronizedMessenger(T messenger)
            : this(messenger, int.MaxValue)
        {
        }

        /// <summary>
        /// Creates a new SynchronizedMessenger object.
        /// </summary>
        /// <param name="messenger">A IMessenger object to be used to send/receive messages</param>
        /// <param name="incomingMessageQueueCapacity">capacity of the incoming message queue</param>
        public SynchronizedMessenger(T messenger, int incomingMessageQueueCapacity)
            : base(messenger)
        {
            _receiveWaiter = new ManualResetEventSlim();
            _receivingMessageQueue = new Queue<IScsMessage>();
            IncomingMessageQueueCapacity = incomingMessageQueueCapacity;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets capacity of the incoming message queue. No message is received from remote
        /// application if number of messages in public queue exceeds this value. Default value:
        /// int.MaxValue (2147483647).
        /// </summary>
        public int IncomingMessageQueueCapacity { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// This method is used to receive a message from remote application. It waits until a
        /// message is received.
        /// </summary>
        /// <returns>Received message</returns>
        public IScsMessage ReceiveMessage()
        {
            return ReceiveMessage(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// This method is used to receive a message from remote application. It waits until a
        /// message is received or timeout occurs.
        /// </summary>
        /// <param name="timeout">
        /// Timeout value to wait if no message is received. Use -1 to wait indefinitely.
        /// </param>
        /// <returns>Received message</returns>
        /// <exception cref="TimeoutException">Throws TimeoutException if timeout occurs</exception>
        /// <exception cref="Exception">
        /// Throws Exception if SynchronizedMessenger stops before a message is received
        /// </exception>
        public IScsMessage ReceiveMessage(int timeout)
        {
            while (_running)
            {
                lock (_receivingMessageQueue)
                {
                    // Check if SynchronizedMessenger is running
                    if (!_running)
                    {
                        throw new Exception("SynchronizedMessenger is stopped. Can not receive message.");
                    }

                    // Get a message immediately if any message does exists
                    if (_receivingMessageQueue.Any())
                    {
                        return _receivingMessageQueue.Dequeue();
                    }

                    _receiveWaiter.Reset();
                }

                // Wait for a message
                var signalled = _receiveWaiter.Wait(timeout);

                // If not signalled, throw exception
                if (!signalled)
                {
                    throw new TimeoutException("Timeout occured. Can not received any message");
                }
            }

            throw new Exception("SynchronizedMessenger is stopped. Can not receive message.");
        }

        /// <summary>
        /// This method is used to receive a specific type of message from remote application. It
        /// waits until a message is received.
        /// </summary>
        /// <returns>Received message</returns>
        public TMessage ReceiveMessage<TMessage>() where TMessage : IScsMessage
        {
            return ReceiveMessage<TMessage>(System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// This method is used to receive a specific type of message from remote application. It
        /// waits until a message is received or timeout occurs.
        /// </summary>
        /// <param name="timeout">
        /// Timeout value to wait if no message is received. Use -1 to wait indefinitely.
        /// </param>
        /// <returns>Received message</returns>
        public TMessage ReceiveMessage<TMessage>(int timeout) where TMessage : IScsMessage
        {
            var receivedMessage = ReceiveMessage(timeout);
            if (!(receivedMessage is TMessage))
            {
                throw new Exception("Unexpected message received." +
                                    " Expected type: " + typeof(TMessage).Name +
                                    ". Received message type: " + receivedMessage.GetType().Name);
            }

            return (TMessage)receivedMessage;
        }

        /// <summary>
        /// Starts the messenger.
        /// </summary>
        public override void Start()
        {
            lock (_receivingMessageQueue)
            {
                _running = true;
            }

            base.Start();
        }

        /// <summary>
        /// Stops the messenger.
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            lock (_receivingMessageQueue)
            {
                _running = false;
                _receiveWaiter.Set();
            }
        }

        /// <summary>
        /// Overrides
        /// </summary>
        /// <param name="message"></param>
        protected override void OnMessageReceived(IScsMessage message)
        {
            lock (_receivingMessageQueue)
            {
                if (_receivingMessageQueue.Count < IncomingMessageQueueCapacity)
                {
                    _receivingMessageQueue.Enqueue(message);
                }

                _receiveWaiter.Set();
            }
        }

        #endregion
    }
}