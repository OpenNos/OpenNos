using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.WebApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

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

        private ClientSession Session => _session;

        #endregion

        #region Methods

        [Packet("Char_NEW")]
        public void CreateCharacter(string packet)
        {
            Logger.Debug(Session.GenerateIdentity(), packet);
            if (Session.HasCurrentMapInstance)
            {
                return;
            }

            // TODO: Hold Account Information in Authorized object
            long accountId = Session.Account.AccountId;
            string[] packetsplit = packet.Split(' ');

            byte slot = Convert.ToByte(packetsplit[3]);
            string characterName = packetsplit[2];
            if (slot <= 2 && DAOFactory.CharacterDAO.LoadBySlot(accountId, slot) == null)
            {
                if (characterName.Length > 3 && characterName.Length < 15)
                {
                    Regex rg = new Regex(@"^[\u0021-\u007E\u00A1-\u00AC\u00AE-\u00FF\u4E00-\u9FA5\u0E01-\u0E3A\u0E3F-\u0E5B]*$");
                    int isIllegalCharacter = rg.Matches(characterName).Count;

                    if (isIllegalCharacter == 1)
                    {
                        if (DAOFactory.CharacterDAO.LoadByName(characterName) == null)
                        {
                            if (Convert.ToByte(packetsplit[3]) > 2)
                            {
                                return;
                            }
                            CharacterDTO newCharacter = new CharacterDTO
                            {
                                Class = (byte)ClassType.Adventurer,
                                Gender = (GenderType)Enum.Parse(typeof(GenderType), packetsplit[4]),
                                HairColor = (HairColorType)Enum.Parse(typeof(HairColorType), packetsplit[6]),
                                HairStyle = (HairStyleType)Enum.Parse(typeof(HairStyleType), packetsplit[5]),
                                Hp = 221,
                                JobLevel = 1,
                                Level = 1,
                                MapId = 1,
                                MapX = (short)ServerManager.RandomNumber(78, 81),
                                MapY = (short)ServerManager.RandomNumber(114, 118),
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

                            IList<ItemInstanceDTO> startupInventory = new List<ItemInstanceDTO>();
                            ItemInstance inventory = new WearableInstance // first weapon
                            {
                                CharacterId = newCharacter.CharacterId,
                                Slot = (byte)EquipmentType.MainWeapon,
                                Type = InventoryType.Wear,
                                Amount = 1,
                                ItemVNum = 1
                            };
                            startupInventory.Add(inventory);

                            inventory = new WearableInstance // second weapon
                            {
                                CharacterId = newCharacter.CharacterId,
                                Slot = (byte)EquipmentType.SecondaryWeapon,
                                Type = InventoryType.Wear,
                                Amount = 1,
                                ItemVNum = 8
                            };
                            startupInventory.Add(inventory);

                            inventory = new WearableInstance // armor
                            {
                                CharacterId = newCharacter.CharacterId,
                                Slot = (byte)EquipmentType.Armor,
                                Type = InventoryType.Wear,
                                Amount = 1,
                                ItemVNum = 12
                            };
                            startupInventory.Add(inventory);

                            inventory = new ItemInstance // snack
                            {
                                CharacterId = newCharacter.CharacterId,
                                Type = InventoryType.Etc,
                                Amount = 10,
                                ItemVNum = 2024
                            };
                            startupInventory.Add(inventory);

                            inventory = new ItemInstance // ammo
                            {
                                CharacterId = newCharacter.CharacterId,
                                Slot = 1,
                                Type = InventoryType.Etc,
                                Amount = 1,
                                ItemVNum = 2081
                            };
                            startupInventory.Add(inventory);

                            DAOFactory.IteminstanceDAO.InsertOrUpdate(startupInventory);
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
                try
                {
                    hasRegisteredAccountLogin = ServerCommunicationClient.Instance.HubProxy.Invoke<bool>("HasRegisteredAccountLogin", loginPacketParts[4], Session.SessionId).Result;
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("WCF Communication Failed.", ex);
                }
                if (loginPacketParts.Length > 4 && hasRegisteredAccountLogin)
                {
                    AccountDTO account = DAOFactory.AccountDAO.LoadByName(loginPacketParts[4]);

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

        [Packet("select")]
        public void SelectCharacter(string packet)
        {
            try
            {
                Logger.Debug(Session.GenerateIdentity(), packet);
                if (Session?.Account != null && !Session.HasSelectedCharacter)
                {
                    string[] packetsplit = packet.Split(' ');
                    Character character = DAOFactory.CharacterDAO.LoadBySlot(Session.Account.AccountId, Convert.ToByte(packetsplit[2])) as Character;
                    if (character != null)
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
                            mate.Monster = ServerManager.GetNpc(s.NpcMonsterVNum);
                            Session.Character.Mates.Add(mate);
                        });
                        Observable.Interval(TimeSpan.FromMilliseconds(300)).Subscribe(x =>
                        {
                            Session.Character.CharacterLife();
                        });
                        Session.Character.GeneralLogs.Add(new GeneralLogDTO { AccountId = Session.Account.AccountId, CharacterId = Session.Character.CharacterId, IpAddress = Session.IpAddress, LogData = "World", LogType = "Connection", Timestamp = DateTime.Now });

                        Session.SendPacket("OK");

                        // Inform everyone about connected character
                        ServerCommunicationClient.Instance.HubProxy.Invoke("ConnectCharacter", ServerManager.ServerGroup, ServerManager.Instance.WorldId, Session.Character.Name, Session.Character.CharacterId);
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