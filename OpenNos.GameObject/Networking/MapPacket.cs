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

namespace OpenNos.GameObject
{
    public class MapPacket
    {
        #region Members

        private ReceiverType all;
        private string characterName;
        private string packet;

        #endregion

        #region Instantiation

        public MapPacket(ClientSession session, string content, ReceiverType receiver)
        {
            Session = session;
            Content = content;
            Receiver = receiver;
        }

        public MapPacket(string characterName, string packet, ReceiverType all)
        {
            this.characterName = characterName;
            this.packet = packet;
            this.all = all;
        }

        #endregion

        #region Properties

        public String Content { get; set; }
        public ReceiverType Receiver { get; set; }
        public ClientSession Session { get; set; }

        #endregion
    }
}