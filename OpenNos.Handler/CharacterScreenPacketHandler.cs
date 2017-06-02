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
using OpenNos.Core.Handling;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace OpenNos.Handler
{
    public class CharacterScreenPacketHandler : IPacketHandler
    {
        #region Instantiation

        public CharacterScreenPacketHandler(ClientSession session)
        {
            Session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Char_NEW character creation character
        /// </summary>
        /// <param name="characterCreatePacket"></param>
        public void CreateCharacter(CharacterCreatePacket characterCreatePacket)
        {
            Logger.Debug(Session.GenerateIdentity(), characterCreatePacket.ToString());
            if (Session.HasCurrentMapInstance)
            {
                return;
            }
            // TODO: Hold Account Information in Authorized object
            long accountId = Session.Account.AccountId;
            byte slot = characterCreatePacket.Slot;
            string characterName = characterCreatePacket.Name;
            if (slot <= 2 && DAOFactory.CharacterDAO.LoadBySlot(accountId, slot) == null)
            {
                if (characterName.Length > 3 && characterName.Length < 15)
                {
                    Regex rg = new Regex(@"^[\u0021-\u007E\u00A1-\u00AC\u00AE-\u00FF\u4E00-\u9FA5\u0E01-\u0E3A\u0E3F-\u0E5B\u002E]*$");
                    if (rg.Matches(characterName).Count == 1)
                    {
                        if (DAOFactory.CharacterDAO.LoadByName(characterName) == null)
                        {
                            if (characterCreatePacket.Slot > 2)
                            {
                                return;
                            }
                            CharacterDTO newCharacter = new CharacterDTO
                            {
                                Class = (byte)ClassType.Adventurer,
                                Gender = characterCreatePacket.Gender,
                                HairColor = characterCreatePacket.HairColor,
                                HairStyle = characterCreatePacket.HairStyle,
                                Hp = 221,
                                JobLevel = 1,
                                Level = 1,
                                MapId = 1,
                                MapX = (short)ServerManager.Instance.RandomNumber(78, 81),
                                MapY = (short)ServerManager.Instance.RandomNumber(114, 118),
                                Mp = 221,
                                MaxMateCount = 10,
                                SpPoint = 10000,
                                SpAdditionPoint = 0,
                                Name = characterName,
                                Slot = slot,
                                AccountId = accountId,
                                MinilandMessage = "Welcome",
                                State = CharacterState.Active
                            };

                            SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref newCharacter);
                            CharacterSkillDTO sk1 = new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 200 };
                            CharacterSkillDTO sk2 = new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 201 };
                            CharacterSkillDTO sk3 = new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 209 };
                            QuicklistEntryDTO qlst1 = new QuicklistEntryDTO
                            {
                                CharacterId = newCharacter.CharacterId,
                                Type = 1,
                                Slot = 1,
                                Pos = 1
                            };
                            QuicklistEntryDTO qlst2 = new QuicklistEntryDTO
                            {
                                CharacterId = newCharacter.CharacterId,
                                Q2 = 1,
                                Slot = 2
                            };
                            QuicklistEntryDTO qlst3 = new QuicklistEntryDTO
                            {
                                CharacterId = newCharacter.CharacterId,
                                Q2 = 8,
                                Type = 1,
                                Slot = 1,
                                Pos = 16
                            };
                            QuicklistEntryDTO qlst4 = new QuicklistEntryDTO
                            {
                                CharacterId = newCharacter.CharacterId,
                                Q2 = 9,
                                Type = 1,
                                Slot = 3,
                                Pos = 1
                            };
                            DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst1);
                            DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst2);
                            DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst3);
                            DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst4);
                            DAOFactory.CharacterSkillDAO.InsertOrUpdate(sk1);
                            DAOFactory.CharacterSkillDAO.InsertOrUpdate(sk2);
                            DAOFactory.CharacterSkillDAO.InsertOrUpdate(sk3);

                            Inventory startupInventory = new Inventory((Character)newCharacter);
                            startupInventory.AddNewToInventory(1, 1, InventoryType.Wear);
                            startupInventory.AddNewToInventory(8, 1, InventoryType.Wear);
                            startupInventory.AddNewToInventory(12, 1, InventoryType.Wear);
                            startupInventory.AddNewToInventory(2024, 10, InventoryType.Etc);
                            startupInventory.AddNewToInventory(2081, 1, InventoryType.Etc);
                            startupInventory.GetAllItems().ForEach(i => DAOFactory.IteminstanceDAO.InsertOrUpdate(i));

                            LoadCharacters(characterCreatePacket.OriginalContent);
                        }
                        else
                        {
                            Session.SendPacketFormat($"info {Language.Instance.GetMessageFromKey("ALREADY_TAKEN")}");
                        }
                    }
                    else
                    {
                        Session.SendPacketFormat($"info {Language.Instance.GetMessageFromKey("INVALID_CHARNAME")}");
                    }
                }
            }
        }

        /// <summary>
        /// Char_DEL packet
        /// </summary>
        /// <param name="characterDeletePacket"></param>
        public void DeleteCharacter(CharacterDeletePacket characterDeletePacket)
        {
            Logger.Debug(Session.GenerateIdentity(), characterDeletePacket.ToString());

            if (Session.HasCurrentMapInstance)
            {
                return;
            }
            AccountDTO account = DAOFactory.AccountDAO.LoadById(Session.Account.AccountId);
            if (account == null)
            {
                return;
            }

            if (account.Password.ToLower() == EncryptionBase.Sha512(characterDeletePacket.Password))
            {
                CharacterDTO character = DAOFactory.CharacterDAO.LoadBySlot(account.AccountId, characterDeletePacket.Slot);
                if (character == null)
                {
                    return;
                }
                DAOFactory.GeneralLogDAO.SetCharIdNull(Convert.ToInt64(character.CharacterId));
                DAOFactory.CharacterDAO.DeleteByPrimaryKey(account.AccountId, characterDeletePacket.Slot);
                LoadCharacters(string.Empty);
            }
            else
            {
                Session.SendPacket($"info {Language.Instance.GetMessageFromKey("BAD_PASSWORD")}");
            }
        }

        /// <summary>
        /// Load Characters, this is the Entrypoint for the Client, Wait for 3 Packets.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        [Packet("OpenNos.EntryPoint", 3)]
        public void LoadCharacters(string packet)
        {
            string[] loginPacketParts = packet.Split(' ');

            // Load account by given SessionId
            if (Session.Account == null)
            {
                bool hasRegisteredAccountLogin = true;
                AccountDTO account = null;
                if (loginPacketParts.Length > 4)
                {
                    account = DAOFactory.AccountDAO.LoadByName(loginPacketParts[4]);
                }
                try
                {
                    if (account != null)
                    {
                        hasRegisteredAccountLogin = CommunicationServiceClient.Instance.IsLoginPermitted(account.AccountId, Session.SessionId);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("MS Communication Failed.", ex);
                    Session.Disconnect();
                    return;
                }
                if (loginPacketParts.Length > 4 && hasRegisteredAccountLogin)
                {
                    if (account != null)
                    {
                        if (account.Password.ToLower().Equals(EncryptionBase.Sha512(loginPacketParts[6])))
                        {
                            Account accountobject = new Account
                            {
                                AccountId = account.AccountId,
                                Name = account.Name,
                                Password = account.Password.ToLower(),
                                Authority = account.Authority
                            };
                            accountobject.Initialize();

                            Session.InitializeAccount(accountobject);
                        }
                        else
                        {
                            Logger.Log.ErrorFormat($"Client {Session.ClientId} forced Disconnection, invalid Password or SessionId.");
                            Session.Disconnect();
                            return;
                        }
                    }
                    else
                    {
                        Logger.Log.ErrorFormat($"Client {Session.ClientId} forced Disconnection, invalid AccountName.");
                        Session.Disconnect();
                        return;
                    }
                }
                else
                {
                    Logger.Log.ErrorFormat($"Client {Session.ClientId} forced Disconnection, login has not been registered or Account is already logged in.");
                    Session.Disconnect();
                    return;
                }
            }

            // TODO: Wrap Database access up to GO
            IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(Session.Account.AccountId);
            Logger.Log.InfoFormat(Language.Instance.GetMessageFromKey("ACCOUNT_ARRIVED"), Session.SessionId);

            // load characterlist packet for each character in CharacterDTO
            Session.SendPacket("clist_start 0");
            foreach (CharacterDTO character in characters)
            {
                IEnumerable<ItemInstanceDTO> inventory = DAOFactory.IteminstanceDAO.LoadByType(character.CharacterId, InventoryType.Wear);

                WearableInstance[] equipment = new WearableInstance[16];
                foreach (ItemInstanceDTO equipmentEntry in inventory)
                {
                    // explicit load of iteminstance
                    WearableInstance currentInstance = equipmentEntry as WearableInstance;
                    equipment[(short)currentInstance.Item.EquipmentSlot] = currentInstance;
                }
                string petlist = string.Empty;
                List<MateDTO> mates = DAOFactory.MateDAO.LoadByCharacterId(character.CharacterId).ToList();
                for (int i = 0; i < 26; i++)
                {
                    //0.2105.1102.319.0.632.0.333.0.318.0.317.0.9.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1
                    petlist += $"{(i != 0 ? "." : "")}{(mates.Count > i ? $"{mates.ElementAt(i).Skin}.{mates.ElementAt(i).NpcMonsterVNum}" : "-1")}";
                }

                // 1 1 before long string of -1.-1 = act completion
                Session.SendPacket($"clist {character.Slot} {character.Name} 0 {(byte)character.Gender} {(byte)character.HairStyle} {(byte)character.HairColor} 0 {(byte)character.Class} {character.Level} {character.HeroLevel} {equipment[(byte)EquipmentType.Hat]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.Armor]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.WeaponSkin]?.ItemVNum ?? (equipment[(byte)EquipmentType.MainWeapon]?.ItemVNum ?? -1)}.{equipment[(byte)EquipmentType.SecondaryWeapon]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.Mask]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.Fairy]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.CostumeSuit]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.CostumeHat]?.ItemVNum ?? -1} {character.JobLevel}  1 1 {petlist} {(equipment[(byte)EquipmentType.Hat] != null && equipment[(byte)EquipmentType.Hat].Item.IsColored ? equipment[(byte)EquipmentType.Hat].Design : 0)} 0");
            }
            Session.SendPacket("clist_end");
        }

        /// <summary>
        /// select packet
        /// </summary>
        /// <param name="selectPacket"></param>
        public void SelectCharacter(SelectPacket selectPacket)
        {
            try
            {
                if (Session?.Account != null && !Session.HasSelectedCharacter)
                {
                    if (DAOFactory.CharacterDAO.LoadBySlot(Session.Account.AccountId, selectPacket.Slot) is Character character)
                    {
                        character.GeneralLogs = DAOFactory.GeneralLogDAO.LoadByAccount(Session.Account.AccountId).Where(s => s.CharacterId == character.CharacterId).ToList();
                        character.MapInstanceId = ServerManager.Instance.GetBaseMapInstanceIdByMapId(character.MapId);
                        character.PositionX = character.MapX;
                        character.PositionY = character.MapY;
                        character.Authority = Session.Account.Authority;
                        Session.SetCharacter(character);
                        if (!Session.Character.GeneralLogs.Any(s => s.Timestamp == DateTime.Now && s.LogData == "World" && s.LogType == "Connection"))
                        {
                            Session.Character.SpAdditionPoint += Session.Character.SpPoint;
                            Session.Character.SpPoint = 10000;
                        }
                        if (Session.Character.Hp > Session.Character.HPLoad())
                        {
                            Session.Character.Hp = (int)Session.Character.HPLoad();
                        }
                        if (Session.Character.Mp > Session.Character.MPLoad())
                        {
                            Session.Character.Mp = (int)Session.Character.MPLoad();
                        }
                        Session.Character.Respawns = DAOFactory.RespawnDAO.LoadByCharacter(Session.Character.CharacterId).ToList();
                        Session.Character.StaticBonusList = DAOFactory.StaticBonusDAO.LoadByCharacterId(Session.Character.CharacterId).ToList();
                        Session.Character.LoadInventory();
                        Session.Character.LoadQuicklists();
                        Session.Character.GenerateMiniland();
                        DAOFactory.MateDAO.LoadByCharacterId(Session.Character.CharacterId).ToList().ForEach(s =>
                        {
                            Mate mate = (Mate)s;
                            mate.Owner = Session.Character;
                            mate.GeneateMateTransportId();
                            mate.Monster = ServerManager.Instance.GetNpc(s.NpcMonsterVNum);
                            Session.Character.Mates.Add(mate);
                        });
                        Observable.Interval(TimeSpan.FromMilliseconds(300)).Subscribe(x =>
                        {
                            Session.Character.CharacterLife();
                        });
                        Session.Character.GeneralLogs.Add(new GeneralLogDTO { AccountId = Session.Account.AccountId, CharacterId = Session.Character.CharacterId, IpAddress = Session.IpAddress, LogData = "World", LogType = "Connection", Timestamp = DateTime.Now });
                        
                        Session.SendPacket("OK");

                        // Inform everyone about connected character
                        CommunicationServiceClient.Instance.ConnectCharacter(ServerManager.Instance.WorldId, character.CharacterId);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Select character failed.", ex);
            }
        }

        #endregion
    }
}