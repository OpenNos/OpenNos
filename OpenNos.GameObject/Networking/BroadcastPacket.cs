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

using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class BroadcastPacket
    {
        #region Instantiation

        public BroadcastPacket(ClientSession session, string packet, ReceiverType receiver, string someonesCharacterName = "", long someonesCharacterId = -1, int xCoordinate = 0, int yCoordinate = 0)
        {
            Sender = session;
            Packet = packet;
            Receiver = receiver;
            SomeonesCharacterName = someonesCharacterName;
            SomeonesCharacterId = someonesCharacterId;
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
        }

        #endregion

        #region Properties

        public string Packet { get; set; }

        public ReceiverType Receiver { get; set; }

        public ClientSession Sender { get; set; }

        public long SomeonesCharacterId { get; set; }

        public string SomeonesCharacterName { get; set; }

        public int XCoordinate { get; set; }

        public int YCoordinate { get; set; }

        #endregion
    }
}