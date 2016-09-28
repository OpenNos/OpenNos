using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.ServiceRef.Internal;
using System;
using System.Collections.Generic;

namespace OpenNos.Handler
{
    public class CharacterScreenPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public CharacterScreenPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        public ClientSession Session
        {
            get
            {
                return _session;
            }
        }

        #endregion

        #region Methods

        [Packet("Char_NEW")]
        public void CreateCharacter(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            if (Session.CurrentMap != null)
            {
                return;
            }

            // TODO: Hold Account Information in Authorized object
            long accountId = Session.Account.AccountId;
            string[] packetsplit = packet.Split(' ');

            byte slot = Convert.ToByte(packetsplit[3]);
            string characterName = packetsplit[2];
            Random random = new Random();
            if (slot <= 2 && DAOFactory.CharacterDAO.LoadBySlot(accountId, slot) == null)
            {
                if (characterName.Length > 3 && characterName.Length < 15)
                {
                    bool isIllegalCharacter = false;

                    System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(@"^[\u4E00-\u9FA5a-zA-Z0-9ąáàâäãåçćéèêëęíìîïłńñóòôöõúùûüśźżýÿæœĄÁÀÂÄÃÅĆÇĘÉÈÊËÍÌÎÏŁŃÑÓÒÔÖÕÚÙÛÜŚŹŻÝŸÆŒ.¤*-|#²§µß™€=$^<>£!()&~{}@]*$`~");
                    isIllegalCharacter = rg.IsMatch(characterName);

                    if (!isIllegalCharacter)
                    {
                        if (DAOFactory.CharacterDAO.LoadByName(characterName) == null)
                        {
                            if (Convert.ToByte(packetsplit[3]) > 2)
                            {
                                return;
                            }
                            CharacterDTO newCharacter = new CharacterDTO()
                            {
                                Class = (byte)ClassType.Adventurer,
                                Gender = (Convert.ToByte(packetsplit[4]) >= 0 && Convert.ToByte(packetsplit[4]) <= 1 ? Convert.ToByte(packetsplit[4]) : Convert.ToByte(0)),
                                Gold = 0,
                                HairColor = Enum.IsDefined(typeof(HairColorType), Convert.ToByte(packetsplit[6])) ? Convert.ToByte(packetsplit[6]) : Convert.ToByte(0),
                                HairStyle = Enum.IsDefined(typeof(HairStyleType), Convert.ToByte(packetsplit[5])) ? Convert.ToByte(packetsplit[5]) : Convert.ToByte(0),
                                Hp = 221,
                                JobLevel = 1,
                                JobLevelXp = 0,
                                Level = 1,
                                LevelXp = 0,
                                MapId = 1,
                                MapX = (short)(random.Next(78, 81)),
                                MapY = (short)(random.Next(114, 118)),
                                Mp = 221,
                                Name = characterName,
                                Slot = slot,
                                AccountId = accountId,
                                StateEnum = CharacterState.Active,
                                WhisperBlocked = false,
                                FamilyRequestBlocked = false,
                                ExchangeBlocked = false,
                                BuffBlocked = false,
                                EmoticonsBlocked = false,
                                FriendRequestBlocked = false,
                                GroupRequestBlocked = false,
                                MinilandInviteBlocked = false,
                                HeroChatBlocked = false,
                                QuickGetUp = false,
                                MouseAimLock = false,
                                HpBlocked = false,
                            };

                            SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref newCharacter);
                            CharacterSkillDTO sk1 = new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 200 };
                            CharacterSkillDTO sk2 = new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 201 };
                            CharacterSkillDTO sk3 = new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 209 };
                            QuicklistEntryDTO qlst1 = new QuicklistEntryDTO
                            {
                                CharacterId = newCharacter.CharacterId,
                                Q1 = 0,
                                Q2 = 0,
                                Type = 1,
                                Slot = 1,
                                Pos = 1
                            };
                            QuicklistEntryDTO qlst2 = new QuicklistEntryDTO
                            {
                                CharacterId = newCharacter.CharacterId,
                                Q1 = 0,
                                Q2 = 1,
                                Type = 0,
                                Slot = 2,
                                Pos = 0
                            };
                            QuicklistEntryDTO qlst3 = new QuicklistEntryDTO
                            {
                                CharacterId = newCharacter.CharacterId,
                                Q1 = 0,
                                Q2 = 8,
                                Type = 1,
                                Slot = 1,
                                Pos = 16
                            };
                            QuicklistEntryDTO qlst4 = new QuicklistEntryDTO
                            {
                                CharacterId = newCharacter.CharacterId,
                                Q1 = 0,
                                Q2 = 9,
                                Type = 1,
                                Slot = 3,
                                Pos = 1
                            };
                            qlst1 = DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst1);
                            qlst2 = DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst2);
                            qlst3 = DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst3);
                            qlst4 = DAOFactory.QuicklistEntryDAO.InsertOrUpdate(qlst4);
                            sk1 = DAOFactory.CharacterSkillDAO.InsertOrUpdate(sk1);
                            sk2 = DAOFactory.CharacterSkillDAO.InsertOrUpdate(sk2);
                            sk3 = DAOFactory.CharacterSkillDAO.InsertOrUpdate(sk3);

                            IList<InventoryDTO> startupInventory = new List<InventoryDTO>();
                            InventoryDTO inventory = new InventoryDTO() // first weapon
                            {
                                CharacterId = newCharacter.CharacterId,
                                Slot = (short)EquipmentType.MainWeapon,
                                Type = InventoryType.Equipment
                            };
                            inventory.ItemInstance = new WearableInstance() { Amount = 1, ItemVNum = 1, Id = inventory.Id };
                            startupInventory.Add(inventory);

                            inventory = new InventoryDTO() // second weapon
                            {
                                CharacterId = newCharacter.CharacterId,
                                Slot = (short)EquipmentType.SecondaryWeapon,
                                Type = InventoryType.Equipment
                            };
                            inventory.ItemInstance = new WearableInstance() { Amount = 1, ItemVNum = 8, Id = inventory.Id };
                            startupInventory.Add(inventory);

                            inventory = new InventoryDTO() // armor
                            {
                                CharacterId = newCharacter.CharacterId,
                                Slot = (short)EquipmentType.Armor,
                                Type = InventoryType.Equipment
                            };
                            inventory.ItemInstance = new WearableInstance() { Amount = 1, ItemVNum = 12, Id = inventory.Id };
                            startupInventory.Add(inventory);

                            inventory = new InventoryDTO() // snack
                            {
                                CharacterId = newCharacter.CharacterId,
                                Slot = 0,
                                Type = InventoryType.Etc
                            };
                            inventory.ItemInstance = new ItemInstance() { Amount = 10, ItemVNum = 2024, Id = inventory.Id };
                            startupInventory.Add(inventory);

                            inventory = new InventoryDTO() // ammo
                            {
                                CharacterId = newCharacter.CharacterId,
                                Slot = 1,
                                Type = InventoryType.Etc
                            };
                            inventory.ItemInstance = new ItemInstance() { Amount = 1, ItemVNum = 2081, Id = inventory.Id };
                            startupInventory.Add(inventory);

                            DAOFactory.InventoryDAO.InsertOrUpdate(startupInventory);
                            LoadCharacters(packet);
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

        [Packet("Char_DEL")]
        public void DeleteCharacter(string packet)
        {
            Logger.Debug(packet, Session.SessionId);

            if (Session.HasCurrentMap)
            {
                return;
            }
            string[] deleteCharacterPacket = packet.Split(' ');
            AccountDTO account = DAOFactory.AccountDAO.LoadBySessionId(Session.SessionId);
            if (account == null)
            {
                return;
            }
            if (deleteCharacterPacket.Length <= 3)
            {
                return;
            }
            if (account != null && account.Password.ToLower() == EncryptionBase.Sha512(deleteCharacterPacket[3]))
            {
                CharacterDTO character = DAOFactory.CharacterDAO.LoadBySlot(account.AccountId, Convert.ToByte(deleteCharacterPacket[2]));
                if (character == null)
                {
                    return;
                }
                DAOFactory.GeneralLogDAO.SetCharIdNull(Convert.ToInt64(character.CharacterId));
                DAOFactory.CharacterDAO.DeleteByPrimaryKey(account.AccountId, Convert.ToByte(deleteCharacterPacket[2]));
                LoadCharacters(packet);
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
                try
                {
                    hasRegisteredAccountLogin = ServiceFactory.Instance.CommunicationService.HasRegisteredAccountLogin(loginPacketParts[4], Session.SessionId);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("WCF Communication Failed.", ex);
                }
                if (loginPacketParts.Length > 4 && hasRegisteredAccountLogin)
                {
                    AccountDTO accountDTO = DAOFactory.AccountDAO.LoadByName(loginPacketParts[4]);

                    if (accountDTO != null)
                    {
                        if (accountDTO.Password.ToLower().Equals(EncryptionBase.Sha512(loginPacketParts[6])))
                        {
                            var account = new Account()
                            {
                                AccountId = accountDTO.AccountId,
                                Name = accountDTO.Name,
                                Password = accountDTO.Password.ToLower(),
                                Authority = accountDTO.Authority,
                                LastCompliment = accountDTO.LastCompliment,
                            };
                            foreach (PenaltyLogDTO penalty in DAOFactory.PenaltyLogDAO.LoadByAccount(accountDTO.AccountId))
                            {
                                account.PenaltyLogs.Add(new PenaltyLog()
                                {
                                    AccountId = penalty.AccountId,
                                    DateEnd = penalty.DateEnd,
                                    DateStart = penalty.DateStart,
                                    Reason = penalty.Reason,
                                    Penalty = penalty.Penalty,
                                    PenaltyLogId = penalty.PenaltyLogId
                                });
                            }
                            foreach (GeneralLogDTO general in DAOFactory.GeneralLogDAO.LoadByAccount(accountDTO.AccountId))
                            {
                                account.GeneralLogs.Add(new GeneralLog()
                                {
                                    AccountId = general.AccountId,
                                    LogData = general.LogData,
                                    IpAddress = general.IpAddress,
                                    LogType = general.LogType,
                                    LogId = general.LogId,
                                    Timestamp = general.Timestamp,
                                    CharacterId = general.CharacterId
                                });
                            }
                            Session.InitializeAccount(account);
                        }
                        else
                        {
                            Logger.Log.ErrorFormat($"Client {Session.ClientId} forced Disconnection, invalid Password or SessionId.");
                            Session.Disconnect();
                        }
                    }
                    else
                    {
                        Logger.Log.ErrorFormat($"Client {Session.ClientId} forced Disconnection, invalid AccountName.");
                        Session.Disconnect();
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
            Session.SendPacket("clist_start 0");
            foreach (CharacterDTO character in characters)
            {
                IEnumerable<InventoryDTO> inventory = DAOFactory.InventoryDAO.LoadByType(character.CharacterId, InventoryType.Equipment);

                WearableInstance[] equipment = new WearableInstance[16];
                foreach (InventoryDTO equipmentEntry in inventory)
                {
                    // explicit load of iteminstance
                    WearableInstance currentInstance = equipmentEntry.ItemInstance as WearableInstance;
                    equipment[currentInstance.Item.EquipmentSlot] = currentInstance;
                }

                // 1 1 before long string of -1.-1 = act completion
                Session.SendPacket($"clist {character.Slot} {character.Name} 0 {character.Gender} {character.HairStyle} {character.HairColor} 0 {character.Class} {character.Level} {character.HeroLevel} {(equipment[(byte)EquipmentType.Hat] != null ? equipment[(byte)EquipmentType.Hat].ItemVNum : -1)}.{(equipment[(byte)EquipmentType.Armor] != null ? equipment[(byte)EquipmentType.Armor].ItemVNum : -1)}.{(equipment[(byte)EquipmentType.WeaponSkin] != null ? equipment[(byte)EquipmentType.WeaponSkin].ItemVNum : equipment[(byte)EquipmentType.MainWeapon] != null ? equipment[(byte)EquipmentType.MainWeapon].ItemVNum : -1)}.{(equipment[(byte)EquipmentType.SecondaryWeapon] != null ? equipment[(byte)EquipmentType.SecondaryWeapon].ItemVNum : -1)}.{(equipment[(byte)EquipmentType.Mask] != null ? equipment[(byte)EquipmentType.Mask].ItemVNum : -1)}.{(equipment[(byte)EquipmentType.Fairy] != null ? equipment[(byte)EquipmentType.Fairy].ItemVNum : -1)}.{(equipment[(byte)EquipmentType.CostumeSuit] != null ? equipment[(byte)EquipmentType.CostumeSuit].ItemVNum : -1)}.{(equipment[(byte)EquipmentType.CostumeHat] != null ? equipment[(byte)EquipmentType.CostumeHat].ItemVNum : -1)} {character.JobLevel}  1 1 -1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1 {(equipment[(byte)EquipmentType.Hat] != null && equipment[(byte)EquipmentType.Hat].Item.IsColored ? equipment[(byte)EquipmentType.Hat].Design : 0)} 0");
            }
            Session.SendPacket("clist_end");
        }

        [Packet("select")]
        public void SelectCharacter(string packet)
        {
            try
            {
                Logger.Debug(packet, Session.SessionId);
                if (Session != null && Session.Account != null && Session.Character == null)
                {
                    string[] packetsplit = packet.Split(' ');
                    CharacterDTO characterDTO = DAOFactory.CharacterDAO.LoadBySlot(Session.Account.AccountId, Convert.ToByte(packetsplit[2]));
                    if (characterDTO != null)
                    {
                        Session.Character = new Character(Session)
                        {
                            AccountId = characterDTO.AccountId,
                            CharacterId = characterDTO.CharacterId,
                            Class = characterDTO.Class,
                            Dignity = characterDTO.Dignity,
                            Gender = characterDTO.Gender,
                            Gold = characterDTO.Gold,
                            HairColor = characterDTO.HairColor,
                            HairStyle = characterDTO.HairStyle,
                            Hp = characterDTO.Hp,
                            JobLevel = characterDTO.JobLevel,
                            JobLevelXp = characterDTO.JobLevelXp,
                            Level = characterDTO.Level,
                            LevelXp = characterDTO.LevelXp,
                            MapId = characterDTO.MapId,
                            MapX = characterDTO.MapX,
                            MapY = characterDTO.MapY,
                            Mp = characterDTO.Mp,
                            State = characterDTO.State,
                            Faction = characterDTO.Faction,
                            Name = characterDTO.Name,
                            Reput = characterDTO.Reput,
                            Slot = characterDTO.Slot,
                            Authority = Session.Account.Authority,
                            SpAdditionPoint = characterDTO.SpAdditionPoint,
                            SpPoint = characterDTO.SpPoint,
                            LastPulse = 0,
                            LastPortal = 0,
                            LastSp = 0,
                            Invisible = false,
                            InvisibleGm = false,
                            ArenaWinner = characterDTO.ArenaWinner,
                            Morph = 0,
                            MorphUpgrade = 0,
                            MorphUpgrade2 = 0,
                            Direction = 0,
                            IsSitting = false,
                            BackPack = characterDTO.Backpack,
                            Compliment = characterDTO.Compliment,
                            Backpack = characterDTO.Backpack,
                            BuffBlocked = characterDTO.BuffBlocked,
                            EmoticonsBlocked = characterDTO.EmoticonsBlocked,
                            WhisperBlocked = characterDTO.WhisperBlocked,
                            FamilyRequestBlocked = characterDTO.FamilyRequestBlocked,
                            ExchangeBlocked = characterDTO.ExchangeBlocked,
                            FriendRequestBlocked = characterDTO.FriendRequestBlocked,
                            GroupRequestBlocked = characterDTO.GroupRequestBlocked,
                            HeroChatBlocked = characterDTO.HeroChatBlocked,
                            HpBlocked = characterDTO.HpBlocked,
                            MinilandInviteBlocked = characterDTO.MinilandInviteBlocked,
                            QuickGetUp = characterDTO.QuickGetUp,
                            MouseAimLock = characterDTO.MouseAimLock,
                            LastLogin = DateTime.Now,
                            SnackHp = 0,
                            SnackMp = 0,
                            SnackAmount = 0,
                            MaxSnack = 0,
                            HeroLevel = characterDTO.HeroLevel,
                            HeroXp = characterDTO.HeroXp
                        };
                        Session.Character.Update();
                        Session.Character.LoadInventory();
                        Session.Character.LoadQuicklists();
                        DAOFactory.AccountDAO.WriteGeneralLog(Session.Character.AccountId, Session.IpAddress, Session.Character.CharacterId, "Connection", "World");
                        Session.HasSelectedCharacter = true;
                        Session.SendPacket("OK");

                        // Inform everyone about connected character
                        ServiceFactory.Instance.CommunicationService.ConnectCharacter(Session.Character.Name, Session.Account.Name);
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