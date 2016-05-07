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

namespace OpenNos.GameObject
{
    public class SessionPacket
    {

        #region Instantiation

        public SessionPacket(ClientSession session, string content, ReceiverType receiver, string senderCharacterName = "", long someonesCharacterId = -1)
        {
            Sender = session;
            Content = content;
            Receiver = receiver;
            SenderCharacterName = senderCharacterName;
            SomeonesCharacterId = SomeonesCharacterId;
        }

        #endregion

        #region Properties

        public string Content { get; set; }
        public ReceiverType Receiver { get; set; }
        public ClientSession Sender { get; set; }
        public string SenderCharacterName { get; set; }
        public long SomeonesCharacterId { get; set; }

        #endregion
    }
}