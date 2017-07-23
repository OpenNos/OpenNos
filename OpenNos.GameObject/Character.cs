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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Packets.ServerPackets;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using OpenNos.PathFinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using static OpenNos.Domain.BCardType;

namespace OpenNos.GameObject
{
    public class Character : CharacterDTO
    {
        #region Members

        private Random _random;
        private byte _speed;
        private readonly object _syncObj = new object();

        #endregion

        #region Instantiation

        public Character()
        {
            GroupSentRequestCharacterIds = new List<long>();
            FamilyInviteCharacters = new List<long>();
            TradeRequests = new List<long>();
            FriendRequestCharacters = new List<long>();
            StaticBonusList = new List<StaticBonusDTO>();
            MinilandObjects = new List<MinilandObject>();
            Mates = new List<Mate>();
            EquipmentBCards = new List<BCard>();
        }

        #endregion

        #region Properties

        public List<BCard> EquipmentBCards { get; set; }

        public AuthorityType Authority { get; set; }

        public Node[,] BrushFire { get; set; }

        public List<Buff> Buff { get; internal set; }

        public bool CanFight => !IsSitting && ExchangeInfo == null;    

        public List<CharacterRelationDTO> CharacterRelations
        {
            get
            {
                lock (ServerManager.Instance.CharacterRelations)
                {
                    return ServerManager.Instance.CharacterRelations == null ? new List<CharacterRelationDTO>() : ServerManager.Instance.CharacterRelations.Where(s => s.CharacterId == CharacterId || s.RelatedCharacterId == CharacterId).ToList();
                }
            }
        }

        public short CurrentMinigame { get; set; }

        public int DarkResistance { get; set; }

        public int Defence { get; set; }

        public int DefenceRate { get; set; }

        public int Direction { get; set; }

        public int DistanceCritical { get; set; }

        public int DistanceCriticalRate { get; set; }

        public int DistanceDefence { get; set; }

        public int DistanceDefenceRate { get; set; }

        public int DistanceRate { get; set; }

        public byte Element { get; set; }

        public int ElementRate { get; set; }

        public int ElementRateSP { get; private set; }

        public ExchangeInfo ExchangeInfo { get; set; }

        public Family Family
        {
            get
            {
                lock (ServerManager.Instance.FamilyList)
                {
                    return ServerManager.Instance.FamilyList.FirstOrDefault(s => s != null && s.FamilyCharacters.Any(c => c != null && c.CharacterId == CharacterId));
                }
            }
        }

        public FamilyCharacterDTO FamilyCharacter
        {
            get
            {
                return Family?.FamilyCharacters.FirstOrDefault(s => s.CharacterId == CharacterId);
            }
        }

        public List<long> FamilyInviteCharacters { get; set; }

        public int FireResistance { get; set; }

        public int FoodAmount { get; set; }

        public int FoodHp { get; set; }

        public int FoodMp { get; set; }

        public List<long> FriendRequestCharacters { get; set; }

        public List<GeneralLogDTO> GeneralLogs { get; set; }

        public bool GmPvtBlock { get; set; }

        public Group Group { get; set; }

        public List<long> GroupSentRequestCharacterIds { get; set; }

        public bool HasGodMode { get; set; }

        public bool HasShopOpened { get; set; }

        public int HitCritical { get; set; }

        public int HitCriticalRate { get; set; }

        public int HitRate { get; set; }

        public bool InExchangeOrTrade => ExchangeInfo != null || Speed == 0;

        public Inventory Inventory { get; set; }

        public bool Invisible { get; set; }

        public bool InvisibleGm { get; set; }

        public bool IsChangingMapInstance { get; set; }

        public bool IsCustomSpeed { get; set; }

        public bool IsDancing { get; set; }

        /// <summary>
        /// Defines if the Character Is currently sending or getting items thru exchange.
        /// </summary>
        public bool IsExchanging { get; set; }

        public bool IsShopping { get; set; }

        public bool IsSitting { get; set; }

        public bool IsVehicled { get; set; }

        public bool IsWaitingForEvent { get; set; }

        public DateTime LastDefence { get; set; }

        public DateTime LastDelay { get; set; }

        public DateTime LastEffect { get; set; }

        public DateTime LastHealth { get; set; }

        public DateTime LastMapObject { get; set; }

        public int LastMonsterId { get; set; }

        public DateTime LastMove { get; set; }

        public int LastNRunId { get; set; }

        public double LastPortal { get; set; }

        public DateTime LastPotion { get; set; }

        public int LastPulse { get; set; }

        public DateTime LastPVPRevive { get; set; }

        public DateTime LastSkillUse { get; set; }

        public double LastSp { get; set; }

        public DateTime LastSpeedChange { get; set; }

        public DateTime LastSpGaugeRemove { get; set; }

        public DateTime LastTransform { get; set; }

        public int LightResistance { get; set; }

        public int MagicalDefence { get; set; }

        public IDictionary<int, MailDTO> MailList { get; set; }

        public MapInstance MapInstance => ServerManager.Instance.GetMapInstance(MapInstanceId);

        public Guid MapInstanceId { get; set; }

        public List<Mate> Mates { get; set; }

        public int MaxDistance { get; set; }

        public int MaxFood { get; set; }

        public int MaxHit { get; set; }

        public int MaxSnack { get; set; }

        public int MinDistance { get; set; }

        public int MinHit { get; set; }

        public MapInstance Miniland { get; private set; }

        public List<MinilandObject> MinilandObjects { get; set; }

        public int Morph { get; set; }

        public int MorphUpgrade { get; set; }

        public int MorphUpgrade2 { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public List<QuicklistEntryDTO> QuicklistEntries { get; private set; }

        public RespawnMapTypeDTO Respawn
        {
            get
            {
                RespawnMapTypeDTO respawn = new RespawnMapTypeDTO
                {
                    DefaultX = 79,
                    DefaultY = 116,
                    DefaultMapId = 1,
                    RespawnMapTypeId = -1
                };

                if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.Map.MapTypes.Any())
                {
                    long? respawnmaptype = Session.CurrentMapInstance.Map.MapTypes.ElementAt(0).RespawnMapTypeId;
                    if (respawnmaptype != null)
                    {
                        RespawnDTO resp = Respawns.FirstOrDefault(s => s.RespawnMapTypeId == respawnmaptype);
                        if (resp == null)
                        {
                            RespawnMapTypeDTO defaultresp = Session.CurrentMapInstance.Map.DefaultRespawn;
                            if (defaultresp != null)
                            {
                                respawn.DefaultX = defaultresp.DefaultX;
                                respawn.DefaultY = defaultresp.DefaultY;
                                respawn.DefaultMapId = defaultresp.DefaultMapId;
                                respawn.RespawnMapTypeId = (long)respawnmaptype;
                            }
                        }
                        else
                        {
                            respawn.DefaultX = resp.X;
                            respawn.DefaultY = resp.Y;
                            respawn.DefaultMapId = resp.MapId;
                            respawn.RespawnMapTypeId = (long)respawnmaptype;
                        }
                    }
                }
                return respawn;
            }
        }

        public List<RespawnDTO> Respawns { private get; set; }

        public RespawnMapTypeDTO Return
        {
            get
            {
                RespawnMapTypeDTO respawn = new RespawnMapTypeDTO();
                if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.Map.MapTypes.Any())
                {
                    long? respawnmaptype = Session.CurrentMapInstance.Map.MapTypes.ElementAt(0).ReturnMapTypeId;
                    if (respawnmaptype != null)
                    {
                        RespawnDTO resp = Respawns.FirstOrDefault(s => s.RespawnMapTypeId == respawnmaptype);
                        if (resp == null)
                        {
                            RespawnMapTypeDTO defaultresp = Session.CurrentMapInstance.Map.DefaultReturn;
                            if (defaultresp != null)
                            {
                                respawn.DefaultX = defaultresp.DefaultX;
                                respawn.DefaultY = defaultresp.DefaultY;
                                respawn.DefaultMapId = defaultresp.DefaultMapId;
                                respawn.RespawnMapTypeId = (long)respawnmaptype;
                            }
                        }
                        else
                        {
                            respawn.DefaultX = resp.X;
                            respawn.DefaultY = resp.Y;
                            respawn.DefaultMapId = resp.MapId;
                            respawn.RespawnMapTypeId = (long)respawnmaptype;
                        }
                    }
                }
                return respawn;
            }
        }

        public short SaveX { get; set; }

        public short SaveY { get; set; }

        public int ScPage { get; set; }

        public ClientSession Session { get; private set; }

        public int Size { get; set; } = 10;

        public ThreadSafeSortedList<int, CharacterSkill> Skills { get; private set; }

        public ThreadSafeSortedList<int, CharacterSkill> SkillsSp { get; set; }

        public int SnackAmount { get; set; }

        public int SnackHp { get; set; }

        public int SnackMp { get; set; }

        public int SpCooldown { get; set; }

        public byte Speed
        {
            get
            {
                byte bonusSpeed = (byte)GetBuff(CardType.Move, (byte)AdditionalTypes.Move.SetMovementNegated, false)[0];
                if (_speed + bonusSpeed > 59)
                {
                    return 59;
                }
                return (byte)(_speed + bonusSpeed);
            }

            set
            {
                LastSpeedChange = DateTime.Now;
                _speed = value > 59 ? (byte)59 : value;
            }
        }

        public List<StaticBonusDTO> StaticBonusList { get; set; }

        public int TimesUsed { get; set; }

        public List<long> TradeRequests { get; set; }

        public bool Undercover { get; set; }

        public bool UseSp { get; set; }

        public byte VehicleSpeed { private get; set; }

        public int WareHouseSize { get; set; }

        public int WaterResistance { get; private set; }

        #endregion

        #region Methods

