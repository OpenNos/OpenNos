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
using OpenNos.Core;
using OpenNos.Core.Communication.Scs.Communication.Messages;
using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class AccountPacketHandler : PacketHandlerBase
    {
        private readonly NetworkClient _client;

        public AccountPacketHandler(NetworkClient client)
        {
            _client = client;
        }
        [Packet("Char_NEW")]
        public string CreateChar(string packet, int sessionId)
        {
            //todo, hold Account Information in Authorized object
            //load account by given SessionId
            AccountDTO account = DAOFactory.AccountDAO.LoadBySessionId(sessionId);

            CharacterDTO newCharacter = new CharacterDTO() {
                Class = 0,
                Gender = 1,
                Gold = 10000,
                HairColor = 5,
                HairStyle = 3,
                Hp = 200,
                JobLevel = 99,
                JobLevelXp = 0,
                Level = 99,
                LevelXp = 0,
                Map = 1,
                MapX = 40,
                MapY = 40,
                Mp = 200,
                Name = "Testdude",
                Slot = 0,
                AccountId = account.AccountId
            };

            SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref newCharacter);

            //change class
            newCharacter.Class = 2;

            SaveResult updateResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref newCharacter);

            return String.Empty;
        }

        [Packet("OpenNos.EntryPoint")]
        public string Initialize(string packet, int sessionId)
        {
            //load account by given SessionId
            AccountDTO account = DAOFactory.AccountDAO.LoadBySessionId(sessionId);
            IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(account.AccountId);
            Logger.Log.InfoFormat("Account with SessionId {0} has arrived.", sessionId);
            _client.SendPacket("clist_start 0");
            for (int i = 0; i < characters.Count(); i++)
            {
                
                _client.SendPacket(String.Format("clist {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}.{10}.{11}.{12}.{13}.{14}.{15}.{16} {17} {18} {19} {20}.{21} {22} {23}",
                    characters.ElementAt(i).Slot, characters.ElementAt(i).Name, characters.ElementAt(i).Gender, 0, characters.ElementAt(i).HairStyle, characters.ElementAt(i).HairColor, 5, characters.ElementAt(i).Class, characters.ElementAt(i).Level, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, -1, -1, characters.ElementAt(i).HairColor,0));
            }
            _client.SendPacket("clist_end");
        
            return String.Empty;
        }
    }
}
