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
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenNos.Core
{
    public class WireProtocol : IScsWireProtocol, IDisposable
    {
        #region Members

        /// <summary>
        /// Maximum length of a message.
        /// </summary>
        private const short MaxMessageLength = 4096;

        private IDictionary<string, DateTime> _connectionHistory;

        private bool _disposed;

        /// <summary>
        /// This MemoryStream object is used to collect receiving bytes to build messages.
        /// </summary>
        private MemoryStream _receiveMemoryStream;

        #endregion

        #region Instantiation

        public WireProtocol()
        {
            _receiveMemoryStream = new MemoryStream();
            _connectionHistory = new Dictionary<string, DateTime>();
        }

        #endregion

        #region Methods

        public IEnumerable<IScsMessage> CreateMessages(byte[] receivedBytes)
        {
            // Write all received bytes to the _receiveMemoryStream
            _receiveMemoryStream.Write(receivedBytes, 0, receivedBytes.Length);

            // Create a list to collect messages
            var messages = new List<IScsMessage>();

            // Read all available messages and add to messages collection
            while (ReadSingleMessage(messages))
            {
            }

            // Return message list
            return messages;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public byte[] GetBytes(IScsMessage message)
        {
            // Serialize the message to a byte array
            ScsTextMessage textMessage = message as ScsTextMessage;
            byte[] bytes = textMessage != null ?
                Encoding.Default.GetBytes(textMessage.Text) :
                ((ScsRawDataMessage)message).MessageData;

            return bytes;
        }

        public void Reset()
        {
            if (_receiveMemoryStream.Length > 0)
            {
                _receiveMemoryStream = new MemoryStream();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _receiveMemoryStream.Dispose();
            }
        }

        /// <summary>
        /// Reads a byte array with specified length.
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="length">Length of the byte array to read</param>
        /// <returns>Read byte array</returns>
        /// <exception cref="EndOfStreamException">
        /// Throws EndOfStreamException if can not read from stream.
        /// </exception>
        private static byte[] ReadByteArray(Stream stream, short length)
        {
            var buffer = new byte[length];

            var read = stream.Read(buffer, 0, length);
            if (read <= 0)
            {
                throw new EndOfStreamException("Can not read from stream! Input stream is closed.");
            }

            return buffer;
        }

        /// <summary>
        /// This method tries to read a single message and add to the messages collection.
        /// </summary>
        /// <param name="messages">Messages collection to collect messages</param>
        /// <returns>
        /// Returns a boolean value indicates that if there is a need to re-call this method.
        /// </returns>
        /// <exception cref="CommunicationException">
        /// Throws CommunicationException if message is bigger than maximum allowed message length.
        /// </exception>
        private bool ReadSingleMessage(ICollection<IScsMessage> messages)
        {
            // Go to the beginning of the stream
            _receiveMemoryStream.Position = 0;

            // check if message length is 0
            if (_receiveMemoryStream.Length == 0)
            {
                return false;
            }

            // get length of frame
            short frameLength = (short)_receiveMemoryStream.Length;

            // Read length of the message
            if (frameLength > MaxMessageLength)
            {
                throw new Exception("Message is too big (" + frameLength + " bytes). Max allowed length is " + MaxMessageLength + " bytes.");
            }

            // Read bytes of serialized message and deserialize it
            var serializedMessageBytes = ReadByteArray(_receiveMemoryStream, frameLength);
            messages.Add(new ScsRawDataMessage(serializedMessageBytes));

            // Read remaining bytes to an array
            if (_receiveMemoryStream.Length > frameLength)
            {
                var remainingBytes = ReadByteArray(_receiveMemoryStream, (short)(_receiveMemoryStream.Length - frameLength));

                // Re-create the receive memory stream and write remaining bytes
                _receiveMemoryStream = new MemoryStream();
                _receiveMemoryStream.Write(remainingBytes, 0, remainingBytes.Length);
            }
            else
            {
                // nothing left, just recreate
                _receiveMemoryStream = new MemoryStream();
            }

            // Return true to re-call this method to try to read next message
            return _receiveMemoryStream.Length > 0;
        }

        #endregion
    }
}