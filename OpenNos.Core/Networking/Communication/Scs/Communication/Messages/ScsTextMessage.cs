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

using System;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Messages
{
    /// <summary>
    /// This message is used to send/receive a text as message data.
    /// </summary>
    [Serializable]
    public class ScsTextMessage : ScsMessage
    {
        #region Instantiation

        /// <summary>
        /// Creates a new ScsTextMessage object.
        /// </summary>
        public ScsTextMessage()
        {
        }

        /// <summary>
        /// Creates a new ScsTextMessage object with Text property.
        /// </summary>
        /// <param name="text">Message text that is being transmitted</param>
        public ScsTextMessage(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Creates a new reply ScsTextMessage object with Text property.
        /// </summary>
        /// <param name="text">Message text that is being transmitted</param>
        /// <param name="repliedMessageId">Replied message id if this is a reply for a message.</param>
        public ScsTextMessage(string text, string repliedMessageId)
            : this(text)
        {
            RepliedMessageId = repliedMessageId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Message text that is being transmitted.
        /// </summary>
        public string Text { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? $"ScsTextMessage [{MessageId}]: {Text}"
                       : $"ScsTextMessage [{MessageId}] Replied To [{RepliedMessageId}]: {Text}";
        }

        #endregion
    }
}