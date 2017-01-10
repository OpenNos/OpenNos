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
using OpenNos.Data.Enums;
using OpenNos.Domain;
using OpenNos.GameObject.Packets.ServerPackets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class Character : CharacterDTO
    {
        #region Members

        private AuthorityType _authority;
        private IList<CharacterRelationDTO> _blacklisted;
        private byte _cmapcount;
        private int _direction;
        private IList<CharacterRelationDTO> _friends;
        private Inventory _inventory;
        private bool _invisible;
        private bool _isDancing;
        private bool _issitting;
        private bool _familymessagechanged;
        private double _lastPortal;
        private int _lastPulse;
        private int _morph;
        private int _morphUpgrade;
        private int _morphUpgrade2;
        private Random _random;
        private ClientSession _session;
        private int _size = 10;
        private byte _speed;
        private bool _undercover;

        #endregion

        #region Instantiation

        public Character()
        {
            GroupSentRequestCharacterIds = new List<long>();
            FamilyInviteCharacters = new List<long>();
            FriendRequestCharacters = new List<long>();
            StaticBonusList = new List<StaticBonusDTO>();
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

        public bool CanFight
        {
            get
            {
                return !IsSitting && ExchangeInfo == null;
            }
        }

        public int CollectedFamilyXp { get; set; }

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

        public List<StaticBonusDTO> StaticBonusList { get; set; }

        public int DistanceCriticalRate { get; set; }

        public int DistanceDefence { get; set; }

        public int DistanceDefenceRate { get; set; }

        public int DistanceRate { get; set; }

        public byte Element { get; set; }

        public int ElementRate { get; set; }

        public int ElementRateSP { get; private set; }

        public ExchangeInfo ExchangeInfo { get; set; }

        public FamilyDTO Family { get; set; }

        public FamilyCharacterDTO FamilyCharacter { get; set; }

        public int FireResistance { get; set; }

        public bool GmPvtBlock { get; set; }

        public Group Group { get; set; }

        public List<long> GroupSentRequestCharacterIds { get; set; }

        public bool HasGodMode { get; set; }

        public bool HasShopOpened { get; set; }

        public int HitCritical { get; set; }

        public int HitCriticalRate { get; set; }

        public int HitRate { get; set; }

        public List<long> FamilyInviteCharacters { get; set; }

        public bool FamilyMessageChanged
        {
            get
            {
                return _familymessagechanged;
            }

            set
            {
                _familymessagechanged = value;
            }
        }

        public List<long> FriendRequestCharacters { get; set; }

        public bool InExchangeOrTrade
        {
            get
            {
                return ExchangeInfo != null || Speed == 0;
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

        /// <summary>
        /// Defines if the Character Is currently sending or getting items thru exchange.
        /// </summary>
        public bool IsExchanging { get; set; }

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

        public DateTime LastSkillUse { get; set; }

        public DateTime LastPVPRevive { get; set; }

        public double LastSp { get; set; }

        public DateTime LastSpGaugeRemove { get; set; }

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

                if (Session.HasCurrentMap && Session.CurrentMap.MapTypes.Any())
                {
                    long? respawnmaptype = Session.CurrentMap.MapTypes.ElementAt(0).RespawnMapTypeId;
                    if (respawnmaptype != null)
                    {
                        RespawnDTO resp = Respawns.FirstOrDefault(s => s.RespawnMapTypeId == respawnmaptype);
                        if (resp == null)
                        {
                            RespawnMapTypeDTO defaultresp = Session.CurrentMap.DefaultRespawn;
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

        public List<RespawnDTO> Respawns { get; set; }

        public RespawnMapTypeDTO Return
        {
            get
            {
                RespawnMapTypeDTO respawn = new RespawnMapTypeDTO();
                if (Session.HasCurrentMap && Session.CurrentMap.MapTypes.Any())
                {
                    long? respawnmaptype = Session.CurrentMap.MapTypes.ElementAt(0).ReturnMapTypeId;
                    if (respawnmaptype != null)
                    {
                        RespawnDTO resp = Respawns.FirstOrDefault(s => s.RespawnMapTypeId == respawnmaptype);
                        if (resp == null)
                        {
                            RespawnMapTypeDTO defaultresp = Session.CurrentMap.DefaultReturn;
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

        public string GenerateRCBList(CBListPacket packet)
        {
            string itembazar = string.Empty;

            string charname = string.Empty;
            ItemInstance item = null;

            List<string> itemssearch = packet.ItemVNumFilter == "0" ? new List<string>() : packet.ItemVNumFilter.Split(' ').ToList();
            List<BazaarItemLink> bzlist = new List<BazaarItemLink>();
            foreach (BazaarItem bz in ServerManager.Instance.BazarItemList)
            {
                ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.CharacterId == bz.SellerId);

                if (session != null)
                {
                    charname = session.Character.Name;
                    item = session.Character.Inventory.LoadByItemInstance<ItemInstance>(bz.ItemInstanceId);
                }
                else if (DAOFactory.CharacterDAO.LoadById(bz.SellerId) != null)
                {
                    charname = DAOFactory.CharacterDAO.LoadById(bz.SellerId)?.Name;
                    item = (ItemInstance)DAOFactory.IteminstanceDao.LoadById(bz.ItemInstanceId);
                }
                if (item == null)
                    continue;

                switch (packet.TypeFilter)
                {
                    default:
                        bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;

                    case 1://weapon
                        if (item.Item.EquipmentSlot == EquipmentType.MainWeapon || item.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)//WeaponFilter
                            if (packet.SubTypeFilter == 0 || ((item.Item.Class + 1 >> (byte)packet.SubTypeFilter) & 1) == 1)//Class Filter
                                if (packet.LevelFilter == 0 || (packet.LevelFilter == 11 && item.Item.IsHeroic) || (item.Item.LevelMinimum < packet.LevelFilter * 10 + 1 && item.Item.LevelMinimum > packet.LevelFilter * 10 - 9))//Level filter
                                    if (packet.RareFilter == 0 || packet.RareFilter == item.Rare + 1) //rare filter
                                        if (packet.UpgradeFilter == 0 || packet.UpgradeFilter == item.Upgrade + 1) //upgrade filter
                                            bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 2://armor
                        if (item.Item.EquipmentSlot == EquipmentType.Armor)
                            if (packet.SubTypeFilter == 0 || ((item.Item.Class + 1 >> (byte)packet.SubTypeFilter) & 1) == 1)//Class Filter
                                if (packet.LevelFilter == 0 || (packet.LevelFilter == 11 && item.Item.IsHeroic) || (item.Item.LevelMinimum < packet.LevelFilter * 10 + 1 && item.Item.LevelMinimum > packet.LevelFilter * 10 - 9))//Level filter
                                    if (packet.RareFilter == 0 || packet.RareFilter == item.Rare + 1) //rare filter
                                        if (packet.UpgradeFilter == 0 || packet.UpgradeFilter == item.Upgrade + 1) //upgrade filter
                                            bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 3://Equipment 
                        if (item.Item.EquipmentSlot == EquipmentType.Mask || item.Item.EquipmentSlot == EquipmentType.Hat || item.Item.EquipmentSlot == EquipmentType.CostumeHat || item.Item.EquipmentSlot == EquipmentType.CostumeSuit || item.Item.EquipmentSlot == EquipmentType.Gloves || item.Item.EquipmentSlot == EquipmentType.Boots)
                            if (packet.SubTypeFilter == 0 || (packet.SubTypeFilter == 2 && item.Item.EquipmentSlot == EquipmentType.Mask) || (packet.SubTypeFilter == 1 && item.Item.EquipmentSlot == EquipmentType.Hat) || (packet.SubTypeFilter == 6 && item.Item.EquipmentSlot == EquipmentType.CostumeHat) || (packet.SubTypeFilter == 5 && item.Item.EquipmentSlot == EquipmentType.CostumeSuit) || (packet.SubTypeFilter == 3 && item.Item.EquipmentSlot == EquipmentType.Gloves) || (packet.SubTypeFilter == 4 && item.Item.EquipmentSlot == EquipmentType.Boots))
                                if (packet.LevelFilter == 0 || (packet.LevelFilter == 11 && item.Item.IsHeroic) || (item.Item.LevelMinimum < packet.LevelFilter * 10 + 1 && item.Item.LevelMinimum > packet.LevelFilter * 10 - 9))//Level filter
                                    bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 4://Access 
                        if (item.Item.EquipmentSlot == EquipmentType.Necklace || item.Item.EquipmentSlot == EquipmentType.Ring || item.Item.EquipmentSlot == EquipmentType.Bracelet || (item.Item.EquipmentSlot == EquipmentType.Fairy || (item.Item.ItemType == ItemType.Box && item.Item.ItemSubType == 5)) || item.Item.EquipmentSlot == EquipmentType.Amulet)
                            if (packet.SubTypeFilter == 0 || (packet.SubTypeFilter == 2 && item.Item.EquipmentSlot == EquipmentType.Ring) || (packet.SubTypeFilter == 1 && item.Item.EquipmentSlot == EquipmentType.Necklace) || (packet.SubTypeFilter == 5 && item.Item.EquipmentSlot == EquipmentType.Amulet) || (packet.SubTypeFilter == 3 && item.Item.EquipmentSlot == EquipmentType.Bracelet) || (packet.SubTypeFilter == 4 && (item.Item.EquipmentSlot == EquipmentType.Fairy || (item.Item.ItemType == ItemType.Box && item.Item.ItemSubType == 5))))
                                if (packet.LevelFilter == 0 || (packet.LevelFilter == 11 && item.Item.IsHeroic) || (item.Item.LevelMinimum < packet.LevelFilter * 10 + 1 && item.Item.LevelMinimum > packet.LevelFilter * 10 - 9))//Level filter
                                    bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 5://Specialist 
                        if ((item.Item.ItemType == ItemType.Box && item.Item.ItemSubType == 2))
                            if (packet.TypeFilter == 0
                                || (packet.TypeFilter == 1 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 10)
                                 || (packet.TypeFilter == 2 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 11)
                                  || (packet.TypeFilter == 3 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 2)
                                   || (packet.TypeFilter == 4 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 3)
                                    || (packet.TypeFilter == 5 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 13)
                                     || (packet.TypeFilter == 6 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 5)
                                      || (packet.TypeFilter == 7 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 12)
                                       || (packet.TypeFilter == 8 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 4)
                                        || (packet.TypeFilter == 9 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 7)
                                         || (packet.TypeFilter == 10 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 15)
                                          || (packet.TypeFilter == 11 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 6)
                                           || (packet.TypeFilter == 12 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 14)
                                            || (packet.TypeFilter == 13 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 9)
                                             || (packet.TypeFilter == 14 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 9)
                                              || (packet.TypeFilter == 15 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 8)
                                               || (packet.TypeFilter == 16 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 1)
                                                || (packet.TypeFilter == 17 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 16)
                                                 || (packet.TypeFilter == 18 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 17)
                                                  || (packet.TypeFilter == 19 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 18)
                                                   || (packet.TypeFilter == 20 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 19)
                                                    || (packet.TypeFilter == 21 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 20)
                                                     || (packet.TypeFilter == 22 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 21)
                                                      || (packet.TypeFilter == 23 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 22)
                                                       || (packet.TypeFilter == 24 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 23)
                                                        || (packet.TypeFilter == 25 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 24)
                                                         || (packet.TypeFilter == 26 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 25)
                                                          || (packet.TypeFilter == 27 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 26)
                                                           || (packet.TypeFilter == 28 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 27)
                                                            || (packet.TypeFilter == 29 && ServerManager.GetItem((item as BoxInstance).HoldingVNum).Morph == 28))
                                if (packet.LevelFilter == 0 || ((item as BoxInstance).SpLevel < packet.LevelFilter * 10 + 1 && (item as BoxInstance).SpLevel > packet.LevelFilter * 10 - 9))//Level filter
                                    if (packet.UpgradeFilter == 0 || packet.UpgradeFilter == item.Upgrade + 1) //upgrade filter
                                        if (packet.SubTypeFilter == 0 || (packet.SubTypeFilter == 1 && (item as BoxInstance).HoldingVNum == 0) || (packet.SubTypeFilter == 2 && (item as BoxInstance).HoldingVNum != 0))
                                            bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 6://Pet 
                        if (item.Item.ItemType == ItemType.Box && (item.Item.ItemSubType == 0))
                            if (packet.LevelFilter == 0 || ((item as BoxInstance).SpLevel < packet.LevelFilter * 10 + 1 && (item as BoxInstance).SpLevel > packet.LevelFilter * 10 - 9))//Level filter
                                if (packet.SubTypeFilter == 0 || (packet.SubTypeFilter == 1 && (item as BoxInstance).HoldingVNum == 0) || (packet.SubTypeFilter == 2 && (item as BoxInstance).HoldingVNum != 0))
                                    bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 7://Npc
                        if (item.Item.ItemType == ItemType.Box && (item.Item.ItemSubType == 1))
                            if (packet.LevelFilter == 0 || ((item as BoxInstance).SpLevel < packet.LevelFilter * 10 + 1 && (item as BoxInstance).SpLevel > packet.LevelFilter * 10 - 9))//Level filter
                                if (packet.SubTypeFilter == 0 || (packet.SubTypeFilter == 1 && (item as BoxInstance).HoldingVNum == 0) || (packet.SubTypeFilter == 2 && (item as BoxInstance).HoldingVNum != 0))
                                    bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 8://Vehicle
                        if ((item.Item.ItemType == ItemType.Box && (item.Item.ItemSubType == 4)))
                            if (packet.SubTypeFilter == 0 || (packet.SubTypeFilter == 1 && (item as BoxInstance).HoldingVNum == 0) || (packet.SubTypeFilter == 2 && (item as BoxInstance).HoldingVNum != 0))
                                bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 9://Shell
                        if (item.Item.ItemType == ItemType.Shell)
                            if (packet.SubTypeFilter == 0 || item.Item.ItemSubType == item.Item.ItemSubType + 1)
                                if (packet.RareFilter == 0 || packet.RareFilter == item.Rare + 1) //rare filter
                                    if (packet.LevelFilter == 0 || ((item as BoxInstance).SpLevel < packet.LevelFilter * 10 + 1 && (item as BoxInstance).SpLevel > packet.LevelFilter * 10 - 9))//Level filter
                                        bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 10://Main
                        if (item.Item.Type == InventoryType.Main)
                            if (packet.SubTypeFilter == 0 || (packet.SubTypeFilter == 1 && item.Item.ItemType == ItemType.Main) || (packet.SubTypeFilter == 2 && item.Item.ItemType == ItemType.Upgrade) || (packet.SubTypeFilter == 3 && item.Item.ItemType == ItemType.Production) || (packet.SubTypeFilter == 4 && item.Item.ItemType == ItemType.Special) || (packet.SubTypeFilter == 5 && item.Item.ItemType == ItemType.Potion) || (packet.SubTypeFilter == 6 && item.Item.ItemType == ItemType.Event))
                                bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 11://Usable
                        if (item.Item.Type == InventoryType.Etc)
                            if (packet.SubTypeFilter == 0 || (packet.SubTypeFilter == 1 && item.Item.ItemType == ItemType.Food) || (packet.SubTypeFilter == 2 && item.Item.ItemType == ItemType.Snack) || (packet.SubTypeFilter == 3 && item.Item.ItemType == ItemType.Magical) || (packet.SubTypeFilter == 4 && item.Item.ItemType == ItemType.Part) || (packet.SubTypeFilter == 5 && item.Item.ItemType == ItemType.Teacher) || (packet.SubTypeFilter == 6 && item.Item.ItemType == ItemType.Sell))
                                bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                    case 12://Others
                        if (item.Item.ItemType == ItemType.Box)
                            bzlist.Add(new BazaarItemLink() { Item = item, BazaarItem = bz, Owner = charname });
                        break;
                }
            }
            List<BazaarItemLink> bzlistsearched = bzlist.Where(s => itemssearch.Contains(s.Item.ItemVNum.ToString())).ToList();
            int i = 0;
            //price up price down quantity up quantity down
            List<BazaarItemLink> definitivelist = (itemssearch.Any() ? bzlistsearched : bzlist);
            switch (packet.OrderFilter)
            {
                case 0:
                    definitivelist = definitivelist.OrderBy(s => s.BazaarItem.Price).ToList();
                    break;
                case 1:
                    definitivelist = definitivelist.OrderByDescending(s => s.BazaarItem.Price).ToList();
                    break;
                case 2:
                    definitivelist = definitivelist.OrderBy(s => s.BazaarItem.Amount).ToList();
                    break;
                case 3:
                    definitivelist = definitivelist.OrderByDescending(s => s.BazaarItem.Amount).ToList();
                    break;
                default:
                    definitivelist = definitivelist.ToList();
                    break;

            }
            foreach (BazaarItemLink bzlink in definitivelist)
            {
                long time = (long)(bzlink.BazaarItem.DateStart.AddHours(bzlink.BazaarItem.Duration) - DateTime.Now).TotalMinutes;
                if (time > 0 && bzlink.Item.Amount > 0)
                {
                    string info = string.Empty;
                    if (bzlink.Item.Type == InventoryType.Equipment)
                        info = (bzlink.Item.Item.EquipmentSlot != EquipmentType.Sp ?
                            Session.Character.GenerateEInfo(bzlink.Item as WearableInstance) : bzlink.Item.Item.SpType == 0 && bzlink.Item.Item.ItemSubType == 4 ?
                            Session.Character.GeneratePslInfo(bzlink.Item as SpecialistInstance, 0) : Session.Character.GenerateSlInfo(bzlink.Item as SpecialistInstance, 0)).Replace(' ', '^').Replace("slinfo^", "").Replace("e_info^", "");


                    itembazar += $"{bzlink.BazaarItem.TemporaryId}|{bzlink.BazaarItem.SellerId}|{bzlink.Owner}|{bzlink.Item.Item.VNum}|{bzlink.Item.Amount}|{(bzlink.BazaarItem.IsPackage ? 1 : 0)}|{bzlink.BazaarItem.Price}|{time}|2|0|{bzlink.Item.Rare}|{bzlink.Item.Upgrade}|{info} ";
                }
            }

            return $"rc_blist 0 {itembazar} ";
        }

        public short SaveX { get; set; }

        public short SaveY { get; set; }

        public ClientSession Session
        {
            get
            {
                return _session;
            }

            private set
            {
                _session = value;
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
                _speed = value > 59 ? (byte)59 : value;
            }
        }

        public int TimesUsed { get; set; }

        public bool Undercover
        {
            get
            {
                return _undercover;
            }

            set
            {
                _undercover = value;
            }
        }

        public bool UseSp { get; set; }

        public byte VehicleSpeed { get; internal set; }

        public int WaterResistance { get; set; }

        #endregion

        #region Methods

        public void AddBlacklisted(long characterId)
        {
            CharacterRelationDTO addRelation = new CharacterRelationDTO
            {
                CharacterId = CharacterId,
                RelatedCharacterId = characterId,
                RelationType = CharacterRelationType.Blocked
            };
            DAOFactory.CharacterRelationDAO.InsertOrUpdate(ref addRelation);
        }

        public void AddFriend(long characterId)
        {
            CharacterRelationDTO addRelation = new CharacterRelationDTO
            {
                CharacterId = CharacterId,
                RelatedCharacterId = characterId,
                RelationType = CharacterRelationType.Friend
            };
            DAOFactory.CharacterRelationDAO.InsertOrUpdate(ref addRelation);
            _friends = DAOFactory.CharacterRelationDAO.GetFriends(CharacterId);
        }

        public void AddSpouse(long characterId)
        {
            CharacterRelationDTO addRelation = new CharacterRelationDTO
            {
                CharacterId = CharacterId,
                RelatedCharacterId = characterId,
                RelationType = CharacterRelationType.Spouse
            };
            DAOFactory.CharacterRelationDAO.InsertOrUpdate(ref addRelation);
            _friends = DAOFactory.CharacterRelationDAO.GetFriends(CharacterId);
        }

        public void ChangeClass(ClassType characterClass)
        {
            JobLevel = 1;
            JobLevelXp = 0;
            Session.SendPacket("npinfo 0");
            Session.SendPacket("p_clear");

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
            Session.CurrentMap?.Broadcast(Session, GenerateEq());
            Session.CurrentMap?.Broadcast(GenerateEff(8), MapX, MapY);
            Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("CLASS_CHANGED"), 0));
            Session.CurrentMap?.Broadcast(GenerateEff(196), MapX, MapY);
            int faction = 1 + ServerManager.RandomNumber(0, 2);
            Faction = faction;
            Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{faction}"), 0));
            Session.SendPacket("scr 0 0 0 0 0 0");
            Session.SendPacket(GenerateFaction());
            Session.SendPacket(GenerateStatChar());
            Session.SendPacket(GenerateEff(4799 + faction));
            Session.SendPacket(GenerateCond());
            Session.SendPacket(GenerateLev());
            Session.CurrentMap?.Broadcast(Session, GenerateCMode());
            Session.CurrentMap?.Broadcast(Session, GenerateIn(), ReceiverType.AllExceptMe);
            Session.CurrentMap?.Broadcast(Session, GenerateGidx(), ReceiverType.AllExceptMe);
            Session.CurrentMap?.Broadcast(GenerateEff(6), MapX, MapY);
            Session.CurrentMap?.Broadcast(GenerateEff(198), MapX, MapY);
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
            if (ServerManager.Instance.Groups.Any(s => s.IsMemberOfGroup(Session)))
            {
                Session.CurrentMap?.Broadcast(Session, $"pidx 1 1.{CharacterId}", ReceiverType.AllExceptMe);
            }
        }

        public string GenerateRCSList(byte filter)
        {
            string list = string.Empty;

            foreach (BazaarItem bz in ServerManager.Instance.BazarItemList.Where(s => s.SellerId == CharacterId))
            {
                ItemInstance item = Inventory.GetItemInstanceById(bz.ItemInstanceId);
                if (item != null)
                {
                    int SoldedAmount = bz.Amount - item.Amount;
                    int Amount = bz.Amount;
                    bool Package = bz.IsPackage;
                    bool IsNosbazar = bz.MedalUsed;
                    long Price = bz.Price;
                    long MinutesLeft = (long)(bz.DateStart.AddHours(bz.Duration) - DateTime.Now).TotalMinutes;
                    byte Status = MinutesLeft >= 0 ? (SoldedAmount < Amount ? (byte)BazaarType.OnSale : (byte)BazaarType.Solded) : (byte)BazaarType.DelayExpired;
                    if (Status == (byte)BazaarType.DelayExpired)
                    {
                        MinutesLeft = (long)(bz.DateStart.AddHours(bz.Duration).AddDays(30) - DateTime.Now).TotalMinutes;
                    }
                    if (filter == 0 || filter == Status)
                    {
                        list += $"{bz.TemporaryId}|{bz.SellerId}|{item.ItemVNum}|{SoldedAmount}|{Amount}|{(Package ? 1 : 0)}|{Price}|{Status}|{MinutesLeft}|{(IsNosbazar ? 1 : 0)}|1|0|0| ";
                    }
                }
            }

            return $"rc_slist 0 {list}";
        }

        public void ChangeSex()
        {
            Gender = Gender == GenderType.Female ? GenderType.Male : GenderType.Female;
            if (IsVehicled)
            {
                Morph = Gender == GenderType.Female ? Morph + 1 : Morph - 1;
            }
            Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("SEX_CHANGED"), 0));
            Session.SendPacket(GenerateEq());
            Session.SendPacket(GenerateGender());
            Session.CurrentMap?.Broadcast(Session, GenerateIn(), ReceiverType.AllExceptMe);
            Session.CurrentMap?.Broadcast(Session, GenerateGidx(), ReceiverType.AllExceptMe);
            Session.CurrentMap?.Broadcast(GenerateCMode());
            Session.CurrentMap?.Broadcast(GenerateEff(196), MapX, MapY);
        }

        public void CharacterLife()
        {
            int x = 1;
            bool change = false;
            if (Session.Character.Hp == 0 && Session.Character.LastHealth.AddSeconds(2) <= DateTime.Now)
            {
                Session.Character.Mp = 0;
                Session.SendPacket(Session.Character.GenerateStat());
                Session.Character.LastHealth = DateTime.Now;
            }
            else
            {
                WearableInstance amulet = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Amulet, InventoryType.Wear);
                if (Session.Character.LastEffect.AddSeconds(5) <= DateTime.Now && amulet != null)
                {
                    if (amulet.ItemVNum == 4503 || amulet.ItemVNum == 4504)
                    {
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(amulet.Item.EffectValue + (Session.Character.Class == ClassType.Adventurer ? 0 : (byte)Session.Character.Class - 1)), Session.Character.MapX, Session.Character.MapY);
                    }
                    else
                    {
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(amulet.Item.EffectValue), Session.Character.MapX, Session.Character.MapY);
                    }
                    Session.Character.LastEffect = DateTime.Now;
                }

                if (Session.Character.LastHealth.AddSeconds(2) <= DateTime.Now || Session.Character.IsSitting && Session.Character.LastHealth.AddSeconds(1.5) <= DateTime.Now)
                {
                    Session.Character.LastHealth = DateTime.Now;
                    if (Session.HealthStop)
                    {
                        Session.HealthStop = false;
                        return;
                    }

                    if (Session.Character.LastDefence.AddSeconds(2) <= DateTime.Now && Session.Character.LastSkillUse.AddSeconds(2) <= DateTime.Now && Session.Character.Hp > 0)
                    {
                        if (x == 0)
                        {
                            x = 1;
                        }
                        if (Session.Character.Hp + Session.Character.HealthHPLoad() < Session.Character.HPLoad())
                        {
                            change = true;
                            Session.Character.Hp += Session.Character.HealthHPLoad();
                        }
                        else
                        {
                            if (Session.Character.Hp != (int)Session.Character.HPLoad())
                            {
                                change = true;
                            }
                            Session.Character.Hp = (int)Session.Character.HPLoad();
                        }
                        if (x == 1)
                        {
                            if (Session.Character.Mp + Session.Character.HealthMPLoad() < Session.Character.MPLoad())
                            {
                                Session.Character.Mp += Session.Character.HealthMPLoad();
                                change = true;
                            }
                            else
                            {
                                if (Session.Character.Mp != (int)Session.Character.MPLoad())
                                {
                                    change = true;
                                }
                                Session.Character.Mp = (int)Session.Character.MPLoad();
                            }
                        }
                        if (change)
                        {
                            Session.SendPacket(Session.Character.GenerateStat());
                        }
                    }
                }
                if (Session.Character.UseSp)
                {
                    if (Session.Character.LastSpGaugeRemove <= new DateTime(0001, 01, 01, 00, 00, 00))
                        Session.Character.LastSpGaugeRemove = DateTime.Now;
                    if (Session.Character.LastSkillUse.AddSeconds(15) >= DateTime.Now && Session.Character.LastSpGaugeRemove.AddSeconds(1) <= DateTime.Now)
                    {
                        SpecialistInstance specialist = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                        byte spType = 0;

                        if (specialist.Item.Morph > 1 && specialist.Item.Morph < 8 || specialist.Item.Morph > 9 && specialist.Item.Morph < 16)
                            spType = 3;
                        else if (specialist.Item.Morph > 16 && specialist.Item.Morph < 29)
                            spType = 2;
                        else if (specialist.Item.Morph == 9)
                            spType = 1;
                        if (Session.Character.SpPoint >= spType)
                        {
                            Session.Character.SpPoint -= spType;
                        }
                        else if (Session.Character.SpPoint < spType && Session.Character.SpPoint != 0)
                        {
                            spType -= (byte)Session.Character.SpPoint;
                            Session.Character.SpPoint = 0;
                            Session.Character.SpAdditionPoint -= spType;
                        }
                        else if (Session.Character.SpPoint == 0 && Session.Character.SpAdditionPoint >= spType)
                        {
                            Session.Character.SpAdditionPoint -= spType;
                        }
                        else if (Session.Character.SpPoint == 0 && Session.Character.SpAdditionPoint < spType)
                        {
                            Session.Character.SpAdditionPoint = 0;

                            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;

                            if (Session.Character.UseSp)
                            {
                                Session.Character.LastSp = currentRunningSeconds;
                                if (Session != null && Session.HasSession)
                                {
                                    if (Session.Character.IsVehicled)
                                    {
                                        return;
                                    }
                                    Logger.Debug(specialist.ItemVNum.ToString(), Session.SessionId);
                                    Session.Character.UseSp = false;
                                    Session.Character.LoadSpeed();
                                    Session.SendPacket(Session.Character.GenerateCond());
                                    Session.SendPacket(Session.Character.GenerateLev());
                                    Session.Character.SpCooldown = 30;
                                    if (Session.Character?.SkillsSp != null)
                                    {
                                        foreach (CharacterSkill ski in Session.Character.SkillsSp.GetAllItems().Where(s => !s.CanBeUsed()))
                                        {
                                            short time = ski.Skill.Cooldown;
                                            double temp = (ski.LastUse - DateTime.Now).TotalMilliseconds + time * 100;
                                            temp /= 1000;
                                            Session.Character.SpCooldown = temp > Session.Character.SpCooldown ? (int)temp : Session.Character.SpCooldown;
                                        }
                                    }
                                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("STAY_TIME"), Session.Character.SpCooldown), 11));
                                    Session.SendPacket($"sd {Session.Character.SpCooldown}");
                                    Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                                    Session.CurrentMap?.Broadcast(Session.Character.GenerateGuri(6, 1), Session.Character.MapX, Session.Character.MapY);

                                    // ms_c
                                    Session.SendPacket(Session.Character.GenerateSki());
                                    Session.SendPackets(Session.Character.GenerateQuicklist());
                                    Session.SendPacket(Session.Character.GenerateStat());
                                    Session.SendPacket(Session.Character.GenerateStatChar());
                                    Observable.Timer(TimeSpan.FromMilliseconds(Session.Character.SpCooldown * 1000))
                                               .Subscribe(
                                               o =>
                                               {
                                                   Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPPEAR"), 11));
                                                   Session.SendPacket("sd 0");
                                               });
                                }
                            }
                        }
                        Session.SendPacket(Session.Character.GenerateSpPoint());
                        Session.Character.LastSpGaugeRemove = DateTime.Now;
                    }
                }
            }
        }

        public void CloseExchangeOrTrade()
        {
            if (InExchangeOrTrade)
            {
                long? targetSessionId = ExchangeInfo?.TargetCharacterId;

                if (targetSessionId.HasValue && Session.HasCurrentMap)
                {
                    ClientSession targetSession = Session.CurrentMap.GetSessionByCharacterId(targetSessionId.Value);

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

        public void CloseShop(bool closedByCharacter = false)
        {
            if (HasShopOpened && Session.HasCurrentMap)
            {
                KeyValuePair<long, MapShop> shop = Session.CurrentMap.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(CharacterId));
                if (!shop.Equals(default(KeyValuePair<long, MapShop>)))
                {
                    Session.CurrentMap.UserShops.Remove(shop.Key);

                    // declare that the shop cannot be closed
                    HasShopOpened = false;

                    Session.CurrentMap?.Broadcast(GenerateShopEnd());
                    Session.CurrentMap?.Broadcast(Session, GeneratePlayerFlag(0), ReceiverType.AllExceptMe);
                    IsSitting = false;
                    IsShopping = false; // close shop by character will always completely close the shop

                    LoadSpeed();
                    Session.SendPacket(GenerateCond());
                    Session.CurrentMap?.Broadcast(GenerateRest());
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

        public void DeleteBlacklisted(long characterId)
        {
            DAOFactory.CharacterRelationDAO.Delete(CharacterId, characterId);
        }

        public void DeleteFriend(long characterId)
        {
            DAOFactory.CharacterRelationDAO.Delete(CharacterId, characterId);
            CharacterRelationDTO deleteReleation = _friends.FirstOrDefault(f => f.RelatedCharacterId == characterId);
            if (deleteReleation != null)
            {
                _friends.Remove(deleteReleation);
            }
        }
        public void DeleteSpouse(long characterId)
        {
            DAOFactory.CharacterRelationDAO.Delete(CharacterId, characterId);
            CharacterRelationDTO deleteReleation = _friends.FirstOrDefault(f => f.RelatedCharacterId == characterId);
            if (deleteReleation != null)
            {
                _friends.Remove(deleteReleation);
            }
        }

        public void DeleteItem(InventoryType type, short slot)
        {
            if (Inventory != null)
            {
                Inventory.DeleteFromSlotAndType(slot, type);
                Session.SendPacket(GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0, 0));
            }
        }

        public void DeleteItemByItemInstanceId(Guid id)
        {
            if (Inventory != null)
            {
                Tuple<short, InventoryType> result = Inventory.DeleteById(id);
                Session.SendPacket(GenerateInventoryAdd(-1, 0, result.Item2, result.Item1, 0, 0, 0, 0));
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
                    Session.SendPacket(GenerateInventoryAdd(-1, 0, item.Type, item.Slot, 0, 0, 0, 0));
                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                }
            }

            foreach (ItemInstance item in Inventory.GetAllItems())
            {
                if (item.IsBound && item.ItemDeleteTime != null && item.ItemDeleteTime < DateTime.Now)
                {
                    Inventory.DeleteById(item.Id);
                    Session.SendPacket(GenerateEquipment());
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
        }

        public string GenerateAt()
        {
            Map mapForMusic = ServerManager.GetMap(MapId);
            return $"at {CharacterId} {MapId} {MapX} {MapY} 2 0 {mapForMusic?.Music ?? 0} -1";
        }

        public string GenerateBlinit()
        {
            string result = "blinit";
            _blacklisted = DAOFactory.CharacterRelationDAO.GetBlacklisted(CharacterId);
            foreach (CharacterRelationDTO relation in _blacklisted)
            {
                result += $" {relation.RelatedCharacterId}|{DAOFactory.CharacterDAO.LoadById(relation.RelatedCharacterId).Name}";
            }
            return result;
        }

        public string GenerateCInfo()
        {
            return $"c_info {Name} - -1 -1 - {CharacterId} {(Invisible ? 6 : Undercover ? (byte)AuthorityType.User : (byte)Authority)} {(byte)Gender} {(byte)HairStyle} {(byte)HairColor} {(byte)Class} {(GetDignityIco() == 1 ? GetReputIco() : -GetDignityIco())} {Compliment} {(UseSp || IsVehicled ? Morph : 0)} {(Invisible ? 1 : 0)} 0 {(UseSp ? MorphUpgrade : 0)} {ArenaWinner}";
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

            short mainUpgrade = 0;
            int mainCritChance = 0;
            int mainCritHit = 0;
            int mainMinDmg = 0;
            int mainMaxDmg = 0;
            int mainHitRate = 0;

            short secUpgrade = 0;
            int secCritChance = 0;
            int secCritHit = 0;
            int secMinDmg = 0;
            int secMaxDmg = 0;
            int secHitRate = 0;

            // int CritChance = 4; int CritHit = 70; int MinDmg = 0; int MaxDmg = 0; int HitRate = 0;
            // sbyte Upgrade = 0;

            #endregion

            #region Get Weapon Stats

            WearableInstance weapon = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
            if (weapon != null)
            {
                mainUpgrade = weapon.Upgrade;
            }

            mainMinDmg += MinHit;
            mainMaxDmg += MaxHit;
            mainHitRate += HitRate;
            mainCritChance += HitCriticalRate;
            mainCritHit += HitCritical;

            WearableInstance weapon2 = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
            if (weapon2 != null)
            {
                secUpgrade = weapon2.Upgrade;
            }

            secMinDmg += MinDistance;
            secMaxDmg += MaxDistance;
            secHitRate += DistanceRate;
            secCritChance += DistanceCriticalRate;
            secCritHit += DistanceCritical;

            #endregion

            #region Switch skill.Type

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
                    break;

                case 2:
                    monsterDefence = monsterToAttack.Monster.MagicDefence;
                    break;

                case 3:
                    switch (Class)
                    {
                        case ClassType.Swordman:
                            monsterDefence = monsterToAttack.Monster.CloseDefence;
                            break;

                        case ClassType.Archer:
                            monsterDefence = monsterToAttack.Monster.DistanceDefence;
                            break;

                        case ClassType.Magician:
                            monsterDefence = monsterToAttack.Monster.MagicDefence;
                            break;

                        case ClassType.Adventurer:
                            monsterDefence = monsterToAttack.Monster.CloseDefence;
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
                    break;
            }

            #endregion

            #region Basic Damage Data Calculation
            // TODO: Implement BCard damage boosts, see Issue

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
                if ((skill.Type == 0 || skill.Type == 1) && !HasGodMode)
                {
                    if (ServerManager.RandomNumber() <= chance)
                    {
                        hitmode = 1;
                        return 0;
                    }
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.RandomNumber(mainMinDmg, mainMaxDmg + 1);
            baseDamage += skill.Damage / 4;
            baseDamage += Level - monsterToAttack.Monster.Level; //Morale
            if (Class == ClassType.Adventurer)
            {
                //HACK: Damage is ~10 lower in OpenNos than in official. Fix this...
                baseDamage += 20;
            }
            int elementalDamage = 0; // placeholder for BCard etc...
            elementalDamage += skill.ElementalDamage / 4;
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
                if (Math.Abs(monsterToAttack.MapX - MapX) < 4 && Math.Abs(monsterToAttack.MapY - MapY) < 4)
                    baseDamage = (int)(baseDamage * 0.7);
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

            if (ServerManager.RandomNumber() <= mainCritChance)
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

            #endregion

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage;
            if (totalDamage < 5)
            {
                totalDamage = ServerManager.RandomNumber(1, 6);
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
            if (monsterToAttack.Target != CharacterId)
            {
                monsterToAttack.LastEffect = DateTime.Now;
            }
            ushort damage = Convert.ToUInt16(totalDamage);
            if (monsterToAttack.IsMoving)
            {
                int nearestDistance = 100;
                foreach (KeyValuePair<long, long> kvp in monsterToAttack.DamageList)
                {
                    ClientSession session = monsterToAttack.Map.GetSessionByCharacterId(kvp.Value);
                    if (session != null)
                    {
                        int distance = Map.GetDistance(new MapCell { X = MapX, Y = MapY }, new MapCell { X = session.Character.MapX, Y = session.Character.MapY });
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            monsterToAttack.Target = session.Character.CharacterId;
                        }
                    }
                }
            }
            return damage;
        }

        public int GeneratePVPDamage(Character target, Skill skill, ref int hitmode)
        {
            #region Definitions

            if (target == null)
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
            short monsterDefLevel = 0;

            short mainUpgrade = 0;
            int mainCritChance = 0;
            int mainCritHit = 0;
            int mainMinDmg = 0;
            int mainMaxDmg = 0;
            int mainHitRate = 0;

            short secUpgrade = 0;
            int secCritChance = 0;
            int secCritHit = 0;
            int secMinDmg = 0;
            int secMaxDmg = 0;
            int secHitRate = 0;

            // int CritChance = 4; int CritHit = 70; int MinDmg = 0; int MaxDmg = 0; int HitRate = 0;
            // sbyte Upgrade = 0;

            #endregion

            #region Get Weapon Stats

            WearableInstance weapon = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.MainWeapon, InventoryType.Wear);
            if (weapon != null)
            {
                mainUpgrade = weapon.Upgrade;
            }

            mainMinDmg += MinHit;
            mainMaxDmg += MaxHit;
            mainHitRate += HitRate;
            mainCritChance += HitCriticalRate;
            mainCritHit += HitCritical;

            WearableInstance weapon2 = Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
            if (weapon2 != null)
            {
                secUpgrade = weapon2.Upgrade;
            }

            secMinDmg += MinDistance;
            secMaxDmg += MaxDistance;
            secHitRate += DistanceRate;
            secCritChance += DistanceCriticalRate;
            secCritHit += DistanceCritical;

            WearableInstance targetArmor = target.Inventory?.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Armor, InventoryType.Wear);
            if (targetArmor != null)
            {
                monsterDefLevel = targetArmor.Upgrade;
            }

            #endregion

            #region Switch skill.Type

            switch (skill.Type)
            {
                case 0:
                    monsterDefence = target.Defence;
                    monsterDodge = (int)(target.DefenceRate * 0.95);
                    if (Class == ClassType.Archer)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    break;

                case 1:
                    monsterDefence = target.DistanceDefence;
                    monsterDodge = (int)(target.DistanceDefenceRate * 0.95);
                    if (Class == ClassType.Swordman || Class == ClassType.Adventurer || Class == ClassType.Magician)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    break;

                case 2:
                    monsterDefence = target.MagicalDefence;
                    break;

                case 3:
                    switch (Class)
                    {
                        case ClassType.Swordman:
                            monsterDefence = target.Defence;
                            break;

                        case ClassType.Archer:
                            monsterDefence = target.DistanceDefence;
                            break;

                        case ClassType.Magician:
                            monsterDefence = target.MagicalDefence;
                            break;

                        case ClassType.Adventurer:
                            monsterDefence = target.Defence;
                            break;
                    }
                    break;

                case 5:
                    monsterDefence = target.Defence;
                    monsterDodge = target.DefenceRate;
                    if (Class == ClassType.Archer)
                    {
                        mainCritHit = secCritHit;
                        mainCritChance = secCritChance;
                        mainHitRate = secHitRate;
                        mainMaxDmg = secMaxDmg;
                        mainMinDmg = secMinDmg;
                        mainUpgrade = secUpgrade;
                    }
                    break;
            }

            #endregion

            #region Basic Damage Data Calculation
            // TODO: Implement BCard damage boosts, see Issue

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
                if ((skill.Type == 0 || skill.Type == 1) && !HasGodMode)
                {
                    if (ServerManager.RandomNumber() <= chance)
                    {
                        hitmode = 1;
                        return 0;
                    }
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.RandomNumber(mainMinDmg, mainMaxDmg + 1);
            baseDamage += skill.Damage / 4;
            baseDamage += Level - target.Level; //Morale
            if (Class == ClassType.Adventurer)
            {
                //HACK: Damage is ~10 lower in OpenNos than in official. Fix this...
                baseDamage += 20;
            }
            int elementalDamage = 0; // placeholder for BCard etc...
            elementalDamage += skill.ElementalDamage / 4;
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
                if (Math.Abs(target.MapX - MapX) < 4 && Math.Abs(target.MapY - MapY) < 4)
                    baseDamage = (int)(baseDamage * 0.7);
            }

            #endregion

            #region Elementary Damage

            #region Calculate Elemental Boost + Rate

            double elementalBoost = 0;
            int monsterResistance = 0;
            switch (Element)
            {
                case 0:
                    break;

                case 1:
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
            elementalDamage = elementalDamage / 100 * (100 - monsterResistance);
            if (elementalDamage < 0)
            {
                elementalDamage = 0;
            }

            #endregion

            #region Critical Damage

            baseDamage -= monsterDefence;

            if (ServerManager.RandomNumber() <= mainCritChance)
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

            #endregion

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage;
            if (totalDamage < 5)
            {
                totalDamage = ServerManager.RandomNumber(1, 6);
            }

            #endregion

            #endregion

            return totalDamage;
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
                    Session.CurrentMap?.Broadcast(Session, GenerateGidx(), ReceiverType.AllExceptMe);
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
            return ServerManager.GetMap(MapId).DroppedList.GetAllItems().Select(item => $"in 9 {item.ItemVNum} {item.TransportId} {item.PositionX} {item.PositionY} {(item is MonsterMapItem && ((MonsterMapItem)item).GoldAmount > 1 ? ((MonsterMapItem)item).GoldAmount : item.Amount)} 0 0 -1").ToList();
        }

        public EffectPacket GenerateEff(int effectid, byte effecttype = 1)
        {
            return new EffectPacket
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
            DateTime test = item.ItemDeleteTime ?? DateTime.Now;
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
                            return $"e_info 3 {item.ItemVNum} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.FireResistance + item.FireResistance} {iteminfo.WaterResistance + item.WaterResistance} {iteminfo.LightResistance + item.LightResistance} {iteminfo.DarkResistance + item.DarkResistance} {iteminfo.Price} {(iteminfo.ItemValidTime == 0 ? -1 : 0)} 2 {(iteminfo.ItemValidTime == 0 ? -1 : seconds / 3600)}";

                        case EquipmentType.CostumeSuit:
                            return $"e_info 2 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.Price} {(iteminfo.ItemValidTime == 0 ? -1 : 0)} 1 {(iteminfo.ItemValidTime == 0 ? -1 : seconds / 3600)}"; // 1 = IsCosmetic -1 = no shells

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
                case ItemType.Specialist:
                    return $"e_info 8 {item.ItemVNum}";

                case ItemType.Box:
                    if (item.GetType() == typeof(BoxInstance))
                    {
                        BoxInstance specialist = (BoxInstance)item;

                        // 0 = NOSMATE pearl 1= npc pearl 2 = sp box 3 = raid box 4= VEHICLE pearl
                        // 5=fairy pearl
                        switch (subtype)
                        {
                            case 2:
                                Item spitem = ServerManager.GetItem(specialist.HoldingVNum);
                                return specialist.HoldingVNum == 0 ?
                                    $"e_info 7 {item.ItemVNum} 0" :
                                    $"e_info 7 {item.ItemVNum} 1 {specialist.HoldingVNum} {specialist.SpLevel} {specialist.XP} {CharacterHelper.SPXPData[specialist.SpLevel - 1]} {item.Upgrade} {CharacterHelper.SlPoint(specialist.SlDamage, 0)} {CharacterHelper.SlPoint(specialist.SlDefence, 1)} {CharacterHelper.SlPoint(specialist.SlElement, 2)} {CharacterHelper.SlPoint(specialist.SlHP, 3)} {CharacterHelper.SPPoint(specialist.SpLevel, item.Upgrade) - specialist.SlDamage - specialist.SlHP - specialist.SlElement - specialist.SlDefence} {specialist.SpStoneUpgrade} {spitem.FireResistance} {spitem.WaterResistance} {spitem.LightResistance} {spitem.DarkResistance} {specialist.SpDamage} {specialist.SpDefence} {specialist.SpElement} {specialist.SpHP} {specialist.SpFire} {specialist.SpWater} {specialist.SpLight} {specialist.SpDark}";
                            case 4:
                                return specialist.HoldingVNum == 0 ?
                                    $"e_info 11 {item.ItemVNum} 0" :
                                    $"e_info 11 {item.ItemVNum} 1 {specialist.HoldingVNum}";
                            case 5:
                                Item fairyitem = ServerManager.GetItem(specialist.HoldingVNum);
                                return specialist.HoldingVNum == 0 ?
                                    $"e_info 12 {item.ItemVNum} 0" :
                                    $"e_info 12 {item.ItemVNum} 1 {specialist.HoldingVNum} {specialist.ElementRate + fairyitem.ElementRate}";

                            default:
                                return $"e_info 8 {item.ItemVNum} {item.Design} {item.Rare}";
                        }
                    }
                    return $"e_info 7 {item.ItemVNum} 0";

                case ItemType.Shell:
                    return $"e_info 4 {item.ItemVNum} {iteminfo.LevelMinimum} {item.Rare} {iteminfo.Price} 0"; // 0 = Number of effects
            }
            return string.Empty;
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

        public bool HaveBackpack()
        {
            return StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.BackPack);
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
            //gmbr 0 972109|16070622|†Socke†|92|2|0|9|0|1 962596|16070622|¥»Nancy»¥|96|3|1|0|0|1 338884|16070622|Ciapa|96|1|1|0|1|1 998939|16033022|†«¢®êe¶êR»†|59|2|3|0|0|0 963863|16070819|•Êìsstérñçhèñ•|80|1|3|0|0|0 1017441|16102917|†SüßeErdbeere†|58|1|3|0|0|0 1003329|16110518|Rising†Redbuff|36|3|3|0|0|0 972112|16070900|†Söckchen†|83|2|3|0|0|0 1044684|16102914|*Necrømancer*|71|3|3|0|0|0 1043396|16122716|rdfeenlvln1|1|0|3|0|1|0
            string str = "gmbr 0";
            try
            {
                foreach (ClientSession groupClientSession in ServerManager.Instance.Sessions.Where(s => s.Character.Family != null && s.Character.Family.FamilyId == Family.FamilyId))
                {
                    str +=
                        $" {groupClientSession.Character.CharacterId}|0|{groupClientSession.Character.Name}|{groupClientSession.Character.Level}|{(byte)groupClientSession.Character.Class}|{(byte)groupClientSession.Character.FamilyCharacter.Authority}|{(byte)groupClientSession.Character.FamilyCharacter.Rank}|1|{groupClientSession.Character.HeroLevel}";
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
                foreach (ClientSession groupClientSession in ServerManager.Instance.Sessions.Where(s => s.Character.Family != null && s.Character.Family.FamilyId == Family.FamilyId))
                {
                    str +=
                        $" {groupClientSession.Character.CharacterId}|{groupClientSession.Character.FamilyCharacter.DailyMessage}";
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
                foreach (ClientSession groupClientSession in ServerManager.Instance.Sessions.Where(s => s.Character.Family != null && s.Character.Family.FamilyId == Family.FamilyId))
                {
                    str +=
                        $" {groupClientSession.Character.CharacterId}|{groupClientSession.Character.FamilyCharacter.Experience}";
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return str;
        }

        public string GenerateFd()
        {
            return $"fd {Reput} {GetReputIco()} {(int)Dignity} {Math.Abs(GetDignityIco())}";
        }

        public string GenerateFinfo(long? relatedCharacterLoggedOutId = null, long? relatedCharacterLoggedInId = null)
        {
            string result = "finfo";

            if (_friends != null)
            {
                foreach (CharacterRelationDTO relation in _friends)
                {
                    if (relatedCharacterLoggedInId.HasValue && relatedCharacterLoggedInId.Value == relation.RelatedCharacterId)
                    {
                        result += $" {relation.RelatedCharacterId}.1";
                    }
                    else if (relatedCharacterLoggedOutId.HasValue && relation.RelatedCharacterId == relatedCharacterLoggedOutId.Value)
                    {
                        result += $" {relation.RelatedCharacterId}.0";
                    }
                }
            }
            return result;
        }

        public string GenerateFinit()
        {
            string result = "finit";
            _friends = DAOFactory.CharacterRelationDAO.GetFriends(CharacterId);
            foreach (CharacterRelationDTO relation in _friends)
            {
                byte isOnline = 0;
                if (ServerManager.Instance.GetSessionByCharacterId(relation.RelatedCharacterId) != null)
                {
                    isOnline = 1;
                }
                result += $" {relation.RelatedCharacterId}|{(short)relation.RelationType}|{isOnline}|{DAOFactory.CharacterDAO.LoadById(relation.RelatedCharacterId).Name}";
            }
            return result;
        }

        public string GenerateGender()
        {
            return $"p_sex {(byte)Gender}";
        }

        public string GenerateGet(long id)
        {
            return $"get 1 {CharacterId} {id} 0";
        }

        public string GenerateGidx()
        {
            if (Family != null && FamilyCharacter != null)
                return $"gidx 1 {CharacterId} {Family.FamilyId} {Family.Name}({FamilyCharacter.Authority.ToString()}) {Family.FamilyLevel}";
            else
                return $"gidx 1 {CharacterId} -1 - 0";
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
                case 2:
                    return $"guri 2 {argument} {CharacterId}";

                case 10:
                    return $"guri 10 {argument} {value} {CharacterId}";

                case 15:
                    return $"guri 15 {argument} 0 0";

                default:
                    return $"guri {type} {argument} {CharacterId} {value}";
            }
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
            return $"in 1 {Name} - {CharacterId} {MapX} {MapY} {Direction} {(Undercover ? (byte)AuthorityType.User : (byte)Authority)} {(byte)Gender} {(byte)HairStyle} {color} {(byte)Class} {GenerateEqListForPacket()} {Math.Ceiling(Hp / HPLoad() * 100)} {Math.Ceiling(Mp / MPLoad() * 100)} {(IsSitting ? 1 : 0)} {Group?.GroupId ?? -1} {(fairy != null ? 2 : 0)} {fairy?.Item.Element ?? 0} 0 {fairy?.Item.Morph ?? 0} 0 {(UseSp || IsVehicled ? Morph : 0)} {GenerateEqRareUpgradeForPacket()} -1 - {(GetDignityIco() == 1 ? GetReputIco() : -GetDignityIco())} {(Invisible ? 1 : 0)} {(UseSp ? MorphUpgrade : 0)} 0 {(UseSp ? MorphUpgrade2 : 0)} {Level} 0 {ArenaWinner} {Compliment} {Size} {HeroLevel}";
        }

        public List<string> GenerateIn2()
        {
            return ServerManager.GetMap(MapId).Npcs.Select(npc => npc.GenerateIn2()).ToList();
        }

        public List<string> GenerateIn3()
        {
            return ServerManager.GetMap(MapId).Monsters.Select(monster => monster.GenerateIn3()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        public string GenerateInbox(byte type, byte value)
        {
            return $"inbox #glmk^ {value} {(type == 1 ? 0 : 1)} {(type == 0 ? Language.Instance.GetMessageFromKey("CREATE_FAMILY") : Language.Instance.GetMessageFromKey("DISMISS_FAMILY"))}";
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
            return string.Empty;
        }

        public string GenerateInvisible()
        {
            return $"cl {CharacterId} {(Invisible ? 1 : 0)} {(InvisibleGm ? 1 : 0)}";
        }

        public void GenerateKillBonus(MapMonster monsterToAttack)
        {
            if (monsterToAttack == null || monsterToAttack.IsAlive)
            {
                return;
            }

            Random random = new Random(DateTime.Now.Millisecond & monsterToAttack.MapMonsterId);

            // owner set
            long? dropOwner = monsterToAttack.DamageList.Any() ? monsterToAttack.DamageList.First().Key : (long?)null;
            Group group = null;
            if (dropOwner != null)
            {
                group = ServerManager.Instance.Groups.FirstOrDefault(g => g.IsMemberOfGroup((long)dropOwner));
            }

            // end owner set
            if (Session.HasCurrentMap)
            {
                List<DropDTO> droplist = monsterToAttack.Monster.Drops.Where(s => Session.CurrentMap.MapTypes.Any(m => m.MapTypeId == s.MapTypeId) || s.MapTypeId == null).ToList();
                if (monsterToAttack.Monster.MonsterType != MonsterType.Special)
                {
                    #region item drop

                    int dropRate = ServerManager.DropRate;
                    int x = 0;
                    foreach (DropDTO drop in droplist.OrderBy(s => random.Next()))
                    {
                        if (x < 4)
                        {
                            double rndamount = ServerManager.RandomNumber() * random.NextDouble();
                            if (rndamount <= (double)drop.DropChance * dropRate / 5000.000)
                            {
                                x++;
                                if (Session.CurrentMap != null)
                                {
                                    if (Session.CurrentMap.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4) || monsterToAttack.Monster.MonsterType == MonsterType.Elite)
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
                                        if (group != null)
                                        {
                                            if (group.SharingMode == (byte)GroupSharingType.ByOrder)
                                            {
                                                dropOwner = group.GetNextOrderedCharacterId(this);
                                                if (dropOwner.HasValue)
                                                {
                                                    group.Characters.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ITEM_BOUND_TO"), ServerManager.GetItem(drop.ItemVNum).Name, group.Characters.Single(c => c.Character.CharacterId == (long)dropOwner).Character.Name, drop.Amount), 10)));
                                                }
                                            }
                                            else
                                            {
                                                group.Characters.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("DROPPED_ITEM"), ServerManager.GetItem(drop.ItemVNum).Name, drop.Amount), 10)));
                                            }
                                        }

                                        long? owner = dropOwner;
                                        Observable.Timer(TimeSpan.FromMilliseconds(500))
                                       .Subscribe(o =>
                                       {
                                           if (Session.HasCurrentMap)
                                           {
                                               Session.CurrentMap.DropItemByMonster(owner, drop, monsterToAttack.MapX, monsterToAttack.MapY);
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
                    gold = gold > 1000000000 ? 1000000000 : gold;
                    double randChance = ServerManager.RandomNumber() * random.NextDouble();

                    if (gold > 0 && randChance <= (int)(ServerManager.GoldDropRate * 10 * CharacterHelper.GoldPenalty(Level, monsterToAttack.Monster.Level)))
                    {
                        DropDTO drop2 = new DropDTO
                        {
                            Amount = gold,
                            ItemVNum = 1046
                        };
                        if (Session.CurrentMap != null)
                        {
                            if (Session.CurrentMap.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4) || monsterToAttack.Monster.MonsterType == MonsterType.Elite)
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
                                            if (session.Character.Gold > 1000000000)
                                            {
                                                session.Character.Gold = 1000000000;
                                                session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                                            }
                                            session.SendPacket(session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(drop2.ItemVNum).Name} x {drop2.Amount}", 10));
                                            session.SendPacket(session.Character.GenerateGold());
                                        }
                                        alreadyGifted.Add(charId);
                                    }
                                }
                            }
                            else
                            {
                                if (group != null)
                                {
                                    if (group.SharingMode == (byte)GroupSharingType.ByOrder)
                                    {
                                        dropOwner = group.GetNextOrderedCharacterId(this);

                                        if (dropOwner.HasValue)
                                        {
                                            group.Characters.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ITEM_BOUND_TO"), ServerManager.GetItem(drop2.ItemVNum).Name, group.Characters.Single(c => c.Character.CharacterId == (long)dropOwner).Character.Name, drop2.Amount), 10)));
                                        }
                                    }
                                    else
                                    {
                                        group.Characters.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("DROPPED_ITEM"), ServerManager.GetItem(drop2.ItemVNum).Name, drop2.Amount), 10)));
                                    }
                                }

                                // delayed Drop
                                Observable.Timer(TimeSpan.FromMilliseconds(500))
                                      .Subscribe(
                                      o =>
                                      {
                                          if (Session.HasCurrentMap)
                                              Session.CurrentMap.DropItemByMonster(dropOwner, drop2, monsterToAttack.MapX, monsterToAttack.MapY);
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
                            foreach (ClientSession targetSession in grp.Characters.Where(g => g.Character.MapId == MapId))
                            {
                                if (targetSession.Character.Level >= monsterToAttack.Monster.Level - 5 && targetSession.Character.Level <= monsterToAttack.Monster.Level + 5)
                                {
                                    if (!DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.BlockRep && s.DateEnd > DateTime.Now))
                                    {
                                        targetSession.Character.Reput += monsterToAttack.Monster.Level;
                                    }
                                    if (!DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.BlockFExp && s.DateEnd > DateTime.Now))
                                    {
                                        targetSession.Character.CollectedFamilyXp += monsterToAttack.Monster.Level;
                                    }
                                }
                                else if (targetSession.Character.Level >= 90 && monsterToAttack.Monster.Level >= 88)
                                {
                                    if (!DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.BlockRep && s.DateEnd > DateTime.Now))
                                    {
                                        targetSession.Character.Reput += monsterToAttack.Monster.Level;
                                    }
                                    if (!DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.BlockFExp && s.DateEnd > DateTime.Now))
                                    {
                                        targetSession.Character.CollectedFamilyXp += monsterToAttack.Monster.Level;
                                    }
                                }
                                if (grp.IsMemberOfGroup(monsterToAttack.DamageList.FirstOrDefault().Key))
                                {
                                    targetSession.Character.GenerateXp(monsterToAttack.Monster, true);
                                }
                                else
                                {
                                    targetSession.SendPacket(targetSession.Character.GenerateSay(Language.Instance.GetMessageFromKey("XP_NOTFIRSTHIT"), 10));
                                    targetSession.Character.GenerateXp(monsterToAttack.Monster, false);
                                }
                            }
                        }
                        else
                        {
                            if (Level >= monsterToAttack.Monster.Level - 5 && Level <= monsterToAttack.Monster.Level + 5)
                            {
                                if (!DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.BlockRep && s.DateEnd > DateTime.Now))
                                {
                                    Reput += monsterToAttack.Monster.Level;
                                }
                                if (!DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.BlockFExp && s.DateEnd > DateTime.Now))
                                {
                                    CollectedFamilyXp += monsterToAttack.Monster.Level;
                                }
                            }
                            else if (Level >= 90 && monsterToAttack.Monster.Level >= 88)
                            {
                                if (!DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.BlockRep && s.DateEnd > DateTime.Now))
                                {
                                    Reput += monsterToAttack.Monster.Level;
                                }
                                if (!DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.BlockFExp && s.DateEnd > DateTime.Now))
                                {
                                    CollectedFamilyXp += monsterToAttack.Monster.Level;
                                }
                            }
                            if (monsterToAttack.DamageList.FirstOrDefault().Key == CharacterId)
                            {
                                GenerateXp(monsterToAttack.Monster, true);
                            }
                            else
                            {
                                Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("XP_NOTFIRSTHIT"), 10));
                                GenerateXp(monsterToAttack.Monster, false);
                            }
                        }
                        GenerateDignity(monsterToAttack.Monster);
                    }

                    #endregion
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

        //public string GenerateGidx()
        //{
        //    Family family = DAOFactory.FamilyDAO.LoadByCharacterId(CharacterId);
        //    FamilyCharacter familyCharacter = DAOFactory.FamilyCharacterDAO.LoadByCharacterId(CharacterId);
        //    if (family != null && familyCharacter != null)
        //    return $"gidx 1 {CharacterId} {family.FamilyId} {family.Name}({familyCharacter.FamilyAuthority}) 1";
        //    return string.Empty;
        //}
        public string GenerateMsg(string message, int type)
        {
            return $"msg {type} {message}";
        }

        //public string GenerateGExp()
        //{
        //    string str = "gexp";
        //    Family family = DAOFactory.FamilyDAO.LoadByCharacterId(CharacterId);
        //    foreach (FamilyCharacter familyCharacter in family)
        //    {
        //        str += $" {familyCharacter.CharacterId}|{familyCharacter.Experience}";
        //    }
        //    return str;
        //}
        public MovePacket GenerateMv()
        {
            return new MovePacket
            {
                CharacterId = CharacterId,
                MapX = MapX,
                MapY = MapY,
                Speed = Speed,
                MoveType = 1
            };
        }

        //public string GenerateGMsg()
        //{
        //    string str = "gmsg";
        //    Family family = DAOFactory.FamilyDAO.LoadByCharacterId(CharacterId);
        //    foreach (FamilyCharacter familyCharacter in family)
        //    {
        //        str += $" {familyCharacter.CharacterId}|{familyCharacter.DailyMessage}";
        //    }
        //    return str;
        //}
        public List<string> GenerateNPCShopOnMap()
        {
            return (from npc in ServerManager.GetMap(MapId).Npcs where npc.Shop != null select $"shop 2 {npc.MapNpcId} {npc.Shop.ShopId} {npc.Shop.MenuType} {npc.Shop.ShopType} {npc.Shop.Name}").ToList();
        }

        // uncomment when done
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
            return mail.AttachmentVNum != null ? $"parcel 1 1 {MailList.First(s => s.Value.MailId == mail.MailId).Key} {(mail.Title == "NOSMALL" ? 1 : 4)} 0 {mail.Date.ToString("yyMMddHHmm")} {mail.Title} {mail.AttachmentVNum} {mail.AttachmentAmount} {(byte)ServerManager.GetItem((short)mail.AttachmentVNum).Type}" : string.Empty;
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
            Group grp = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(CharacterId));
            if (grp != null)
            {
                string str = $"pinit {grp.CharacterCount}";
                int i = 0;
                foreach (ClientSession groupSessionForId in grp.Characters)
                {
                    i++;
                    str += $" 1|{groupSessionForId.Character.CharacterId}|{i}|{groupSessionForId.Character.Level}|{groupSessionForId.Character.Name}|0|{(byte)groupSessionForId.Character.Gender}|{(byte)groupSessionForId.Character.Class}|{(groupSessionForId.Character.UseSp ? groupSessionForId.Character.Morph : 0)}|{groupSessionForId.Character.HeroLevel}";
                }
                return str;
            }
            return string.Empty;
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
            return $"post 1 {type} {MailList.First(s => s.Value.MailId == mail.MailId).Key} 0 {(mail.IsOpened ? 1 : 0)} {mail.Date.ToString("yyMMddHHmm")} {(type == 2 ? DAOFactory.CharacterDAO.LoadById(mail.ReceiverId).Name : DAOFactory.CharacterDAO.LoadById(mail.SenderId).Name)} {mail.Title}";
        }

        public string GeneratePostMessage(MailDTO mailDTO, byte type)
        {
            CharacterDTO sender = DAOFactory.CharacterDAO.LoadById(mailDTO.SenderId);

            return $"post 5 {type} {MailList.First(s => s.Value == mailDTO).Key} 0 0 {(byte)mailDTO.SenderClass} {(byte)mailDTO.SenderGender} {mailDTO.SenderMorphId} {(byte)mailDTO.SenderHairStyle} {(byte)mailDTO.SenderHairColor} {mailDTO.EqPacket} {sender.Name} {mailDTO.Title} {mailDTO.Message}";
        }

        public string GeneratePslInfo(SpecialistInstance inventoryItem, int type)
        {
            // 1235.3 1237.4 1239.5 <= skills SkillVNum.Grade
            return $"pslinfo {inventoryItem.Item.VNum} {inventoryItem.Item.Element} {inventoryItem.Item.ElementRate} {inventoryItem.Item.LevelJobMinimum} {inventoryItem.Item.Speed} {inventoryItem.Item.FireResistance} {inventoryItem.Item.WaterResistance} {inventoryItem.Item.LightResistance} {inventoryItem.Item.DarkResistance} 0.0 0.0 0.0";
        }

        public string[] GenerateQuicklist()
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

            bool isPVPPrimary = false;
            bool isPVPSecondary = false;
            bool isPVPArmor = false;

            if (weapon?.Item.Name.Contains(": ") == true)
            {
                isPVPPrimary = true;
            }
            if (weapon2?.Item.Name.Contains(": ") == true)
            {
                isPVPSecondary = true;
            }
            if (armor?.Item.Name.Contains(": ") == true)
            {
                isPVPArmor = true;
            }

            // tc_info 0 name 0 0 0 0 -1 - 0 0 0 0 0 0 0 0 0 0 0 wins deaths reput 0 0 0 morph
            // talentwin talentlose capitul rankingpoints arenapoints 0 0 ispvpprimary ispvpsecondary
            // ispvparmor herolvl desc
            return $"tc_info {Level} {Name} {fairy?.Item.Element ?? 0} {ElementRate} {(byte)Class} {(byte)Gender} -1 - {GetReputIco()} {GetDignityIco()} {(weapon != null ? 1 : 0)} {weapon?.Rare ?? 0} {weapon?.Upgrade ?? 0} {(weapon2 != null ? 1 : 0)} {weapon2?.Rare ?? 0} {weapon2?.Upgrade ?? 0} {(armor != null ? 1 : 0)} {armor?.Rare ?? 0} {armor?.Upgrade ?? 0} 0 0 {Reput} {Act4Kill} {Act4Dead} {Act4Points} {(UseSp ? Morph : 0)} {TalentWin} {TalentLose} {TalentSurrender} 0 {MasterPoints} {Compliment} 0 {(isPVPPrimary ? 1 : 0)} {(isPVPSecondary ? 1 : 0)} {(isPVPArmor ? 1 : 0)} {HeroLevel} {(string.IsNullOrEmpty(Biography) ? Language.Instance.GetMessageFromKey("NO_PREZ_MESSAGE") : Biography)}";
        }

        public string GenerateRest()
        {
            return $"rest 1 {CharacterId} {(IsSitting ? 1 : 0)}";
        }

        public string GenerateRevive()
        {
            return $"revive 1 {CharacterId} 0";
        }

        public string GenerateRp(int mapid, int x, int y, string param)
        {
            return $"rp {mapid} {x} {y} {param}";
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

        public string GenerateSlInfo(SpecialistInstance inventoryItem, int type)
        {
            int freepoint = CharacterHelper.SPPoint(inventoryItem.SpLevel, inventoryItem.Upgrade) - inventoryItem.SlDamage - inventoryItem.SlHP - inventoryItem.SlElement - inventoryItem.SlDefence;

            int slElement = CharacterHelper.SlPoint(inventoryItem.SlElement, 2);
            int slHp = CharacterHelper.SlPoint(inventoryItem.SlHP, 3);
            int slDefence = CharacterHelper.SlPoint(inventoryItem.SlDefence, 1);
            int slHit = CharacterHelper.SlPoint(inventoryItem.SlDamage, 0);

            string skill = string.Empty;
            List<CharacterSkill> skillsSp = new List<CharacterSkill>();
            foreach (Skill ski in ServerManager.GetAllSkill().Where(ski => ski.Class == inventoryItem.Item.Morph + 31 && ski.LevelMinimum <= inventoryItem.SpLevel))
            {
                skillsSp.Add(new CharacterSkill { SkillVNum = ski.SkillVNum, CharacterId = CharacterId });
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
            short firstskillvnum = skillsSp[0].SkillVNum;

            for (int i = 1; i < 11; i++)
            {
                if (skillsSp.Count >= i + 1)
                {
                    if (skillsSp[i].SkillVNum <= firstskillvnum + 10)
                        skill += $"{skillsSp[i].SkillVNum}.";
                }
            }

            // 10 9 8 '0 0 0 0'<- bonusdamage bonusarmor bonuselement bonushpmp its after upgrade and
            // 3 first values are not important
            skill = skill.TrimEnd('.');
            return $"slinfo {type} {inventoryItem.ItemVNum} {inventoryItem.Item.Morph} {inventoryItem.SpLevel} {inventoryItem.Item.LevelJobMinimum} {inventoryItem.Item.ReputationMinimum} 0 0 0 0 0 0 0 {inventoryItem.Item.SpType} {inventoryItem.Item.FireResistance} {inventoryItem.Item.WaterResistance} {inventoryItem.Item.LightResistance} {inventoryItem.Item.DarkResistance} {inventoryItem.XP} {CharacterHelper.SPXPData[inventoryItem.SpLevel - 1]} {skill} {inventoryItem.TransportId} {freepoint} {slHit} {slDefence} {slElement} {slHp} {inventoryItem.Upgrade} 0 0 {spdestroyed} 0 0 0 0 {inventoryItem.SpStoneUpgrade} {inventoryItem.SpDamage} {inventoryItem.SpDefence} {inventoryItem.SpElement} {inventoryItem.SpHP} {inventoryItem.SpFire} {inventoryItem.SpWater} {inventoryItem.SpLight} {inventoryItem.SpDark}";
        }

        public string GenerateSpk(object message, int v)
        {
            return $"spk 1 {CharacterId} {v} {Name} {message}";
        }

        public string GenerateSpPoint()
        {
            return $"sp {SpAdditionPoint} 1000000 {SpPoint} 10000";
        }

        [Obsolete("GenerateStartupInventory is deprecated, for refreshing inventory please use GenerateInventoryAdd instead.")]
        public void GenerateStartupInventory()
        {
            string inv0 = "inv 0", inv1 = "inv 1", inv2 = "inv 2", inv3 = "inv 3", inv6 = "inv 6", inv7 = "inv 7"; // inv 3 used for miniland objects
            if (Inventory != null)
            {
                foreach (ItemInstance inv in Inventory.GetAllItems())
                {
                    switch (inv.Type)
                    {
                        case InventoryType.Equipment:
                            if (inv.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                var specialistInstance = inv as SpecialistInstance;
                                if (specialistInstance != null)
                                    inv0 += $" {inv.Slot}.{inv.ItemVNum}.{specialistInstance.Rare}.{specialistInstance.Upgrade}.{specialistInstance.SpStoneUpgrade}";
                            }
                            else
                            {
                                var wearableInstance = inv as WearableInstance;
                                if (wearableInstance != null)
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
                            if (specialist != null)
                                inv6 += $" {inv.Slot}.{inv.ItemVNum}.{specialist.Rare}.{specialist.Upgrade}.{specialist.SpStoneUpgrade}";
                            break;

                        case InventoryType.Costume:
                            var costumeInstance = inv as WearableInstance;
                            if (costumeInstance != null)
                                inv7 += $" {inv.Slot}.{inv.ItemVNum}.{costumeInstance.Rare}.{costumeInstance.Upgrade}.0";
                            break;
                    }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "Readability")]
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
                SpecialistInstance specialist = Inventory?.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                if (specialist != null)
                {
                    MinHit += specialist.DamageMinimum;
                    MaxHit += specialist.DamageMaximum;
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
                    FireResistance += specialist.FireResistance;
                    WaterResistance += specialist.WaterResistance;
                    LightResistance += specialist.LightResistance;
                    DarkResistance += specialist.DarkResistance;
                    ElementRateSP += specialist.ElementRate;
                    Defence += specialist.CloseDefence;
                    DistanceDefence += specialist.DistanceDefence;
                    MagicalDefence += specialist.MagicDefence;

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

            // handle specialist
            if (UseSp)
            {
                SpecialistInstance specialist = Inventory?.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                if (specialist != null)
                {
                    FireResistance += specialist.SpFire;
                    LightResistance += specialist.SpLight;
                    WaterResistance += specialist.SpWater;
                    DarkResistance += specialist.SpDark;

                    Defence += specialist.SpDefence * 10;
                    DistanceDefence += specialist.SpDefence * 10;
                    MagicalDefence += specialist.SpDefence * 10;

                    MinHit += specialist.SpDamage * 10;
                    MaxHit += specialist.SpDamage * 10;

                    ElementRateSP += specialist.SpElement;
                }
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
            return $"st 1 {CharacterId} {Level} {HeroLevel} {(int)(Hp / (float)HPLoad() * 100)} {(int)(Mp / (float)MPLoad() * 100)} {Hp} {Mp}";
        }

        public string GenerateTit()
        {
            return $"tit {Language.Instance.GetMessageFromKey(Class == (byte)ClassType.Adventurer ? ClassType.Adventurer.ToString().ToUpper() : Class == ClassType.Swordman ? ClassType.Swordman.ToString().ToUpper() : Class == ClassType.Archer ? ClassType.Archer.ToString().ToUpper() : ClassType.Magician.ToString().ToUpper())} {Name}";
        }

        public string GenerateTp()
        {
            return $"tp 1 {CharacterId} {MapX} {MapY} 0";
        }

        public string[] GenerateVb()
        {
            return new[] { "vb 340 0 0", "vb 339 0 0", "vb 472 0 0", "vb 471 0 0" };
        }

        public void GenerateXp(NpcMonster monsterinfo, bool isMonsterOwner)
        {
            if (!DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.BlockExp && s.DateEnd > DateTime.Now))
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
                    specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp,
                        InventoryType.Wear);
                }

                if (Level < 99)
                {
                    if (isMonsterOwner)
                    {
                        LevelXp += GetXP(monsterinfo, grp);
                    }
                    else
                    {
                        LevelXp += GetXP(monsterinfo, grp) / 3;
                    }
                }
                if (Class == 0 && JobLevel < 20 || Class != 0 && JobLevel < 80)
                {
                    if (specialist != null && UseSp && specialist.SpLevel < 99 && specialist.SpLevel > 19)
                    {
                        JobLevelXp += GetJXP(monsterinfo, grp) / 2;
                    }
                    else
                    {
                        JobLevelXp += GetJXP(monsterinfo, grp);
                    }
                }
                if (specialist != null && UseSp && specialist.SpLevel < 99)
                {
                    int multiplier = specialist.SpLevel < 10 ? 10 : specialist.SpLevel < 19 ? 5 : 1;
                    specialist.XP += GetJXP(monsterinfo, grp) * multiplier;
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

                WearableInstance fairy = Inventory?.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
                if (fairy != null)
                {
                    if (fairy.ElementRate + fairy.Item.ElementRate < fairy.Item.MaxElementRate &&
                        Level <= monsterinfo.Level + 15 && Level >= monsterinfo.Level - 15)
                    {
                        fairy.XP += ServerManager.FairyXpRate;
                    }
                    t = CharacterHelper.LoadFairyXPData(fairy.ElementRate);
                    while (fairy.XP >= t)
                    {
                        fairy.XP -= (int)t;
                        fairy.ElementRate++;
                        if (fairy.ElementRate + fairy.Item.ElementRate == fairy.Item.MaxElementRate)
                        {
                            fairy.XP = 0;
                            Session.SendPacket(
                                GenerateMsg(
                                    string.Format(Language.Instance.GetMessageFromKey("FAIRYMAX"), fairy.Item.Name), 10));
                        }
                        else
                        {
                            Session.SendPacket(
                                GenerateMsg(
                                    string.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVELUP"), fairy.Item.Name),
                                    10));
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
                    Session.CurrentMap?.Broadcast(GenerateEff(8), MapX, MapY);
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
                        Session.CurrentMap?.Broadcast(GenerateEff(8), MapX, MapY);
                        Session.CurrentMap?.Broadcast(GenerateEff(198), MapX, MapY);
                    }
                }
                Session.SendPacket(GenerateLev());
            }
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
            Dispose();

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

        public int GetJXP(NpcMonster monster, Group group)
        {
            int partySize = 1;
            float partyPenalty = 1f;

            if (group != null)
            {
                int levelSum = group.Characters.Sum(g => g.Character.JobLevel);
                partySize = group.CharacterCount;
                partyPenalty = 12 / partySize / (float)levelSum;
            }

            int jobxp = (int)Math.Round(monster.JobXP * CharacterHelper.ExperiencePenalty(JobLevel, monster.Level) * ServerManager.XPRate);

            // divide jobexp by multiplication of partyPenalty with level e.g. 57 * 0,014...
            if (partySize > 1 && group != null)
            {
                jobxp = (int)Math.Round(jobxp / (JobLevel * partyPenalty));
            }

            return jobxp;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "Readability")]
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
            return Reput <= 5000000 ? 26 : 27;
        }

        public long GetXP(NpcMonster monster, Group group)
        {
            int partySize = 1;
            double partyPenalty = 1d;
            int levelDifference = Level - monster.Level;

            if (group != null)
            {
                int levelSum = group.Characters.Sum(g => g.Character.Level);
                partySize = group.CharacterCount;
                partyPenalty = 12 / partySize / (double)levelSum;
            }

            long xpcalculation = levelDifference < 5 ? monster.XP : monster.XP / 3 * 2;

            long xp = (long)Math.Round(xpcalculation * CharacterHelper.ExperiencePenalty(Level, monster.Level) * ServerManager.XPRate);

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

            if (partySize > 1 && group != null)
            {
                xp = (long)Math.Round(xp / (Level * partyPenalty));
            }

            return xp;
        }

        public void GiftAdd(short itemVNum, byte amount)
        {
            if (Inventory != null)
            {
                lock (Inventory)
                {
                    ItemInstance newItem = Inventory.InstantiateItemInstance(itemVNum, CharacterId, amount);
                    if (newItem != null)
                    {
                        if (newItem.Item.ItemType == ItemType.Armor || newItem.Item.ItemType == ItemType.Weapon || newItem.Item.ItemType == ItemType.Shell)
                        {
                            ((WearableInstance)newItem).RarifyItem(Session, RarifyMode.Drop, RarifyProtection.None);
                        }
                        ItemInstance newInv = Inventory.AddToInventory(newItem);
                        if (newInv != null)
                        {
                            Session.SendPacket(GenerateInventoryAdd(newInv.ItemVNum, newInv.Amount, newInv.Type, newInv.Slot, newInv.Rare, newInv.Design, newInv.Upgrade, 0));
                            Session.SendPacket(GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {newItem.Item.Name} x {amount}", 10));
                        }
                        else
                        {
                            if (MailList.Count <= 40)
                            {
                                SendGift(CharacterId, itemVNum, amount, newItem.Rare, newItem.Upgrade, false);
                                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_ACQUIRED_BY_THE_GIANT_MONSTER"), 0));
                            }
                        }
                    }
                }
            }
        }

        public int HealthHPLoad()
        {
            if (IsSitting)
            {
                return CharacterHelper.HPHealth[(byte)Class];
            }
            else if ((DateTime.Now - LastDefence).TotalSeconds > 2)
            {
                return CharacterHelper.HPHealthStand[(byte)Class];
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
                return CharacterHelper.MPHealth[(byte)Class];
            }
            else if ((DateTime.Now - LastDefence).TotalSeconds > 2)
            {
                return CharacterHelper.MPHealthStand[(byte)Class];
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
                    hp = specialist.HP + specialist.SpHP * 100;
                }
            }
            return (int)((CharacterHelper.HPData[(byte)Class, Level] + hp) * multiplicator);
        }

        public override void Initialize()
        {
            _random = new Random();
            ExchangeInfo = null;
            SpCooldown = 30;
            SaveX = 0;
            SaveY = 0;
            LastDefence = DateTime.Now.AddSeconds(-21);
            LastHealth = DateTime.Now;
            LastEffect = DateTime.Now;
            Session = null;
            MailList = new Dictionary<int, MailDTO>();
            LastMailRefresh = DateTime.Now;
            Group = null;
            GmPvtBlock = false;
        }

        public bool IsBlockedByCharacter(long characterId)
        {
            ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
            if (otherSession != null)
            {
                return otherSession.Character.IsBlockingCharacter(CharacterId);
            }
            return DAOFactory.CharacterRelationDAO.GetBlacklisted(characterId).FirstOrDefault(b => b.RelatedCharacterId.Equals(CharacterId)) != null;
        }

        public bool IsBlockingCharacter(long characterId)
        {
            return _blacklisted != null && _blacklisted.Any(c => c.RelatedCharacterId.Equals(characterId));
        }

        public bool IsFriendlistFull()
        {
            return _friends?.Count >= 80;
        }

        public bool IsFriendOfCharacter(long characterId)
        {
            return _friends?.FirstOrDefault(c => c.RelatedCharacterId.Equals(characterId)) != null;
        }

        /// <summary>
        /// Checks if the current character is in range of the given position
        /// </summary>
        /// <param name="xCoordinate">The x coordinate of the object to check.</param>
        /// <param name="yCoordinate">The y coordinate of the object to check.</param>
        /// <returns>True if the object is in Range, False if not.</returns>
        public bool IsInRange(int xCoordinate, int yCoordinate)
        {
            return Math.Abs(MapX - xCoordinate) <= 50 && Math.Abs(MapY - yCoordinate) <= 50;
        }

        /// <summary>
        /// Checks if the current character is in range of the given position
        /// </summary>
        /// <param name="xCoordinate">The x coordinate of the object to check.</param>
        /// <param name="yCoordinate">The y coordinate of the object to check.</param>
        /// <param name="range">The range of the coordinates to be maximal distanced.</param>
        /// <returns>True if the object is in Range, False if not.</returns>
        public bool IsInRange(int xCoordinate, int yCoordinate, int range)
        {
            return Math.Abs(MapX - xCoordinate) <= range && Math.Abs(MapY - yCoordinate) <= range;
        }

        public bool IsMuted()
        {
            return DAOFactory.PenaltyLogDAO.LoadByAccount(AccountId).Any(s => s.Penalty == PenaltyType.Muted && s.DateEnd > DateTime.Now);
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
                        if (Skills.GetAllItems().All(s => s.SkillVNum != i))
                        {
                            NewSkill = 1;
                            Skills[i] = new CharacterSkill { SkillVNum = (short)i, CharacterId = CharacterId };
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
            SpecialistInstance specialist = null;
            if (Inventory != null)
            {
                specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            byte SkillSpCount = (byte)SkillsSp.Count;
            SkillsSp = new ThreadSafeSortedList<int, CharacterSkill>();
            foreach (Skill ski in ServerManager.GetAllSkill())
            {
                if (specialist != null && ski.Class == Morph + 31 && specialist.SpLevel >= ski.LevelMinimum)
                {
                    SkillsSp[ski.SkillVNum] = new CharacterSkill { SkillVNum = ski.SkillVNum, CharacterId = CharacterId };
                }
            }
            if (SkillsSp.Count != SkillSpCount)
            {
                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                Session.SendPacket(GenerateSki());
                Session.SendPackets(GenerateQuicklist());
            }
        }

        public void LoadInventory()
        {
            IEnumerable<ItemInstanceDTO> inventories = DAOFactory.IteminstanceDao.LoadByCharacterId(CharacterId).ToList();

            Inventory = new Inventory(this);
            Inventory = new Inventory(this);
            foreach (ItemInstanceDTO inventory in inventories)
            {
                inventory.CharacterId = CharacterId;

                if (inventory.Type != InventoryType.Wear)
                {
                    Inventory[inventory.Id] = (ItemInstance)inventory;
                }
                else
                {
                    Inventory[inventory.Id] = (ItemInstance)inventory;
                }
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
            foreach (MailDTO mail in ServerManager.Mails.Where(s => s.SenderId == CharacterId && s.IsSenderCopy && MailList.All(m => m.Value.MailId != s.MailId)))
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
            return (int)((CharacterHelper.MPData[(byte)Class, Level] + mp) * multiplicator);
        }

        public void NotifyRarifyResult(sbyte rare)
        {
            Session.SendPacket(GenerateSay(string.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), rare), 12));
            Session.SendPacket(GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), rare), 0));
            ServerManager.GetMap(MapId).Broadcast(GenerateEff(3005), MapX, MapY);
            Session.SendPacket("shop_end 1");
        }

        public void RefreshMail()
        {
            int i = 0;
            int j = 0;
            try
            {
                List<MailDTO> mails = ServerManager.Mails.Where(s => s.ReceiverId == CharacterId && !s.IsSenderCopy && MailList.All(m => m.Value.MailId != s.MailId)).Take(50).ToList();
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
            LastMailRefresh = DateTime.Now;
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
            Session.CurrentMap?.Broadcast(GenerateCMode());
            Session.SendPacket(GenerateCond());
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
                CharacterDTO character = DeepCopy();
                SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref character); // unused variable, check for success?

                // wait for any exchange to be finished
                while (IsExchanging)
                {
                    // do nothing and wait until Exchange has been finished
                }

                if (Inventory != null)
                {
                    // be sure that noone tries to edit while saving is currently editing
                    lock (Inventory)
                    {
                        // load and concat inventory with equipment
                        List<ItemInstance> inventories = Inventory.GetAllItems();
                        IList<Guid> currentlySavedInventoryIds = DAOFactory.IteminstanceDao.LoadSlotAndTypeByCharacterId(CharacterId);

                        // remove all which are saved but not in our current enumerable
                        foreach (var inventoryToDeleteId in currentlySavedInventoryIds.Except(inventories.Select(i => i.Id)))
                        {
                            DAOFactory.IteminstanceDao.Delete(inventoryToDeleteId);
                        }

                        // create or update all which are new or do still exist
                        foreach (ItemInstance itemInstance in inventories)
                        {
                            DAOFactory.IteminstanceDao.InsertOrUpdate(itemInstance);
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

                foreach (StaticBonusDTO bonus in Session.Character.StaticBonusList)
                {
                    DAOFactory.StaticBonusDAO.InsertOrUpdate(bonus);
                }

                DAOFactory.StaticBonusDAO.RemoveOutDated();

                foreach (GeneralLogDTO general in Session.Account.GeneralLogs)
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

                foreach (BazaarItem bz in ServerManager.Instance.BazarItemList.Where(s => s.SellerId == CharacterId))
                {
                    if (DAOFactory.BazaarItemDAO.LoadById(bz.BazaarItemId) == null)
                    {
                        BazaarItemDTO bzsave = bz;
                        DAOFactory.BazaarItemDAO.InsertOrUpdate(ref bzsave);
                    }
                }

                if ((CollectedFamilyXp != 0 || _familymessagechanged) && Family != null && FamilyCharacter != null)
                {
                    string msg = Family.FamilyMessage;
                    FamilyDTO _family = DAOFactory.FamilyDAO.LoadById(Family.FamilyId);
                    FamilyCharacterDTO _familyCharacter = DAOFactory.FamilyCharacterDAO.LoadById(FamilyCharacter.FamilyCharacterId);
                    _family.FamilyExperience += CollectedFamilyXp;
                    _familyCharacter.Experience += CollectedFamilyXp;

                    CollectedFamilyXp = 0;

                    if (CharacterHelper.LoadFamilyXPData(_family.FamilyLevel) <= _family.FamilyExperience)
                    {
                        _family.FamilyExperience -= CharacterHelper.LoadFamilyXPData(_family.FamilyLevel);
                        _family.FamilyLevel += 1;
                    }

                    _family.FamilyMessage = msg;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref _familyCharacter);
                    DAOFactory.FamilyDAO.InsertOrUpdate(ref _family);
                    _familymessagechanged = false;
                    Family = _family;
                    FamilyCharacter = _familyCharacter;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Save Character failed. SessionId: " + Session.SessionId, e);
            }
        }

        public void SendGift(long id, short vnum, byte amount, sbyte rare, byte upgrade, bool isNosmall)
        {
            Item it = ServerManager.GetItem(vnum);

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
                    Title = isNosmall ? "NOSMALL" : "NOSTALE",
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
            if (Session.HasCurrentMap && Session.CurrentMap.MapTypes.Any())
            {
                long? respawnmaptype = Session.CurrentMap.MapTypes.ElementAt(0).RespawnMapTypeId;
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
            if (Session.HasCurrentMap && Session.CurrentMap.MapTypes.Any())
            {
                long? respawnmaptype = Session.CurrentMap.MapTypes.ElementAt(0).ReturnMapTypeId;
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
                        resp.X = MapX;
                        resp.Y = MapY;
                        resp.MapId = MapId;
                    }
                }
            }
        }

        public double SPXPLoad()
        {
            SpecialistInstance specialist = null;
            if (Inventory != null)
            {
                specialist = Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            return specialist != null ? CharacterHelper.SPXPData[specialist.SpLevel - 1] : 0;
        }

        public bool Update()
        {
            try
            {
                CharacterDTO characterToUpdate = this;
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
                                        Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_ADVENTURER"), 10));
                                        return false;
                                    }
                                    Inventory.RemoveItemAmount(2081);
                                    wearable.Ammo = 100;
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_ADVENTURER"), 10));
                                    return true;
                                }
                                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
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
                                        Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_SWORDSMAN"), 10));
                                        return false;
                                    }

                                    Inventory.RemoveItemAmount(2082);
                                    inv.Ammo = 100;
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_SWORDSMAN"), 10));
                                    return true;
                                }
                                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
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
                                        Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_ARCHER"), 10));
                                        return false;
                                    }

                                    Inventory.RemoveItemAmount(2083);
                                    inv.Ammo = 100;
                                    Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_ARCHER"), 10));
                                    return true;
                                }
                                Session.SendPacket(GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
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

        public double XPLoad()
        {
            return CharacterHelper.XPData[Level - 1];
        }

        internal void RefreshValidity()
        {
            if (Session.Character.StaticBonusList.RemoveAll(s => s.StaticBonusType == StaticBonusType.BackPack && s.DateEnd < DateTime.Now) > 0)
            {
                Session.SendPacket(GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                Session.SendPacket(Session.Character.GenerateExts());
            }

            if (Session.Character.StaticBonusList.RemoveAll(s => s.DateEnd < DateTime.Now) > 0)
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
                            Session.CurrentMap?.Broadcast(GenerateEq());
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

        private static object HeroXPLoad()
        {
            return 949560;
        }

        private int GetGold(MapMonster mapMonster)
        {
            int lowBaseGold = ServerManager.RandomNumber(6 * mapMonster.Monster?.Level ?? 1, 12 * mapMonster.Monster?.Level ?? 1);
            int actMultiplier = Session?.CurrentMap?.MapTypes?.Any(s => s.MapTypeId == (short)MapTypeEnum.Act52) ?? false ? 10 : 1;
            int gold = lowBaseGold * ServerManager.GoldRate * actMultiplier;
            return gold;
        }

        #endregion
    }
}