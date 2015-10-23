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

        [Packet("Char_DEL")]
        public string DeleteChar(string packet, int sessionId)
        {
            string[] packetsplit = packet.Split(' ');
            AccountDTO account = DAOFactory.AccountDAO.LoadBySessionId(sessionId);
            if (account.Password == OpenNos.Core.EncryptionBase.sha256(packetsplit[3]))
            {
                DAOFactory.CharacterDAO.Delete(account.AccountId, Convert.ToByte(packetsplit[2]));
                Initialize(packet, sessionId);
            }
            else
            {
                _client.SendPacket("info Bad Password");
            }

            return String.Empty;
        }
        [Packet("Char_NEW")]
        public string CreateChar(string packet, int sessionId)
        {
            //todo, hold Account Information in Authorized object
            //load account by given SessionId
            AccountDTO account = DAOFactory.AccountDAO.LoadBySessionId(sessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2].Length > 3 && packetsplit[2].Length < 15)
            {

                if (!DAOFactory.CharacterDAO.IsAlreadyDefined(packetsplit[2]))
                {
                    CharacterDTO newCharacter = new CharacterDTO()
                    {
                        Class = 0,
                        Gender = Convert.ToByte(packetsplit[4]),
                        Gold = 10000,
                        HairColor = Convert.ToByte(packetsplit[6]),
                        HairStyle = Convert.ToByte(packetsplit[5]),
                        Hp = 200,
                        JobLevel = 1,
                        JobLevelXp = 0,
                        Level = 1,
                        LevelXp = 0,
                        Map = 1,
                        MapX = 40,
                        MapY = 40,
                        Mp = 200,
                        Name = packetsplit[2],
                        Slot = Convert.ToByte(packetsplit[3]),
                        AccountId = account.AccountId
                    };

                    SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref newCharacter);
                    Initialize(packet, sessionId);
                }
                else _client.SendPacket("info this name is already taken");
            }
            else _client.SendPacket("info the name must use beetween 4 and 14 key");
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
            foreach (CharacterDTO character in characters)
            {

                _client.SendPacket(String.Format("clist {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}.{10}.{11}.{12}.{13}.{14}.{15}.{16} {17} {18} {19} {20}.{21} {22} {23}",
                    character.Slot, character.Name, 0, character.Gender, character.HairStyle, character.HairColor, 5, character.Class, character.Level, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, -1, -1, character.HairColor, 0));
            }
            _client.SendPacket("clist_end");

            return String.Empty;
        }
    }
}
