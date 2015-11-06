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
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class AccountPacketHandler
    {
        private readonly ClientSession _session;

        public AccountPacketHandler(ClientSession session)
        {
            _session = session;
        }

        [Packet("Char_DEL")]
        public string DeleteCharacter(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            AccountDTO account = DAOFactory.AccountDAO.LoadBySessionId(_session.SessionId);
            if (account.Password == OpenNos.Core.EncryptionBase.sha256(packetsplit[3]))
            {
                DAOFactory.CharacterDAO.Delete(account.AccountId, Convert.ToByte(packetsplit[2]));
                LoadCharacters(packet);
            }
            else
            {
                _session.Client.SendPacket(String.Format("info {0}", Language.Instance.GetMessageFromKey("BAD_PASSWORD")));
            }

            return String.Empty;
        }
        [Packet("Char_NEW")]
        public string CreateCharacter(string packet)
        {
            //todo, hold Account Information in Authorized object
            long accountId = _session.Account.AccountId;
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2].Length > 3 && packetsplit[2].Length < 15)
            {

                if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) == null)
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
                        MapX = 80,
                        MapY = 118,
                        Mp = 200,
                        Name = packetsplit[2],
                        Slot = Convert.ToByte(packetsplit[3]),
                        AccountId = accountId,
                    };

                    SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref newCharacter);
                    LoadCharacters(packet);
                }

                else _session.Client.SendPacketFormat("info {0}", Language.Instance.GetMessageFromKey("ALREADY_TAKEN"));
            }
            return String.Empty;
        }

        /// <summary>
        /// Load Characters, this is the Entrypoint for the Client, Wait for 3 Packets.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        [Packet("OpenNos.EntryPoint", 3)]
        public string LoadCharacters(string packet)
        {
            string[] loginPacketParts = packet.Split(' ');

            //load account by given SessionId
            if (_session.Account == null)
            {
                AccountDTO accountDTO = DAOFactory.AccountDAO.LoadByName(loginPacketParts[4]);

                if (accountDTO != null)
                {
                    if (accountDTO.Password.Equals(EncryptionBase.sha256(loginPacketParts[6]))
                        && accountDTO.LastSession.Equals(_session.SessionId))
                    {
                        _session.Account = new GameObject.Account()
                        {
                            AccountId = accountDTO.AccountId,
                            Name = accountDTO.Name,
                            Password = accountDTO.Password,
                            Authority = accountDTO.Authority
                        };
                    }
                    else
                    {
                        Logger.Log.ErrorFormat("Client {0} forced Disconnection, invalid Password or SessionId.", _session.Client.ClientId);
                        _session.Client.Disconnect();
                    }
                }
                else
                {
                    Logger.Log.ErrorFormat("Client {0} forced Disconnection, invalid AccountName.", _session.Client.ClientId);
                    _session.Client.Disconnect();
                }

            }

            IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(_session.Account.AccountId);
            Logger.Log.InfoFormat(Language.Instance.GetMessageFromKey("ACCOUNT_ARRIVED"), _session.SessionId);
            _session.Client.SendPacket("clist_start 0");
            foreach (CharacterDTO character in characters)
            {

                _session.Client.SendPacket(String.Format("clist {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}.{10}.{11}.{12}.{13}.{14}.{15}.{16} {17} {18} {19} {20}.{21} {22} {23}",
                    character.Slot, character.Name, 0, character.Gender, character.HairStyle, character.HairColor, 5, character.Class, character.Level, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, -1, -1, character.HairColor, 0));
            }
            _session.Client.SendPacket("clist_end");

            return String.Empty;
        }

        [Packet("select")]
        public string SelectCharacter(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            CharacterDTO characterDTO = DAOFactory.CharacterDAO.LoadByAccount(_session.Account.AccountId).ElementAt(Convert.ToInt32(packetsplit[2]));
            _session.Character = new GameObject.Character()
            {
                AccountId = characterDTO.AccountId,
                CharacterId = characterDTO.CharacterId,
                Class = characterDTO.Class,
                Dignite = characterDTO.Dignite,
                Gender = characterDTO.Gender,
                Gold = characterDTO.Gold,
                HairColor = characterDTO.HairColor,
                HairStyle = characterDTO.HairStyle,
                Hp = characterDTO.Hp,
                JobLevel = characterDTO.JobLevel,
                JobLevelXp = characterDTO.JobLevelXp,
                Level = characterDTO.Level,
                LevelXp = characterDTO.LevelXp,
                Map = characterDTO.Map,
                MapX = characterDTO.MapX,
                MapY = characterDTO.MapY,
                Mp = characterDTO.Mp,
                Name = characterDTO.Name,
                Reput = characterDTO.Reput,
                Slot = characterDTO.Slot,
                Authority = _session.Account.Authority,
                LastPulse = 0,
                Invisible = 0,
                Speed = 11,
                ArenaWinner = 0,
                Sp = 0,
                SpUpgrade = 0,
            };

            _session.CurrentMap = MapManager.GetMap(_session.Character.Map);
            _session.RegisterForMapNotification();
            _session.Client.SendPacket("OK");
            return String.Empty;
        }
        [Packet("pulse")]
        public string Pulse(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            _session.Character.LastPulse += 60;
            if (Convert.ToInt32(packetsplit[2]) != _session.Character.LastPulse) { _session.Client.Disconnect(); }
            return String.Empty;
        }
        [Packet("lbs")]
        public string lbs(string packet)
        {
            //i don't know why there is this packet
            return string.Empty;
        }
        [Packet("c_close")]
        public string c_close(string packet)
        {
            //i don't know why there is this packet
            return string.Empty;
        }
        [Packet("f_stash_end")]
        public string f_stash_end(string packet)
        {
            //i don't know why there is this packet
            return string.Empty;
        }
        [Packet("walk")]
        public string walk(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            _session.Character.MapX = Convert.ToInt16(packetsplit[2]);
            _session.Character.MapY = Convert.ToInt16(packetsplit[3]);
            _session.CurrentMap.QueuePacket(new KeyValuePair<string, ClientSession>(_session.Character.GenerateMv(_session.Character.MapX, _session.Character.MapY), _session));
            _session.Client.SendPacket(_session.Character.GenerateCond());
            return string.Empty;
        }
        [Packet("guri")]
        public string guri(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2] == "10" && Convert.ToInt32(packetsplit[5]) >= 973 && Convert.ToInt32(packetsplit[5]) <= 999)
            {

                _session.Client.SendPacket(_session.Character.GenerateEff(Convert.ToInt32(packetsplit[5]) + 4099));
                _session.CurrentMap.QueuePacket(new KeyValuePair<string, ClientSession>(_session.Character.GenerateEff(Convert.ToInt32(packetsplit[5]) + 4099), _session));

            }
            return string.Empty;
        }
        [Packet("game_start")]
        public string StartGame(string packet)
        {
            _session.Client.SendPacket(_session.Character.GenerateTit());
            _session.Client.SendPacket(_session.Character.GenerateCInfo());
            _session.Client.SendPacket(_session.Character.GenerateFd());
            //TODO if first connect add _session.Client.SendPacket(String.Format("scene 40"));
            _session.Client.SendPacket(_session.Character.GenerateLev());
            //stat
            _session.Client.SendPacket(_session.Character.GenerateStat());
            //ski
            _session.Client.SendPacket(_session.Character.GenerateAt());
            _session.Client.SendPacket(_session.Character.GenerateCMap());
            //sc
            _session.Client.SendPacket(_session.Character.GenerateCond());
            //pairy
            _session.Client.SendPacket(String.Format("rsfi {0} {1} {2} {3} {4} {5}", 1, 1, 4, 9, 4, 9));

            _session.Client.SendPacket("rank_cool 0 0 18000");//TODO add rank cool

            _session.Client.SendPacket("scr 0 0 0 0 0 0");
            //bn
            _session.Client.SendPacket(_session.Character.GenerateExts());
            
            //gidx
            _session.Client.SendPacket("mlinfo 3800 2000 100 0 0 10 0 Mélodie^du^printemps Bienvenue");
            //cond
            _session.Client.SendPacket("p_clear");
            //sc_p pet
            _session.Client.SendPacket("pinit 0");
            _session.Client.SendPacket("zzim");
            _session.Client.SendPacket(String.Format("twk 1 {0} {1} {2} shtmxpdlfeoqkr", _session.Character.CharacterId, _session.Account.Name, _session.Character.Name));


            _session.CurrentMap.QueuePacket(new KeyValuePair<string, ClientSession>(String.Format("info INFORMATION FROM {0}", _session.Client.ClientId), _session));

            return String.Empty;
        }
    }
}
