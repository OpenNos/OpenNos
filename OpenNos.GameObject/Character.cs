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

using AutoMapper;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using OpenNos.GameObject.Packets.ServerPackets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Character : CharacterDTO
    {
        #region Members

        private readonly ClientSession _session;
        private AuthorityType _authority;
        private int _backpack;
        private byte _cmapcount = 0;
        private int _direction;
        private Inventory _inventory;
        private bool _invisible;
        private bool _isDancing;
        private bool _issitting;
        private double _lastPortal;
        private int _lastPulse;
        private int _morph;
        private int _morphUpgrade;
        private int _morphUpgrade2;
        private Random _random;
        private int _size = 10;
        private byte _speed;

        #endregion

        #region Instantiation

        public Character(ClientSession Session)
        {
            _random = new Random();
            ExchangeInfo = null;
            SpCooldown = 30;
            SaveX = 0;
            SaveY = 0;
            LastDefence = DateTime.Now.AddSeconds(-21);
            LastHealth = DateTime.Now;
            LastEffect = DateTime.Now;
            MailList = new Dictionary<int, MailDTO>();
            LastMailRefresh = DateTime.Now;
            _session = Session;
            Group = null;
            GmPvtBlock = false;
        }

        #endregion

        #region Properties

        public AuthorityType Authority
        {
            get
            {
                return _authority;
            }

            set
            {
                _authority = value;
            }
        }

        public int BackPack
        {
            get
            {
                return _backpack;
            }

            set
            {
                _backpack = value;
            }
        }

        public bool CanFight
        {
            get
            {
                return !IsSitting && ExchangeInfo == null;
            }
        }

        public int DarkResistance { get; set; }

        public int Defence { get; set; }

        public int DefenceRate { get; set; }

        public int Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value;
            }
        }

        public int DistanceCritical { get; set; }

        public int DistanceCriticalRate { get; set; }

        public int DistanceDefence { get; set; }

        public int DistanceDefenceRate { get; set; }

        public int DistanceRate { get; set; }

        public byte Element { get; set; }

        public int ElementRate { get; set; }

        public int ElementRateSP { get; private set; }

        public ExchangeInfo ExchangeInfo { get; set; }

        public int FireResistance { get; set; }

        public bool GmPvtBlock { get; set; }

        public Group Group { get; set; }

        public bool HasGodMode { get; set; }

        public bool HasShopOpened { get; set; }

        public int HitCritical { get; set; }

        public int HitCriticalRate { get; set; }

        public int HitRate { get; set; }

        public bool InExchangeOrTrade
        {
            get
            {
                return (ExchangeInfo != null || Speed == 0);
            }
        }

        public Inventory Inventory
        {
            get
            {
                return _inventory;
            }

            set
            {
                _inventory = value;
            }
        }

        public bool Invisible
        {
            get
            {
                return _invisible;
            }

            set
            {
                _invisible = value;
            }
        }

        public bool InvisibleGm { get; set; }

        public bool IsChangingMap { get; set; }

        public bool IsCustomSpeed { get; set; }

        public bool IsDancing
        {
            get
            {
                return _isDancing;
            }

            set
            {
                _isDancing = value;
            }
        }

        public bool IsShopping { get; set; }

        public bool IsSitting
        {
            get
            {
                return _issitting;
            }

            set
            {
                _issitting = value;
            }
        }

        public bool IsVehicled { get; set; }

        public DateTime LastDefence { get; set; }

        public DateTime LastEffect { get; set; }

        public DateTime LastHealth { get; set; }

        public DateTime LastLogin { get; set; }

        public DateTime LastMailRefresh { get; set; }

        public DateTime LastMapObject { get; set; }

        public int LastMonsterId { get; set; }

        public DateTime LastMove { get; set; }

        public short LastNRunId { get; set; }

        public double LastPortal
        {
            get
            {
                return _lastPortal;
            }

            set
            {
                _lastPortal = value;
            }
        }

        public DateTime LastPotion { get; set; }

        public int LastPulse
        {
            get
            {
                return _lastPulse;
            }

            set
            {
                _lastPulse = value;
            }
        }

        public DateTime LastSkill { get; set; }

        public double LastSp { get; set; }

        public DateTime LastTransform { get; set; }

        public int LightResistance { get; set; }

        public int MagicalDefence { get; set; }

        public IDictionary<int, MailDTO> MailList { get; set; }

        public int MaxDistance { get; set; }

        public int MaxHit { get; set; }

        public int MaxSnack { get; set; }

        public int MinDistance { get; set; }

        public int MinHit { get; set; }

        public int Morph
        {
            get
            {
                return _morph;
            }

            set
            {
                _morph = value;
            }
        }

        public int MorphUpgrade
        {
            get
            {
                return _morphUpgrade;
            }

            set
            {
                _morphUpgrade = value;
            }
        }

        public int MorphUpgrade2
        {
            get
            {
                return _morphUpgrade2;
            }

            set
            {
                _morphUpgrade2 = value;
            }
        }

        public List<QuicklistEntryDTO> QuicklistEntries { get; set; }

        public short SaveX { get; set; }

        public short SaveY { get; set; }

        public ClientSession Session
        {
            get
            {
                return _session;
            }
        }

        public int Size
        {
            get
            {
                return _size;
            }

            set
            {
                _size = value;
            }
        }

        public ThreadSafeSortedList<int, CharacterSkill> Skills { get; set; }

        public ThreadSafeSortedList<int, CharacterSkill> SkillsSp { get; set; }

        public int SnackAmount { get; set; }

        public int SnackHp { get; set; }

        public int SnackMp { get; set; }

        public int SpCooldown { get; set; }

        public byte Speed
        {
            get
            {
                return _speed;
            }

            set
            {
                if (value > 59)
                {
                    _speed = 59;
                }
                else
                {
                    _speed = value;
                }
            }
        }

        public bool UseSp { get; set; }

        public int WaterResistance { get; set; }

        #endregion

        #region Methods

        public void ChangeClass(byte characterClass)
        {
            if (characterClass < 4)
            {
                JobLevel = 1;
                JobLevelXp = 0;
                Session.SendPacket("npinfo 0");
                Session.SendPacket("p_clear");

                if (characterClass == (byte)ClassType.Adventurer)
                {
                    HairStyle = HairStyle > 1 ? (byte)0 : HairStyle;
                }

                LoadSpeed();
                Class = characterClass;
                Hp = (int)HPLoad();
                Mp = (int)MPLoad();
                Session.SendPacket(GenerateTit());

                // Session.SendPacket(GenerateEquipment());
                Session.SendPacket(GenerateStat());
                Session.CurrentMap?.Broadcast(Session, GenerateEq(), ReceiverType.All);
                Session.CurrentMap?.Broadcast(GenerateEff(8), MapX, MapY);
                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("CLASS_CHANGED"), 0));
                Session.CurrentMap?.Broadcast(GenerateEff(196), MapX, MapY);

                int faction = 1 + (int)_random.Next(0, 2);
                Faction = faction;
                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{faction}"), 0));

                Session.SendPacket("scr 0 0 0 0 0 0");
                Session.SendPacket(GenerateFaction());
                Session.SendPacket(GenerateStatChar());
                Session.SendPacket(GenerateEff(4799 + faction));
                Session.SendPacket(GenerateCond());
                Session.SendPacket(GenerateLev());
                Session.CurrentMap?.Broadcast(Session, GenerateCMode(), ReceiverType.All);
                Session.CurrentMap?.Broadcast(Session, GenerateIn(), ReceiverType.AllExceptMe);
                Session.CurrentMap?.Broadcast(GenerateEff(6), MapX, MapY);
                Session.CurrentMap?.Broadcast(GenerateEff(198), MapX, MapY);

                foreach (var skill in Skills.GetAllItems())
                {
                    if (skill.SkillVNum >= 200)
                    {
                        Skills.Remove(skill.SkillVNum);
                    }
                }

                Skills[(short)(200 + 20 * Class)] = new CharacterSkill { SkillVNum = (short)(200 + 20 * Class), CharacterId = CharacterId };
                Skills[(short)(201 + 20 * Class)] = new CharacterSkill { SkillVNum = (short)(201 + 20 * Class), CharacterId = CharacterId };
                Skills[236] = new CharacterSkill { SkillVNum = 236, CharacterId = CharacterId };

                Session.SendPacket(GenerateSki());

                // TODO: Reset Quicklist (just add Rest-on-T Item)
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
                if (ServerManager.Instance.Groups.Any(s => s.IsMemberOfGroup(Session)))
                {
                    Session.CurrentMap?.Broadcast(Session, $"pidx 1 1.{CharacterId}", ReceiverType.AllExceptMe);
                }
            }
        }

        public void ChangeSex()
        {
            Gender = Gender == 1 ? (byte)0 : (byte)1;
            if (IsVehicled)
            {
                Morph = Gender == 1 ? Morph + 1 : Morph - 1;
            }
            Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("SEX_CHANGED"), 0));
            Session.SendPacket(GenerateEq());
            Session.SendPacket(GenerateGender());
            Session.CurrentMap?.Broadcast(Session, GenerateIn(), ReceiverType.AllExceptMe);
            Session.CurrentMap?.Broadcast(GenerateCMode());
            Session.CurrentMap?.Broadcast(GenerateEff(196), MapX, MapY);
        }

        public void CloseShop()
        {
            if (HasShopOpened)
            {
                KeyValuePair<long, MapShop> shop = this.Session.CurrentMap.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(this.CharacterId));
                if (!shop.Equals(default(KeyValuePair<long, MapShop>)))
                {
                    Session.CurrentMap.UserShops.Remove(shop.Key);
                    Session.CurrentMap?.Broadcast(GenerateShopEnd());
                    Session.CurrentMap?.Broadcast(Session, GeneratePlayerFlag(0), ReceiverType.AllExceptMe);
                    IsSitting = false;
                    LoadSpeed();
                    Session.SendPacket(GenerateCond());
                    Session.CurrentMap?.Broadcast(GenerateRest());
                    Session.SendPacket("shop_end 0");
                }
                HasShopOpened = false;
            }
        }

        public void CloseTrade()
        {
            if (InExchangeOrTrade)
            {
                ClientSession targetSession = Session.CurrentMap.GetSessionByCharacterId(Session.Character.ExchangeInfo.TargetCharacterId);

                if (targetSession == null)
                {
                    return;
                }

                Session.SendPacket("exc_close 0");
                targetSession.SendPacket("exc_close 0");
                Session.Character.ExchangeInfo = null;
                targetSession.Character.ExchangeInfo = null;
            }
        }

        public void Dance()
        {
            IsDancing = IsDancing ? false : true;
        }

        public Character DeepCopy()
        {
            Character clonedCharacter = (Character)this.MemberwiseClone();
            return clonedCharacter;
        }

        public void DeleteItem(InventoryType type, short slot)
        {
            Inventory.DeleteFromSlotAndType(slot, type);
            Session.SendPacket(GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0, 0));
        }

        public void DeleteItemByItemInstanceId(Guid id)
        {
            Tuple<short, InventoryType> result = Inventory.DeleteById(id);
            Session.SendPacket(GenerateInventoryAdd(-1, 0, result.Item2, result.Item1, 0, 0, 0, 0));
        }

        public void DeleteTimeout()
        {
            for (int i = Inventory.Count() - 1; i >= 0; i--)
            {
                ItemInstance item = Inventory[i];
                if (item != null)
                {
                    if (((ItemInstance)item).IsBound && item.ItemDeleteTime != null && item.ItemDeleteTime < DateTime.Now)
                    {
                        Inventory.DeleteById(item.Id);
                        Session.SendPacket(GenerateInventoryAdd(-1, 0, item.Type, item.Slot, 0, 0, 0, 0));
                        Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                    }
                }
            }
            for (int i = Inventory.Count() - 1; i >= 0; i--)
            {
                ItemInstance item = Inventory[i];
                if (item != null)
                {
                    if (((ItemInstance)item).IsBound && item.ItemDeleteTime != null && item.ItemDeleteTime < DateTime.Now)
                    {
                        Inventory.DeleteById(item.Id);
                        Session.SendPacket(GenerateEquipment());
                        Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                    }
                }
            }
        }

        public string GenerateAt()
        {
            Map mapForMusic = ServerManager.GetMap(MapId);
            return $"at {CharacterId} {MapId} {MapX} {MapY} 2 0 {(mapForMusic != null ? mapForMusic.Music : 0)} -1";
        }

        public string GenerateCInfo()
        {
            return $"c_info {Name} - -1 -1 - {CharacterId} {(Invisible ? 6 : (byte)Authority)} {Gender} {HairStyle} {HairColor} {Class} {((GetDignityIco() == 1) ? GetReputIco() : -GetDignityIco())} {Compliment} {(UseSp || IsVehicled ? Morph : 0)} {(Invisible ? 1 : 0)} 0 {(UseSp ? MorphUpgrade : 0)} {ArenaWinner}";
        }

        public string GenerateCMap()
        {
            _cmapcount = _cmapcount == 1 ? (byte)0 : (byte)1;
            return $"c_map 0 {MapId} {_cmapcount}";
        }

        public string GenerateCMode()
        {
            return $"c_mode 1 {CharacterId} {(UseSp || IsVehicled ? Morph : 0)} {(UseSp ? MorphUpgrade : 0)} {(UseSp ? MorphUpgrade2 : 0)} {ArenaWinner}";
        }

        public string GenerateCond()
        {
            return $"cond 1 {CharacterId} 0 0 {Speed}";
        }

        public string GenerateDelay(int delay, int type, string argument)
        {
            return $"delay {delay} {type} {argument}";
        }

        public string GenerateDialog(string dialog)
        {
            return $"dlg {dialog}";
        }

        public void GenerateDignity(NpcMonster monsterinfo)
        {
            if (Level < monsterinfo.Level && Dignity < 100 && Level > 20)
            {
                Dignity += (float)0.5;
                if (Dignity == (int)Dignity)
                {
                    Session.SendPacket(GenerateFd());
                    Session.CurrentMap?.Broadcast(Session, GenerateIn(), ReceiverType.AllExceptMe);
                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("RESTORE_DIGNITY"), 11));
                }
            }
        }

        public string GenerateDir()
        {
            return $"dir 1 {CharacterId} {Direction}";
        }

        public List<string> GenerateDroppedItem()
        {
            return ServerManager.GetMap(MapId).DroppedList.Select(item => $"in 9 {item.Value.ItemVNum} {item.Key} {item.Value.PositionX} {item.Value.PositionY} {(item.Value is MonsterMapItem && ((MonsterMapItem)item.Value).GoldAmount > 1 ? ((MonsterMapItem)item.Value).GoldAmount : item.Value.Amount)} 0 0 -1").ToList();
        }

        public EffectPacket GenerateEff(int effectid, byte effecttype = 1)
        {
            return new EffectPacket()
            {
                EffectType = effecttype,
                CharacterId = CharacterId,
                Id = effectid
            };
        }

        public string GenerateEInfo(WearableInstance item)
        {
            Item iteminfo = item.Item;
            EquipmentType equipmentslot = iteminfo.EquipmentSlot;
            ItemType itemType = iteminfo.ItemType;
            byte classe = iteminfo.Class;
            byte subtype = iteminfo.ItemSubType;
            DateTime test = item.ItemDeleteTime != null ? (DateTime)item.ItemDeleteTime : DateTime.Now;
            long time = item.ItemDeleteTime != null ? (long)test.Subtract(DateTime.Now).TotalSeconds : 0;
            long seconds = item.IsBound ? time : iteminfo.ItemValidTime;
            if (seconds < 0)
            {
                seconds = 0;
            }
            switch (itemType)
            {
                case ItemType.Weapon:
                    switch (equipmentslot)
                    {
                        case EquipmentType.MainWeapon:
                            switch (classe)
                            {
                                case 4:
                                    return $"e_info 1 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0"; // -1 = {item.ShellEffectValue} {item.FirstShell}...
                                case 8:
                                    return $"e_info 5 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0";

                                default:
                                    return $"e_info 0 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0";
                            }
                        case EquipmentType.SecondaryWeapon:
                            switch (classe)
                            {
                                case 1:
                                    return $"e_info 1 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0";

                                case 2:
                                    return $"e_info 1 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0";

                                default:
                                    return $"e_info 0 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0";
                            }
                    }
                    break;

                case ItemType.Armor:
                    return $"e_info 2 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.Price} -1 0 0 0";

                case ItemType.Fashion:
                    switch (equipmentslot)
                    {
                        case EquipmentType.CostumeHat:
                            return $"e_info 3 {item.ItemVNum} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.FireResistance + item.FireResistance} {iteminfo.WaterResistance + item.WaterResistance} {iteminfo.LightResistance + item.LightResistance} {iteminfo.DarkResistance + item.DarkResistance} {iteminfo.Price} {(iteminfo.ItemValidTime == 0 ? -1 : 0)} 2 {(iteminfo.ItemValidTime == 0 ? -1 : seconds / (3600))}";

                        case EquipmentType.CostumeSuit:
                            return $"e_info 2 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.Price} {(iteminfo.ItemValidTime == 0 ? -1 : 0)} 1 {(iteminfo.ItemValidTime == 0 ? -1 : seconds / (3600))}"; // 1 = IsCosmetic -1 = no shells

                        default:
                            return $"e_info 3 {item.ItemVNum} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.FireResistance + item.FireResistance} {iteminfo.WaterResistance + item.WaterResistance} {iteminfo.LightResistance + item.LightResistance} {iteminfo.DarkResistance + item.DarkResistance} {iteminfo.Price} {item.Upgrade} 0 -1"; // after iteminfo.Price theres TimesConnected {(iteminfo.ItemValidTime == 0 ? -1 : iteminfo.ItemValidTime / (3600))}
                    }
                case ItemType.Jewelery:
                    switch (equipmentslot)
                    {
                        case EquipmentType.Amulet:
                            return $"e_info 4 {item.ItemVNum} {iteminfo.LevelMinimum} {seconds * 10} 0 0 {iteminfo.Price}";

                        case EquipmentType.Fairy:
                            return $"e_info 4 {item.ItemVNum} {iteminfo.Element} {item.ElementRate + iteminfo.ElementRate} 0 0 0 0 0"; // last IsNosmall

                        default:
                            return $"e_info 4 {item.ItemVNum} {iteminfo.LevelMinimum} {iteminfo.MaxCellonLvl} {iteminfo.MaxCellon} {item.Cellon} {iteminfo.Price}";
                    }
                case ItemType.Box:
                    SpecialistInstance specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);

                    // 0 = NOSMATE pearl 1= npc pearl 2 = sp box 3 = raid box 4= VEHICLE pearl
                    // 5=fairy pearl
                    switch (subtype)
                    {
                        case 2:
                            return $"e_info 7 {item.ItemVNum} {(item.IsEmpty ? 1 : 0)} {item.Design} {specialist.SpLevel} {CharacterHelper.SpXPData[JobLevelXp]} {CharacterHelper.SpXPData[JobLevel - 1]} {item.Upgrade} {specialist.SlDamage} {specialist.SlDefence} {specialist.SlElement} {specialist.SlHP} {(specialist != null ? (CharacterHelper.SpPoint(specialist.SpLevel, item.Upgrade) - specialist.SlDamage - specialist.SlHP - specialist.SlElement - specialist.SlDefence) : 0)} {item.FireResistance} {item.WaterResistance} {item.LightResistance} {item.DarkResistance} {specialist.SpStoneUpgrade} {specialist.SpDamage} {specialist.SpDefence} {specialist.SpElement} {specialist.SpHP} {specialist.SpFire} {specialist.SpWater} {specialist.SpLight} {specialist.SpDark}";

                        default:
                            return $"e_info 8 {item.ItemVNum} {item.Design} {item.Rare}";
                    }

                case ItemType.Shell:
                    return $"e_info 4 {item.ItemVNum} {iteminfo.LevelMinimum} {item.Rare} {iteminfo.Price} 0"; // 0 = Number of effects
            }
            return string.Empty;
        }

        public string GenerateEq()
        {
            int color = HairColor;
            WearableInstance head = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);

            if (head != null && head.Item.IsColored)
            {
                color = head.Design;
            }

            return $"eq {CharacterId} {(Invisible ? 6 : (byte)Authority)} {Gender} {HairStyle} {color} {Class} {GenerateEqListForPacket()} {GenerateEqRareUpgradeForPacket()}";
        }

        public string GenerateEqListForPacket()
        {
            string[] invarray = new string[16];
            for (short i = 0; i < 16; i++)
            {
                ItemInstance inv = Inventory.LoadBySlotAndType(i, InventoryType.Wear);
                if (inv != null)
                {
                    invarray[i] = inv.ItemVNum.ToString();
                }
                else
                {
                    invarray[i] = "-1";
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
            for (short i = 0; i < 15; i++)
            {
                WearableInstance wearable = Inventory.LoadBySlotAndType<WearableInstance>(i, InventoryType.Wear);
                if (wearable != null)
                {
                    if (wearable.Item.EquipmentSlot == EquipmentType.Armor)
                    {
                        armorRare = wearable.Rare;
                        armorUpgrade = wearable.Upgrade;
                    }
                    else if (wearable.Item.EquipmentSlot == EquipmentType.MainWeapon)
                    {
                        weaponRare = wearable.Rare;
                        weaponUpgrade = wearable.Upgrade;
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
                ItemInstance wearable = Inventory.LoadBySlotAndType<WearableInstance>(i, InventoryType.Wear);
                if (wearable == null)
                {
                    wearable = Inventory.LoadBySlotAndType<SpecialistInstance>(i, InventoryType.Wear);
                }
                if (wearable != null)
                {
                    if (wearable.Item.EquipmentSlot == EquipmentType.Armor)
                    {
                        armorRare = wearable.Rare;
                        armorUpgrade = wearable.Upgrade;
                    }
                    else if (wearable.Item.EquipmentSlot == EquipmentType.MainWeapon)
                    {
                        weaponRare = wearable.Rare;
                        weaponUpgrade = wearable.Upgrade;
                    }
                    eqlist += $" {i}.{wearable.Item.VNum}.{wearable.Rare}.{(wearable.Item.IsColored ? wearable.Design : wearable.Upgrade)}.0";
                }
            }
            return $"equip {weaponUpgrade}{weaponRare} {armorUpgrade}{armorRare}{eqlist}";
        }

        public string GenerateExts()
        {
            return $"exts 0 {48 + BackPack * 12} {48 + BackPack * 12} {48 + BackPack * 12}";
        }

        public string GenerateFaction()
        {
            return $"fs {Faction}";
        }

        public string GenerateFd()
        {
            return $"fd {Reput} {GetReputIco()} {(int)Dignity} {Math.Abs(GetDignityIco())}";
        }

        public string GenerateGender()
        {
            return $"p_sex {Gender}";
        }

        public string GenerateGet(long id)
        {
            return $"get 1 {CharacterId} {id} 0";
        }

        public string GenerateGold()
        {
            return $"gold {Gold} 0";
        }

        public List<string> GenerateGp()
        {
            List<string> gpList = new List<string>();
            int i = 0;
            foreach (PortalDTO portal in ServerManager.GetMap(MapId).Portals)
            {
                gpList.Add($"gp {portal.SourceX} {portal.SourceY} {portal.DestinationMapId} {portal.Type} {i} {(portal.IsDisabled ? 1 : 0)}");
                i++;
            }

            return gpList;
        }

        public string GenerateGp(PortalDTO portal)
        {
            List<PortalDTO> portalList = ServerManager.GetMap(MapId).Portals;
            return $"gp {portal.SourceX} {portal.SourceY} {portal.DestinationMapId} {portal.Type} {portalList.Count} {(portalList.Contains(portal) ? (portal.IsDisabled ? 1 : 0) : 1)}";
        }

        public string GenerateGuri(byte type, byte argument, int value = 0)
        {
            switch (type)
            {
                case 10:
                    return $"guri 10 {argument} {value} {Session.Character.CharacterId}";

                case 15:
                    return $"guri 15 {argument} 0 0";

                default:
                    return $"guri {type} {argument} {Session.Character.CharacterId} {value}";
            }
        }

        public string GenerateIn()
        {
            int color = HairColor;
            WearableInstance headWearable = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);
            if (headWearable != null && headWearable.Item.IsColored)
            {
                color = headWearable.Design;
            }
            ItemInstance fairy = Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);

            return $"in 1 {Name} - {CharacterId} {MapX} {MapY} {Direction} {(byte)Authority} {Gender} {HairStyle} {color} {Class} {GenerateEqListForPacket()} {Math.Ceiling(Hp / HPLoad() * 100)} {Math.Ceiling(Mp / MPLoad() * 100)} {(IsSitting ? 1 : 0)} {(Group != null ? Group.GroupId : -1)} {(fairy != null ? 2 : 0)} {(fairy != null ? fairy.Item.Element : 0)} 0 {(fairy != null ? fairy.Item.Morph : 0)} 0 {(UseSp || IsVehicled ? Morph : 0)} {GenerateEqRareUpgradeForPacket()} -1 - {((GetDignityIco() == 1) ? GetReputIco() : -GetDignityIco())} {(Invisible ? 1 : 0)} {(UseSp ? MorphUpgrade : 0)} 0 {(UseSp ? MorphUpgrade2 : 0)} {Level} 0 {ArenaWinner} {Compliment} {Size} {HeroLevel}";
        }

        public List<string> GenerateIn2()
        {
            return ServerManager.GetMap(MapId).Npcs.Select(npc => npc.GenerateIn2()).ToList();
        }

        public List<string> GenerateIn3()
        {
            List<string> listIn = new List<string>();
            foreach (MapMonster monster in ServerManager.GetMap(MapId).Monsters)
            {
                string in3 = monster.GenerateIn3();
                if (in3 != string.Empty)
                {
                    listIn.Add(in3);
                }
            }
            return listIn;
        }

        public string GenerateInfo(string message)
        {
            return $"info {message}";
        }

        public string GenerateInventoryAdd(short vnum, int amount, InventoryType type, short slot, sbyte rare, short color, byte upgrade, byte upgrade2)
        {
            Item item = ServerManager.GetItem(vnum);
            switch (type)
            {
                case InventoryType.Equipment:
                    return $"ivn 0 {slot}.{vnum}.{rare}.{(item != null ? (item.IsColored ? color : upgrade) : upgrade)}.{upgrade2}";

                case InventoryType.Main:
                    return $"ivn 1 {slot}.{vnum}.{amount}.0";

                case InventoryType.Etc:
                    return $"ivn 2 {slot}.{vnum}.{amount}.0";

                case InventoryType.Miniland:
                    return $"ivn 3 {slot}.{vnum}.{amount}";

                case InventoryType.Specialist:
                    return $"ivn 6 {slot}.{vnum}.{rare}.{upgrade}.{upgrade2}";

                case InventoryType.Costume:
                    return $"ivn 7 {slot}.{vnum}.{rare}.{upgrade}.{upgrade2}";
            }
            return String.Empty;
        }

        public string GenerateInvisible()
        {
            return $"cl {CharacterId} {(Invisible ? 1 : 0)} 0";
        }

        public string GenerateLev()
        {
            SpecialistInstance specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            return $"lev {Level} {LevelXp} {(!UseSp || specialist == null ? JobLevel : specialist.SpLevel)} {(!UseSp || specialist == null ? JobLevelXp : specialist.XP)} {XPLoad()} {(!UseSp || specialist == null ? JobXPLoad() : SPXPLoad())} {Reput} {GetCP()} {HeroXp} {HeroLevel} {HeroXPLoad()}";
        }

        public string GenerateMapOut()
        {
            return "mapout";
        }

        public string GenerateModal(string message, int type)
        {
            return $"modal {type} {message}";
        }

        public string GenerateMsg(string message, int type)
        {
            return $"msg {type} {message}";
        }

        public MovePacket GenerateMv()
        {
            return new MovePacket()
            {
                CharacterId = CharacterId,
                MapX = MapX,
                MapY = MapY,
                Speed = Speed,
                MoveType = 1
            };
        }

        public List<string> GenerateNPCShopOnMap()
        {
            return (from npc in ServerManager.GetMap(MapId).Npcs where npc.Shop != null select $"shop 2 {npc.MapNpcId} {npc.Shop.ShopId} {npc.Shop.MenuType} {npc.Shop.ShopType} {npc.Shop.Name}").ToList();
        }

        public string GenerateOut()
        {
            return $"out 1 {CharacterId}";
        }

        public string GeneratePairy()
        {
            WearableInstance fairy = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
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
            return $"parcel 1 1 {MailList.First(s => s.Value.MailId == (mail.MailId)).Key} {(mail.Title == "NOSMALL" ? 1 : 4)} 0 {mail.Date.ToString("yyMMddHHmm")} {mail.Title} {mail.AttachmentVNum} {mail.AttachmentAmount} {ServerManager.GetItem((short)mail.AttachmentVNum).Type}";
        }

        public string GeneratePidx(bool isLeaveGroup = false)
        {
            string str = String.Empty;
            if (!isLeaveGroup && Group != null)
            {
                str = $"pidx {Group.GroupId}";
                foreach (ClientSession c in Group.Characters.Where(s => s.Character != null))
                {
                    str += $" {(Group.IsMemberOfGroup(CharacterId) ? 1 : 0)}.{c.Character.CharacterId} ";
                }
                return str;
            }
            return $"pidx -1 1.{Session.Character.CharacterId}";
        }

        public string GeneratePinit()
        {
            Group grp = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(CharacterId));
            string str = $"pinit {grp.CharacterCount}";
            int i = 0;
            foreach (ClientSession groupSessionForId in grp.Characters)
            {
                i++;
                str += $" 1|{groupSessionForId.Character.CharacterId}|{i}|{groupSessionForId.Character.Level}|{groupSessionForId.Character.Name}|0|{groupSessionForId.Character.Gender}|{groupSessionForId.Character.Class}|{(groupSessionForId.Character.UseSp ? groupSessionForId.Character.Morph : 0)}|{groupSessionForId.Character.HeroLevel}";
            }
            return str;
        }

        public string GeneratePlayerFlag(long pflag)
        {
            return $"pflag 1 {CharacterId} {pflag}";
        }

        public List<string> GeneratePlayerShopOnMap()
        {
            return ServerManager.GetMap(MapId).UserShops.Select(shop => $"pflag 1 {shop.Value.OwnerId} {shop.Key + 1}").ToList();
        }

        public string GeneratePost(MailDTO mail, byte type)
        {
            return $"post 1 {type} {MailList.First(s => s.Value.MailId == (mail.MailId)).Key} 0 {(mail.IsOpened ? 1 : 0)} {mail.Date.ToString("yyMMddHHmm")} {DAOFactory.CharacterDAO.LoadById(mail.SenderId).Name} {mail.Title}";
        }

        public string GeneratePostMessage(MailDTO mailDTO, byte type)
        {
            CharacterDTO sender = DAOFactory.CharacterDAO.LoadById(mailDTO.SenderId);

            return $"post 5 {type} {MailList.First(s => s.Value == (mailDTO)).Key} 0 0 {mailDTO.SenderClass} {mailDTO.SenderGender} {mailDTO.SenderMorphId} {mailDTO.SenderHairStyle} {mailDTO.SenderHairColor} {mailDTO.EqPacket} {sender.Name} {mailDTO.Title} {mailDTO.Message}";
        }

        public string GeneratePslInfo(SpecialistInstance inventoryItem, int type)
        {
            // 1235.3 1237.4 1239.5 <= skills SkillVNum.Grade
            return $"pslinfo {inventoryItem.Item.VNum} {inventoryItem.Item.Element} {inventoryItem.Item.ElementRate} {inventoryItem.Item.LevelJobMinimum} {inventoryItem.Item.Speed} {inventoryItem.Item.FireResistance} {inventoryItem.Item.WaterResistance} {inventoryItem.Item.LightResistance} {inventoryItem.Item.DarkResistance} 0.0 0.0 0.0";
        }

        public string[] GenerateQuicklist()
        {
            string[] pktQs = new[] { "qslot 0", "qslot 1", "qslot 2" };

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    QuicklistEntryDTO qi = QuicklistEntries.FirstOrDefault(n => n.Q1 == j && n.Q2 == i && n.Morph == (UseSp ? Morph : 0));
                    pktQs[j] += $" {(qi?.Type != null ? qi?.Type : 7)}.{(qi?.Slot != null ? qi?.Slot : 7)}.{(qi != null ? qi.Pos.ToString() : "-1")}";
                }
            }

            return pktQs;
        }

        public string GenerateRc(int v)
        {
            return $"rc 1 {CharacterId} {v} 0";
        }

        public string GenerateReqInfo()
        {
            WearableInstance fairy = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
            WearableInstance armor = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Armor, InventoryType.Wear);
            WearableInstance weapon2 = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
            WearableInstance weapon = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);

            // tc_info 0 name 0 0 0 0 -1 - 0 0 0 0 0 0 0 0 0 0 0 wins deaths reput 0 0 0 morph
            // talentwin talentlose capitul rankingpoints arenapoints 0 0 ispvpprimary ispvpsecondary
            // ispvparmor herolvl desc
            return $"tc_info {Level} {Name} {(fairy != null ? fairy.Item.Element : 0)} {ElementRate} {Class} {Gender} -1 - {GetReputIco()} {GetDignityIco()} {(weapon != null ? 1 : 0)} {weapon?.Rare ?? 0} {weapon?.Upgrade ?? 0} {(weapon2 != null ? 1 : 0)} {weapon2?.Rare ?? 0} {weapon2?.Upgrade ?? 0} {(armor != null ? 1 : 0)} {armor?.Rare ?? 0} {armor?.Upgrade ?? 0} 0 0 {Reput} 0 0 0 {(UseSp ? Morph : 0)} {TalentWin} {TalentLose} {TalentSurrender} 0 {MasterPoints} {Compliment} 0 0 0 0 {HeroLevel} {(String.IsNullOrEmpty(Biography) ? Language.Instance.GetMessageFromKey("NO_PREZ_MESSAGE") : Biography)}";
        }

        public string GenerateRest()
        {
            return $"rest 1 {CharacterId} {(IsSitting ? 1 : 0)}";
        }

        public string GenerateRevive()
        {
            return $"revive 1 {CharacterId} 0";
        }

        public string GenerateSay(string message, int type)
        {
            return $"say 1 {CharacterId} {type} {message}";
        }

        public string GenerateScal()
        {
            return $"char_sc 1 {CharacterId} {Size}";
        }

        public string GenerateShop(string shopname)
        {
            return $"shop 1 {CharacterId} 1 3 0 {shopname}";
        }

        public string GenerateShopEnd()
        {
            return $"shop 1 {CharacterId} 0 0";
        }

        public string GenerateShopMemo(int type, string message)
        {
            return $"s_memo {type} {message}";
        }

        public IEnumerable<string> GenerateShopOnMap()
        {
            return ServerManager.GetMap(MapId).GenerateUserShops();
        }

        public string GenerateSki()
        {
            List<CharacterSkill> characterSkills = UseSp ? SkillsSp.GetAllItems() : Skills.GetAllItems();
            string skibase = String.Empty;
            if (!UseSp)
            {
                skibase = $"{200 + 20 * Class} {201 + 20 * Class}";
            }
            else if (characterSkills.Any())
            {
                skibase = $"{characterSkills.ElementAt(0).SkillVNum} {characterSkills.ElementAt(0).SkillVNum}";
            }
            string generatedSkills = String.Empty;
            foreach (CharacterSkill ski in characterSkills)
            {
                generatedSkills += $" {ski.SkillVNum}";
            }

            return $"ski {skibase}{generatedSkills}";
        }

        public string GenerateSlInfo(SpecialistInstance inventoryItem, int type)
        {
            int freepoint = CharacterHelper.SpPoint(inventoryItem.SpLevel, inventoryItem.Upgrade) - inventoryItem.SlDamage - inventoryItem.SlHP - inventoryItem.SlElement - inventoryItem.SlDefence;

            int slElement = CharacterHelper.SlPoint(inventoryItem.SlElement, 2);
            int slHp = CharacterHelper.SlPoint(inventoryItem.SlHP, 3);
            int slDefence = CharacterHelper.SlPoint(inventoryItem.SlDefence, 1);
            int slHit = CharacterHelper.SlPoint(inventoryItem.SlDamage, 0);

            string skill = String.Empty;
            List<CharacterSkill> skillsSp = new List<CharacterSkill>();
            foreach (Skill ski in ServerManager.GetAllSkill().Where(ski => ski.Class == inventoryItem.Item.Morph + 31 && ski.LevelMinimum <= inventoryItem.SpLevel && !skillsSp.Any(s => s.Skill.MpCost == ski.MpCost && s.Skill.Cooldown == ski.Cooldown && s.Skill.Duration == ski.Duration)))
            {
                skillsSp.Add(new CharacterSkill() { SkillVNum = ski.SkillVNum, CharacterId = CharacterId });
            }
            byte spdestroyed = 0;
            if (inventoryItem.Rare == -2)
            {
                spdestroyed = 1;
            }
            if (!skillsSp.Any())
            {
                skill = "-1";
            }
            for (int i = 1; i < 11; i++)
            {
                if (skillsSp.Count >= i + 1)
                {
                    skill += $"{skillsSp[i].SkillVNum}.";
                }
            }

            // 10 9 8 '0 0 0 0'<- bonusdamage bonusarmor bonuselement bonushpmp its after upgrade and
            // 3 first values are not important
            skill = skill.TrimEnd('.');
            return $"slinfo {type} {inventoryItem.ItemVNum} {inventoryItem.Item.Morph} {inventoryItem.SpLevel} {inventoryItem.Item.LevelJobMinimum} {inventoryItem.Item.ReputationMinimum} 0 0 0 0 0 0 0 {inventoryItem.Item.SpType} {inventoryItem.Item.FireResistance} {inventoryItem.Item.WaterResistance} {inventoryItem.Item.LightResistance} {inventoryItem.Item.DarkResistance} {inventoryItem.XP} {CharacterHelper.SpXPData[inventoryItem.SpLevel - 1]} {skill} {inventoryItem.TransportId} {freepoint} {slHit} {slDefence} {slElement} {slHp} {inventoryItem.Upgrade} 0 0 {spdestroyed} 0 0 0 0 {inventoryItem.SpStoneUpgrade} {inventoryItem.SpDamage} {inventoryItem.SpDefence} {inventoryItem.SpElement} {inventoryItem.SpHP} {inventoryItem.SpFire} {inventoryItem.SpWater} {inventoryItem.SpLight} {inventoryItem.SpDark}";
        }

        public string GenerateSpk(object message, int v)
        {
            return $"spk 1 {CharacterId} {v} {Name} {message}";
        }

        public string GenerateSpPoint()
        {
            return $"sp {SpAdditionPoint} 1000000 {SpPoint} 10000";
        }

        public void GenerateStartupInventory()
        {
            string inv0 = "inv 0", inv1 = "inv 1", inv2 = "inv 2", inv3 = "inv 3", inv6 = "inv 6", inv7 = "inv 7"; // inv 3 used for miniland objects
            foreach (ItemInstance inv in Inventory)
            {
                switch (inv.Type)
                {
                    case InventoryType.Equipment:
                        if (inv.Item.EquipmentSlot == EquipmentType.Sp)
                        {
                            var specialistInstance = inv as SpecialistInstance;
                            inv0 += $" {inv.Slot}.{inv.ItemVNum}.{specialistInstance.Rare}.{specialistInstance.Upgrade}.{specialistInstance.SpStoneUpgrade}";
                        }
                        else
                        {
                            var wearableInstance = inv as WearableInstance;
                            inv0 += $" {inv.Slot}.{inv.ItemVNum}.{wearableInstance.Rare}.{(inv.Item.IsColored ? wearableInstance.Design : wearableInstance.Upgrade)}.0";
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
                        var specialist = inv as SpecialistInstance;
                        inv6 += $" {inv.Slot}.{inv.ItemVNum}.{specialist.Rare}.{specialist.Upgrade}.{specialist.SpStoneUpgrade}";
                        break;

                    case InventoryType.Costume:
                        var costumeInstance = inv as WearableInstance;
                        inv7 += $" {inv.Slot}.{inv.ItemVNum}.{costumeInstance.Rare}.{costumeInstance.Upgrade}.0";
                        break;

                    case InventoryType.Wear:
                        break;
                }
            }
            Session.SendPacket(inv0);
            Session.SendPacket(inv1);
            Session.SendPacket(inv2);
            Session.SendPacket(inv6);
            Session.SendPacket(inv7);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "Easier to read")]
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

                case (byte)ClassType.Magician:
                    type = 2;
                    type2 = 1;
                    break;

                case (byte)ClassType.Swordman:
                    type = 0;
                    type2 = 1;
                    break;

                case (byte)ClassType.Archer:
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
            FireResistance = CharacterHelper.FireResistance(Class, Level);
            LightResistance = CharacterHelper.LightResistance(Class, Level);
            WaterResistance = CharacterHelper.WaterResistance(Class, Level);
            DarkResistance = CharacterHelper.DarkResistance(Class, Level);
            Defence = CharacterHelper.Defence(Class, Level);
            DefenceRate = CharacterHelper.DefenceRate(Class, Level);
            ElementRate = CharacterHelper.ElementRate(Class, Level);
            ElementRateSP = 0;
            DistanceDefence = CharacterHelper.DistanceDefence(Class, Level);
            DistanceDefenceRate = CharacterHelper.DistanceDefenceRate(Class, Level);
            MagicalDefence = CharacterHelper.MagicalDefence(Class, Level);
            if (UseSp)
            {
                SpecialistInstance specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                if (specialist != null)
                {
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

                    p = 0;
                    if (point <= 10)
                        p = point;
                    else if (point <= 20)
                        p = 10 + (point - 10) * 2;
                    else if (point <= 30)
                        p = 30 + (point - 10) * 3;
                    else if (point <= 40)
                        p = 60 + (point - 10) * 4;
                    else if (point <= 50)
                        p = 100 + (point - 10) * 5;
                    else if (point <= 60)
                        p = 150 + (point - 10) * 6;
                    else if (point <= 70)
                        p = 210 + (point - 10) * 7;
                    else if (point <= 80)
                        p = 280 + (point - 10) * 8;
                    else if (point <= 90)
                        p = 360 + (point - 10) * 9;
                    else if (point <= 100)
                        p = 450 + (point - 10) * 10;
                    MinDistance += p;
                    MaxDistance += p;

                    point = CharacterHelper.SlPoint(specialist.SlDefence, 1);
                    p = 0;
                    if (point <= 50)
                    {
                        p = point;
                    }
                    else
                    {
                        p = 50 + (point - 50) * 2;
                    }
                    Defence += p;
                    MagicalDefence += p;
                    DistanceDefence += p;

                    point = CharacterHelper.SlPoint(specialist.SlElement, 2);
                    p = 0;
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
            WearableInstance weapon = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
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

            WearableInstance weapon2 = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
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

            WearableInstance armor = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Armor, InventoryType.Wear);
            if (armor != null)
            {
                armorUpgrade = armor.Upgrade;
                Defence += armor.CloseDefence + armor.Item.CloseDefence;
                DistanceDefence += armor.DistanceDefence + armor.Item.DistanceDefence;
                MagicalDefence += armor.MagicDefence + armor.Item.MagicDefence;
                DefenceRate += armor.DefenceDodge + armor.Item.DefenceDodge;
                DistanceDefenceRate += armor.DistanceDefenceDodge + armor.Item.DistanceDefenceDodge;
            }

            WearableInstance fairy = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (fairy != null)
            {
                ElementRate += fairy.ElementRate + fairy.Item.ElementRate;
            }

            // handle specialist
            if (UseSp)
            {
                SpecialistInstance specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                if (specialist != null)
                {
                    FireResistance += specialist.SpFire;
                    LightResistance += specialist.SpLight;
                    WaterResistance += specialist.SpWater;
                    DarkResistance += specialist.SpWater;
                    Defence += specialist.SpDefence * 10;
                    DistanceDefence += specialist.SpDefence * 10;

                    MinHit += specialist.SpDamage * 10;
                    MaxHit += specialist.SpDamage * 10;
                }
            }

            WearableInstance item = null;
            for (short i = 1; i < 14; i++)
            {
                item = Inventory.LoadBySlotAndType<WearableInstance>(i, InventoryType.Wear);

                if (item != null)
                {
                    if (((item.Item.EquipmentSlot != EquipmentType.MainWeapon)
                        && (item.Item.EquipmentSlot != EquipmentType.SecondaryWeapon)
                        && item.Item.EquipmentSlot != EquipmentType.Armor
                        && item.Item.EquipmentSlot != EquipmentType.Sp))
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
            return $"st 1 {CharacterId} {Level} {HeroLevel} {(int)(Hp / (float)HPLoad() * 100)} {(int)(Mp / (float)MPLoad() * 100)} {Hp} {Mp}";
        }

        public string GenerateTit()
        {
            return $"tit {Language.Instance.GetMessageFromKey(Class == (byte)ClassType.Adventurer ? ClassType.Adventurer.ToString().ToUpper() : Class == (byte)ClassType.Swordman ? ClassType.Swordman.ToString().ToUpper() : Class == (byte)ClassType.Archer ? ClassType.Archer.ToString().ToUpper() : ClassType.Magician.ToString().ToUpper())} {Name}";
        }

        public string GenerateTp()
        {
            return $"tp 1 {CharacterId} {MapX} {MapY} 0";
        }

        public string[] GenerateVb()
        {
            return new string[] { "vb 340 0 0", "vb 339 0 0", "vb 472 0 0", "vb 471 0 0" };
        }

        public void GenerateXp(NpcMonster monsterinfo)
        {
            int partySize = 1;
            Group grp = ServerManager.Instance.Groups.FirstOrDefault(g => g.IsMemberOfGroup(CharacterId));

            if (grp != null)
            {
                partySize = grp.CharacterCount;
            }
            if ((int)(LevelXp / (XPLoad() / 10)) < (int)((LevelXp + monsterinfo.XP) / (XPLoad() / 10)))
            {
                Hp = (int)HPLoad();
                Mp = (int)MPLoad();
                Session.SendPacket(GenerateStat());
                Session.SendPacket(GenerateEff(5));
            }

            SpecialistInstance specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);

            if (Level < 99)
            {
                LevelXp += monsterinfo.XP * ServerManager.XPRate / partySize;
            }
            if ((Class == 0 && JobLevel < 20) || (Class != 0 && JobLevel < 80))
            {
                if (specialist != null && UseSp && specialist.SpLevel < 99)
                {
                    JobLevelXp += ((int)((double)monsterinfo.JobXP / (double)100 * specialist.SpLevel)) * ServerManager.XPRate / partySize;
                }
                else
                {
                    JobLevelXp += monsterinfo.JobXP * ServerManager.XPRate / partySize;
                }
            }
            if (specialist != null && UseSp && specialist.SpLevel < 99)
            {
                specialist.XP += monsterinfo.JobXP * ServerManager.XPRate * (100 - specialist.SpLevel) / partySize;
            }
            double t = XPLoad();
            while (LevelXp >= t)
            {
                LevelXp -= (long)t;
                Level++;
                t = XPLoad();
                if (Level >= 99)
                {
                    Level = 99;
                    LevelXp = 0;
                }
                Hp = (int)HPLoad();
                Mp = (int)MPLoad();
                Session.SendPacket(GenerateStat());
                Session.SendPacket($"levelup {CharacterId}");
                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("LEVELUP"), 0));
                Session.CurrentMap?.Broadcast(GenerateEff(6), MapX, MapY);
                Session.CurrentMap?.Broadcast(GenerateEff(198), MapX, MapY);
                ServerManager.Instance.UpdateGroup(CharacterId);
            }

            WearableInstance fairy = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (fairy != null)
            {
                if ((fairy.ElementRate + fairy.Item.ElementRate) < fairy.Item.MaxElementRate && Level <= monsterinfo.Level + 15 && Level >= monsterinfo.Level - 15)
                {
                    fairy.XP += ServerManager.FairyXpRate;
                }
                t = CharacterHelper.LoadFairyXpData(fairy.ElementRate);
                while (fairy.XP >= t)
                {
                    fairy.XP -= (int)t;
                    fairy.ElementRate++;
                    if ((fairy.ElementRate + fairy.Item.ElementRate) == fairy.Item.MaxElementRate)
                    {
                        fairy.XP = 0;
                        Session.SendPacket(GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAIRYMAX"), fairy.Item.Name), 10));
                    }
                    else
                    {
                        Session.SendPacket(GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVELUP"), fairy.Item.Name), 10));
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
                else if (JobLevel >= 80)
                {
                    JobLevel = 80;
                    JobLevelXp = 0;
                }
                Hp = (int)HPLoad();
                Mp = (int)MPLoad();
                Session.SendPacket(GenerateStat());
                Session.SendPacket($"levelup {CharacterId}");
                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("JOB_LEVELUP"), 0));
                LearnAdventurerSkill();
                Session.CurrentMap?.Broadcast(GenerateEff(6), MapX, MapY);
                Session.CurrentMap?.Broadcast(GenerateEff(198), MapX, MapY);
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
                    Session.SendPacket($"levelup {CharacterId}");
                    if (specialist.SpLevel >= 99)
                    {
                        specialist.SpLevel = 99;
                        specialist.XP = 0;
                    }
                    LearnSPSkill();

                    Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("SP_LEVELUP"), 0));
                    Session.CurrentMap?.Broadcast(GenerateEff(6), MapX, MapY);
                    Session.CurrentMap?.Broadcast(GenerateEff(198), MapX, MapY);
                }
            }
            Session.SendPacket(GenerateLev());
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

        public int GetDamage(int damage)
        {
            CloseShop();

            Hp -= damage;
            if (Hp < 0)
            {
                Hp = 0;
            }
            return Hp;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "Easier to read")]
        public int GetReputIco()
        {
            if (Reput >= 5000001)
            {
                switch (DAOFactory.CharacterDAO.IsReputHero(CharacterId))
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
            if (Reput <= 5000000) return 26;
            return 27;
        }

        public void GiftAdd(short itemVNum, byte amount)
        {
            ItemInstance newItem = Inventory.InstantiateItemInstance(itemVNum, amount);
            if (newItem != null)
            {
                if (newItem.Item.ItemType == ItemType.Armor || newItem.Item.ItemType == ItemType.Weapon || newItem.Item.ItemType == ItemType.Shell)
                {
                    ((WearableInstance)newItem).RarifyItem(Session, RarifyMode.Drop, RarifyProtection.None);
                }
                ItemInstance newInv = Inventory.AddToInventory(newItem);
                if (newInv != null)
                {
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                    Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {newItem.Item.Name} x {amount}", 10));
                }
                else
                {
                    SendGift(CharacterId, itemVNum, amount, newItem.Rare, newItem.Upgrade, false);
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_ACQUIRED_BY_THE_GIANT_MONSTER"), 0));
                }
            }
        }

        public int HealthHPLoad()
        {
            if (IsSitting)
            {
                return CharacterHelper.HpHealth[Class];
            }
            else if ((DateTime.Now - LastDefence).TotalSeconds > 2)
            {
                return CharacterHelper.HpHealthStand[Class];
            }
            else
            {
                return 0;
            }
        }

        public int HealthMPLoad()
        {
            if (IsSitting)
            {
                return CharacterHelper.MpHealth[Class];
            }
            else if ((DateTime.Now - LastDefence).TotalSeconds > 2)
            {
                return CharacterHelper.MpHealthStand[Class];
            }
            else
            {
                return 0;
            }
        }

        public double HPLoad()
        {
            double multiplicator = 1.0;
            int hp = 0;
            if (UseSp)
            {
                SpecialistInstance inventory = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                if (inventory != null)
                {
                    int point = CharacterHelper.SlPoint(inventory.SlHP, 3);

                    if (point <= 50)
                    {
                        multiplicator += point / 100.0;
                    }
                    else
                    {
                        multiplicator += 0.5 + (point - 50.00) / 50.00;
                    }
                    hp = inventory.HP + inventory.SpHP * 100;
                }
            }
            return (int)((CharacterHelper.HPData[Class, Level] + hp) * multiplicator);
        }

        public bool IsMuted()
        {
            return Session.Account.PenaltyLogs.Any(s => s.Penalty == PenaltyType.Muted && s.DateEnd > DateTime.Now);
        }

        public double JobXPLoad()
        {
            if (Class == (byte)ClassType.Adventurer)
            {
                return CharacterHelper.FirstJobXPData[JobLevel - 1];
            }
            return CharacterHelper.SecondJobXPData[JobLevel - 1];
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

                    Skill skinfo = ServerManager.GetSkill((short)i);
                    if (skinfo.Class == 0 && JobLevel >= skinfo.LevelMinimum)
                    {
                        if (!Skills.GetAllItems().Any(s => s.SkillVNum == i))
                        {
                            NewSkill = 1;
                            Skills[i] = new CharacterSkill() { SkillVNum = (short)i, CharacterId = CharacterId };
                        }
                    }
                }
                if (NewSkill > 0)
                {
                    Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                    Session.SendPacket(GenerateSki());
                    Session.SendPackets(GenerateQuicklist());
                }
            }
        }

        public void LearnSPSkill()
        {
            SpecialistInstance specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            byte SkillSpCount = (byte)SkillsSp.Count;
            SkillsSp = new ThreadSafeSortedList<int, CharacterSkill>();
            foreach (Skill ski in ServerManager.GetAllSkill())
            {
                if (ski.Class == Morph + 31 && specialist.SpLevel >= ski.LevelMinimum)
                {
                    SkillsSp[ski.SkillVNum] = new CharacterSkill() { SkillVNum = ski.SkillVNum, CharacterId = CharacterId };
                }
            }
            if (SkillsSp.Count != SkillSpCount)
            {
                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                Session.SendPacket(GenerateSki());
                Session.SendPackets(GenerateQuicklist());
            }
        }

        public IEnumerable<ItemInstance> LoadBySlotAllowed(short itemVNum, int amount)
        {
            return Inventory.Where(i => i.ItemVNum.Equals(itemVNum) && i.Amount + amount < 100);
        }

        public void LoadInventory()
        {
            IEnumerable<ItemInstanceDTO> inventories = DAOFactory.ItemInstanceDAO.LoadByCharacterId(CharacterId).ToList();

            Inventory = new Inventory(this);
            Inventory = new Inventory(this);
            foreach (ItemInstanceDTO inventory in inventories)
            {
                inventory.CharacterId = CharacterId;

                if (inventory.Type != InventoryType.Wear)
                {
                    Inventory.Add((ItemInstance)inventory);
                }
                else
                {
                    Inventory.Add((ItemInstance)inventory);
                }
            }
        }

        public void LoadQuicklists()
        {
            QuicklistEntries = new List<QuicklistEntryDTO>();
            IEnumerable<QuicklistEntryDTO> quicklistDTO = DAOFactory.QuicklistEntryDAO.LoadByCharacterId(CharacterId).ToList();
            foreach (QuicklistEntryDTO qle in quicklistDTO)
            {
                QuicklistEntries.Add(Mapper.DynamicMap<QuicklistEntryDTO>(qle));
            }
        }

        public void LoadSendedMail()
        {
            foreach (MailDTO mail in DAOFactory.MailDAO.LoadBySenderId(CharacterId).Where(s => !MailList.Any(m => m.Value.MailId == s.MailId)))
            {
                MailList.Add((MailList.Any() ? MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mail);

                Session.SendPacket(Session.Character.GeneratePost(mail, 2));
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
                    Skills[characterskill.SkillVNum] = Mapper.DynamicMap<CharacterSkill>(characterskill);
                }
            }
        }

        public void LoadSpeed()
        {
            // only load speed if you dont use custom speed
            if (!IsVehicled && !IsCustomSpeed)
            {
                Speed = CharacterHelper.SpeedData[Class];

                if (UseSp)
                {
                    SpecialistInstance specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
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
            }
        }

        public double MPLoad()
        {
            int mp = 0;
            double multiplicator = 1.0;
            if (UseSp)
            {
                SpecialistInstance inventory = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                if (inventory != null)
                {
                    int point = CharacterHelper.SlPoint(inventory.SlHP, 3);

                    if (point <= 50)
                    {
                        multiplicator += point / 100.0;
                    }
                    else
                    {
                        multiplicator += 0.5 + (point - 50.00) / 50.00;
                    }

                    mp = inventory.MP + inventory.SpHP * 100;
                }
            }
            return (int)((CharacterHelper.MPData[Class, Level] + mp) * multiplicator);
        }

        public void NotifyRarifyResult(sbyte rare)
        {
            Session.SendPacket(GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), rare), 12));
            Session.SendPacket(GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), rare), 0));
            ServerManager.GetMap(MapId).Broadcast(GenerateEff(3005), MapX, MapY);
            Session.SendPacket("shop_end 1");
        }

        public void RefreshMail()
        {
            int i = 0;
            int j = 0;
            List<MailDTO> mails = DAOFactory.MailDAO.LoadByReceiverId(CharacterId).Where(s => !MailList.Any(m => m.Value.MailId == s.MailId)).ToList();
            for (int x = mails.Count - 1; x >= 0; x--)
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
                    Session.SendPacket(Session.Character.GeneratePost(mails.ElementAt(x), 1));
                }
            }
            if (i > 0)
            {
                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("GIFTED"), i), 11));
            }
            if (j > 0)
            {
                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NEW_MAIL"), j), 10));
            }

            LastMailRefresh = DateTime.Now;
        }

        public void RemoveVehicle()
        {
            SpecialistInstance sp = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
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
            Session.CurrentMap?.Broadcast(GenerateCMode());
            Session.SendPacket(GenerateCond());
        }

        public void Rest()
        {
            if (LastSkill.AddSeconds(4) > DateTime.Now || LastDefence.AddSeconds(4) > DateTime.Now)
            {
                return;
            }
            if (!IsVehicled)
            {
                IsSitting = !IsSitting;
                Session.CurrentMap?.Broadcast(GenerateRest());
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
                CharacterDTO character = this.DeepCopy();
                SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref character); // unused variable, check for success?

                // load and concat inventory with equipment
                Inventory copiedInventory = Inventory.DeepCopy();
                IEnumerable<ItemInstanceDTO> inventories = copiedInventory.Concat(Inventory);
                IList<Guid> currentlySavedInventoryIds = DAOFactory.ItemInstanceDAO.LoadSlotAndTypeByCharacterId(CharacterId);

                // remove all which are saved but not in our current enumerable
                foreach (var inventoryToDeleteId in currentlySavedInventoryIds.Except(inventories.Select(i => i.Id)))
                {
                    DAOFactory.ItemInstanceDAO.Delete(inventoryToDeleteId);
                }

                // create or update all which are new or do still exist
                foreach (ItemInstanceDTO inventory in inventories)
                {
                    DAOFactory.ItemInstanceDAO.InsertOrUpdate(inventory);
                }

                if (Skills != null)
                {
                    IEnumerable<Guid> currentlySavedCharacterSkills = DAOFactory.CharacterSkillDAO.LoadKeysByCharacterId(CharacterId).ToList();

                    foreach (Guid characterSkillToDeleteId in currentlySavedCharacterSkills.Except(Skills.GetAllItems().Select(s => s.Id)))
                    {
                        DAOFactory.CharacterSkillDAO.Delete(characterSkillToDeleteId);
                    }

                    foreach (CharacterSkillDTO characterSkill in Skills.GetAllItems())
                    {
                        DAOFactory.CharacterSkillDAO.InsertOrUpdate(characterSkill);
                    }
                }

                IEnumerable<QuicklistEntryDTO> quickListEntriesToInsertOrUpdate = QuicklistEntries.ToList();

                if (quickListEntriesToInsertOrUpdate != null)
                {
                    IEnumerable<Guid> currentlySavedQuicklistEntries = DAOFactory.QuicklistEntryDAO.LoadKeysByCharacterId(CharacterId).ToList();
                    if (currentlySavedQuicklistEntries != null)
                    {
                        foreach (Guid quicklistEntryToDelete in currentlySavedQuicklistEntries.Except(QuicklistEntries.Select(s => s.Id)))
                        {
                            DAOFactory.QuicklistEntryDAO.Delete(quicklistEntryToDelete);
                        }
                        foreach (QuicklistEntryDTO quicklistEntry in quickListEntriesToInsertOrUpdate)
                        {
                            DAOFactory.QuicklistEntryDAO.InsertOrUpdate(quicklistEntry);
                        }
                    }
                }

                foreach (GeneralLogDTO general in Session.Account.GeneralLogs)
                {
                    if (!DAOFactory.GeneralLogDAO.IdAlreadySet(general.LogId))
                    {
                        DAOFactory.GeneralLogDAO.Insert(general);
                    }
                }
                foreach (PenaltyLogDTO penalty in Session.Account.PenaltyLogs)
                {
                    if (!DAOFactory.PenaltyLogDAO.IdAlreadySet(penalty.PenaltyLogId))
                    {
                        DAOFactory.PenaltyLogDAO.Insert(penalty);
                    }
                    else
                    {
                        DAOFactory.PenaltyLogDAO.Update(penalty);
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
            Item it = ServerManager.GetItem((short)vnum);
            int color = HairColor;
            if (it.ItemType != ItemType.Weapon && it.ItemType != ItemType.Armor)
            {
                rare = 0;
                upgrade = 0;
            }
            if (rare > 8 || rare < -2)
            {
                rare = 0;
            }
            if (upgrade < 0 || upgrade > 10)
            {
                upgrade = 0;
            }

            //maximum size of the amount is 99
            if (amount > 99)
            {
                amount = 99;
            }

            MailDTO mail = new MailDTO()
            {
                AttachmentAmount = (it.Type == InventoryType.Etc || it.Type == InventoryType.Main) ? amount : (byte)1,
                IsOpened = false,
                Date = DateTime.Now,
                ReceiverId = id,
                SenderId = id,
                AttachmentRarity = (byte)rare,
                AttachmentUpgrade = upgrade,
                IsSenderCopy = false,
                Title = isNosmall ? "NOSMALL" : "NOSTALE",
                AttachmentVNum = vnum,
                SenderClass = Session.Character.Class,
                SenderGender = Session.Character.Gender,
                SenderHairColor = Session.Character.HairColor,
                SenderHairStyle = Session.Character.HairStyle,
                EqPacket = Session.Character.GenerateEqListForPacket(),
                SenderMorphId = Session.Character.Morph == 0 ? (short)-1 : (short)((Session.Character.Morph > short.MaxValue) ? 0 : Session.Character.Morph)
            };
            DAOFactory.MailDAO.InsertOrUpdate(ref mail);
            if (id == CharacterId)
            {
                Session.Character.MailList.Add((MailList.Any() ? MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mail);
                Session.SendPacket(GenerateParcel(mail));
                Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_GIFTED")} {mail.AttachmentAmount}", 12));
            }
        }

        public double SPXPLoad()
        {
            SpecialistInstance sp2 = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);

            return CharacterHelper.SpXPData[sp2.SpLevel - 1];
        }

        public bool Update()
        {
            try
            {
                CharacterDTO characterToUpdate = Mapper.DynamicMap<CharacterDTO>(this);
                DAOFactory.CharacterDAO.InsertOrUpdate(ref characterToUpdate);
                return true;
            }
            catch (Exception)
            {
                return false;
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

                    case 0:
                        if (ski.Skill.Type == 1)
                        {
                            WearableInstance inv = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                            if (inv != null)
                            {
                                if (inv.Ammo > 0)
                                {
                                    inv.Ammo--;
                                    return true;
                                }
                                else
                                {
                                    if (Inventory.CountItem(2081) < 1)
                                    {
                                        Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_ADVENTURER"), 10));
                                        return false;
                                    }

                                    Inventory.RemoveItemAmount(2081);
                                    inv.Ammo = 100;
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_ADVENTURER"), 10));
                                    return true;
                                }
                            }
                            else
                            {
                                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                                return false;
                            }
                        }
                        else return true;
                    case 1:
                        if (ski.Skill.Type == 1)
                        {
                            WearableInstance inv = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                            if (inv != null)
                            {
                                if (inv.Ammo > 0)
                                {
                                    inv.Ammo--;
                                    return true;
                                }
                                else
                                {
                                    if (Inventory.CountItem(2082) < 1)
                                    {
                                        Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_SWORDSMAN"), 10));
                                        return false;
                                    }

                                    Inventory.RemoveItemAmount(2082);
                                    inv.Ammo = 100;
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_SWORDSMAN"), 10));
                                    return true;
                                }
                            }
                            else
                            {
                                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                                return false;
                            }
                        }
                        else return true;
                    case 2:
                        if (ski.Skill.Type == 1)
                        {
                            WearableInstance inv = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            if (inv != null)
                            {
                                if (inv.Ammo > 0)
                                {
                                    inv.Ammo--;
                                    return true;
                                }
                                else
                                {
                                    if (Inventory.CountItem(2083) < 1)
                                    {
                                        Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_ARCHER"), 10));
                                        return false;
                                    }

                                    Inventory.RemoveItemAmount(2083);
                                    inv.Ammo = 100;
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_ARCHER"), 10));
                                    return true;
                                }
                            }
                            else
                            {
                                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                                return false;
                            }
                        }
                        else return true;
                    case 3:
                        return true;
                }
            }

            return false;
        }

        public double XPLoad()
        {
            return CharacterHelper.XPData[Level - 1];
        }

        /// <summary>
        /// Checks if the current character is in range of the given position
        /// </summary>
        /// <param name="xCoordinate">The x coordinate of the object to check.</param>
        /// <param name="yCoordinate">The y coordinate of the object to check.</param>
        /// <returns>True if the object is in Range, False if not.</returns>
        internal bool IsInRange(int xCoordinate, int yCoordinate)
        {
            return Math.Abs(MapX - xCoordinate) <= 50 && Math.Abs(MapY - yCoordinate) <= 50;
        }

        private object HeroXPLoad()
        {
            return 949560; // need to load true algoritm
        }

        #endregion
    }
}