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

namespace OpenNos.Master.Library.Data
{
    internal class AccountConnection
    {
        #region Instantiation

        public AccountConnection(long accountId, long session)
        {
            AccountId = accountId;
            SessionId = session;
            LastPulse = DateTime.Now;
        }

        #endregion

        #region Properties

        public long AccountId { get; private set; }

        public long CharacterId { get; set; }

        public DateTime LastPulse { get; set; }

        public WorldServer ConnectedWorld { get; set; }

        public long SessionId { get; private set; }

        #endregion
    }
}