        public bool AddPet(Mate mate)
        {
            if (mate.MateType == MateType.Pet ? MaxMateCount > Mates.Count() : 3 > Mates.Count(s => s.MateType == MateType.Partner))
            {
                Mates.Add(mate);
                MapInstance.Broadcast(mate.GenerateIn());
                Session.SendPacket(GenerateSay(string.Format(Language.Instance.GetMessageFromKey("YOU_GET_PET"), mate.Name), 12));
                Session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());
                Session.SendPackets(GenerateScP());
                Session.SendPackets(GenerateScN());
                return true;
            }
            return false;
        }

        public void AddRelation(long characterId, CharacterRelationType Relation)
        {
            CharacterRelationDTO addRelation = new CharacterRelationDTO
            {
                CharacterId = CharacterId,
                RelatedCharacterId = characterId,
                RelationType = Relation
            };

            DAOFactory.CharacterRelationDAO.InsertOrUpdate(ref addRelation);
            ServerManager.Instance.RelationRefresh(addRelation.CharacterRelationId);
            Session.SendPacket(GenerateFinit());
            ClientSession target = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.CharacterId == characterId);
            target?.SendPacket(target?.Character.GenerateFinit());
        }

        public void ChangeClass(ClassType characterClass)
        {
            JobLevel = 1;
            JobLevelXp = 0;
            Session.SendPacket("npinfo 0");
            Session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());

            if (characterClass == (byte)ClassType.Adventurer)
            {
                HairStyle = (byte)HairStyle > 1 ? 0 : HairStyle;
            }
            LoadSpeed();
            Class = characterClass;
            Hp = (int)HPLoad();
            Mp = (int)MPLoad();
            Session.SendPacket(GenerateTit());
            Session.SendPacket(GenerateStat());
            Session.CurrentMapInstance?.Broadcast(Session, GenerateEq());
            Session.CurrentMapInstance?.Broadcast(GenerateEff(8), PositionX, PositionY);
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CLASS_CHANGED"), 0));
            Session.CurrentMapInstance?.Broadcast(GenerateEff(196), PositionX, PositionY);
            int faction = 1 + ServerManager.Instance.RandomNumber(0, 2);
            Faction = faction;
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{faction}"), 0));
            Session.SendPacket("scr 0 0 0 0 0 0");
            Session.SendPacket(GenerateFaction());
            Session.SendPacket(GenerateStatChar());
            Session.SendPacket(GenerateEff(4799 + faction));
            Session.SendPacket(GenerateCond());
            Session.SendPacket(GenerateLev());
            Session.CurrentMapInstance?.Broadcast(Session, GenerateCMode());
            Session.CurrentMapInstance?.Broadcast(Session, GenerateIn(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(Session, GenerateGidx(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(GenerateEff(6), PositionX, PositionY);
            Session.CurrentMapInstance?.Broadcast(GenerateEff(198), PositionX, PositionY);
            foreach (CharacterSkill skill in Skills.GetAllItems())
            {
                if (skill.SkillVNum >= 200)
                {
                    Skills.Remove(skill.SkillVNum);
                }
            }

            Skills[(short)(200 + 20 * (byte)Class)] = new CharacterSkill { SkillVNum = (short)(200 + 20 * (byte)Class), CharacterId = CharacterId };
            Skills[(short)(201 + 20 * (byte)Class)] = new CharacterSkill { SkillVNum = (short)(201 + 20 * (byte)Class), CharacterId = CharacterId };
            Skills[236] = new CharacterSkill { SkillVNum = 236, CharacterId = CharacterId };

            Session.SendPacket(GenerateSki());

            foreach (QuicklistEntryDTO quicklists in DAOFactory.QuicklistEntryDAO.LoadByCharacterId(CharacterId).Where(quicklists => QuicklistEntries.Any(qle => qle.Id == quicklists.Id)))
            {
                DAOFactory.QuicklistEntryDAO.Delete(quicklists.Id);
            }

            QuicklistEntries = new List<QuicklistEntryDTO>
                {
                    new QuicklistEntryDTO
                    {
                        CharacterId = CharacterId,
                        Q1 = 0,
                        Q2 = 9,
                        Type = 1,
                        Slot = 3,
                        Pos = 1
                    }
                };
            if (ServerManager.Instance.Groups.Any(s => s.IsMemberOfGroup(Session) && s.GroupType == GroupType.Group))
            {
                Session.CurrentMapInstance?.Broadcast(Session, $"pidx 1 1.{CharacterId}", ReceiverType.AllExceptMe);
            }
        }

        public void ChangeSex()
        {
            Gender = Gender == GenderType.Female ? GenderType.Male : GenderType.Female;
            if (IsVehicled)
            {
                Morph = Gender == GenderType.Female ? Morph + 1 : Morph - 1;
            }
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SEX_CHANGED"), 0));
            Session.SendPacket(GenerateEq());
            Session.SendPacket(GenerateGender());
            Session.CurrentMapInstance?.Broadcast(Session, GenerateIn(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(Session, GenerateGidx(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(GenerateCMode());
            Session.CurrentMapInstance?.Broadcast(GenerateEff(196), PositionX, PositionY);
        }

        public void CharacterLife()
        {
            int x = 1;
            bool change = false;
            if (Hp == 0 && LastHealth.AddSeconds(2) <= DateTime.Now)
            {
                Mp = 0;
                Session.SendPacket(GenerateStat());
                LastHealth = DateTime.Now;
            }
            else
            {
                if (CurrentMinigame != 0 && LastEffect.AddSeconds(3) <= DateTime.Now)
                {
                    Session.CurrentMapInstance?.Broadcast(GenerateEff(CurrentMinigame));
                    LastEffect = DateTime.Now;
                }

                if (LastEffect.AddSeconds(5) <= DateTime.Now)
                {
                    if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        Session.SendPacket(Session.Character.GenerateRaid(3, false));
                    }

                    WearableInstance amulet = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Amulet, InventoryType.Wear);
                    if (amulet != null)
                    {
                        if (amulet.ItemVNum == 4503 || amulet.ItemVNum == 4504)
                        {
                            Session.CurrentMapInstance?.Broadcast(GenerateEff(amulet.Item.EffectValue + (Class == ClassType.Adventurer ? 0 : (byte)Class - 1)), PositionX, PositionY);
                        }
                        else
                        {
                            Session.CurrentMapInstance?.Broadcast(GenerateEff(amulet.Item.EffectValue), PositionX, PositionY);
                        }
                    }
                    if (Group != null && (Group.GroupType == GroupType.Team || Group.GroupType == GroupType.BigTeam || Group.GroupType == GroupType.GiantTeam))
                    {
                        Session.CurrentMapInstance?.Broadcast(Session, GenerateEff(828 + (Group.IsLeader(Session) ? 1 : 0)), ReceiverType.AllExceptGroup);
                        Session.CurrentMapInstance?.Broadcast(Session, GenerateEff(830 + (Group.IsLeader(Session) ? 1 : 0)), ReceiverType.Group);
                    }
                    Mates.Where(s => s.CanPickUp).ToList().ForEach(s => Session.CurrentMapInstance?.Broadcast(s.GenerateEff(3007)));
                    LastEffect = DateTime.Now;
                }

                if (LastHealth.AddSeconds(2) <= DateTime.Now || IsSitting && LastHealth.AddSeconds(1.5) <= DateTime.Now)
                {
                    LastHealth = DateTime.Now;
                    if (Session.HealthStop)
                    {
                        Session.HealthStop = false;
                        return;
                    }

                    if (LastDefence.AddSeconds(4) <= DateTime.Now && LastSkillUse.AddSeconds(2) <= DateTime.Now && Hp > 0)
                    {
                        if (x == 0)
                        {
                            x = 1;
                        }
                        if (Hp + HealthHPLoad() < HPLoad())
                        {
                            change = true;
                            Hp += HealthHPLoad();
                        }
                        else
                        {
                            if (Hp != (int)HPLoad())
                            {
                                change = true;
                            }
                            Hp = (int)HPLoad();
                        }
                        if (x == 1)
                        {
                            if (Mp + HealthMPLoad() < MPLoad())
                            {
                                Mp += HealthMPLoad();
                                change = true;
                            }
                            else
                            {
                                if (Mp != (int)MPLoad())
                                {
                                    change = true;
                                }
                                Mp = (int)MPLoad();
                            }
                        }
                        if (change)
                        {
                            if (Session.Character.Group != null)
                            {
                                Session.Character.Group.Characters.ForEach(s => ServerManager.Instance.GetSessionByCharacterId(s.Character.CharacterId)?.SendPacket(Session.Character.GenerateStat()));
                            }
                            else
                            {
                                Session.SendPacket(GenerateStat());
                            }
                        }
                    }
                }
                if (UseSp)
                {
                    if (LastSpGaugeRemove <= new DateTime(0001, 01, 01, 00, 00, 00))
                    {
                        LastSpGaugeRemove = DateTime.Now;
                    }
                    if (LastSkillUse.AddSeconds(15) >= DateTime.Now && LastSpGaugeRemove.AddSeconds(1) <= DateTime.Now)
                    {
                        SpecialistInstance specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                        if (specialist == null)
                        {
                            return;
                        }
                        byte spType = 0;

                        if (specialist.Item.Morph > 1 && specialist.Item.Morph < 8 || specialist.Item.Morph > 9 && specialist.Item.Morph < 16)
                        {
                            spType = 3;
                        }
                        else if (specialist.Item.Morph > 16 && specialist.Item.Morph < 29)
                        {
                            spType = 2;
                        }
                        else if (specialist.Item.Morph == 9)
                        {
                            spType = 1;
                        }
                        if (SpPoint >= spType)
                        {
                            SpPoint -= spType;
                        }
                        else if (SpPoint < spType && SpPoint != 0)
                        {
                            spType -= (byte)SpPoint;
                            SpPoint = 0;
                            SpAdditionPoint -= spType;
                        }
                        else if (SpPoint == 0 && SpAdditionPoint >= spType)
                        {
                            SpAdditionPoint -= spType;
                        }
                        else if (SpPoint == 0 && SpAdditionPoint < spType)
                        {
                            SpAdditionPoint = 0;

                            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;

                            if (UseSp)
                            {
                                LastSp = currentRunningSeconds;
                                if (Session != null && Session.HasSession)
                                {
                                    if (IsVehicled)
                                    {
                                        return;
                                    }
                                    Logger.Debug(GenerateIdentity(), specialist.ItemVNum.ToString());
                                    UseSp = false;
                                    LoadSpeed();
                                    Session.SendPacket(GenerateCond());
                                    Session.SendPacket(GenerateLev());
                                    SpCooldown = 30;
                                    if (SkillsSp != null)
                                    {
                                        foreach (CharacterSkill ski in SkillsSp.GetAllItems().Where(s => !s.CanBeUsed()))
                                        {
                                            short time = ski.Skill.Cooldown;
                                            double temp = (ski.LastUse - DateTime.Now).TotalMilliseconds + time * 100;
                                            temp /= 1000;
                                            SpCooldown = temp > SpCooldown ? (int)temp : SpCooldown;
                                        }
                                    }
                                    Session.SendPacket(GenerateSay(string.Format(Language.Instance.GetMessageFromKey("STAY_TIME"), SpCooldown), 11));
                                    Session.SendPacket($"sd {SpCooldown}");
                                    Session.CurrentMapInstance?.Broadcast(GenerateCMode());
                                    Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(6, 1, CharacterId), PositionX, PositionY);

                                    // ms_c
                                    Session.SendPacket(GenerateSki());
                                    Session.SendPackets(GenerateQuicklist());
                                    Session.SendPacket(GenerateStat());
                                    Session.SendPacket(GenerateStatChar());
                                    Observable.Timer(TimeSpan.FromMilliseconds(SpCooldown * 1000)).Subscribe(o =>
                                    {
                                        Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPPEAR"), 11));
                                        Session.SendPacket("sd 0");
                                    });
                                }
                            }
                        }
                        Session.SendPacket(GenerateSpPoint());
                        LastSpGaugeRemove = DateTime.Now;
                    }
                }
            }
        }

        public void CloseExchangeOrTrade()
        {
            if (InExchangeOrTrade)
            {
                long? targetSessionId = ExchangeInfo?.TargetCharacterId;

                if (targetSessionId.HasValue && Session.HasCurrentMapInstance)
                {
                    ClientSession targetSession = Session.CurrentMapInstance.GetSessionByCharacterId(targetSessionId.Value);

                    if (targetSession == null)
                    {
                        return;
                    }

                    Session.SendPacket("exc_close 0");
                    targetSession.SendPacket("exc_close 0");
                    ExchangeInfo = null;
                    targetSession.Character.ExchangeInfo = null;
                }
            }
        }

        public void CloseShop()
        {
            if (HasShopOpened && Session.HasCurrentMapInstance)
            {
                KeyValuePair<long, MapShop> shop = Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(CharacterId));
                if (!shop.Equals(default(KeyValuePair<long, MapShop>)))
                {
                    Session.CurrentMapInstance.UserShops.Remove(shop.Key);

                    // declare that the shop cannot be closed
                    HasShopOpened = false;

                    Session.CurrentMapInstance?.Broadcast(GenerateShopEnd());
                    Session.CurrentMapInstance?.Broadcast(Session, GeneratePlayerFlag(0), ReceiverType.AllExceptMe);
                    IsSitting = false;
                    IsShopping = false; // close shop by character will always completely close the shop

                    LoadSpeed();
                    Session.SendPacket(GenerateCond());
                    Session.CurrentMapInstance?.Broadcast(GenerateRest());
                }
            }
        }

        public void Dance()
        {
            IsDancing = !IsDancing;
        }

        public Character DeepCopy()
        {
            Character clonedCharacter = (Character)MemberwiseClone();
            return clonedCharacter;
        }

        public void DeleteBlackList(long characterId)
        {
            CharacterRelationDTO chara = CharacterRelations.FirstOrDefault(s => s.RelatedCharacterId == characterId);
            if (chara != null)
            {
                long id = chara.CharacterRelationId;
                DAOFactory.CharacterRelationDAO.Delete(id);
                ServerManager.Instance.RelationRefresh(id);
                Session.SendPacket(GenerateBlinit());
            }
        }

        public void DeleteItem(InventoryType type, short slot)
        {
            if (Inventory != null)
            {
                Inventory.DeleteFromSlotAndType(slot, type);
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(type, slot));
            }
        }

        public void DeleteItemByItemInstanceId(Guid id)
        {
            if (Inventory != null)
            {
                Tuple<short, InventoryType> result = Inventory.DeleteById(id);
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(result.Item2, result.Item1));
            }
        }

        public void DeleteRelation(long characterId)
        {
            CharacterRelationDTO chara = CharacterRelations.FirstOrDefault(s => s.RelatedCharacterId == characterId || s.CharacterId == characterId);
            if (chara != null)
            {
                long id = chara.CharacterRelationId;
                CharacterDTO charac = DAOFactory.CharacterDAO.LoadById(characterId);
                DAOFactory.CharacterRelationDAO.Delete(id);
                ServerManager.Instance.RelationRefresh(id);

                Session.SendPacket(GenerateFinit());
                if (charac != null)
                {
                    List<CharacterRelationDTO> lst = ServerManager.Instance.CharacterRelations.Where(s => s.CharacterId == CharacterId || s.RelatedCharacterId == CharacterId).ToList();
                    string result = "finit";
                    foreach (CharacterRelationDTO relation in lst.Where(c => c.RelationType == CharacterRelationType.Friend))
                    {
                        long id2 = relation.RelatedCharacterId == CharacterId ? relation.CharacterId : relation.RelatedCharacterId;
                        bool isOnline = CommunicationServiceClient.Instance.IsCharacterConnected(ServerManager.Instance.ServerGroup, id2);
                        result += $" {id2}|{(short)relation.RelationType}|{(isOnline ? 1 : 0)}|{DAOFactory.CharacterDAO.LoadById(id2).Name}";
                    }
                    int? sentChannelId = CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                    {
                        DestinationCharacterId = charac.CharacterId,
                        SourceCharacterId = CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = result,
                        Type = MessageType.PrivateChat
                    });
                }
            }
        }

        public void DeleteTimeout()
        {
            if (Inventory == null)
            {
                return;
            }

            foreach (ItemInstance item in Inventory.GetAllItems())
            {
                if (item.IsBound && item.ItemDeleteTime != null && item.ItemDeleteTime < DateTime.Now)
                {
                    Inventory.DeleteById(item.Id);
                    Session.Character.EquipmentBCards.RemoveAll(o => o.ItemVNum == item.ItemVNum);
                    if (item.Type == InventoryType.Wear)
                    {
                        Session.SendPacket(GenerateEquipment());
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(item.Type, item.Slot));
                    }
                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                }
            }
        }

        /// <summary>
        /// Make the character moveable also from Teleport, ..
        /// </summary>
        public void Dispose()
        {
            CloseShop();
            CloseExchangeOrTrade();
            GroupSentRequestCharacterIds.Clear();
            FamilyInviteCharacters.Clear();
            FriendRequestCharacters.Clear();
        }

        public string GenerateAct()
        {
            return $"act 6"; // act6 1 0 14 0 0 0 14 0 0 0
        }

        public string GenerateAt()
        {
            MapInstance mapForMusic = MapInstance;

            //at 698495 20001 5 8 2 0 {SecondaryMusic} {SecondaryMusicType} -1
            return $"at {CharacterId} {MapInstance.Map.MapId} {PositionX} {PositionY} 2 0 {mapForMusic?.Map.Music ?? 0} -1";
        }

        public string GenerateBlinit()
        {
            string result = "blinit";
            foreach (CharacterRelationDTO relation in CharacterRelations.Where(s => s.CharacterId == CharacterId && s.RelationType == CharacterRelationType.Blocked))
            {
                result += $" {relation.RelatedCharacterId}|{DAOFactory.CharacterDAO.LoadById(relation.RelatedCharacterId).Name}";
            }
            return result;
        }

        public string GenerateCInfo()
        {
            return $"c_info {(Authority == AuthorityType.Moderator ? "[Support]" + Name : Name)} - -1 {(Family != null ? $"{Family.FamilyId} {Family.Name}({Language.Instance.GetMessageFromKey(FamilyCharacter.Authority.ToString().ToUpper())})" : "-1 -")} {CharacterId} {(Invisible ? 6 : Undercover ? (byte)AuthorityType.User : Authority < AuthorityType.User ? (byte)AuthorityType.User : (byte)Authority)} {(byte)Gender} {(byte)HairStyle} {(byte)HairColor} {(byte)Class} {(GetDignityIco() == 1 ? GetReputIco() : -GetDignityIco())} {(Authority == AuthorityType.Moderator ? 500 : Compliment)} {(UseSp || IsVehicled ? Morph : 0)} {(Invisible ? 1 : 0)} {Family?.FamilyLevel ?? 0} {(UseSp ? MorphUpgrade : 0)} {ArenaWinner}";
        }

        public string GenerateCMap()
        {
            return $"c_map 0 {MapInstance.Map.MapId} {(MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance ? 1 : 0)}";
        }

        public string GenerateCMode()
        {
            return $"c_mode 1 {CharacterId} {(UseSp || IsVehicled ? Morph : 0)} {(UseSp ? MorphUpgrade : 0)} {(UseSp ? MorphUpgrade2 : 0)} {ArenaWinner}";
        }

        public string GenerateCond()
        {
            return $"cond 1 {CharacterId} 0 0 {Speed}";
        }

        public ushort GenerateDamage(MapMonster monsterToAttack, Skill skill, ref int hitmode)
        {
            #region Definitions

            if (monsterToAttack == null)
            {
                return 0;
            }
            if (Inventory == null)
            {
                return 0;
            }

            // int miss_chance = 20;
            int monsterDefence = 0;
            int monsterDodge = 0;

            int morale = Level + GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased, false)[0];

            short mainUpgrade = (short)GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0];
            int mainCritChance = 0;
            int mainCritHit = 0;
            int mainMinDmg = 0;
            int mainMaxDmg = 0;
            int mainHitRate = morale;

            short secUpgrade = mainUpgrade;
            int secCritChance = 0;
            int secCritHit = 0;
            int secMinDmg = 0;
            int secMaxDmg = 0;
            int secHitRate = morale;

            // int CritChance = 4; int CritHit = 70; int MinDmg = 0; int MaxDmg = 0; int HitRate = 0;
            // sbyte Upgrade = 0;

            #endregion

            #region Get Weapon Stats

            WearableInstance weapon = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
            if (weapon != null)
            {
                mainUpgrade += weapon.Upgrade;
            }

            mainMinDmg += MinHit;
            mainMaxDmg += MaxHit;
            mainHitRate += HitRate;
            mainCritChance += HitCriticalRate;
            mainCritHit += HitCritical;

            WearableInstance weapon2 = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
            if (weapon2 != null)
            {
                secUpgrade += weapon2.Upgrade;
            }

            secMinDmg += MinDistance;
            secMaxDmg += MaxDistance;
            secHitRate += DistanceRate;
            secCritChance += DistanceCriticalRate;
            secCritHit += DistanceCritical;

            #endregion

            #region Switch skill.Type

            int boost, boostpercentage;

            switch (skill.Type)
            {
                case 0:
                    monsterDefence = monsterToAttack.Monster.CloseDefence;
                    monsterDodge = (int)(monsterToAttack.Monster.DefenceDodge * 0.95);
                    if (Class == ClassType.Archer)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                        + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                    boostpercentage = GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0]
                        + GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 1:
                    monsterDefence = monsterToAttack.Monster.DistanceDefence;
                    monsterDodge = (int)(monsterToAttack.Monster.DistanceDefenceDodge * 0.95);
                    if (Class == ClassType.Swordman || Class == ClassType.Adventurer || Class == ClassType.Magician)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                        + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, false)[0];
                    boostpercentage = GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0]
                        + GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 2:
                    monsterDefence = monsterToAttack.Monster.MagicDefence;
                    boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                    boostpercentage = GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0]
                        + GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 3:
                    switch (Class)
                    {
                        case ClassType.Swordman:
                            monsterDefence = monsterToAttack.Monster.CloseDefence;
                            boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                            boostpercentage = GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0]
                                + GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.IncreasingPropability, false)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;

                        case ClassType.Archer:
                            monsterDefence = monsterToAttack.Monster.DistanceDefence;
                            boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, false)[0];
                            boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                                + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedDecreased, false)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;

                        case ClassType.Magician:
                            monsterDefence = monsterToAttack.Monster.MagicDefence;
                            boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                            boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                                + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;

                        case ClassType.Adventurer:
                            monsterDefence = monsterToAttack.Monster.CloseDefence;
                            boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                            boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0]
                                + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;
                    }
                    break;

                case 5:
                    monsterDefence = monsterToAttack.Monster.CloseDefence;
                    monsterDodge = monsterToAttack.Monster.DefenceDodge;
                    if (Class == ClassType.Archer)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    if (Class == ClassType.Magician)
                    {
                        boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                        boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, false)[0];
                        mainMinDmg += boost;
                        mainMaxDmg += boost;
                        mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                        mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    }
                    else
                    {
                        boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                        boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, false)[0];
                        mainMinDmg += boost;
                        mainMaxDmg += boost;
                        mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                        mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    }
                    break;
            }

            #endregion

            #region Basic Damage Data Calculation

            mainCritChance += GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased, false)[0];
            mainCritChance -= GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased, false)[0];
            mainCritHit += GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased, false)[0];
            mainCritHit -= GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased, false)[0];

            mainUpgrade -= monsterToAttack.Monster.DefenceUpgrade;
            if (mainUpgrade < -10)
            {
                mainUpgrade = -10;
            }
            else if (mainUpgrade > 10)
            {
                mainUpgrade = 10;
            }

            #endregion

            #region Detailed Calculation

            #region Dodge

            if (Class != ClassType.Magician)
            {
                double multiplier = monsterDodge / (mainHitRate + 1);
                if (multiplier > 5)
                {
                    multiplier = 5;
                }
                double chance = -0.25 * Math.Pow(multiplier, 3) - 0.57 * Math.Pow(multiplier, 2) + 25.3 * multiplier - 1.41;
                if (chance <= 1)
                {
                    chance = 1;
                }
                if (GetBuff(CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased, false)[0] != 0)
                {
                    chance = 10;
                }
                if ((skill.Type == 0 || skill.Type == 1) && !HasGodMode)
                {
                    if (ServerManager.Instance.RandomNumber() <= chance)
                    {
                        hitmode = 1;
                        return 0;
                    }
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.Instance.RandomNumber(mainMinDmg, mainMaxDmg + 1);
            // baseDamage += skill.Damage / 4; it's a bcard need a skillbcardload
            baseDamage += morale - monsterToAttack.Monster.Level; //Morale
            if (Class == ClassType.Adventurer)
            {
                //HACK: Damage is ~10 lower in OpenNos than in official. Fix this...
                baseDamage += 20;
            }
            int elementalDamage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0];
            //   elementalDamage += skill.ElementalDamage / 4; it's a bcard need a skillbcardload
            switch (mainUpgrade)
            {
                case -10:
                    monsterDefence += monsterDefence * 2;
                    break;

                case -9:
                    monsterDefence += (int)(monsterDefence * 1.2);
                    break;

                case -8:
                    monsterDefence += (int)(monsterDefence * 0.9);
                    break;

                case -7:
                    monsterDefence += (int)(monsterDefence * 0.65);
                    break;

                case -6:
                    monsterDefence += (int)(monsterDefence * 0.54);
                    break;

                case -5:
                    monsterDefence += (int)(monsterDefence * 0.43);
                    break;

                case -4:
                    monsterDefence += (int)(monsterDefence * 0.32);
                    break;

                case -3:
                    monsterDefence += (int)(monsterDefence * 0.22);
                    break;

                case -2:
                    monsterDefence += (int)(monsterDefence * 0.15);
                    break;

                case -1:
                    monsterDefence += (int)(monsterDefence * 0.1);
                    break;

                case 0:
                    break;

                case 1:
                    baseDamage += (int)(baseDamage * 0.1);
                    break;

                case 2:
                    baseDamage += (int)(baseDamage * 0.15);
                    break;

                case 3:
                    baseDamage += (int)(baseDamage * 0.22);
                    break;

                case 4:
                    baseDamage += (int)(baseDamage * 0.32);
                    break;

                case 5:
                    baseDamage += (int)(baseDamage * 0.43);
                    break;

                case 6:
                    baseDamage += (int)(baseDamage * 0.54);
                    break;

                case 7:
                    baseDamage += (int)(baseDamage * 0.65);
                    break;

                case 8:
                    baseDamage += (int)(baseDamage * 0.9);
                    break;

                case 9:
                    baseDamage += (int)(baseDamage * 1.2);
                    break;

                case 10:
                    baseDamage += baseDamage * 2;
                    break;

                // sush don't tell ciapa
                default:
                    if (mainUpgrade > 10)
                    {
                        baseDamage += baseDamage * (mainUpgrade / 5);
                    }
                    break;
            }
            if (skill.Type == 1)
            {
                if (Map.GetDistance(new MapCell { X = PositionX, Y = PositionY }, new MapCell { X = monsterToAttack.MapX, Y = monsterToAttack.MapY }) < 4)
                    baseDamage = (int)(baseDamage * 0.85);
            }

            #endregion

            #region Elementary Damage

            #region Calculate Elemental Boost + Rate

            double elementalBoost = 0;
            short monsterResistance = 0;
            switch (Element)
            {
                case 0:
                    break;

                case 1:
                    elementalDamage += GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.FireIncreased, false)[0];
                    monsterResistance = monsterToAttack.Monster.FireResistance;
                    switch (monsterToAttack.Monster.Element)
                    {
                        case 0:
                            elementalBoost = 1.3; // Damage vs no element
                            break;

                        case 1:
                            elementalBoost = 1; // Damage vs fire
                            break;

                        case 2:
                            elementalBoost = 2; // Damage vs water
                            break;

                        case 3:
                            elementalBoost = 1; // Damage vs light
                            break;

                        case 4:
                            elementalBoost = 1.5; // Damage vs darkness
                            break;
                    }
                    break;

                case 2:
                    elementalDamage += GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.WaterIncreased, false)[0];
                    monsterResistance = monsterToAttack.Monster.WaterResistance;
                    switch (monsterToAttack.Monster.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 2;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1.5;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }
                    break;

                case 3:
                    elementalDamage += GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.LightIncreased, false)[0];
                    monsterResistance = monsterToAttack.Monster.LightResistance;
                    switch (monsterToAttack.Monster.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1.5;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1;
                            break;

                        case 4:
                            elementalBoost = 3;
                            break;
                    }
                    break;

                case 4:
                    elementalDamage += GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.DarkIncreased, false)[0];
                    monsterResistance = monsterToAttack.Monster.DarkResistance;
                    switch (monsterToAttack.Monster.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1;
                            break;

                        case 2:
                            elementalBoost = 1.5;
                            break;

                        case 3:
                            elementalBoost = 3;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }
                    break;
            }

            #endregion;

            if (skill.Element == 0)
            {
                if (elementalBoost == 0.5)
                {
                    elementalBoost = 0;
                }
                else if (elementalBoost == 1)
                {
                    elementalBoost = 0.05;
                }
                else if (elementalBoost == 1.3)
                {
                    elementalBoost = 0.15;
                }
                else if (elementalBoost == 1.5)
                {
                    elementalBoost = 0.15;
                }
                else if (elementalBoost == 2)
                {
                    elementalBoost = 0.2;
                }
                else if (elementalBoost == 3)
                {
                    elementalBoost = 0.2;
                }
            }
            else if (skill.Element != Element)
            {
                elementalBoost = 0;
            }

            elementalDamage = (int)((elementalDamage + (elementalDamage + baseDamage) * ((ElementRate + ElementRateSP) / 100D)) * elementalBoost);
            elementalDamage = elementalDamage / 100 * (100 - monsterResistance);

            #endregion

            #region Critical Damage

            baseDamage -= monsterDefence;
            if (GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased, false)[0] == 0)
            {
                if (ServerManager.Instance.RandomNumber() <= mainCritChance
                    || GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, false)[0] != 0)
                {
                    if (skill.Type == 2)
                    {
                    }
                    else if (skill.Type == 3 && Class != ClassType.Magician)
                    {
                        double multiplier = mainCritHit / 100D;
                        if (multiplier > 3)
                            multiplier = 3;
                        baseDamage += (int)(baseDamage * multiplier);
                        hitmode = 3;
                    }
                    else
                    {
                        double multiplier = mainCritHit / 100D;
                        if (multiplier > 3)
                            multiplier = 3;
                        baseDamage += (int)(baseDamage * multiplier);
                        hitmode = 3;
                    }
                }
            }

            #endregion

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage;
            if (totalDamage < 5)
            {
                totalDamage = ServerManager.Instance.RandomNumber(1, 6);
            }

            #endregion

            #endregion

            if (monsterToAttack.DamageList.ContainsKey(CharacterId))
            {
                monsterToAttack.DamageList[CharacterId] += totalDamage;
            }
            else
            {
                monsterToAttack.DamageList.Add(CharacterId, totalDamage);
            }
            if (monsterToAttack.CurrentHp <= totalDamage)
            {
                monsterToAttack.IsAlive = false;
                monsterToAttack.CurrentHp = 0;
                monsterToAttack.CurrentMp = 0;
                monsterToAttack.Death = DateTime.Now;
                monsterToAttack.LastMove = DateTime.Now;
            }
            else
            {
                monsterToAttack.CurrentHp -= totalDamage;
            }

            while (totalDamage > ushort.MaxValue)
            {
                totalDamage -= ushort.MaxValue;
            }

            // only set the hit delay if we become the monsters target with this hit
            if (monsterToAttack.Target == -1)
            {
                monsterToAttack.LastSkill = DateTime.Now;
            }
            ushort damage = Convert.ToUInt16(totalDamage);

            int nearestDistance = 100;
            foreach (KeyValuePair<long, long> kvp in monsterToAttack.DamageList)
            {
                ClientSession session = monsterToAttack.MapInstance.GetSessionByCharacterId(kvp.Key);
                if (session != null)
                {
                    int distance = Map.GetDistance(new MapCell
                    {
                        X = monsterToAttack.MapX,
                        Y = monsterToAttack.MapY
                    }, new MapCell
                    {
                        X = session.Character.PositionX,
                        Y = session.Character.PositionY
                    });
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        monsterToAttack.Target = session.Character.CharacterId;
                    }
                }
            }
            return damage;
        }

        public void GenerateDignity(NpcMonster monsterinfo)
        {
            if (Level < monsterinfo.Level && Dignity < 100 && Level > 20)
            {
                Dignity += (float)0.5;
                if (Dignity == (int)Dignity)
                {
                    Session.SendPacket(GenerateFd());
                    Session.CurrentMapInstance?.Broadcast(Session, GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, GenerateGidx(), ReceiverType.AllExceptMe);
                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("RESTORE_DIGNITY"), 11));
                }
            }
        }

        public string GenerateDir()
        {
            return $"dir 1 {CharacterId} {Direction}";
        }

        public EffectPacket GenerateEff(int effectid)
        {
            return new EffectPacket
            {
                EffectType = 1,
                CharacterId = CharacterId,
                Id = effectid
            };
        }

        public string GenerateEq()
        {
            int color = (byte)HairColor;
            WearableInstance head = Inventory?.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);

            if (head != null && head.Item.IsColored)
            {
                color = head.Design;
            }
            return $"eq {CharacterId} {(Invisible ? 6 : Undercover ? (byte)AuthorityType.User : (byte)Authority)} {(byte)Gender} {(byte)HairStyle} {color} {(byte)Class} {GenerateEqListForPacket()} {(!InvisibleGm ? GenerateEqRareUpgradeForPacket() : null)}";
        }

        public string GenerateEqListForPacket()
        {
            string[] invarray = new string[16];
            if (Inventory != null)
            {
                for (short i = 0; i < 16; i++)
                {
                    ItemInstance item = Inventory.LoadBySlotAndType(i, InventoryType.Wear);
                    if (item != null)
                    {
                        invarray[i] = item.ItemVNum.ToString();
                    }
                    else
                    {
                        invarray[i] = "-1";
                    }
                }
            }
            return $"{invarray[(byte)EquipmentType.Hat]}.{invarray[(byte)EquipmentType.Armor]}.{invarray[(byte)EquipmentType.MainWeapon]}.{invarray[(byte)EquipmentType.SecondaryWeapon]}.{invarray[(byte)EquipmentType.Mask]}.{invarray[(byte)EquipmentType.Fairy]}.{invarray[(byte)EquipmentType.CostumeSuit]}.{invarray[(byte)EquipmentType.CostumeHat]}.{invarray[(byte)EquipmentType.WeaponSkin]}";
        }

        public string GenerateEqRareUpgradeForPacket()
        {
            sbyte weaponRare = 0;
            byte weaponUpgrade = 0;
            sbyte armorRare = 0;
            byte armorUpgrade = 0;
            if (Inventory != null)
            {
                for (short i = 0; i < 15; i++)
                {
                    WearableInstance wearable = Inventory.LoadBySlotAndType<WearableInstance>(i, InventoryType.Wear);
                    if (wearable != null)
                    {
                        switch (wearable.Item.EquipmentSlot)
                        {
                            case EquipmentType.Armor:
                                armorRare = wearable.Rare;
                                armorUpgrade = wearable.Upgrade;
                                break;

                            case EquipmentType.MainWeapon:
                                weaponRare = wearable.Rare;
                                weaponUpgrade = wearable.Upgrade;
                                break;
                        }
                    }
                }
            }
            return $"{weaponUpgrade}{weaponRare} {armorUpgrade}{armorRare}";
        }

        public string GenerateEquipment()
        {
            string eqlist = string.Empty;
            sbyte weaponRare = 0;
            byte weaponUpgrade = 0;
            sbyte armorRare = 0;
            byte armorUpgrade = 0;

            for (short i = 0; i < 16; i++)
            {
                if (Inventory != null)
                {
                    ItemInstance item = Inventory.LoadBySlotAndType<WearableInstance>(i, InventoryType.Wear) ??
                                        Inventory.LoadBySlotAndType<SpecialistInstance>(i, InventoryType.Wear);
                    if (item != null)
                    {
                        switch (item.Item.EquipmentSlot)
                        {
                            case EquipmentType.Armor:
                                armorRare = item.Rare;
                                armorUpgrade = item.Upgrade;
                                break;

                            case EquipmentType.MainWeapon:
                                weaponRare = item.Rare;
                                weaponUpgrade = item.Upgrade;
                                break;
                        }
                        eqlist += $" {i}.{item.Item.VNum}.{item.Rare}.{(item.Item.IsColored ? item.Design : item.Upgrade)}.0";
                    }
                }
            }
            return $"equip {weaponUpgrade}{weaponRare} {armorUpgrade}{armorRare}{eqlist}";
        }

        public string GenerateExts()
        {
            return $"exts 0 {48 + (HaveBackpack() ? 1 : 0) * 12} {48 + (HaveBackpack() ? 1 : 0) * 12} {48 + (HaveBackpack() ? 1 : 0) * 12}";
        }

        public string GenerateFaction()
        {
            return $"fs {Faction}";
        }

        public string GenerateFamilyMember()
        {
            string str = "gmbr 0";
            try
            {
                if (Family?.FamilyCharacters != null)
                {
                    foreach (FamilyCharacter TargetCharacter in Family?.FamilyCharacters)
                    {
                        bool isOnline = CommunicationServiceClient.Instance.IsCharacterConnected(ServerManager.Instance.ServerGroup, TargetCharacter.CharacterId);
                        str += $" {TargetCharacter.Character.CharacterId}|{Family.FamilyId}|{TargetCharacter.Character.Name}|{TargetCharacter.Character.Level}|{(byte)TargetCharacter.Character.Class}|{(byte)TargetCharacter.Authority}|{(byte)TargetCharacter.Rank}|{(isOnline ? 1 : 0)}|{TargetCharacter.Character.HeroLevel}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return str;
        }

        public string GenerateFamilyMemberExp()
        {
            string str = "gexp";
            try
            {
                if (Family?.FamilyCharacters != null)
                {
                    foreach (FamilyCharacter TargetCharacter in Family?.FamilyCharacters)
                    {
                        str += $" {TargetCharacter.CharacterId}|{TargetCharacter.Experience}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return str;
        }

        public string GenerateFamilyMemberMessage()
        {
            string str = "gmsg";
            try
            {
                if (Family?.FamilyCharacters != null)
                {
                    foreach (FamilyCharacter TargetCharacter in Family?.FamilyCharacters)
                    {
                        str += $" {TargetCharacter.CharacterId}|{TargetCharacter.DailyMessage}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return str;
        }

        public List<string> GenerateFamilyWarehouseHist()
        {
            if (Family != null)
            {
                List<string> packetList = new List<string>();
                string packet = string.Empty;
                int i = 0;
                int amount = -1;
                foreach (FamilyLogDTO log in Family.FamilyLogs.Where(s => s.FamilyLogType == FamilyLogType.WareHouseAdded || s.FamilyLogType == FamilyLogType.WareHouseRemoved).OrderByDescending(s => s.Timestamp).Take(100))
                {
                    packet += $" {(log.FamilyLogType == FamilyLogType.WareHouseAdded ? 0 : 1)}|{log.FamilyLogData}|{(int)(DateTime.Now - log.Timestamp).TotalHours}";
                    i++;
                    if (i == 50)
                    {
                        i = 0;
                        packetList.Add($"fslog_stc {amount}{packet}");
                        amount++;
                    }
                    else if (i == Family.FamilyLogs.Count)
                    {
                        packetList.Add($"fslog_stc {amount}{packet}");
                    }
                }

                return packetList;
            }
            return new List<string>();
        }

        public void GenerateFamilyXp(int FXP)
        {
            if (!Session.Account.PenaltyLogs.Any(s => s.Penalty == PenaltyType.BlockFExp && s.DateEnd > DateTime.Now))
            {
                if (Family != null && FamilyCharacter != null)
                {
                    FamilyCharacterDTO famchar = FamilyCharacter;
                    FamilyDTO fam = Family;
                    fam.FamilyExperience += FXP;
                    famchar.Experience += FXP;
                    if (CharacterHelper.LoadFamilyXPData(Family.FamilyLevel) <= fam.FamilyExperience)
                    {
                        fam.FamilyExperience -= CharacterHelper.LoadFamilyXPData(Family.FamilyLevel);
                        fam.FamilyLevel++;
                        Family.InsertFamilyLog(FamilyLogType.FamilyLevelUp, level: fam.FamilyLevel);
                        CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                        {
                            DestinationCharacterId = Family.FamilyId,
                            SourceCharacterId = CharacterId,
                            SourceWorldId = ServerManager.Instance.WorldId,
                            Message = UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAMILY_UP")), 0),
                            Type = MessageType.Family
                        });
                    }
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref famchar);
                    DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                    ServerManager.Instance.FamilyRefresh(Family.FamilyId);
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                    {
                        DestinationCharacterId = Family.FamilyId,
                        SourceCharacterId = CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = "fhis_stc",
                        Type = MessageType.Family
                    });
                }
            }
        }

        public string GenerateFd()
        {
            return $"fd {Reput} {GetReputIco()} {(int)Dignity} {Math.Abs(GetDignityIco())}";
        }

        public string GenerateFinfo(long? relatedCharacterLoggedId, bool isConnected)
        {
            string result = "finfo";

            foreach (CharacterRelationDTO relation in CharacterRelations.Where(c => c.RelationType == CharacterRelationType.Friend))
            {
                if (relatedCharacterLoggedId.HasValue && (relatedCharacterLoggedId.Value == relation.RelatedCharacterId || relatedCharacterLoggedId.Value == relation.CharacterId))
                {
                    result += $" {relation.RelatedCharacterId}.{(isConnected ? 1 : 0)}";
                }
            }

            return result;
        }

        public string GenerateFinit()
        {
            string result = "finit";
            foreach (CharacterRelationDTO relation in CharacterRelations.Where(c => c.RelationType == CharacterRelationType.Friend))
            {
                long id = relation.RelatedCharacterId == CharacterId ? relation.CharacterId : relation.RelatedCharacterId;
                bool isOnline = CommunicationServiceClient.Instance.IsCharacterConnected(ServerManager.Instance.ServerGroup, id);
                result += $" {id}|{(short)relation.RelationType}|{(isOnline ? 1 : 0)}|{DAOFactory.CharacterDAO.LoadById(id).Name}";
            }
            return result;
        }

        public string GenerateFStashAll()
        {
            string stash = $"f_stash_all {Family.WarehouseSize}";
            foreach (ItemInstance item in Family.Warehouse.GetAllItems())
            {
                stash += $" {item.GenerateStashPacket()}";
            }
            return stash;
        }

        public string GenerateGender()
        {
            return $"p_sex {(byte)Gender}";
        }

        public string GenerateGet(long id)
        {
            return $"get 1 {CharacterId} {id} 0";
        }

        public string GenerateGExp()
        {
            string str = "gexp";
            foreach (FamilyCharacter familyCharacter in Family.FamilyCharacters)
            {
                str += $" {familyCharacter.CharacterId}|{familyCharacter.Experience}";
            }
            return str;
        }

        public string GenerateGidx()
        {
            return Family != null ? $"gidx 1 {CharacterId} {Family.FamilyId} {Family.Name}({Language.Instance.GetMessageFromKey(Family.FamilyCharacters.FirstOrDefault(s => s.CharacterId == CharacterId)?.Authority.ToString().ToUpper())}) {Family.FamilyLevel}" : $"gidx 1 {CharacterId} -1 - 0";
        }

        public string GenerateGInfo()
        {
            if (Family != null)
            {
                try
                {
                    FamilyCharacter familyCharacter = Family.FamilyCharacters.FirstOrDefault(s => s.Authority == FamilyAuthority.Head);
                    if (familyCharacter != null)
                    {
                        return $"ginfo {Family.Name} {familyCharacter.Character.Name} {(byte)Family.FamilyHeadGender} {Family.FamilyLevel} {Family.FamilyExperience} {CharacterHelper.LoadFamilyXPData(Family.FamilyLevel)} {Family.FamilyCharacters.Count} {Family.MaxSize} {(byte)FamilyCharacter.Authority} {(Family.ManagerCanInvite ? 1 : 0)} {(Family.ManagerCanNotice ? 1 : 0)} {(Family.ManagerCanShout ? 1 : 0)} {(Family.ManagerCanGetHistory ? 1 : 0)} {(byte)Family.ManagerAuthorityType} {(Family.MemberCanGetHistory ? 1 : 0)} {(byte)Family.MemberAuthorityType} {Family.FamilyMessage.Replace(' ', '^')}";
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public string GenerateGold()
        {
            return $"gold {Gold} 0";
        }

        public string GenerateIcon(int v1, int v2, short itemVNum)
        {
            return $"icon {v1} {CharacterId} {v2} {itemVNum}";
        }

        public string GenerateIdentity()
        {
            return $"Character: {Name}";
        }

        public string GenerateIn()
        {
            int color = (byte)HairColor;
            ItemInstance fairy = null;
            if (Inventory != null)
            {
                WearableInstance headWearable = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);
                if (headWearable != null && headWearable.Item.IsColored)
                {
                    color = headWearable.Design;
                }
                fairy = Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
            }
            return $"in 1 {(Authority == AuthorityType.Moderator ? "[Support]" + Name : Name)} - {CharacterId} {PositionX} {PositionY} {Direction} {(Undercover ? (byte)AuthorityType.User : Authority < AuthorityType.User ? (byte)AuthorityType.User : (byte)Authority)} {(byte)Gender} {(byte)HairStyle} {color} {(byte)Class} {GenerateEqListForPacket()} {Math.Ceiling(Hp / HPLoad() * 100)} {Math.Ceiling(Mp / MPLoad() * 100)} {(IsSitting ? 1 : 0)} {(Group?.GroupType == GroupType.Group ? (Group?.GroupId ?? -1) : -1)} {(fairy != null ? 4 : 0)} {fairy?.Item.Element ?? 0} 0 {fairy?.Item.Morph ?? 0} 0 {(UseSp || IsVehicled ? Morph : 0)} {GenerateEqRareUpgradeForPacket()} {Family?.FamilyId ?? -1} {Family?.Name ?? "-"} {(GetDignityIco() == 1 ? GetReputIco() : -GetDignityIco())} {(Invisible ? 1 : 0)} {(UseSp ? MorphUpgrade : 0)} 0 {(UseSp ? MorphUpgrade2 : 0)} {Level} {Family?.FamilyLevel ?? 0} {ArenaWinner} {(Authority == AuthorityType.Moderator ? 500 : Compliment)} {Size} {HeroLevel}";
        }

        public string GenerateInvisible()
        {
            return $"cl {CharacterId} {(Invisible ? 1 : 0)} {(InvisibleGm ? 1 : 0)}";
        }

        public void GenerateKillBonus(MapMonster monsterToAttack)
        {
            lock (_syncObj)
            {
                if (monsterToAttack == null || monsterToAttack.IsAlive)
                {
                    return;
                }
                monsterToAttack.RunDeathEvent();

                Random random = new Random(DateTime.Now.Millisecond & monsterToAttack.MapMonsterId);

                // owner set
                long? dropOwner = monsterToAttack.DamageList.Any() ? monsterToAttack.DamageList.First().Key : (long?)null;
                Group group = null;
                if (dropOwner != null)
                {
                    group = ServerManager.Instance.Groups.FirstOrDefault(g => g.IsMemberOfGroup((long)dropOwner));
                }

                // end owner set
                if (Session.HasCurrentMapInstance)
                {
                    List<DropDTO> droplist = monsterToAttack.Monster.Drops.Where(s => Session.CurrentMapInstance.Map.MapTypes.Any(m => m.MapTypeId == s.MapTypeId) || s.MapTypeId == null).ToList();
                    if (monsterToAttack.Monster.MonsterType != MonsterType.Special)
                    {
                        #region item drop

                        int dropRate = ServerManager.Instance.DropRate * MapInstance.DropRate;
                        int x = 0;
                        foreach (DropDTO drop in droplist.OrderBy(s => random.Next()))
                        {
                            if (x < 4)
                            {
                                double rndamount = ServerManager.Instance.RandomNumber() * random.NextDouble();
                                if (rndamount <= (double)drop.DropChance * dropRate / 5000d)
                                {
                                    x++;
                                    if (Session.CurrentMapInstance != null)
                                    {
                                        if (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4) || monsterToAttack.Monster.MonsterType == MonsterType.Elite)
                                        {
                                            List<long> alreadyGifted = new List<long>();
                                            foreach (long charId in monsterToAttack.DamageList.Keys)
                                            {
                                                if (!alreadyGifted.Contains(charId))
                                                {
                                                    ClientSession giftsession = ServerManager.Instance.GetSessionByCharacterId(charId);
                                                    giftsession?.Character.GiftAdd(drop.ItemVNum, (byte)drop.Amount);
                                                    alreadyGifted.Add(charId);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (group != null && group.GroupType == GroupType.Group)
                                            {
                                                if (group.SharingMode == (byte)GroupSharingType.ByOrder)
                                                {
                                                    dropOwner = group.GetNextOrderedCharacterId(this);
                                                    if (dropOwner.HasValue)
                                                    {
                                                        group.Characters.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ITEM_BOUND_TO"), ServerManager.Instance.GetItem(drop.ItemVNum).Name, group.Characters.Single(c => c.Character.CharacterId == (long)dropOwner).Character.Name, drop.Amount), 10)));
                                                    }
                                                }
                                                else
                                                {
                                                    group.Characters.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("DROPPED_ITEM"), ServerManager.Instance.GetItem(drop.ItemVNum).Name, drop.Amount), 10)));
                                                }
                                            }

                                            long? owner = dropOwner;
                                            Observable.Timer(TimeSpan.FromMilliseconds(500)).Subscribe(o =>
                                            {
                                                if (Session.HasCurrentMapInstance)
                                                {
                                                    Session.CurrentMapInstance.DropItemByMonster(owner, drop, monsterToAttack.MapX, monsterToAttack.MapY);
                                                }
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        #region gold drop

                        // gold calculation
                        int gold = GetGold(monsterToAttack);
                        long maxGold = ServerManager.Instance.MaxGold;
                        gold = gold > maxGold ? (int)maxGold : gold;
                        double randChance = ServerManager.Instance.RandomNumber() * random.NextDouble();

                        if (gold > 0 && randChance <= (int)(ServerManager.Instance.GoldDropRate * 10 * CharacterHelper.GoldPenalty(Level, monsterToAttack.Monster.Level)))
                        {
                            DropDTO drop2 = new DropDTO
                            {
                                Amount = gold,
                                ItemVNum = 1046
                            };
                            if (Session.CurrentMapInstance != null)
                            {
                                if (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4) || monsterToAttack.Monster.MonsterType == MonsterType.Elite)
                                {
                                    List<long> alreadyGifted = new List<long>();
                                    foreach (long charId in monsterToAttack.DamageList.Keys)
                                    {
                                        if (!alreadyGifted.Contains(charId))
                                        {
                                            ClientSession session = ServerManager.Instance.GetSessionByCharacterId(charId);
                                            if (session != null)
                                            {
                                                session.Character.Gold += drop2.Amount;
                                                if (session.Character.Gold > maxGold)
                                                {
                                                    session.Character.Gold = maxGold;
                                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                                                }
                                                session.SendPacket(session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.Instance.GetItem(drop2.ItemVNum).Name} x {drop2.Amount}", 10));
                                                session.SendPacket(session.Character.GenerateGold());
                                            }
                                            alreadyGifted.Add(charId);
                                        }
                                    }
                                }
                                else
                                {
                                    if (group != null && MapInstance.MapInstanceType != MapInstanceType.LodInstance)
                                    {
                                        if (group.SharingMode == (byte)GroupSharingType.ByOrder)
                                        {
                                            dropOwner = group.GetNextOrderedCharacterId(this);

                                            if (dropOwner.HasValue)
                                            {
                                                group.Characters.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ITEM_BOUND_TO"), ServerManager.Instance.GetItem(drop2.ItemVNum).Name, group.Characters.Single(c => c.Character.CharacterId == (long)dropOwner).Character.Name, drop2.Amount), 10)));
                                            }
                                        }
                                        else
                                        {
                                            group.Characters.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("DROPPED_ITEM"), ServerManager.Instance.GetItem(drop2.ItemVNum).Name, drop2.Amount), 10)));
                                        }
                                    }

                                    // delayed Drop
                                    Observable.Timer(TimeSpan.FromMilliseconds(500))
                                          .Subscribe(
                                          o =>
                                          {
                                              if (Session.HasCurrentMapInstance)
                                              {
                                                  Session.CurrentMapInstance.DropItemByMonster(dropOwner, drop2, monsterToAttack.MapX, monsterToAttack.MapY);
                                              }
                                          });
                                }
                            }
                        }

                        #endregion

                        #region exp

                        if (Hp > 0)
                        {
                            Group grp = ServerManager.Instance.Groups.FirstOrDefault(g => g.IsMemberOfGroup(CharacterId));
                            if (grp != null)
                            {
                                foreach (ClientSession targetSession in grp.Characters.Where(g => g.Character.MapInstanceId == MapInstanceId))
                                {
                                    if (grp.IsMemberOfGroup(monsterToAttack.DamageList.FirstOrDefault().Key))
                                    {
                                        targetSession.Character.GenerateXp(monsterToAttack, true);
                                    }
                                    else
                                    {
                                        targetSession.SendPacket(targetSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("XP_NOTFIRSTHIT"), 10));
                                        targetSession.Character.GenerateXp(monsterToAttack, false);
                                    }
                                }
                            }
                            else
                            {
                                if (monsterToAttack.DamageList.FirstOrDefault().Key == CharacterId)
                                {
                                    GenerateXp(monsterToAttack, true);
                                }
                                else
                                {
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("XP_NOTFIRSTHIT"), 10));
                                    GenerateXp(monsterToAttack, false);
                                }
                            }
                            GenerateDignity(monsterToAttack.Monster);
                        }

                        #endregion
                    }
                }
            }
        }

        public string GenerateLev()
        {
            SpecialistInstance specialist = null;
            if (Inventory != null)
            {
                specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            return $"lev {Level} {LevelXp} {(!UseSp || specialist == null ? JobLevel : specialist.SpLevel)} {(!UseSp || specialist == null ? JobLevelXp : specialist.XP)} {XPLoad()} {(!UseSp || specialist == null ? JobXPLoad() : SPXPLoad())} {Reput} {GetCP()} {HeroXp} {HeroLevel} {HeroXPLoad()} {0}";
        }

        public string GenerateLevelUp()
        {
            return $"levelup {CharacterId}";
        }

        public void GenerateMiniland()
        {
            if (Miniland == null)
            {
                Miniland = ServerManager.Instance.GenerateMapInstance(20001, MapInstanceType.NormalInstance, new InstanceBag());
                foreach (MinilandObjectDTO obj in DAOFactory.MinilandObjectDAO.LoadByCharacterId(CharacterId))
                {
                    MinilandObject mapobj = (MinilandObject)obj;
                    if (mapobj.ItemInstanceId != null)
                    {
                        ItemInstance item = Inventory.LoadByItemInstance<ItemInstance>((Guid)mapobj.ItemInstanceId);
                        if (item != null)
                        {
                            mapobj.ItemInstance = item;
                            MinilandObjects.Add(mapobj);
                        }
                    }
                }
            }
        }

        public string GenerateMinilandObjectForFriends()
        {
            string mlobjstring = "mltobj";
            int i = 0;
            foreach (MinilandObject mp in MinilandObjects)
            {
                mlobjstring += $" {mp.ItemInstance.ItemVNum}.{i}.{mp.MapX}.{mp.MapY}";
                i++;
            }
            return mlobjstring;
        }

        public string GenerateMinilandPoint()
        {
            return $"mlpt {MinilandPoint} 100";
        }

        public string GenerateMinimapPosition()
        {
            if (MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance || MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
            {
                return $"rsfp {MapInstance.MapIndexX} {MapInstance.MapIndexY}";
            }
            else
            {
                return $"rsfp 0 -1";
            }
        }

        public string GenerateMlinfo()
        {
            return $"mlinfo 3800 {MinilandPoint} 100 {GeneralLogs.Count(s => s.LogData == "Miniland" && s.Timestamp.Day == DateTime.Now.Day)} {GeneralLogs.Count(s => s.LogData == "Miniland")} 10 {(byte)MinilandState} {Language.Instance.GetMessageFromKey("WELCOME_MUSIC_INFO")} {Language.Instance.GetMessageFromKey("MINILAND_WELCOME_MESSAGE")}";
        }

        public string GenerateMlinfobr()
        {
            return $"mlinfobr 3800 {Name} {GeneralLogs.Count(s => s.LogData == "Miniland" && s.Timestamp.Day == DateTime.Now.Day)} {GeneralLogs.Count(s => s.LogData == "Miniland")} 25 {MinilandMessage.Replace(' ', '^')}";
        }

        public string GenerateMloMg(MinilandObject mlobj, MinigamePacket packet)
        {
            return $"mlo_mg {packet.MinigameVNum} {MinilandPoint} 0 0 {mlobj.ItemInstance.DurabilityPoint} {mlobj.ItemInstance.Item.MinilandObjectPoint}";
        }

        public MovePacket GenerateMv()
        {
            return new MovePacket
            {
                CharacterId = CharacterId,
                MapX = PositionX,
                MapY = PositionY,
                Speed = Speed,
                MoveType = 1
            };
        }

        public string GenerateNpcDialog(int value)
        {
            return $"npc_req 1 {CharacterId} {value}";
        }

        public string GenerateOut()
        {
            return $"out 1 {CharacterId}";
        }

        public string GeneratePairy()
        {
            WearableInstance fairy = null;
            if (Inventory != null)
            {
                fairy = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
            }
            ElementRate = 0;
            Element = 0;
            if (fairy != null)
            {
                ElementRate += fairy.ElementRate + fairy.Item.ElementRate;
                Element = fairy.Item.Element;
            }

            return fairy != null
                ? $"pairy 1 {CharacterId} 4 {fairy.Item.Element} {fairy.ElementRate + fairy.Item.ElementRate} {fairy.Item.Morph}"
                : $"pairy 1 {CharacterId} 0 0 0 0";
        }

        public string GenerateParcel(MailDTO mail)
        {
            return mail.AttachmentVNum != null ? $"parcel 1 1 {MailList.First(s => s.Value.MailId == mail.MailId).Key} {(mail.Title == "NOSMALL" ? 1 : 4)} 0 {mail.Date.ToString("yyMMddHHmm")} {mail.Title} {mail.AttachmentVNum} {mail.AttachmentAmount} {(byte)ServerManager.Instance.GetItem((short)mail.AttachmentVNum).Type}" : string.Empty;
        }

        public string GeneratePidx(bool isLeaveGroup = false)
        {
            if (!isLeaveGroup && Group != null)
            {
                string str = $"pidx {Group.GroupId}";
                string result = str;
                foreach (ClientSession s in Group.Characters)
                {
                    if (s.Character != null)
                    {
                        result = result + $" {(Group.IsMemberOfGroup(CharacterId) ? 1 : 0)}.{s.Character.CharacterId} ";
                    }
                }
                return result;
            }
            return $"pidx -1 1.{CharacterId}";
        }

        public string GeneratePinit()
        {
            Group grp = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(CharacterId) && s.GroupType == GroupType.Group);
            List<Mate> mates = Mates;
            int i = 0;
            string str = string.Empty;
            if (mates != null)
            {
                foreach (Mate mate in mates.Where(s => s.IsTeamMember).OrderByDescending(s => s.MateType))
                {
                    i++;
                    str += $" 2|{mate.MateTransportId}|{(mate.MateType == MateType.Partner ? "0" : "1")}|{mate.Level}|{mate.Name.Replace(' ', '^')}|-1|{mate.Monster.NpcMonsterVNum}|0";
                }
            }
            if (grp != null)
            {
                foreach (ClientSession groupSessionForId in grp.Characters)
                {
                    i++;
                    str += $" 1|{groupSessionForId.Character.CharacterId}|{i}|{groupSessionForId.Character.Level}|{groupSessionForId.Character.Name}|0|{(byte)groupSessionForId.Character.Gender}|{(byte)groupSessionForId.Character.Class}|{(groupSessionForId.Character.UseSp ? groupSessionForId.Character.Morph : 0)}|{groupSessionForId.Character.HeroLevel}";
                }
            }
            return $"pinit {i}{str}";
        }

        public string GeneratePlayerFlag(long pflag)
        {
            return $"pflag 1 {CharacterId} {pflag}";
        }

        public string GeneratePost(MailDTO mail, byte type)
        {
            return $"post 1 {type} {MailList.First(s => s.Value.MailId == mail.MailId).Key} 0 {(mail.IsOpened ? 1 : 0)} {mail.Date.ToString("yyMMddHHmm")} {(type == 2 ? DAOFactory.CharacterDAO.LoadById(mail.ReceiverId).Name : DAOFactory.CharacterDAO.LoadById(mail.SenderId).Name)} {mail.Title}";
        }

        public string GeneratePostMessage(MailDTO mailDTO, byte type)
        {
            CharacterDTO sender = DAOFactory.CharacterDAO.LoadById(mailDTO.SenderId);

            return $"post 5 {type} {MailList.First(s => s.Value == mailDTO).Key} 0 0 {(byte)mailDTO.SenderClass} {(byte)mailDTO.SenderGender} {mailDTO.SenderMorphId} {(byte)mailDTO.SenderHairStyle} {(byte)mailDTO.SenderHairColor} {mailDTO.EqPacket} {sender.Name} {mailDTO.Title} {mailDTO.Message}";
        }

        public List<string> GeneratePst()
        {
            return Mates.Where(s => s.IsTeamMember).OrderByDescending(s => s.MateType).Select(mate => $"pst 2 {mate.MateTransportId} {(mate.MateType == MateType.Partner ? "0" : "1")} {mate.Hp / mate.MaxHp * 100} {mate.Mp / mate.MaxMp * 100} {mate.Hp} {mate.Mp} 0 0 0").ToList();
        }

        public string GeneratePStashAll()
        {
            string stash = $"pstash_all {(StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBackPack) ? 50 : 0)}";
            return Inventory.GetAllItems().Where(s => s.Type == InventoryType.PetWarehouse).Aggregate(stash, (current, item) => current + $" {item.GenerateStashPacket()}");
        }

        public int GeneratePVPDamage(Character target, Skill skill, ref int hitmode)
        {
            #region Definitions

            if (target == null || Inventory == null)
            {
                return 0;
            }

            // int miss_chance = 20;
            int monsterMorale = target.Level + target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0];
            int monsterDefence = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0] - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased, true)[0] + monsterMorale;

            int monsterDodge = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0] + monsterMorale;
            short monsterDefLevel = (short)target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelDecreased, true)[0];

            int morale = Level + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0];
            short mainUpgrade = (short)GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0];
            int mainCritChance = 0;
            int mainCritHit = 0;
            int mainMinDmg = 0;
            int mainMaxDmg = 0;
            int mainHitRate = morale;

            short secUpgrade = mainUpgrade;
            int secCritChance = 0;
            int secCritHit = 0;
            int secMinDmg = 0;
            int secMaxDmg = 0;
            int secHitRate = morale;

            // int CritChance = 4; int CritHit = 70; int MinDmg = 0; int MaxDmg = 0; int HitRate = 0;
            // sbyte Upgrade = 0;

            #endregion

            #region Get Weapon Stats

            WearableInstance weapon = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
            if (weapon != null)
            {
                mainUpgrade += weapon.Upgrade;
            }

            mainMinDmg += MinHit;
            mainMaxDmg += MaxHit;
            mainHitRate += HitRate;
            mainCritChance += HitCriticalRate;
            mainCritHit += HitCritical;

            WearableInstance weapon2 = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
            if (weapon2 != null)
            {
                secUpgrade += weapon2.Upgrade;
            }

            secMinDmg += MinDistance;
            secMaxDmg += MaxDistance;
            secHitRate += DistanceRate;
            secCritChance += DistanceCriticalRate;
            secCritHit += DistanceCritical;

            WearableInstance targetArmor = target.Inventory?.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Armor, InventoryType.Wear);
            if (targetArmor != null)
            {
                monsterDefLevel += targetArmor.Upgrade;
            }

            #endregion

            #region Switch skill.Type

            int boost, boostpercentage;

            switch (skill.Type)
            {
                case 0:
                    boost = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                    boostpercentage = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                    boost -= target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.AllDecreased, true)[0];
                    boostpercentage -= target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelDecreased, true)[0];
                    monsterDefence += target.Defence + boost;
                    monsterDefence = (int)(monsterDefence * (1 + boostpercentage / 100D));
                    boost = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                    boost -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true)[0];
                    boostpercentage = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                    boostpercentage += target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0];
                    monsterDodge += target.DefenceRate + boost;
                    monsterDodge = (int)(monsterDodge * (1 + boostpercentage / 100D));

                    if (Class == ClassType.Archer)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                        + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                    boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                        + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0]
                        - target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageDecreased, true, true)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 1:
                    boost = target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.RangedIncreased, true)[0];
                    boostpercentage = target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelDecreased, true)[0];
                    boost -= target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.RangedDecreased, true)[0];
                    boostpercentage -= target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.RangedIncreased, true)[0];
                    monsterDefence += target.DistanceDefence + boost;
                    monsterDefence = (int)(monsterDefence * (1 + boostpercentage / 100D));
                    boost = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, true)[0];
                    boostpercentage = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, true)[0];
                    boostpercentage += target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0];
                    monsterDodge += target.DistanceDefenceRate + boost;
                    monsterDodge = (int)(monsterDodge * (1 + boostpercentage / 100D));
                    if (Class == ClassType.Swordman || Class == ClassType.Adventurer || Class == ClassType.Magician)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                        + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, true)[0];
                    boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                        + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, true)[0]
                        - target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedDecreased, true, true)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 2:
                    boost = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0];
                    boostpercentage = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0];
                    boost -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased, true)[0];
                    boostpercentage -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased, true)[0];
                    monsterDefence += target.MagicalDefence + boost;
                    monsterDefence = (int)(monsterDefence * (1 + boostpercentage / 100D));

                    boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0];
                    boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                        + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0]
                        - target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased, true, true)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 3:
                    switch (Class)
                    {
                        case ClassType.Swordman:
                            boost = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                            boostpercentage = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                            boost -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true)[0];
                            boostpercentage -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true)[0];
                            monsterDefence += target.Defence + boost;
                            monsterDefence = (int)(monsterDefence * (1 + boostpercentage / 100D));
                            boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                            boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                                + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0]
                                - target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true, true)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;

                        case ClassType.Archer:
                            boost = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, true)[0];
                            boostpercentage = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedDecreased, true)[0];
                            monsterDefence += target.DistanceDefence + boost;
                            monsterDefence = (int)(monsterDefence * (1 + boostpercentage / 100D));
                            boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, true)[0];
                            boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                                + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased, true)[0]
                                - target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedDecreased, true, true)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;

                        case ClassType.Magician:
                            boost = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0];
                            boostpercentage = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0];
                            boost -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased, true)[0];
                            boostpercentage -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased, true)[0];
                            monsterDefence += target.MagicalDefence + boost;
                            monsterDefence = (int)(monsterDefence * (1 + boostpercentage / 100D));
                            boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0];
                            boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                                + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0]
                                - target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased, true, true)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;

                        case ClassType.Adventurer:
                            boost = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                            boostpercentage = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                            boost -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true)[0];
                            boostpercentage -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true)[0];
                            monsterDefence += target.Defence + boost;
                            monsterDefence = (int)(monsterDefence * (1 + boostpercentage / 100D));
                            boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                            boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                                + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0]
                                - target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true, true)[0];
                            mainMinDmg += boost;
                            mainMaxDmg += boost;
                            mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                            mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                            break;
                    }
                    break;

                case 5:
                    boost = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                    boostpercentage = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                    boost -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true)[0];
                    boostpercentage -= target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true)[0];
                    monsterDefence += target.Defence + boost;
                    monsterDefence = (int)(monsterDefence * (1 + boostpercentage / 100D));
                    if (Class == ClassType.Archer)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    if (Class == ClassType.Magician)
                    {
                        boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0];
                        boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                            + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased, true)[0]
                            - target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased, true, true)[0];
                        mainMinDmg += boost;
                        mainMaxDmg += boost;
                        mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                        mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    }
                    else
                    {
                        boost = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
    + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0];
                        boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0]
                            + GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased, true)[0]
                            - target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased, true, true)[0];
                        mainMinDmg += boost;
                        mainMaxDmg += boost;
                        mainMinDmg = (int)(mainMinDmg * (1 + boostpercentage / 100D));
                        mainMaxDmg = (int)(mainMaxDmg * (1 + boostpercentage / 100D));
                    }
                    break;
            }

            #endregion

            #region Basic Damage Data Calculation

            mainCritChance += GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreasingPropability, true)[0];
            mainCritChance -= GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased, true)[0];
            mainCritChance += target.GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreasingPropability, true)[0];
            mainCritChance -= target.GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalIncreased, true)[0];
            mainCritHit += GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalIncreased, true)[0];
            mainCritHit -= GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased, true)[0];
            mainCritHit += target.GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalIncreased, true)[0];
            mainCritHit -= target.GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalDecreased, true)[0];

            mainUpgrade -= monsterDefLevel;
            if (mainUpgrade < -10)
            {
                mainUpgrade = -10;
            }
            else if (mainUpgrade > 10)
            {
                mainUpgrade = 10;
            }

            #endregion

            #region Detailed Calculation

            #region Dodge

            if (Class != ClassType.Magician)
            {
                double multiplier = monsterDodge / (mainHitRate + 1);
                if (multiplier > 5)
                {
                    multiplier = 5;
                }
                double chance = -0.25 * Math.Pow(multiplier, 3) - 0.57 * Math.Pow(multiplier, 2) + 25.3 * multiplier - 1.41;
                if (chance <= 1)
                {
                    chance = 1;
                }
                if (GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.IgnoreEnemyMorale, true)[0] != 0)
                {
                    chance = 10;
                }
                if ((skill.Type == 0 || skill.Type == 1) && !HasGodMode)
                {
                    if (ServerManager.Instance.RandomNumber() <= chance)
                    {
                        hitmode = 1;
                        return 0;
                    }
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.Instance.RandomNumber(mainMinDmg, mainMaxDmg + 1);
            //baseDamage += skill.Damage / 4;  it's a bcard need a skillbcardload
            baseDamage += Level - target.Level; //Morale
            if (Class == ClassType.Adventurer)
            {
                //HACK: Damage is ~10 lower in OpenNos than in official. Fix this...
                baseDamage += 20;
            }
            int elementalDamage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0];
            //  elementalDamage += skill.ElementalDamage / 4;  it's a bcard need a skillbcardload
            switch (mainUpgrade)
            {
                case -10:
                    monsterDefence += monsterDefence * 2;
                    break;

                case -9:
                    monsterDefence += (int)(monsterDefence * 1.2);
                    break;

                case -8:
                    monsterDefence += (int)(monsterDefence * 0.9);
                    break;

                case -7:
                    monsterDefence += (int)(monsterDefence * 0.65);
                    break;

                case -6:
                    monsterDefence += (int)(monsterDefence * 0.54);
                    break;

                case -5:
                    monsterDefence += (int)(monsterDefence * 0.43);
                    break;

                case -4:
                    monsterDefence += (int)(monsterDefence * 0.32);
                    break;

                case -3:
                    monsterDefence += (int)(monsterDefence * 0.22);
                    break;

                case -2:
                    monsterDefence += (int)(monsterDefence * 0.15);
                    break;

                case -1:
                    monsterDefence += (int)(monsterDefence * 0.1);
                    break;

                case 0:
                    break;

                case 1:
                    baseDamage += (int)(baseDamage * 0.1);
                    break;

                case 2:
                    baseDamage += (int)(baseDamage * 0.15);
                    break;

                case 3:
                    baseDamage += (int)(baseDamage * 0.22);
                    break;

                case 4:
                    baseDamage += (int)(baseDamage * 0.32);
                    break;

                case 5:
                    baseDamage += (int)(baseDamage * 0.43);
                    break;

                case 6:
                    baseDamage += (int)(baseDamage * 0.54);
                    break;

                case 7:
                    baseDamage += (int)(baseDamage * 0.65);
                    break;

                case 8:
                    baseDamage += (int)(baseDamage * 0.9);
                    break;

                case 9:
                    baseDamage += (int)(baseDamage * 1.2);
                    break;

                case 10:
                    baseDamage += baseDamage * 2;
                    break;
            }
            if (skill.Type == 1)
            {
                if (Map.GetDistance(new MapCell { X = PositionX, Y = PositionY }, new MapCell { X = target.PositionX, Y = target.PositionY }) < 4)
                    baseDamage = (int)(baseDamage * 0.85);
            }

            #endregion

            #region Elementary Damage

            int bonusrez = target.GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased, true)[0];

            #region Calculate Elemental Boost + Rate

            double elementalBoost = 0;
            int monsterResistance = 0;
            switch (Element)
            {
                case 0:
                    break;

                case 1:
                    bonusrez += target.GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.FireIncreased, true)[0];
                    elementalDamage += GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.FireDecreased, true)[0];
                    monsterResistance = target.FireResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3; // Damage vs no element
                            break;

                        case 1:
                            elementalBoost = 1; // Damage vs fire
                            break;

                        case 2:
                            elementalBoost = 2; // Damage vs water
                            break;

                        case 3:
                            elementalBoost = 1; // Damage vs light
                            break;

                        case 4:
                            elementalBoost = 1.5; // Damage vs darkness
                            break;
                    }
                    break;

                case 2:
                    bonusrez += target.GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.WaterIncreased, true)[0];
                    elementalDamage += GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.WaterIncreased, true)[0];
                    monsterResistance = target.WaterResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 2;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1.5;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }
                    break;

                case 3:
                    bonusrez += target.GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.LightIncreased, true)[0];
                    elementalDamage += GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.LightDecreased, true)[0];
                    monsterResistance = target.LightResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1.5;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1;
                            break;

                        case 4:
                            elementalBoost = 3;
                            break;
                    }
                    break;

                case 4:
                    bonusrez += target.GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.DarkIncreased, true)[0];
                    elementalDamage += GetBuff(CardType.IncreaseDamage, (byte)AdditionalTypes.IncreaseDamage.DarkDecreased, true)[0];
                    monsterResistance = target.DarkResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1;
                            break;

                        case 2:
                            elementalBoost = 1.5;
                            break;

                        case 3:
                            elementalBoost = 3;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }
                    break;
            }

            #endregion;

            if (skill.Element == 0)
            {
                if (elementalBoost == 0.5)
                {
                    elementalBoost = 0;
                }
                else if (elementalBoost == 1)
                {
                    elementalBoost = 0.05;
                }
                else if (elementalBoost == 1.3)
                {
                    elementalBoost = 0.15;
                }
                else if (elementalBoost == 1.5)
                {
                    elementalBoost = 0.15;
                }
                else if (elementalBoost == 2)
                {
                    elementalBoost = 0.2;
                }
                else if (elementalBoost == 3)
                {
                    elementalBoost = 0.2;
                }
            }
            else if (skill.Element != Element)
            {
                elementalBoost = 0;
            }

            elementalDamage = (int)((elementalDamage + (elementalDamage + baseDamage) * ((ElementRate + ElementRateSP) / 100D)) * elementalBoost);
            elementalDamage = elementalDamage / 100 * (100 - monsterResistance - bonusrez);
            if (elementalDamage < 0)
            {
                elementalDamage = 0;
            }

            #endregion

            #region Critical Damage

            baseDamage -= monsterDefence;
            if (GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.ReceivingDecreased, true)[0] == 0
                && target.GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.ReceivingDecreased, true, true)[0] == 0)
            {
                if (ServerManager.Instance.RandomNumber() <= mainCritChance
                    || GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.ReceivingIncreased, true)[0] != 0
                    || target.GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.ReceivingIncreased, true, true)[0] != 0)
                {
                    if (skill.Type == 2)
                    {
                    }
                    else if (skill.Type == 3 && Class != ClassType.Magician)
                    {
                        double multiplier = mainCritHit / 100D;
                        if (multiplier > 3)
                            multiplier = 3;
                        baseDamage += (int)(baseDamage * multiplier);
                        hitmode = 3;
                    }
                    else
                    {
                        double multiplier = mainCritHit / 100D;
                        if (multiplier > 3)
                            multiplier = 3;
                        baseDamage += (int)(baseDamage * multiplier);
                        hitmode = 3;
                    }
                }
            }

            #endregion

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage;
            if (totalDamage < 5)
            {
                totalDamage = ServerManager.Instance.RandomNumber(1, 6);
            }

            #endregion

            #endregion

            return totalDamage;
        }

        public IEnumerable<string> GenerateQuicklist()
        {
            string[] pktQs = { "qslot 0", "qslot 1", "qslot 2" };

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    QuicklistEntryDTO qi = QuicklistEntries.FirstOrDefault(n => n.Q1 == j && n.Q2 == i && n.Morph == (UseSp ? Morph : 0));
                    pktQs[j] += $" {qi?.Type ?? 7}.{qi?.Slot ?? 7}.{qi?.Pos.ToString() ?? "-1"}";
                }
            }

            return pktQs;
        }

        public string GenerateRc(int characterHealth)
        {
            return $"rc 1 {CharacterId} {characterHealth} 0";
        }

        public string GenerateRCSList(CSListPacket packet)
        {
            string list = string.Empty;
            BazaarItemLink[] billist = new BazaarItemLink[ServerManager.Instance.BazaarList.Count + 20];
            ServerManager.Instance.BazaarList.CopyTo(billist);
            foreach (BazaarItemLink bz in billist.Where(s => s != null && s.BazaarItem.SellerId == CharacterId).Skip(packet.Index * 50).Take(50))
            {
                if (bz.Item != null)
                {
                    int soldedAmount = bz.BazaarItem.Amount - bz.Item.Amount;
                    int amount = bz.BazaarItem.Amount;
                    bool package = bz.BazaarItem.IsPackage;
                    bool isNosbazar = bz.BazaarItem.MedalUsed;
                    long price = bz.BazaarItem.Price;
                    long minutesLeft = (long)(bz.BazaarItem.DateStart.AddHours(bz.BazaarItem.Duration) - DateTime.Now).TotalMinutes;
                    byte Status = minutesLeft >= 0 ? (soldedAmount < amount ? (byte)BazaarType.OnSale : (byte)BazaarType.Solded) : (byte)BazaarType.DelayExpired;
                    if (Status == (byte)BazaarType.DelayExpired)
                    {
                        minutesLeft = (long)(bz.BazaarItem.DateStart.AddHours(bz.BazaarItem.Duration).AddDays(isNosbazar ? 30 : 7) - DateTime.Now).TotalMinutes;
                    }
                    string info = string.Empty;
                    if (bz.Item.Item.Type == InventoryType.Equipment)
                    {
                        if ((bz.Item as WearableInstance) != null)
                        {
                            info = (bz.Item as WearableInstance).GenerateEInfo().Replace(' ', '^').Replace("e_info^", "");
                        }
                    }

                    if (packet.Filter == 0 || packet.Filter == Status)
                    {
                        list += $"{bz.BazaarItem.BazaarItemId}|{bz.BazaarItem.SellerId}|{bz.Item.ItemVNum}|{soldedAmount}|{amount}|{(package ? 1 : 0)}|{price}|{Status}|{minutesLeft}|{(isNosbazar ? 1 : 0)}|0|{bz.Item.Rare}|{bz.Item.Upgrade}|{info} ";
                    }
                }
            }

            return $"rc_slist {packet.Index} {list}";
        }

        public string GenerateReqInfo()
        {
            WearableInstance fairy = null;
            WearableInstance armor = null;
            WearableInstance weapon2 = null;
            WearableInstance weapon = null;
            if (Inventory != null)
            {
                fairy = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
                armor = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Armor, InventoryType.Wear);
                weapon2 = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                weapon = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
            }

            bool isPvpPrimary = false;
            bool isPvpSecondary = false;
            bool isPvpArmor = false;

            if (weapon?.Item.Name.Contains(": ") == true)
            {
                isPvpPrimary = true;
            }
            if (weapon2?.Item.Name.Contains(": ") == true)
            {
                isPvpSecondary = true;
            }
            if (armor?.Item.Name.Contains(": ") == true)
            {
                isPvpArmor = true;
            }

            // tc_info 0 name 0 0 0 0 -1 - 0 0 0 0 0 0 0 0 0 0 0 wins deaths reput 0 0 0 morph
            // talentwin talentlose capitul rankingpoints arenapoints 0 0 ispvpprimary ispvpsecondary
            // ispvparmor herolvl desc
            return $"tc_info {Level} {Name} {fairy?.Item.Element ?? 0} {ElementRate} {(byte)Class} {(byte)Gender} {(Family != null ? $"{Family.FamilyId} {Family.Name}({Language.Instance.GetMessageFromKey(FamilyCharacter.Authority.ToString().ToUpper())})" : "-1 -")} {GetReputIco()} {GetDignityIco()} {(weapon != null ? 1 : 0)} {weapon?.Rare ?? 0} {weapon?.Upgrade ?? 0} {(weapon2 != null ? 1 : 0)} {weapon2?.Rare ?? 0} {weapon2?.Upgrade ?? 0} {(armor != null ? 1 : 0)} {armor?.Rare ?? 0} {armor?.Upgrade ?? 0} 0 0 {Reput} {Act4Kill} {Act4Dead} {Act4Points} {(UseSp ? Morph : 0)} {TalentWin} {TalentLose} {TalentSurrender} 0 {MasterPoints} {Compliment} 0 {(isPvpPrimary ? 1 : 0)} {(isPvpSecondary ? 1 : 0)} {(isPvpArmor ? 1 : 0)} {HeroLevel} {(string.IsNullOrEmpty(Biography) ? Language.Instance.GetMessageFromKey("NO_PREZ_MESSAGE") : Biography)}";
        }

        public string GenerateRest()
        {
            return $"rest 1 {CharacterId} {(IsSitting ? 1 : 0)}";
        }

        public string GenerateRaidBf(byte type)
        {
            return $"raidbf 0 {type} 25 ";
        }

        public string GenerateRevive()
        {
            int lives = MapInstance.InstanceBag.Lives - MapInstance.InstanceBag.DeadList.Count() + 1;
            return $"revive 1 {CharacterId} {(lives > 0 ? lives : 0)}";
        }

        public string GenerateSay(string message, int type)
        {
            return $"say 1 {CharacterId} {type} {message}";
        }

        public string GenerateScal()
        {
            return $"char_sc 1 {CharacterId} {Size}";
        }

        public List<string> GenerateScN()
        {
            List<string> list = new List<string>();
            byte i = 0;
            Mates.Where(s => s.MateType == MateType.Partner).ToList().ForEach(s =>
            {
                s.PetId = i;
                list.Add(s.GenerateScPacket());
                i++;
            });
            return list;
        }

        public List<string> GenerateScP(byte Page = 0)
        {
            List<string> list = new List<string>();
            byte i = 0;
            Mates.Where(s => s.MateType == MateType.Pet).Skip(Page * 10).Take(10).ToList().ForEach(s =>
            {
                s.PetId = i;
                list.Add(s.GenerateScPacket());
                i++;
            });
            return list;
        }

        public string GenerateScpStc()
        {
            return $"sc_p_stc {(MaxMateCount / 10)}";
        }

        public string GenerateShop(string shopname)
        {
            return $"shop 1 {CharacterId} 1 3 0 {shopname}";
        }

        public string GenerateShopEnd()
        {
            return $"shop 1 {CharacterId} 0 0";
        }

        public string GenerateSki()
        {
            List<CharacterSkill> characterSkills = UseSp ? SkillsSp.GetAllItems() : Skills.GetAllItems();
            string skibase = string.Empty;
            if (!UseSp)
            {
                skibase = $"{200 + 20 * (byte)Class} {201 + 20 * (byte)Class}";
            }
            else if (characterSkills.Any())
            {
                skibase = $"{characterSkills.ElementAt(0).SkillVNum} {characterSkills.ElementAt(0).SkillVNum}";
            }
            string generatedSkills = string.Empty;
            foreach (CharacterSkill ski in characterSkills)
            {
                generatedSkills += $" {ski.SkillVNum}";
            }

            return $"ski {skibase}{generatedSkills}";
        }

        public string GenerateSpk(object message, int type)
        {
            return $"spk 1 {CharacterId} {type} {Name} {message}";
        }

        public string GenerateSpPoint()
        {
            return $"sp {SpAdditionPoint} 1000000 {SpPoint} 10000";
        }


        [Obsolete("GenerateStartupInventory should be used only on startup, for refreshing an inventory slot please use GenerateInventoryAdd instead.")]
        public void GenerateStartupInventory()
        {
            string inv0 = "inv 0", inv1 = "inv 1", inv2 = "inv 2", inv3 = "inv 3", inv6 = "inv 6", inv7 = "inv 7"; // inv 3 used for miniland objects
            if (Inventory != null)
            {
                foreach (ItemInstance inv in Inventory.GetAllItems())
                {
                    EquipmentBCards.AddRange(inv.Item.BCards);
                    switch (inv.Type)
                    {
                        case InventoryType.Equipment:
                            if (inv.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                if (inv is SpecialistInstance specialistInstance)
                                {
                                    inv0 += $" {inv.Slot}.{inv.ItemVNum}.{specialistInstance.Rare}.{specialistInstance.Upgrade}.{specialistInstance.SpStoneUpgrade}";
                                }
                            }
                            else
                            {
                                if (inv is WearableInstance wearableInstance)
                                {
                                    inv0 += $" {inv.Slot}.{inv.ItemVNum}.{wearableInstance.Rare}.{(inv.Item.IsColored ? wearableInstance.Design : wearableInstance.Upgrade)}.0";
                                }
                            }
                            break;

                        case InventoryType.Main:
                            inv1 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Amount}.0";
                            break;

                        case InventoryType.Etc:
                            inv2 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Amount}.0";
                            break;

                        case InventoryType.Miniland:
                            inv3 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Amount}";
                            break;

                        case InventoryType.Specialist:
                            SpecialistInstance specialist = inv as SpecialistInstance;
                            if (specialist != null)
                            {
                                inv6 += $" {inv.Slot}.{inv.ItemVNum}.{specialist.Rare}.{specialist.Upgrade}.{specialist.SpStoneUpgrade}";
                            }
                            break;

                        case InventoryType.Costume:
                            WearableInstance costumeInstance = inv as WearableInstance;
                            if (costumeInstance != null)
                            {
                                inv7 += $" {inv.Slot}.{inv.ItemVNum}.{costumeInstance.Rare}.{costumeInstance.Upgrade}.0";
                            }
                            break;
                    }
                }
            }
            Session.SendPacket(inv0);
            Session.SendPacket(inv1);
            Session.SendPacket(inv2);
            Session.SendPacket(inv3);
            Session.SendPacket(inv6);
            Session.SendPacket(inv7);
            Session.SendPacket(GetMinilandObjectList());
        }

        public string GenerateStashAll()
        {
            string stash = $"stash_all {WareHouseSize}";
            foreach (ItemInstance item in Inventory.GetAllItems().Where(s => s.Type == InventoryType.Warehouse))
            {
                stash += $" {item.GenerateStashPacket()}";
            }
            return stash;
        }

        public string GenerateStat()
        {
            double option =
                (WhisperBlocked ? Math.Pow(2, (int)CharacterOption.WhisperBlocked - 1) : 0)
                + (FamilyRequestBlocked ? Math.Pow(2, (int)CharacterOption.FamilyRequestBlocked - 1) : 0)
                + (!MouseAimLock ? Math.Pow(2, (int)CharacterOption.MouseAimLock - 1) : 0)
                + (MinilandInviteBlocked ? Math.Pow(2, (int)CharacterOption.MinilandInviteBlocked - 1) : 0)
                + (ExchangeBlocked ? Math.Pow(2, (int)CharacterOption.ExchangeBlocked - 1) : 0)
                + (FriendRequestBlocked ? Math.Pow(2, (int)CharacterOption.FriendRequestBlocked - 1) : 0)
                + (EmoticonsBlocked ? Math.Pow(2, (int)CharacterOption.EmoticonsBlocked - 1) : 0)
                + (HpBlocked ? Math.Pow(2, (int)CharacterOption.HpBlocked - 1) : 0)
                + (BuffBlocked ? Math.Pow(2, (int)CharacterOption.BuffBlocked - 1) : 0)
                + (GroupRequestBlocked ? Math.Pow(2, (int)CharacterOption.GroupRequestBlocked - 1) : 0)
                + (HeroChatBlocked ? Math.Pow(2, (int)CharacterOption.HeroChatBlocked - 1) : 0)
                + (QuickGetUp ? Math.Pow(2, (int)CharacterOption.QuickGetUp - 1) : 0);
            return $"stat {Hp} {HPLoad()} {Mp} {MPLoad()} 0 {option}";
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "Readability")]
        public string GenerateStatChar()
        {
            int type = 0;
            int type2 = 0;
            switch (Class)
            {
                case (byte)ClassType.Adventurer:
                    type = 0;
                    type2 = 1;
                    break;

                case ClassType.Magician:
                    type = 2;
                    type2 = 1;
                    break;

                case ClassType.Swordman:
                    type = 0;
                    type2 = 1;
                    break;

                case ClassType.Archer:
                    type = 1;
                    type2 = 0;
                    break;
            }

            int weaponUpgrade = 0;
            int secondaryUpgrade = 0;
            int armorUpgrade = 0;

            MinHit = CharacterHelper.MinHit(Class, Level);
            MaxHit = CharacterHelper.MaxHit(Class, Level);
            HitRate = CharacterHelper.HitRate(Class, Level);
            HitCriticalRate = CharacterHelper.HitCriticalRate(Class, Level);
            HitCritical = CharacterHelper.HitCritical(Class, Level);
            MinDistance = CharacterHelper.MinDistance(Class, Level);
            MaxDistance = CharacterHelper.MaxDistance(Class, Level);
            DistanceRate = CharacterHelper.DistanceRate(Class, Level);
            DistanceCriticalRate = CharacterHelper.DistCriticalRate(Class, Level);
            DistanceCritical = CharacterHelper.DistCritical(Class, Level);
            FireResistance = CharacterHelper.FireResistance(Class, Level) + GetStuffBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.FireIncreased, false)[0] + GetStuffBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased, false)[0];
            LightResistance = CharacterHelper.LightResistance(Class, Level) + GetStuffBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.LightIncreased, false)[0] + GetStuffBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased, false)[0];
            WaterResistance = CharacterHelper.WaterResistance(Class, Level) + GetStuffBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.WaterIncreased, false)[0] + GetStuffBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased, false)[0];
            DarkResistance = CharacterHelper.DarkResistance(Class, Level) + GetStuffBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.DarkIncreased, false)[0] + GetStuffBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased, false)[0];
            Defence = CharacterHelper.Defence(Class, Level);
            DefenceRate = CharacterHelper.DefenceRate(Class, Level);
            ElementRate = CharacterHelper.ElementRate(Class, Level);
            ElementRateSP = 0;
            DistanceDefence = CharacterHelper.DistanceDefence(Class, Level);
            DistanceDefenceRate = CharacterHelper.DistanceDefenceRate(Class, Level);
            MagicalDefence = CharacterHelper.MagicalDefence(Class, Level);
            if (UseSp)
            {
                // handle specialist
                SpecialistInstance specialist = Inventory?.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                if (specialist != null)
                {
                    MinHit += specialist.DamageMinimum + specialist.SpDamage * 10;
                    MaxHit += specialist.DamageMaximum + specialist.SpDamage * 10;
                    MinDistance += specialist.DamageMinimum;
                    MaxDistance += specialist.DamageMaximum;
                    HitCriticalRate += specialist.CriticalLuckRate;
                    HitCritical += specialist.CriticalRate;
                    DistanceCriticalRate += specialist.CriticalLuckRate;
                    DistanceCritical += specialist.CriticalRate;
                    HitRate += specialist.HitRate;
                    DistanceRate += specialist.HitRate;
                    DefenceRate += specialist.DefenceDodge;
                    DistanceDefenceRate += specialist.DistanceDefenceDodge;
                    FireResistance += specialist.Item.FireResistance + specialist.SpFire;
                    WaterResistance += specialist.Item.WaterResistance + specialist.SpWater;
                    LightResistance += specialist.Item.LightResistance + specialist.SpLight;
                    DarkResistance += specialist.Item.DarkResistance + specialist.SpDark;
                    ElementRateSP += specialist.ElementRate + specialist.SpElement;
                    Defence += specialist.CloseDefence + specialist.SpDefence * 10;
                    DistanceDefence += specialist.DistanceDefence + specialist.SpDefence * 10;
                    MagicalDefence += specialist.MagicDefence + specialist.SpDefence * 10;

                    int point = CharacterHelper.SlPoint(specialist.SlDamage, 0);

                    int p = 0;
                    if (point <= 10)
                        p = point * 5;
                    else if (point <= 20)
                        p = 50 + (point - 10) * 6;
                    else if (point <= 30)
                        p = 110 + (point - 20) * 7;
                    else if (point <= 40)
                        p = 180 + (point - 30) * 8;
                    else if (point <= 50)
                        p = 260 + (point - 40) * 9;
                    else if (point <= 60)
                        p = 350 + (point - 50) * 10;
                    else if (point <= 70)
                        p = 450 + (point - 60) * 11;
                    else if (point <= 80)
                        p = 560 + (point - 70) * 13;
                    else if (point <= 90)
                        p = 690 + (point - 80) * 14;
                    else if (point <= 94)
                        p = 830 + (point - 90) * 15;
                    else if (point <= 95)
                        p = 890 + 16;
                    else if (point <= 97)
                        p = 906 + (point - 95) * 17;
                    else if (point <= 100)
                        p = 940 + (point - 97) * 20;
                    MaxHit += p;
                    MinHit += p;
                    MaxDistance += p;
                    MinDistance += p;

                    point = CharacterHelper.SlPoint(specialist.SlDefence, 1);
                    p = 0;
                    if (point <= 10)
                        p = point;
                    else if (point <= 20)
                        p = 10 + (point - 10) * 2;
                    else if (point <= 30)
                        p = 30 + (point - 20) * 3;
                    else if (point <= 40)
                        p = 60 + (point - 30) * 4;
                    else if (point <= 50)
                        p = 100 + (point - 40) * 5;
                    else if (point <= 60)
                        p = 150 + (point - 50) * 6;
                    else if (point <= 70)
                        p = 210 + (point - 60) * 7;
                    else if (point <= 80)
                        p = 280 + (point - 70) * 8;
                    else if (point <= 90)
                        p = 360 + (point - 80) * 9;
                    else if (point <= 100)
                        p = 450 + (point - 90) * 10;
                    Defence += p;
                    MagicalDefence += p;
                    DistanceDefence += p;

                    point = CharacterHelper.SlPoint(specialist.SlElement, 2);
                    if (point <= 50)
                    {
                        p = point;
                    }
                    else
                    {
                        p = 50 + (point - 50) * 2;
                    }
                    ElementRateSP += p;
                }
            }

            // TODO: add base stats
            WearableInstance weapon = Inventory?.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
            if (weapon != null)
            {
                weaponUpgrade = weapon.Upgrade;
                MinHit += weapon.DamageMinimum + weapon.Item.DamageMinimum;
                MaxHit += weapon.DamageMaximum + weapon.Item.DamageMaximum;
                HitRate += weapon.HitRate + weapon.Item.HitRate;
                HitCriticalRate += weapon.CriticalLuckRate + weapon.Item.CriticalLuckRate;
                HitCritical += weapon.CriticalRate + weapon.Item.CriticalRate;

                // maxhp-mp
            }

            WearableInstance weapon2 = Inventory?.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
            if (weapon2 != null)
            {
                secondaryUpgrade = weapon2.Upgrade;
                MinDistance += weapon2.DamageMinimum + weapon2.Item.DamageMinimum;
                MaxDistance += weapon2.DamageMaximum + weapon2.Item.DamageMaximum;
                DistanceRate += weapon2.HitRate + weapon2.Item.HitRate;
                DistanceCriticalRate += weapon2.CriticalLuckRate + weapon2.Item.CriticalLuckRate;
                DistanceCritical += weapon2.CriticalRate + weapon2.Item.CriticalRate;

                // maxhp-mp
            }

            WearableInstance armor = Inventory?.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Armor, InventoryType.Wear);
            if (armor != null)
            {
                armorUpgrade = armor.Upgrade;
                Defence += armor.CloseDefence + armor.Item.CloseDefence;
                DistanceDefence += armor.DistanceDefence + armor.Item.DistanceDefence;
                MagicalDefence += armor.MagicDefence + armor.Item.MagicDefence;
                DefenceRate += armor.DefenceDodge + armor.Item.DefenceDodge;
                DistanceDefenceRate += armor.DistanceDefenceDodge + armor.Item.DistanceDefenceDodge;
            }

            WearableInstance fairy = Inventory?.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (fairy != null)
            {
                ElementRate += fairy.ElementRate + fairy.Item.ElementRate;
            }

            for (short i = 1; i < 14; i++)
            {
                WearableInstance item = Inventory?.LoadBySlotAndType<WearableInstance>(i, InventoryType.Wear);
                if (item != null)
                {
                    if (item.Item.EquipmentSlot != EquipmentType.MainWeapon
                        && item.Item.EquipmentSlot != EquipmentType.SecondaryWeapon
                        && item.Item.EquipmentSlot != EquipmentType.Armor
                        && item.Item.EquipmentSlot != EquipmentType.Sp)
                    {
                        FireResistance += item.FireResistance + item.Item.FireResistance;
                        LightResistance += item.LightResistance + item.Item.LightResistance;
                        WaterResistance += item.WaterResistance + item.Item.WaterResistance;
                        DarkResistance += item.DarkResistance + item.Item.DarkResistance;
                        Defence += item.CloseDefence + item.Item.CloseDefence;
                        DefenceRate += item.DefenceDodge + item.Item.DefenceDodge;
                        DistanceDefence += item.DistanceDefence + item.Item.DistanceDefence;
                        DistanceDefenceRate += item.DistanceDefenceDodge + item.Item.DistanceDefenceDodge;
                    }
                }
            }
            return $"sc {type} {weaponUpgrade} {MinHit} {MaxHit} {HitRate} {HitCriticalRate} {HitCritical} {type2} {secondaryUpgrade} {MinDistance} {MaxDistance} {DistanceRate} {DistanceCriticalRate} {DistanceCritical} {armorUpgrade} {Defence} {DefenceRate} {DistanceDefence} {DistanceDefenceRate} {MagicalDefence} {FireResistance} {WaterResistance} {LightResistance} {DarkResistance}";
        }

        public string GenerateStatInfo()
        {
            return $"st 1 {CharacterId} {Level} {HeroLevel} {(int)(Hp / (float)HPLoad() * 100)} {(int)(Mp / (float)MPLoad() * 100)} {Hp} {Mp}{(Buff.Aggregate(string.Empty, (current, buff) => current + $" {buff.Card.CardId}"))}";
        }

        public TalkPacket GenerateTalk(string message)
        {
            return new TalkPacket()
            {
                CharacterId = CharacterId,
                Message = message
            };
        }

        public string GenerateTit()
        {
            return $"tit {Language.Instance.GetMessageFromKey(Class == (byte)ClassType.Adventurer ? ClassType.Adventurer.ToString().ToUpper() : Class == ClassType.Swordman ? ClassType.Swordman.ToString().ToUpper() : Class == ClassType.Archer ? ClassType.Archer.ToString().ToUpper() : ClassType.Magician.ToString().ToUpper())} {Name}";
        }

        public string GenerateTp()
        {
            return $"tp 1 {CharacterId} {PositionX} {PositionY} 0";
        }

        public void GetAct4Points(int point)
        {
            //RefreshComplimentRankingIfNeeded();
            Act4Points += point;
        }

        public int GetCP()
        {
            int cpmax = (Class > 0 ? 40 : 0) + JobLevel * 2;
            int cpused = 0;
            foreach (CharacterSkill ski in Skills.GetAllItems())
            {
                cpused += ski.Skill.CPCost;
            }
            return cpmax - cpused;
        }

        public void GetDamage(int damage)
        {
            LastDefence = DateTime.Now;
            Dispose();

            Hp -= damage;
            if (Hp < 0)
            {
                Hp = 0;
            }
        }

        public int GetDignityIco()
        {
            int icoDignity = 1;

            if (Dignity <= -100)
            {
                icoDignity = 2;
            }
            if (Dignity <= -200)
            {
                icoDignity = 3;
            }
            if (Dignity <= -400)
            {
                icoDignity = 4;
            }
            if (Dignity <= -600)
            {
                icoDignity = 5;
            }
            if (Dignity <= -800)
            {
                icoDignity = 6;
            }

            return icoDignity;
        }

        public List<Portal> GetExtraPortal()
        {
            return MapInstancePortalHandler.GenerateMinilandEntryPortals(MapInstance.Map.MapId, Miniland.MapInstanceId);
        }

        public List<string> GetFamilyHistory()
        {
            //TODO: Fix some bugs(missing history etc)
            if (Family != null)
            {
                string packetheader = "ghis";
                List<string> packetList = new List<string>();
                string packet = string.Empty;
                int i = 0;
                int amount = 0;
                foreach (FamilyLogDTO log in Family.FamilyLogs.Where(s => s.FamilyLogType != FamilyLogType.WareHouseAdded && s.FamilyLogType != FamilyLogType.WareHouseRemoved).OrderByDescending(s => s.Timestamp).Take(100))
                {
                    packet += $" {(byte)log.FamilyLogType}|{log.FamilyLogData}|{(int)(DateTime.Now - log.Timestamp).TotalHours}";
                    i++;
                    if (i == 50)
                    {
                        i = 0;
                        packetList.Add($"{packetheader}{(amount == 0 ? " 0 " : "")}{packet}");
                        amount++;
                    }
                    else if (i + 50 * amount == Family.FamilyLogs.Count)
                    {
                        packetList.Add($"{packetheader}{(amount == 0 ? " 0 " : "")}{packet}");
                    }
                }

                return packetList;
            }
            return new List<string>();
        }

        public IEnumerable<string> GetMinilandEffects()
        {
            return MinilandObjects.Select(mp => mp.GenerateMinilandEffect(false)).ToList();
        }

        public string GetMinilandObjectList()
        {
            string mlobjstring = "mlobjlst";
            foreach (ItemInstance item in Inventory.GetAllItems().Where(s => s.Type == InventoryType.Miniland).OrderBy(s => s.Slot))
            {
                if (item.Item.IsMinilandObject)
                {
                    WareHouseSize = item.Item.MinilandObjectPoint;
                }
                MinilandObject mp = MinilandObjects.FirstOrDefault(s => s.ItemInstanceId == item.Id);
                bool used = mp != null;
                mlobjstring += $" {item.Slot}.{(used ? 1 : 0)}.{(used ? mp.MapX : 0)}.{(used ? mp.MapY : 0)}.{(item.Item.Width != 0 ? item.Item.Width : 1) }.{(item.Item.Height != 0 ? item.Item.Height : 1) }.{(used ? mp.ItemInstance.DurabilityPoint : 0)}.100.0.1";
            }

            return mlobjstring;
        }

        public void GetReput(int val)
        {
            Reput += val;
            Session.SendPacket(GenerateFd());
            Session.SendPacket(GenerateSay(string.Format(Language.Instance.GetMessageFromKey("REPUT_INCREASE"), val), 11));
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "Readability")]
        public int GetReputIco()
        {
            if (Reput >= 5000001)
            {
                switch (IsReputHero())
                {
                    case 1:
                        return 28;

                    case 2:
                        return 29;

                    case 3:
                        return 30;

                    case 4:
                        return 31;

                    case 5:
                        return 32;
                }
            }
            if (Reput <= 50) return 1;
            if (Reput <= 150) return 2;
            if (Reput <= 250) return 3;
            if (Reput <= 500) return 4;
            if (Reput <= 750) return 5;
            if (Reput <= 1000) return 6;
            if (Reput <= 2250) return 7;
            if (Reput <= 3500) return 8;
            if (Reput <= 5000) return 9;
            if (Reput <= 9500) return 10;
            if (Reput <= 19000) return 11;
            if (Reput <= 25000) return 12;
            if (Reput <= 40000) return 13;
            if (Reput <= 60000) return 14;
            if (Reput <= 85000) return 15;
            if (Reput <= 115000) return 16;
            if (Reput <= 150000) return 17;
            if (Reput <= 190000) return 18;
            if (Reput <= 235000) return 19;
            if (Reput <= 285000) return 20;
            if (Reput <= 350000) return 21;
            if (Reput <= 500000) return 22;
            if (Reput <= 1500000) return 23;
            if (Reput <= 2500000) return 24;
            if (Reput <= 3750000) return 25;
            return Reput <= 5000000 ? 26 : 27;
        }

        public void GiftAdd(short itemVNum, byte amount, byte rare = 0, short design = 0)
        {
            //TODO add the rare support
            if (Inventory != null)
            {
                lock (Inventory)
                {
                    ItemInstance newItem = Inventory.InstantiateItemInstance(itemVNum, CharacterId, amount);
                    if (newItem != null)
                    {
                        newItem.Design = design;
                        if (newItem.Item.ItemType == ItemType.Armor || newItem.Item.ItemType == ItemType.Weapon || newItem.Item.ItemType == ItemType.Shell)
                        {
                            ((WearableInstance)newItem).RarifyItem(Session, RarifyMode.Drop, RarifyProtection.None);
                            newItem.Upgrade = newItem.Item.BasicUpgrade;
                        }
                        List<ItemInstance> newInv = Inventory.AddToInventory(newItem);
                        if (newInv.Any())
                        {
                            Session.SendPacket(GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {newItem.Item.Name} x {amount}", 10));
                        }
                        else
                        {
                            if (MailList.Count <= 40)
                            {
                                SendGift(CharacterId, itemVNum, amount, newItem.Rare, newItem.Upgrade, false);
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_ACQUIRED_BY_THE_GIANT_MONSTER"), 0));
                            }
                        }
                    }
                }
            }
        }

        public bool HaveBackpack()
        {
            return StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.BackPack);
        }

        public double HPLoad()
        {
            double multiplicator = 1.0;
            int hp = 0;
            if (UseSp)
            {
                SpecialistInstance specialist =
                    Inventory?.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                if (specialist != null)
                {
                    int point = CharacterHelper.SlPoint(specialist.SlHP, 3);

                    if (point <= 50)
                    {
                        multiplicator += point / 100.0;
                    }
                    else
                    {
                        multiplicator += 0.5 + (point - 50.00) / 50.00;
                    }
                    hp = specialist.HP + specialist.SpHP * 100;
                }
            }
            return (int)((CharacterHelper.HPData[(byte)Class, Level] + hp + GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPIncreased, false)[0]) * multiplicator);
        }

        public override void Initialize()
        {
            _random = new Random();
            ExchangeInfo = null;
            SpCooldown = 30;
            SaveX = 0;
            SaveY = 0;
            LastDefence = DateTime.Now.AddSeconds(-21);
            LastDelay = DateTime.Now.AddSeconds(-5);
            LastHealth = DateTime.Now;
            LastEffect = DateTime.Now;
            Session = null;
            MailList = new Dictionary<int, MailDTO>();
            Group = null;
            GmPvtBlock = false;
        }

        public void InsertOrUpdatePenalty(PenaltyLogDTO log)
        {
            DAOFactory.PenaltyLogDAO.InsertOrUpdate(ref log);
            CommunicationServiceClient.Instance.RefreshPenalty(log.PenaltyLogId);
        }

        public bool IsBlockedByCharacter(long characterId)
        {
            return CharacterRelations.Any(b => b.RelationType == CharacterRelationType.Blocked && b.CharacterId.Equals(characterId) && characterId != CharacterId);
        }

        public bool IsBlockingCharacter(long characterId)
        {
            return CharacterRelations.Any(c => c.RelationType == CharacterRelationType.Blocked && c.RelatedCharacterId.Equals(characterId));
        }

        public bool IsFriendlistFull()
        {
            return CharacterRelations.Where(s => s.RelationType == CharacterRelationType.Friend).ToList().Count >= 80;
        }

        public bool IsFriendOfCharacter(long characterId)
        {
            return CharacterRelations.Any(c => c.RelationType == CharacterRelationType.Friend && (c.RelatedCharacterId.Equals(characterId) || c.CharacterId.Equals(characterId)));
        }

        /// <summary>
        /// Checks if the current character is in range of the given position
        /// </summary>
        /// <param name="xCoordinate">The x coordinate of the object to check.</param>
        /// <param name="yCoordinate">The y coordinate of the object to check.</param>
        /// <param name="range">The range of the coordinates to be maximal distanced.</param>
        /// <returns>True if the object is in Range, False if not.</returns>
        public bool IsInRange(int xCoordinate, int yCoordinate, int range = 50)
        {
            return Math.Abs(PositionX - xCoordinate) <= range && Math.Abs(PositionY - yCoordinate) <= range;
        }

        public bool IsMuted()
        {
            return Session.Account.PenaltyLogs.Any(s => s.Penalty == PenaltyType.Muted && s.DateEnd > DateTime.Now);
        }

        public int IsReputHero()
        {
            int i = 0;
            foreach (CharacterDTO characterDto in ServerManager.Instance.TopReputation)
            {
                Character character = (Character)characterDto;
                i++;
                if (character.CharacterId == CharacterId)
                {
                    if (i == 1)
                    {
                        return 5;
                    }
                    if (i == 2)
                    {
                        return 4;
                    }
                    if (i == 3)
                    {
                        return 3;
                    }
                    if (i <= 13)
                    {
                        return 2;
                    }
                    if (i <= 43)
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }

        public void LearnAdventurerSkill()
        {
            if (Class == 0)
            {
                byte NewSkill = 0;
                for (int i = 200; i <= 210; i++)
                {
                    if (i == 209)
                    {
                        i++;
                    }

                    Skill skinfo = ServerManager.Instance.GetSkill((short)i);
                    if (skinfo.Class == 0 && JobLevel >= skinfo.LevelMinimum)
                    {
                        if (Skills.GetAllItems().All(s => s.SkillVNum != i))
                        {
                            NewSkill = 1;
                            Skills[i] = new CharacterSkill { SkillVNum = (short)i, CharacterId = CharacterId };
                        }
                    }
                }
                if (NewSkill > 0)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                    Session.SendPacket(GenerateSki());
                    Session.SendPackets(GenerateQuicklist());
                }
            }
        }

        public void LearnSPSkill()
        {
            SpecialistInstance specialist = null;
            if (Inventory != null)
            {
                specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            byte SkillSpCount = (byte)SkillsSp.Count;
            SkillsSp = new ThreadSafeSortedList<int, CharacterSkill>();
            foreach (Skill ski in ServerManager.Instance.GetAllSkill())
            {
                if (specialist != null && ski.Class == Morph + 31 && specialist.SpLevel >= ski.LevelMinimum)
                {
                    SkillsSp[ski.SkillVNum] = new CharacterSkill { SkillVNum = ski.SkillVNum, CharacterId = CharacterId };
                }
            }
            if (SkillsSp.Count != SkillSpCount)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
            }
        }

        public void LoadInventory()
        {
            IEnumerable<ItemInstanceDTO> inventories = DAOFactory.IteminstanceDAO.LoadByCharacterId(CharacterId).Where(s => s.Type != InventoryType.FamilyWareHouse).ToList();
            IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(Session.Account.AccountId);
            foreach (CharacterDTO character in characters.Where(s => s.CharacterId != CharacterId))
            {
                inventories = inventories.Concat(DAOFactory.IteminstanceDAO.LoadByCharacterId(character.CharacterId).Where(s => s.Type == InventoryType.Warehouse).ToList());
            }
            Inventory = new Inventory(this);
            foreach (ItemInstanceDTO inventory in inventories)
            {
                inventory.CharacterId = CharacterId;
                Inventory[inventory.Id] = (ItemInstance)inventory;
            }
        }

        public void LoadQuicklists()
        {
            QuicklistEntries = new List<QuicklistEntryDTO>();
            IEnumerable<QuicklistEntryDTO> quicklistDTO = DAOFactory.QuicklistEntryDAO.LoadByCharacterId(CharacterId).ToList();
            foreach (QuicklistEntryDTO qle in quicklistDTO)
            {
                QuicklistEntries.Add(qle);
            }
        }

        public void LoadSentMail()
        {
            foreach (MailDTO mail in ServerManager.Instance.Mails.Where(s => s.SenderId == CharacterId && s.IsSenderCopy && MailList.All(m => m.Value.MailId != s.MailId)))
            {
                MailList.Add((MailList.Any() ? MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mail);

                Session.SendPacket(GeneratePost(mail, 2));
            }
        }

        public void LoadSkills()
        {
            Skills = new ThreadSafeSortedList<int, CharacterSkill>();
            IEnumerable<CharacterSkillDTO> characterskillDTO = DAOFactory.CharacterSkillDAO.LoadByCharacterId(CharacterId).ToList();
            foreach (CharacterSkillDTO characterskill in characterskillDTO.OrderBy(s => s.SkillVNum))
            {
                if (!Skills.ContainsKey(characterskill.SkillVNum))
                {
                    Skills[characterskill.SkillVNum] = characterskill as CharacterSkill;
                }
            }
        }

        public void LoadSpeed()
        {
            // only load speed if you dont use custom speed
            if (!IsVehicled && !IsCustomSpeed)
            {
                Speed = CharacterHelper.SpeedData[(byte)Class];

                if (UseSp)
                {
                    SpecialistInstance specialist = Inventory?.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                    if (specialist != null)
                    {
                        Speed += specialist.Item.Speed;
                    }
                }
            }

            if (IsShopping)
            {
                Speed = 0;
                IsCustomSpeed = false;
                return;
            }

            // reload vehicle speed after opening an shop for instance
            if (IsVehicled)
            {
                Speed = VehicleSpeed;
            }
        }

        public double MPLoad()
        {
            int mp = 0;
            double multiplicator = 1.0;
            if (UseSp)
            {
                SpecialistInstance specialist = Inventory?.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                if (specialist != null)
                {
                    int point = CharacterHelper.SlPoint(specialist.SlHP, 3);

                    if (point <= 50)
                    {
                        multiplicator += point / 100.0;
                    }
                    else
                    {
                        multiplicator += 0.5 + (point - 50.00) / 50.00;
                    }
                    mp = specialist.MP + specialist.SpHP * 100;
                }
            }
            return (int)((CharacterHelper.MPData[(byte)Class, Level] + mp + GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.IncreasesMaximumMP, false)[0]) * multiplicator);
        }

        public void NotifyRarifyResult(sbyte rare)
        {
            Session.SendPacket(GenerateSay(string.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), rare), 12));
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), rare), 0));
            MapInstance.Broadcast(GenerateEff(3005), PositionX, PositionY);
            Session.SendPacket("shop_end 1");
        }

        public string OpenFamilyWarehouse()
        {
            if (Family == null || Family.WarehouseSize == 0)
            {
                return UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_WAREHOUSE"));
            }
            return GenerateFStashAll();
        }

        public List<string> OpenFamilyWarehouseHist()
        {
            List<string> packetList = new List<string>();
            if (Family == null ||
            !
         (FamilyCharacter.Authority == FamilyAuthority.Head
         || FamilyCharacter.Authority == FamilyAuthority.Assistant
         || FamilyCharacter.Authority == FamilyAuthority.Member && Family.MemberCanGetHistory
         || FamilyCharacter.Authority == FamilyAuthority.Manager && Family.ManagerCanGetHistory
         )
        )
            {
                packetList.Add(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return packetList;
            }
            return GenerateFamilyWarehouseHist();
        }

        public void RefreshMail()
        {
            int i = 0;
            int j = 0;
            try
            {
                List<MailDTO> mails = ServerManager.Instance.Mails.Where(s => s.ReceiverId == CharacterId && !s.IsSenderCopy && MailList.All(m => m.Value.MailId != s.MailId)).Take(50).ToList();
                for (int x = 0; x < mails.Count; x++)
                {
                    MailList.Add((MailList.Any() ? MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mails.ElementAt(x));
                    if (mails.ElementAt(x).AttachmentVNum != null)
                    {
                        i++;
                        Session.SendPacket(GenerateParcel(mails.ElementAt(x)));
                    }
                    else
                    {
                        if (!mails.ElementAt(x).IsOpened)
                        {
                            j++;
                        }
                        Session.SendPacket(GeneratePost(mails.ElementAt(x), 1));
                    }
                }
                if (i > 0)
                {
                    Session.SendPacket(GenerateSay(string.Format(Language.Instance.GetMessageFromKey("GIFTED"), i), 11));
                }
                if (j > 0)
                {
                    Session.SendPacket(GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NEW_MAIL"), j), 10));
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Debug("Error while refreshing mail: " + ex.Message);
            }
        }

        public void RemoveVehicle()
        {
            SpecialistInstance sp = null;
            if (Inventory != null)
            {
                sp = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            IsVehicled = false;
            LoadSpeed();
            if (UseSp)
            {
                if (sp != null)
                {
                    Morph = sp.Item.Morph;
                    MorphUpgrade = sp.Upgrade;
                    MorphUpgrade2 = sp.Design;
                }
            }
            else
            {
                Morph = 0;
            }
            Session.CurrentMapInstance?.Broadcast(GenerateCMode());
            Session.SendPacket(GenerateCond());
            LastSpeedChange = DateTime.Now;
        }

        public void Rest()
        {
            if (LastSkillUse.AddSeconds(4) > DateTime.Now || LastDefence.AddSeconds(4) > DateTime.Now)
            {
                return;
            }
            if (!IsVehicled)
            {
                IsSitting = !IsSitting;
                Session.CurrentMapInstance?.Broadcast(GenerateRest());
            }
            else
            {
                Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("IMPOSSIBLE_TO_USE"), 10));
            }
        }

        public void Save()
        {
            try
            {
                AccountDTO account = Session.Account;
                DAOFactory.AccountDAO.InsertOrUpdate(ref account);

                CharacterDTO character = DeepCopy();
                DAOFactory.CharacterDAO.InsertOrUpdate(ref character);

                if (Inventory != null)
                {
                    // be sure that noone tries to edit while saving is currently editing
                    lock (Inventory)
                    {
                        // load and concat inventory with equipment
                        List<ItemInstance> inventories = Inventory.GetAllItems();
                        IEnumerable<Guid> currentlySavedInventoryIds = DAOFactory.IteminstanceDAO.LoadSlotAndTypeByCharacterId(CharacterId);
                        IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(Session.Account.AccountId);
                        foreach (CharacterDTO characteraccount in characters.Where(s => s.CharacterId != CharacterId))
                        {
                            currentlySavedInventoryIds = currentlySavedInventoryIds.Concat(DAOFactory.IteminstanceDAO.LoadByCharacterId(characteraccount.CharacterId).Where(s => s.Type == InventoryType.Warehouse).Select(i => i.Id).ToList());
                        }

                        IEnumerable<MinilandObjectDTO> currentlySavedMinilandObjectEntries = DAOFactory.MinilandObjectDAO.LoadByCharacterId(CharacterId).ToList();
                        foreach (MinilandObjectDTO mobjToDelete in currentlySavedMinilandObjectEntries.Except(MinilandObjects))
                        {
                            DAOFactory.MinilandObjectDAO.DeleteById(mobjToDelete.MinilandObjectId);
                        }

                        // remove all which are saved but not in our current enumerable
                        foreach (var inventoryToDeleteId in currentlySavedInventoryIds.Except(inventories.Select(i => i.Id)))
                        {
                            try
                            {
                                DAOFactory.IteminstanceDAO.Delete(inventoryToDeleteId);
                            }
                            catch (Exception err)
                            {
                                Logger.Error(err);
                                Logger.Debug(Name, $"Detailed Item Information: Item ID = {inventoryToDeleteId}");
                            }
                        }

                        // create or update all which are new or do still exist
                        foreach (ItemInstance itemInstance in inventories.Where(s => s.Type != InventoryType.Bazaar && s.Type != InventoryType.FamilyWareHouse))
                        {
                            DAOFactory.IteminstanceDAO.InsertOrUpdate(itemInstance);
                        }
                    }
                }

                if (Skills != null)
                {
                    IEnumerable<Guid> currentlySavedCharacterSkills = DAOFactory.CharacterSkillDAO.LoadKeysByCharacterId(CharacterId).ToList();

                    foreach (Guid characterSkillToDeleteId in currentlySavedCharacterSkills.Except(Skills.GetAllItems().Select(s => s.Id)))
                    {
                        DAOFactory.CharacterSkillDAO.Delete(characterSkillToDeleteId);
                    }

                    foreach (CharacterSkill characterSkill in Skills.GetAllItems())
                    {
                        DAOFactory.CharacterSkillDAO.InsertOrUpdate(characterSkill);
                    }
                }

                IEnumerable<long> currentlySavedMates = DAOFactory.MateDAO.LoadByCharacterId(CharacterId).Select(s => s.MateId);

                foreach (long matesToDeleteId in currentlySavedMates.Except(Mates.Select(s => s.MateId)))
                {
                    DAOFactory.MateDAO.Delete(matesToDeleteId);
                }

                foreach (Mate mate in Mates)
                {
                    MateDTO matesave = mate;
                    DAOFactory.MateDAO.InsertOrUpdate(ref matesave);
                }

                IEnumerable<QuicklistEntryDTO> quickListEntriesToInsertOrUpdate = QuicklistEntries.ToList();

                IEnumerable<Guid> currentlySavedQuicklistEntries = DAOFactory.QuicklistEntryDAO.LoadKeysByCharacterId(CharacterId).ToList();
                foreach (Guid quicklistEntryToDelete in currentlySavedQuicklistEntries.Except(QuicklistEntries.Select(s => s.Id)))
                {
                    DAOFactory.QuicklistEntryDAO.Delete(quicklistEntryToDelete);
                }
                foreach (QuicklistEntryDTO quicklistEntry in quickListEntriesToInsertOrUpdate)
                {
                    DAOFactory.QuicklistEntryDAO.InsertOrUpdate(quicklistEntry);
                }

                IEnumerable<MinilandObjectDTO> minilandobjectEntriesToInsertOrUpdate = MinilandObjects.ToList();

                foreach (MinilandObjectDTO mobjEntry in minilandobjectEntriesToInsertOrUpdate)
                {
                    MinilandObjectDTO mobj = mobjEntry;
                    DAOFactory.MinilandObjectDAO.InsertOrUpdate(ref mobj);
                }

                IEnumerable<short> currentlySavedBonus = DAOFactory.StaticBonusDAO.LoadTypeByCharacterId(CharacterId);
                foreach (short bonusToDelete in currentlySavedBonus.Except(Buff.Select(s => s.Card.CardId)))
                {
                    DAOFactory.StaticBonusDAO.Delete(bonusToDelete, CharacterId);
                }
                foreach (StaticBonusDTO bonus in StaticBonusList.ToArray())
                {
                    StaticBonusDTO bonus2 = bonus;
                    DAOFactory.StaticBonusDAO.InsertOrUpdate(ref bonus2);
                }

                IEnumerable<short> currentlySavedBuff = DAOFactory.StaticBuffDAO.LoadByTypeCharacterId(CharacterId);
                foreach (short bonusToDelete in currentlySavedBuff.Except(Buff.Select(s => s.Card.CardId)))
                {
                    DAOFactory.StaticBuffDAO.Delete(bonusToDelete, CharacterId);
                }

                foreach (Buff buff in Buff.Where(s => s.StaticBuff).ToArray())
                {
                    StaticBuffDTO bf = new StaticBuffDTO()
                    {
                        CharacterId = CharacterId,
                        RemainingTime = (int)(buff.RemainingTime - (DateTime.Now - buff.Start).TotalSeconds),
                        CardId = buff.Card.CardId
                    };
                    DAOFactory.StaticBuffDAO.InsertOrUpdate(ref bf);
                }

                foreach (StaticBonusDTO bonus in StaticBonusList.ToArray())
                {
                    StaticBonusDTO bonus2 = bonus;
                    DAOFactory.StaticBonusDAO.InsertOrUpdate(ref bonus2);
                }

                foreach (GeneralLogDTO general in GeneralLogs)
                {
                    if (!DAOFactory.GeneralLogDAO.IdAlreadySet(general.LogId))
                    {
                        DAOFactory.GeneralLogDAO.Insert(general);
                    }
                }
                foreach (RespawnDTO Resp in Respawns)
                {
                    RespawnDTO res = Resp;
                    if (Resp.MapId != 0 && Resp.X != 0 && Resp.Y != 0)
                    {
                        DAOFactory.RespawnDAO.InsertOrUpdate(ref res);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Save Character failed. SessionId: " + Session.SessionId, e);
            }
        }

        public void SendGift(long id, short vnum, byte amount, sbyte rare, byte upgrade, bool isNosmall)
        {
            Item it = ServerManager.Instance.GetItem(vnum);

            if (it != null)
            {
                if (it.ItemType != ItemType.Weapon && it.ItemType != ItemType.Armor && it.ItemType != ItemType.Specialist)
                {
                    upgrade = 0;
                }
                else if (it.ItemType != ItemType.Weapon && it.ItemType != ItemType.Armor)
                {
                    rare = 0;
                }
                if (rare > 8 || rare < -2)
                {
                    rare = 0;
                }
                if (upgrade > 10 && it.ItemType != ItemType.Specialist)
                {
                    upgrade = 0;
                }
                else if (it.ItemType == ItemType.Specialist && upgrade > 15)
                {
                    upgrade = 0;
                }

                // maximum size of the amount is 99
                if (amount > 99)
                {
                    amount = 99;
                }

                MailDTO mail = new MailDTO
                {
                    AttachmentAmount = it.Type == InventoryType.Etc || it.Type == InventoryType.Main ? amount : (byte)1,
                    IsOpened = false,
                    Date = DateTime.Now,
                    ReceiverId = id,
                    SenderId = CharacterId,
                    AttachmentRarity = (byte)rare,
                    AttachmentUpgrade = upgrade,
                    IsSenderCopy = false,
                    Title = isNosmall ? "NOSMALL" : Name,
                    AttachmentVNum = vnum,
                    SenderClass = Class,
                    SenderGender = Gender,
                    SenderHairColor = HairColor,
                    SenderHairStyle = HairStyle,
                    EqPacket = GenerateEqListForPacket(),
                    SenderMorphId = Morph == 0 ? (short)-1 : (short)(Morph > short.MaxValue ? 0 : Morph)
                };
                DAOFactory.MailDAO.InsertOrUpdate(ref mail);
                if (id == CharacterId)
                {
                    MailList.Add((MailList.Any() ? MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mail);
                    Session.SendPacket(GenerateParcel(mail));
                    Session.SendPacket(GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_GIFTED")} {mail.AttachmentAmount}", 12));
                }
            }
        }

        public void SetRespawnPoint(short mapId, short mapX, short mapY)
        {
            if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.Map.MapTypes.Any())
            {
                long? respawnmaptype = Session.CurrentMapInstance.Map.MapTypes.ElementAt(0).RespawnMapTypeId;
                if (respawnmaptype != null)
                {
                    RespawnDTO resp = Respawns.FirstOrDefault(s => s.RespawnMapTypeId == respawnmaptype);
                    if (resp == null)
                    {
                        resp = new RespawnDTO { CharacterId = CharacterId, MapId = mapId, X = mapX, Y = mapY, RespawnMapTypeId = (long)respawnmaptype };
                        Respawns.Add(resp);
                    }
                    else
                    {
                        resp.X = mapX;
                        resp.Y = mapY;
                        resp.MapId = mapId;
                    }
                }
            }
        }

        public void SetReturnPoint(short mapId, short mapX, short mapY)
        {
            if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.Map.MapTypes.Any())
            {
                long? respawnmaptype = Session.CurrentMapInstance.Map.MapTypes.ElementAt(0).ReturnMapTypeId;
                if (respawnmaptype != null)
                {
                    RespawnDTO resp = Respawns.FirstOrDefault(s => s.RespawnMapTypeId == respawnmaptype);
                    if (resp == null)
                    {
                        resp = new RespawnDTO { CharacterId = CharacterId, MapId = mapId, X = mapX, Y = mapY, RespawnMapTypeId = (long)respawnmaptype };
                        Respawns.Add(resp);
                    }
                    else
                    {
                        resp.X = mapX;
                        resp.Y = mapY;
                        resp.MapId = mapId;
                    }
                }
            }
        }

        public bool WeaponLoaded(CharacterSkill ski)
        {
            if (ski != null)
            {
                switch (Class)
                {
                    default:
                        return false;

                    case ClassType.Adventurer:
                        if (ski.Skill.Type == 1)
                        {
                            if (Inventory != null)
                            {
                                WearableInstance wearable = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                                if (wearable != null)
                                {
                                    if (wearable.Ammo > 0)
                                    {
                                        wearable.Ammo--;
                                        return true;
                                    }
                                    if (Inventory.CountItem(2081) < 1)
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_ADVENTURER"), 10));
                                        return false;
                                    }
                                    Inventory.RemoveItemAmount(2081);
                                    wearable.Ammo = 100;
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_ADVENTURER"), 10));
                                    return true;
                                }
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                                return false;
                            }
                            return true;
                        }
                        return true;

                    case ClassType.Swordman:
                        if (ski.Skill.Type == 1)
                        {
                            if (Inventory != null)
                            {
                                WearableInstance inv = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                                if (inv != null)
                                {
                                    if (inv.Ammo > 0)
                                    {
                                        inv.Ammo--;
                                        return true;
                                    }
                                    if (Inventory.CountItem(2082) < 1)
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_SWORDSMAN"), 10));
                                        return false;
                                    }

                                    Inventory.RemoveItemAmount(2082);
                                    inv.Ammo = 100;
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_SWORDSMAN"), 10));
                                    return true;
                                }
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                                return false;
                            }
                            return true;
                        }
                        return true;

                    case ClassType.Archer:
                        if (ski.Skill.Type == 1)
                        {
                            if (Inventory != null)
                            {
                                WearableInstance inv = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                                if (inv != null)
                                {
                                    if (inv.Ammo > 0)
                                    {
                                        inv.Ammo--;
                                        return true;
                                    }
                                    if (Inventory.CountItem(2083) < 1)
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_ARCHER"), 10));
                                        return false;
                                    }

                                    Inventory.RemoveItemAmount(2083);
                                    inv.Ammo = 100;
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_ARCHER"), 10));
                                    return true;
                                }
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                                return false;
                            }
                            return true;
                        }
                        return true;

                    case ClassType.Magician:
                        return true;
                }
            }

            return false;
        }

        internal void RefreshValidity()
        {
            if (StaticBonusList.RemoveAll(s => s.StaticBonusType == StaticBonusType.BackPack && s.DateEnd < DateTime.Now) > 0)
            {
                Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                Session.SendPacket(GenerateExts());
            }

            if (StaticBonusList.RemoveAll(s => s.DateEnd < DateTime.Now) > 0)
            {
                Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
            }

            if (Inventory != null)
            {
                foreach (var suit in Enum.GetValues(typeof(EquipmentType)))
                {
                    WearableInstance item = Inventory.LoadBySlotAndType<WearableInstance>((byte)suit, InventoryType.Wear);
                    if (item != null && item.DurabilityPoint > 0)
                    {
                        item.DurabilityPoint--;
                        if (item.DurabilityPoint == 0)
                        {
                            Inventory.DeleteById(item.Id);
                            Session.SendPacket(GenerateStatChar());
                            Session.CurrentMapInstance?.Broadcast(GenerateEq());
                            Session.SendPacket(GenerateEquipment());
                            Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                        }
                    }
                }
            }
        }

        internal void SetSession(ClientSession clientSession)
        {
            Session = clientSession;
        }

        private void GenerateXp(MapMonster monster, bool isMonsterOwner)
        {
            NpcMonster monsterinfo = monster.Monster;
            if (!Session.Account.PenaltyLogs.Any(s => s.Penalty == PenaltyType.BlockExp && s.DateEnd > DateTime.Now))
            {
                Group grp = ServerManager.Instance.Groups.FirstOrDefault(g => g.IsMemberOfGroup(CharacterId));
                SpecialistInstance specialist = null;
                if (Hp <= 0)
                {
                    return;
                }
                if ((int)(LevelXp / (XPLoad() / 10)) < (int)((LevelXp + monsterinfo.XP) / (XPLoad() / 10)))
                {
                    Hp = (int)HPLoad();
                    Mp = (int)MPLoad();
                    Session.SendPacket(GenerateStat());
                    Session.SendPacket(GenerateEff(5));
                }

                if (Inventory != null)
                {
                    specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                }

                if (Level < ServerManager.Instance.MaxLevel)
                {
                    if (isMonsterOwner)
                    {
                        LevelXp += (int)(GetXP(monsterinfo, grp) * (1 + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased, false)[0] / 100D));
                    }
                    else
                    {
                        LevelXp += (int)(GetXP(monsterinfo, grp) / 3D * (1 + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased, false)[0] / 100D));
                    }
                }
                if (Class == 0 && JobLevel < 20 || Class != 0 && JobLevel < ServerManager.Instance.MaxJobLevel)
                {
                    if (specialist != null && UseSp && specialist.SpLevel < ServerManager.Instance.MaxSPLevel && specialist.SpLevel > 19)
                    {
                        JobLevelXp += (int)(GetJXP(monsterinfo, grp) / 2D * (1 + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased, false)[0] / 100D));
                    }
                    else
                    {
                        JobLevelXp += (int)(GetJXP(monsterinfo, grp) * (1 + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased, false)[0] / 100D));
                    }
                }
                if (specialist != null && UseSp && specialist.SpLevel < ServerManager.Instance.MaxSPLevel)
                {
                    int multiplier = specialist.SpLevel < 10 ? 10 : specialist.SpLevel < 19 ? 5 : 1;
                    specialist.XP += (int)(GetJXP(monsterinfo, grp) * (multiplier + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased, false)[0] / 100D));
                }
                if (HeroLevel > 0 && HeroLevel < ServerManager.Instance.MaxHeroLevel)
                {
                    HeroXp += (int)
                        (GetHXP(monsterinfo, grp) *
                         (1 + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased, false)[0] / 100D));
                }
                double t = XPLoad();
                while (LevelXp >= t)
                {
                    LevelXp -= (long)t;
                    Level++;
                    t = XPLoad();
                    if (Level >= ServerManager.Instance.MaxLevel)
                    {
                        Level = ServerManager.Instance.MaxLevel;
                        LevelXp = 0;
                    }
                    else if (Level == ServerManager.Instance.HeroicStartLevel)
                    {
                        HeroLevel = 1;
                        HeroXp = 0;
                    }
                    Hp = (int)HPLoad();
                    Mp = (int)MPLoad();
                    Session.SendPacket(GenerateStat());
                    if (Family != null)
                    {
                        if (Level > 20 && Level % 10 == 0)
                        {
                            Family.InsertFamilyLog(FamilyLogType.LevelUp, Name, level: Level);
                            Family.InsertFamilyLog(FamilyLogType.FamilyXP, Name, experience: 20 * Level);
                            GenerateFamilyXp(20 * Level);
                        }
                        else if (Level > 80)
                        {
                            Family.InsertFamilyLog(FamilyLogType.LevelUp, Name, level: Level);
                        }
                        else
                        {
                            ServerManager.Instance.FamilyRefresh(Family.FamilyId);
                            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                            {
                                DestinationCharacterId = Family.FamilyId,
                                SourceCharacterId = CharacterId,
                                SourceWorldId = ServerManager.Instance.WorldId,
                                Message = "fhis_stc",
                                Type = MessageType.Family
                            });
                        }
                    }
                    Session.SendPacket(GenerateLevelUp());
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("LEVELUP"), 0));
                    Session.CurrentMapInstance?.Broadcast(GenerateEff(6), PositionX, PositionY);
                    Session.CurrentMapInstance?.Broadcast(GenerateEff(198), PositionX, PositionY);
                    ServerManager.Instance.UpdateGroup(CharacterId);
                }

                WearableInstance fairy = Inventory?.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
                if (fairy != null)
                {
                    if (fairy.ElementRate + fairy.Item.ElementRate < fairy.Item.MaxElementRate &&
                        Level <= monsterinfo.Level + 15 && Level >= monsterinfo.Level - 15)
                    {
                        fairy.XP += ServerManager.Instance.FairyXpRate;
                    }
                    t = CharacterHelper.LoadFairyXPData(fairy.ElementRate + fairy.Item.ElementRate);
                    while (fairy.XP >= t)
                    {
                        fairy.XP -= (int)t;
                        fairy.ElementRate++;
                        if (fairy.ElementRate + fairy.Item.ElementRate == fairy.Item.MaxElementRate)
                        {
                            fairy.XP = 0;
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAIRYMAX"), fairy.Item.Name), 10));
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVELUP"), fairy.Item.Name), 10));
                        }
                        Session.SendPacket(GeneratePairy());
                    }
                }

                t = JobXPLoad();
                while (JobLevelXp >= t)
                {
                    JobLevelXp -= (long)t;
                    JobLevel++;
                    t = JobXPLoad();
                    if (JobLevel >= 20 && Class == 0)
                    {
                        JobLevel = 20;
                        JobLevelXp = 0;
                    }
                    else if (JobLevel >= ServerManager.Instance.MaxJobLevel)
                    {
                        JobLevel = ServerManager.Instance.MaxJobLevel;
                        JobLevelXp = 0;
                    }
                    Hp = (int)HPLoad();
                    Mp = (int)MPLoad();
                    Session.SendPacket(GenerateStat());
                    Session.SendPacket(GenerateLevelUp());
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("JOB_LEVELUP"), 0));
                    LearnAdventurerSkill();
                    Session.CurrentMapInstance?.Broadcast(GenerateEff(8), PositionX, PositionY);
                    Session.CurrentMapInstance?.Broadcast(GenerateEff(198), PositionX, PositionY);
                }
                if (specialist != null)
                {
                    t = SPXPLoad();

                    while (UseSp && specialist.XP >= t)
                    {
                        specialist.XP -= (long)t;
                        specialist.SpLevel++;
                        t = SPXPLoad();
                        Session.SendPacket(GenerateStat());
                        Session.SendPacket(GenerateLevelUp());
                        if (specialist.SpLevel >= ServerManager.Instance.MaxSPLevel)
                        {
                            specialist.SpLevel = ServerManager.Instance.MaxSPLevel;
                            specialist.XP = 0;
                        }
                        LearnSPSkill();
                        Skills.GetAllItems().ForEach(s => s.LastUse = DateTime.Now.AddDays(-1));
                        Session.SendPacket(GenerateSki());
                        Session.SendPackets(GenerateQuicklist());

                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SP_LEVELUP"), 0));
                        Session.CurrentMapInstance?.Broadcast(GenerateEff(8), PositionX, PositionY);
                        Session.CurrentMapInstance?.Broadcast(GenerateEff(198), PositionX, PositionY);
                    }
                }
                t = HeroXPLoad();
                while (HeroXp >= t)
                {
                    HeroXp -= (long)t;
                    HeroLevel++;
                    t = HeroXPLoad();
                    if (HeroLevel >= ServerManager.Instance.MaxHeroLevel)
                    {
                        HeroLevel = ServerManager.Instance.MaxHeroLevel;
                        HeroXp = 0;
                    }
                    Hp = (int)HPLoad();
                    Mp = (int)MPLoad();
                    Session.SendPacket(GenerateStat());
                    Session.SendPacket(GenerateLevelUp());
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("HERO_LEVELUP"), 0));
                    Session.CurrentMapInstance?.Broadcast(GenerateEff(8), PositionX, PositionY);
                    Session.CurrentMapInstance?.Broadcast(GenerateEff(198), PositionX, PositionY);
                }
                Session.SendPacket(GenerateLev());
            }
        }

        private int GetGold(MapMonster mapMonster)
        {
            if (MapId == 2006 || MapId == 150)
                return 0;
            int lowBaseGold = ServerManager.Instance.RandomNumber(6 * mapMonster.Monster?.Level ?? 1, 12 * mapMonster.Monster?.Level ?? 1);
            int actMultiplier = Session?.CurrentMapInstance?.Map.MapTypes?.Any(s => s.MapTypeId == (short)MapTypeEnum.Act52) ?? false ? 10 : 1;
            if (Session?.CurrentMapInstance?.Map.MapTypes?.Any(s => s.MapTypeId == (short)MapTypeEnum.Act61 || s.MapTypeId == (short)MapTypeEnum.Act61a || s.MapTypeId == (short)MapTypeEnum.Act61d) == true)
            {
                actMultiplier = 5;
            }
            int gold = lowBaseGold * ServerManager.Instance.GoldRate * actMultiplier;
            return gold;
        }

        private int GetHXP(NpcMonsterDTO monster, Group group)
        {
            int partySize = 1;
            float partyPenalty = 1f;

            if (group != null)
            {
                int levelSum = group.Characters.Sum(g => g.Character.HeroLevel);
                partySize = group.CharacterCount;
                partyPenalty = 12f / partySize / levelSum;
            }

            int heroXp = (int)Math.Round(monster.HeroXp * CharacterHelper.ExperiencePenalty(Level, monster.Level) * ServerManager.Instance.HeroXpRate * MapInstance.XpRate);

            // divide jobexp by multiplication of partyPenalty with level e.g. 57 * 0,014...
            if (partySize > 1 && group != null)
            {
                heroXp = (int)Math.Round(HeroLevel / (HeroLevel * partyPenalty));
            }

            return heroXp;
        }

        private int GetJXP(NpcMonsterDTO monster, Group group)
        {
            int partySize = 1;
            float partyPenalty = 1f;

            if (group != null)
            {
                int levelSum = group.Characters.Sum(g => g.Character.JobLevel);
                partySize = group.CharacterCount;
                partyPenalty = 12f / partySize / levelSum;
            }

            int jobxp = (int)Math.Round(monster.JobXP * CharacterHelper.ExperiencePenalty(JobLevel, monster.Level) * ServerManager.Instance.XPRate * MapInstance.XpRate);

            // divide jobexp by multiplication of partyPenalty with level e.g. 57 * 0,014...
            if (partySize > 1 && group != null)
            {
                jobxp = (int)Math.Round(jobxp / (JobLevel * partyPenalty));
            }

            return jobxp;
        }

        private long GetXP(NpcMonsterDTO monster, Group group)
        {
            int partySize = 1;
            double partyPenalty = 1d;
            int levelDifference = Level - monster.Level;

            if (group != null)
            {
                int levelSum = group.Characters.Sum(g => g.Character.Level);
                partySize = group.CharacterCount;
                partyPenalty = 12f / partySize / levelSum;
            }

            long xpcalculation = levelDifference < 5 ? monster.XP : monster.XP / 3 * 2;

            long xp = (long)Math.Round(xpcalculation * CharacterHelper.ExperiencePenalty(Level, monster.Level) * ServerManager.Instance.XPRate * MapInstance.XpRate);

            // bonus percentage calculation for level 1 - 5 and difference of levels bigger or equal
            // to 4
            if (levelDifference < -20)
            {
                xp /= 10;
            }
            if (Level <= 5 && levelDifference < -4)
            {
                xp += xp / 2;
            }
            if (monster.Level >= 75)
            {
                xp *= 2;
            }
            if (monster.Level >= 100)
            {
                xp *= 2;
                if (Level < 96)
                {
                    xp = 1;
                }
            }

            if (partySize > 1 && group != null)
            {
                xp = (long)Math.Round(xp / (Level * partyPenalty));
            }

            return xp;
        }

        private int HealthHPLoad()
        {
            if (IsSitting)
            {
                return CharacterHelper.HPHealth[(byte)Class];
            }
            return (DateTime.Now - LastDefence).TotalSeconds > 4 ? CharacterHelper.HPHealthStand[(byte)Class] : 0;
        }

        private int HealthMPLoad()
        {
            if (IsSitting)
            {
                return CharacterHelper.MPHealth[(byte)Class];
            }
            return (DateTime.Now - LastDefence).TotalSeconds > 4 ? CharacterHelper.MPHealthStand[(byte)Class] : 0;
        }

        private double HeroXPLoad()
        {
            return HeroLevel == 0 ? 1 : CharacterHelper.HeroXpData[HeroLevel - 1];
        }

        private double JobXPLoad()
        {
            return Class == (byte)ClassType.Adventurer ? CharacterHelper.FirstJobXPData[JobLevel - 1] : CharacterHelper.SecondJobXPData[JobLevel - 1];
        }

        private double SPXPLoad()
        {
            SpecialistInstance specialist = null;
            if (Inventory != null)
            {
                specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            return specialist != null ? CharacterHelper.SPXPData[specialist.SpLevel - 1] : 0;
        }

        private double XPLoad()
        {
            return CharacterHelper.XPData[Level - 1];
        }

        public string GenerateRaid(int Type, bool Exit)
        {
            string result = string.Empty;
            switch (Type)
            {
                case 0:
                    result = $"raid 0";
                    Group?.Characters?.ForEach(s => { result += $" {s.Character?.CharacterId}"; });
                    break;
                case 2:
                    result = $"raid 2 {(Exit ? "-1" : $"{CharacterId}")}";
                    break;
                case 1:
                    result = $"raid 1 {(Exit ? 0 : 1)}";
                    break;
                case 3:
                    result = $"raid 3";
                    Group?.Characters?.ForEach(s => { result += $" {s.Character?.CharacterId}.{Math.Ceiling(s.Character.Hp / s.Character.HPLoad() * 100)}.{Math.Ceiling(s.Character.Mp / s.Character.MPLoad() * 100)}"; });
                    break;
                case 4:
                    result = $"raid 4";
                    break;
                case 5:
                    result = $"raid 5 1";
                    break;
            }
            return result;
        }

        public void AddStaticBuff(StaticBuffDTO staticBuff)
        {
            Buff bf = new Buff(staticBuff.CardId, Session.Character.Level)
            {
                Start = DateTime.Now,
                StaticBuff = true
            };
            Buff oldbuff = Buff.FirstOrDefault(s => s.Card.CardId == staticBuff.CardId);
            if (staticBuff.RemainingTime > 0)
            {
                bf.RemainingTime = staticBuff.RemainingTime;
                Buff.Add(bf);
            }
            else if (oldbuff != null)
            {
                Buff.RemoveAll(s => s.Card.CardId.Equals(bf.Card.CardId));

                bf.RemainingTime = bf.Card.Duration * 6 / 10 + oldbuff.RemainingTime;
                Buff.Add(bf);
            }
            else
            {
                bf.RemainingTime = bf.Card.Duration * 6 / 10;
                Buff.Add(bf);
            }
            bf.Card.BCards.ForEach(c => c.ApplyBCards(Session.Character));
            Observable.Timer(TimeSpan.FromSeconds(bf.RemainingTime))
                .Subscribe(
                    o =>
                    {
                        RemoveBuff(bf.Card.CardId);
                        if (bf.Card.TimeoutBuff != 0 && ServerManager.Instance.RandomNumber() <
                            bf.Card.TimeoutBuffChance)
                        {
                            AddBuff(new Buff(bf.Card.TimeoutBuff, Level));
                        }
                    });

            Session.SendPacket($"vb {bf.Card.CardId} 1 {bf.RemainingTime * 10}");
            Session.SendPacket(
                Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("UNDER_EFFECT"), Name),
                    12));
        }

        public void AddBuff(Buff indicator)
        {
            Buff.RemoveAll(s => s.Card.CardId.Equals(indicator.Card.CardId));
            Buff.Add(indicator);
            indicator.RemainingTime = indicator.Card.Duration;
            indicator.Start = DateTime.Now;

            Session.SendPacket(
                $"bf 1 {Session.Character.CharacterId} 0.{indicator.Card.CardId}.{indicator.RemainingTime} {Level}");
            Session.SendPacket(
                Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("UNDER_EFFECT"), Name),
                    20));

            indicator.Card.BCards.ForEach(c => c.ApplyBCards(Session.Character));
            Observable.Timer(TimeSpan.FromMilliseconds(indicator.Card.Duration * 100))
                .Subscribe(
                    o =>
                    {
                        RemoveBuff(indicator.Card.CardId);
                        if (indicator.Card.TimeoutBuff != 0 && ServerManager.Instance.RandomNumber() <
                            indicator.Card.TimeoutBuffChance)
                        {
                            AddBuff(new Buff(indicator.Card.TimeoutBuff, Level));
                        }
                    });
        }

        private void RemoveBuff(int id)
        {
            Buff indicator = Buff.FirstOrDefault(s => s.Card.CardId == id);
            if (indicator != null)
            {
                if (indicator.StaticBuff)
                {
                    Session.SendPacket($"vb {indicator.Card.CardId} 0 {indicator.Card.Duration}");
                    Session.SendPacket(
                        Session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("EFFECT_TERMINATED"), Name), 11));
                }
                else
                {
                    Session.SendPacket($"bf 1 {Session.Character.CharacterId} 0.{indicator.Card.CardId}.0 {Level}");
                    Session.SendPacket(
                        Session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("EFFECT_TERMINATED"), Name), 20));
                }
                Buff.Remove(indicator);
                if (indicator.Card.BCards.Any(s => s.Type == (byte)BCardType.CardType.Move))
                {
                    LastSpeedChange = DateTime.Now;
                    Session.SendPacket(GenerateCond());
                }
            }
        }

        public void DisableBuffs(List<BuffType> types, int level = 100)
        {
            lock (Buff)
            {
                Buff.Where(s => types.Contains(s.Card.BuffType) && !s.StaticBuff && s.Card.Level < level).ToList()
                    .ForEach(s => RemoveBuff(s.Card.CardId));
            }
        }

        /// <summary>
        /// Get Stuff Buffs
        /// Useful for Stats for example
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subtype"></param>
        /// <param name="pvp"></param>
        /// <param name="affectingOpposite"></param>
        /// <returns></returns>
        public int[] GetStuffBuff(CardType type, byte subtype, bool pvp, bool affectingOpposite = false)
        {
            int value1 = 0;
            int value2 = 0;
            foreach (BCard entry in EquipmentBCards.Where(
                s => s.Type.Equals((byte)type) && s.SubType.Equals((byte)(subtype / 10))))
            {
                value1 += entry.FirstData;
                value2 += entry.SecondData;
            }

            return new[] { value1, value2 };
        }

        public int[] GetBuff(CardType type, byte subtype, bool pvp, bool affectingOpposite = false)
        {
            int value1 = 0;
            int value2 = 0;

            lock (Buff)
            {
                foreach (Buff buff in Buff)
                {
                    // THIS ONE DOES NOT FOR STUFFS
                    foreach (BCard entry in buff.Card.BCards.Concat(EquipmentBCards).Where(
                        s => s.Type.Equals((byte)type)
                             && s.SubType.Equals((byte)(subtype / 10)) &&
                             (s.CastType != 1 || (s.CastType == 1 &&
                                               buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now))))
                    {
                        value1 += entry.FirstData;
                        value2 += entry.SecondData;
                    }
                }
            }

            return new[] { value1, value2 };
        }

        #endregion

    }
}