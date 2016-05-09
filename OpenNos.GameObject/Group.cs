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

using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Group
    {
        #region Instantiation

        public Group()
        {
            Characters = new List<ClientSession>();
        }

        #endregion

        #region Properties

        public int GroupId { get; set; }
        public byte SharingMode { get; set; }
        public List<ClientSession> Characters { get; set; }

        #endregion

        #region Methods

        public List<string> GeneratePst()
        {
            int i = 0;
            List<string> str = new List<string>();
            foreach (ClientSession session in Characters)
            {
                str.Add($"pst 1 {session.Character.CharacterId} {++i} { session.Character.Hp / session.Character.HPLoad() * 100 } {(int)(session.Character.Mp / session.Character.MPLoad() * 100) } 0 0 {session.Character.Class} {session.Character.Gender} {(session.Character.UseSp ? session.Character.Morph : 0)}");
            }
            return str;
        }

        public bool IsMemberOfGroup(long characterId)
        {
            return Characters.Any(c => c.Character.CharacterId.Equals(characterId));
        }

        public bool IsMemberOfGroup(ClientSession session)
        {
            return Characters.Any(c => c.Character.CharacterId.Equals(session.Character.CharacterId));
        }

        public void JoinGroup(long characterId)
        {
            JoinGroup(ServerManager.Instance.Sessions.SingleOrDefault(s => s.Character.CharacterId.Equals(characterId)));
        }

        public void JoinGroup(ClientSession session)
        {
            session.Character.Group = this;
            Characters.Add(session);
        }

        public void LeaveGroup(ClientSession session)
        {
            session.Character.Group = null;
            Characters.Remove(session);
        }

        #endregion
    }
}