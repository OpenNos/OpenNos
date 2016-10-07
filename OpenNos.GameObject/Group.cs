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

using OpenNos.Core.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Group
    {
        #region Members

        private int order;
        private ThreadSafeSortedList<long, ClientSession> _characters;

        #endregion

        #region Instantiation

        public Group()
        {
            _characters = new ThreadSafeSortedList<long, ClientSession>();
            GroupId = ServerManager.Instance.GetNextGroupId();
            order = 0;
        }

        public int CharacterCount
        {
            get
            {
                return _characters.Count;
            }
        }

        #endregion

        #region Properties

        public List<ClientSession> Characters
        {
            get
            {
                return _characters.GetAllItems();
            }
        }

        public long GroupId { get; set; }

        public byte SharingMode { get; set; }

        #endregion

        #region Methods

        public List<string> GeneratePst()
        {
            int i = 0;
            List<string> str = new List<string>();
            foreach (ClientSession session in Characters)
            {
                str.Add($"pst 1 {session.Character.CharacterId} {++i} { (int)(session.Character.Hp / session.Character.HPLoad() * 100) } {(int)(session.Character.Mp / session.Character.MPLoad() * 100) } {session.Character.HPLoad()} {session.Character.MPLoad()} {session.Character.Class} {session.Character.Gender} {(session.Character.UseSp ? session.Character.Morph : 0)}");
            }
            return str;
        }

        public bool IsMemberOfGroup(long characterId)
        {
            return _characters.ContainsKey(characterId);
        }

        public bool IsMemberOfGroup(ClientSession session)
        {
            return _characters.ContainsKey(session.Character.CharacterId);
        }

        public void JoinGroup(long characterId)
        {
            ClientSession session = ServerManager.Instance.GetSessionByCharacterId(characterId);
            if (session != null)
            {
                JoinGroup(session);
            }
        }

        public void JoinGroup(ClientSession session)
        {
            session.Character.Group = this;
            _characters[session.Character.CharacterId] = session;
        }

        public void LeaveGroup(ClientSession session)
        {
            session.Character.Group = null;
            _characters.Remove(session.Character.CharacterId);
        }

        public long OrderedCharacterId(Character Character)
        {
            order++;
            IEnumerable<ClientSession> lst = Characters.Where(s => Map.GetDistance(s.Character, Character) < 50);
            if (order > lst.Count() - 1)
            {
                order = 0;
            }

            return lst.ElementAt(order).Character.CharacterId;
        }

        #endregion
    }
}