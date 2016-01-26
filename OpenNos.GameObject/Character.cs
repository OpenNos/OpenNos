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
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenNos.Core.Communication.Scs.Communication.Messages;

namespace OpenNos.GameObject
{
    public class Character : CharacterDTO, IGameObject
    {
        #region Members

        private int _lastPulse;
        private double _lastPortal;
        private int _size = 10;
        private int _morph;
        private int _authority;
        private int _invisible;
        private int _speed;
        private int _arenaWinner;
        private int _morphUpgrade;
        private int _morphUpgrade2;
        private int _direction;
        private int _isDancing;
        private int _rested;
        private int _backpack;

        private InventoryList _inventorylist;
        private InventoryList _equipmentlist;

        #endregion 

        #region Instantiation

        public Character()
        {
            Mapper.CreateMap<CharacterDTO, Character>();
            Mapper.CreateMap<Character, CharacterDTO>();

        }

        #endregion

        #region Properties
        public ExchangeInfo ExchangeInfo { get; set; }
        public InventoryList InventoryList
        {
            get { return _inventorylist; }
            set
            {
                _inventorylist = value;

            }
        }
        public InventoryList EquipmentList
        {
            get { return _equipmentlist; }
            set
            {
                _equipmentlist = value;

            }
        }

        public Thread ThreadCharChange
        {
            get; set;
        }
        public int LastPulse
        {
            get { return _lastPulse; }
            set
            {
                _lastPulse = value;

            }
        }
        public double LastSp
        {
            get;
            set;
        }

        public double LastPortal
        {
            get { return _lastPortal; }
            set
            {
                _lastPortal = value;

            }
        }

        public int Morph
        {
            get { return _morph; }
            set
            {
                _morph = value;

            }
        }

        public int Authority
        {
            get { return _authority; }
            set
            {
                _authority = value;

            }
        }

        public int Invisible
        {
            get { return _invisible; }
            set
            {
                _invisible = value;

            }
        }

        public bool UseSp
        {
            get;
            set;
        }


        public int Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;

            }
        }
        public int LastSpeed
        {
            get;
            set;
        }
        public int ArenaWinner
        {
            get { return _arenaWinner; }
            set
            {
                _arenaWinner = value;

            }
        }

        public int MorphUpgrade
        {
            get { return _morphUpgrade; }
            set
            {
                _morphUpgrade = value;

            }
        }
        public int Size
        {
            get { return _size; }
            set
            {
                _size = value;

            }
        }
        public int MorphUpgrade2
        {
            get { return _morphUpgrade2; }
            set
            {
                _morphUpgrade2 = value;

            }
        }

        public int Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;

            }
        }

        public int IsDancing
        {
            get { return _isDancing; }
            set
            {
                _isDancing = value;

            }
        }
        public int BackPack
        {
            get { return _backpack; }
            set
            {
                _backpack = value;

            }
        }
        public int Rested
        {
            get { return _rested; }
            set
            {
                _rested = value;

            }
        }

        public bool IsVehicled { get; set; }

        #endregion

        #region Methods

        public String GenerateGold()
        {
            return String.Format("gold {0}", Gold);
        }

        public void LoadInventory()
        {

            IEnumerable<InventoryDTO> inventorysDTO = DAOFactory.InventoryDAO.LoadByCharacterId(CharacterId);

            InventoryList = new InventoryList();
            EquipmentList = new InventoryList();
            foreach (InventoryDTO inventory in inventorysDTO)
            {
                InventoryItemDTO inventoryItemDTO = DAOFactory.InventoryItemDAO.LoadById(inventory.InventoryItemId);
                Item item = ServerManager.GetItem(inventoryItemDTO.ItemVNum);

                if (inventory.Type != (short)InventoryType.Equipment)
                    InventoryList.Inventory.Add(new GameObject.Inventory()
                    {
                        CharacterId = inventory.CharacterId,
                        Slot = inventory.Slot,
                        InventoryId = inventory.InventoryId,
                        Type = inventory.Type,
                        InventoryItemId = inventory.InventoryItemId,
                        InventoryItem = new InventoryItem
                        {
                            Amount = inventoryItemDTO.Amount,
                            ElementRate = inventoryItemDTO.ElementRate
                            ,
                            HitRate = inventoryItemDTO.HitRate,
                            Color = inventoryItemDTO.Color,
                            Concentrate = inventoryItemDTO.Concentrate,
                            CriticalLuckRate = inventoryItemDTO.CriticalLuckRate,
                            CriticalRate = inventoryItemDTO.CriticalRate,
                            DamageMaximum = inventoryItemDTO.DamageMaximum,
                            DamageMinimum = inventoryItemDTO.DamageMinimum,
                            DarkElement = inventoryItemDTO.DarkElement,
                            DistanceDefence = inventoryItemDTO.DistanceDefence,
                            DistanceDefenceDodge = inventoryItemDTO.DistanceDefenceDodge,
                            DefenceDodge = inventoryItemDTO.DefenceDodge,
                            FireElement = inventoryItemDTO.FireElement,
                            InventoryItemId = inventoryItemDTO.InventoryItemId,
                            ItemVNum = inventoryItemDTO.ItemVNum,
                            LightElement = inventoryItemDTO.LightElement,
                            MagicDefence = inventoryItemDTO.MagicDefence,
                            RangeDefence = inventoryItemDTO.RangeDefence,
                            Rare = inventoryItemDTO.Rare,
                            SpXp = inventoryItemDTO.SpXp,
                            SpLevel = inventoryItemDTO.SpLevel,
                            SlDefence = inventoryItemDTO.SlDefence,
                            SlElement = inventoryItemDTO.SlElement,
                            SlHit = inventoryItemDTO.SlHit,
                            SlHP = inventoryItemDTO.SlHP,
                            Upgrade = inventoryItemDTO.Upgrade,
                            WaterElement = inventoryItemDTO.WaterElement,


                        }
                    });
                else
                    EquipmentList.Inventory.Add(new GameObject.Inventory()
                    {
                        CharacterId = inventory.CharacterId,
                        Slot = inventory.Slot,
                        InventoryId = inventory.InventoryId,
                        Type = inventory.Type,
                        InventoryItemId = inventory.InventoryItemId,

                    });
            }
        }

        public List<String> GenerateStartupInventory()
        {
            List<String> inventoriesStringPacket = new List<String>();
            String inv0 = "inv 0", inv1 = "inv 1", inv2 = "inv 2", inv6 = "inv 6", inv7 = "inv 7";

            foreach (Inventory inv in InventoryList.Inventory)
            {
                Item item = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                switch (inv.Type)
                {
                    case (short)InventoryType.Costume:
                        inv7 += String.Format(" {0}.{1}.{2}.{3}", inv.Slot, inv.InventoryItem.ItemVNum, inv.InventoryItem.Rare, inv.InventoryItem.Upgrade);
                        break;
                    case (short)InventoryType.Wear:
                        inv0 += String.Format(" {0}.{1}.{2}.{3}", inv.Slot, inv.InventoryItem.ItemVNum, inv.InventoryItem.Rare, item.Colored ? inv.InventoryItem.Color : inv.InventoryItem.Upgrade);
                        break;
                    case (short)InventoryType.Main:
                        inv1 += String.Format(" {0}.{1}.{2}.0", inv.Slot, inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount);
                        break;
                    case (short)InventoryType.Etc:
                        inv2 += String.Format(" {0}.{1}.{2}.0", inv.Slot, inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount);
                        break;
                    case (short)InventoryType.Sp:
                        inv6 += String.Format(" {0}.{1}.{2}.{3}", inv.Slot, inv.InventoryItem.ItemVNum, inv.InventoryItem.Rare, inv.InventoryItem.Upgrade);
                        break;
                    case (short)InventoryType.Equipment:
                        break;
                }

            }
            inventoriesStringPacket.Add(inv0 as String);
            inventoriesStringPacket.Add(inv1 as String);
            inventoriesStringPacket.Add(inv2 as String);
            inventoriesStringPacket.Add(inv6 as String);
            inventoriesStringPacket.Add(inv7 as String);
            return inventoriesStringPacket;
        }

        public bool Update()
        {
            try
            {
                CharacterDTO characterToUpdate = Mapper.Map<CharacterDTO>(this);
                DAOFactory.CharacterDAO.InsertOrUpdate(ref characterToUpdate);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public string GenerateEff(int effectid)
        {
            return String.Format("eff 1 {0} {1}", CharacterId, effectid);
        }
        public List<string> GenerateDroppedItem()
        {
            List<String> droplist = new List<String>();
            foreach (KeyValuePair<long, MapItem> item in ServerManager.GetMap(this.MapId).DroppedList)
                droplist.Add(String.Format("drop {0} {1} {2} {3} {4} {5} {6}", item.Value.ItemVNum, item.Key, item.Value.PositionX, item.Value.PositionY, item.Value.Amount, 0, -1));
            return droplist;
        }
        public List<string> GenerateNPCShopOnMap()
        {
            List<String> droplist = new List<String>();
            foreach (Npc npc in ServerManager.GetMap(this.MapId).Npcs)
                if (npc.Shop != null)
                    droplist.Add(String.Format("shop 2 {0} {1} {2} {3} {4}", npc.NpcId, 1, 0, npc.Shop.MenuType, npc.Shop.Name));
            return droplist;
        }
        public List<string> GenerateShopOnMap()
        {
            List<String> droplist = new List<String>();
            foreach (KeyValuePair<long, MapShop> shop in ServerManager.GetMap(this.MapId).ShopUserList)
                droplist.Add(String.Format("shop 1 {0} 1 3 0 {1}", shop.Key + 1, shop.Value.Name));
            return droplist;
        }

        public List<String> GenerateGp()
        {
            List<String> gpList = new List<String>();
            foreach (Portal portal in ServerManager.GetMap(this.MapId).Portals)
                gpList.Add(String.Format("gp {0} {1} {2} {3} {4}", portal.SourceX, portal.SourceY, portal.DestinationMapId, portal.Type, 0));
            return gpList;
        }

        public List<String> Generatein2()
        {
            List<String> in2List = new List<String>();
            foreach (Npc npc in ServerManager.GetMap(this.MapId).Npcs)
                in2List.Add(String.Format("in 2 {0} {1} {2} {3} {4} 100 100 9632 0 0 - 1 1 0 - 1 - 0 - 1 0 0 0 0 0 0 0 0", npc.Vnum, npc.NpcId, npc.MapX, npc.MapY, npc.Position));
            return in2List;
        }

        public string GenerateFd()
        {
            return String.Format("fd {0} {1} {2} {3}", Reput, GetReputIco(), Dignite, Math.Abs(GetDigniteIco()));
        }

        public string GenerateMapOut()
        {
            return String.Format("mapout");
        }

        public string GenerateOut()
        {
            return String.Format("out 1 {0}", CharacterId);
        }

        public double HPLoad()
        {
            return ServersData.HPData[Class, Level];
        }

        public double MPLoad()
        {
            return ServersData.MPData[Class, Level];
        }

        public double SPXPLoad()
        {
            return ServersData.SpXPData[JobLevel - 1];
        }

        public double XPLoad()
        {
            return ServersData.XPData[Level - 1];
        }

        public int HealthHPLoad()
        {
            if (_rested == 1)
                return ServersData.HpHealth[Class];
            else
                return ServersData.HpHealthStand[Class];
        }

        public int HealthMPLoad()
        {
            if (_rested == 1)
                return ServersData.MpHealth[Class];
            else
                return ServersData.MpHealthStand[Class];
        }

        public double JobXPLoad()
        {
            if (Class == (byte)ClassType.Adventurer)
                return ServersData.FirstJobXPData[JobLevel - 1];
            else
                return ServersData.SecondJobXPData[JobLevel - 1];
        }

        public string GenerateLev()
        {
            Inventory specialist = EquipmentList.LoadBySlotAndType((short)EquipmentType.Sp, (short)InventoryType.Equipment);

            return String.Format("lev {0} {1} {2} {3} {4} {5} {6} 2", Level, LevelXp, !UseSp || specialist == null ? JobLevel : specialist.InventoryItem.SpLevel, !UseSp || specialist == null ? JobLevelXp : specialist.InventoryItem.SpXp, XPLoad(), JobXPLoad(), Reput);
        }

        public string GenerateCInfo()
        {
            return String.Format("c_info {0} - {1} {2} - {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} ", Name, -1, -1, CharacterId, Authority, Gender, HairStyle, HairColor, Class, GetReputIco(), Compliment, UseSp || IsVehicled ? Morph : 0, Invisible, 0, UseSp ? MorphUpgrade : 0, ArenaWinner);
        }

        public int GetDigniteIco()
        {
            int icoDignite = 1;
            if (Convert.ToInt32(Dignite) <= -100)
                icoDignite = 2;
            if (Convert.ToInt32(Dignite) <= -200)
                icoDignite = 3;
            if (Convert.ToInt32(Dignite) <= -400)
                icoDignite = 4;
            if (Convert.ToInt32(Dignite) <= -600)
                icoDignite = 5;
            if (Convert.ToInt32(Dignite) <= -800)
                icoDignite = 6;

            return icoDignite;
        }

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
                    default:
                        break;
                }
            }
            return Reput <= 50 ? 1 : Reput <= 150 ? 2 : Reput <= 250 ? 3 : Reput <= 500 ? 4 : Reput <= 750 ? 5 : Reput <= 1000 ? 6 : Reput <= 2250 ? 7 :
                    Reput <= 3500 ? 8 : Reput <= 5000 ? 9 : Reput <= 9500 ? 10 : Reput <= 19000 ? 11 : Reput <= 25000 ? 12 : Reput <= 40000 ? 13 : Reput <= 60000 ? 14 :
                    Reput <= 85000 ? 15 : Reput <= 115000 ? 16 : Reput <= 150000 ? 17 : Reput <= 190000 ? 18 : Reput <= 235000 ? 19 : Reput <= 285000 ? 20 : Reput <= 350000 ? 21 :
                    Reput <= 500000 ? 22 : Reput <= 1500000 ? 23 : Reput <= 2500000 ? 24 : Reput <= 3750000 ? 25 : Reput <= 5000000 ? 26 : 27;
        }

        public string GenerateTit()
        {
            return String.Format("tit {0} {1}", Language.Instance.GetMessageFromKey(Class == (byte)ClassType.Adventurer ? ClassType.Adventurer.ToString().ToUpper() : Class == (byte)ClassType.Swordman ? ClassType.Swordman.ToString().ToUpper() : Class == (byte)ClassType.Archer ? ClassType.Archer.ToString().ToUpper() : ClassType.Magician.ToString().ToUpper()), Name);
        }

        public string GenerateStat()
        {
            return String.Format("stat {0} {1} {2} {3} 0 1024", Hp, HPLoad(), Mp, MPLoad());
        }

        public string GenerateAt()
        {
            return String.Format("at {0} {1} {2} {3} 2 0 0 1", CharacterId, MapId, MapX, MapY);
        }
        public string GenerateReqInfo()
        {
            Inventory Fairy = EquipmentList.LoadBySlotAndType((short)EquipmentType.Fairy, (short)InventoryType.Equipment);
            Inventory Armor = EquipmentList.LoadBySlotAndType((short)EquipmentType.Armor, (short)InventoryType.Equipment);
            Inventory Weapon2 = EquipmentList.LoadBySlotAndType((short)EquipmentType.SecondaryWeapon, (short)InventoryType.Equipment);
            Inventory Weapon = EquipmentList.LoadBySlotAndType((short)EquipmentType.MainWeapon, (short)InventoryType.Equipment);
            return String.Format("tc_info {0} {1} {2} {3} {4} {9} 0 {8} {5} {6} {10} {11} {12} {13} {14} {15} {16} {17} {18} 0 0 {20} 0 0 0 {19} 0 0 0 0 0 {21} 0 0 0 0 {7}", Level, Name, Fairy != null ? ServerManager.GetItem(Fairy.InventoryItem.ItemVNum).Element : 0, Fairy != null ? Fairy.InventoryItem.ElementRate : 0, Class, GetReputIco(), GetDigniteIco(),
                Language.Instance.GetMessageFromKey("NO_PREZ_MESSAGE"), Language.Instance.GetMessageFromKey("NO_FAMILY"),
                Gender, Weapon != null ? 1 : 0, Weapon != null ? Weapon.InventoryItem.Rare : 0, Weapon != null ? Weapon.InventoryItem.Upgrade : 0,
                Weapon2 != null ? 1 : 0, Weapon2 != null ? Weapon2.InventoryItem.Rare : 0, Weapon2 != null ? Weapon2.InventoryItem.Upgrade : 0,
                Armor != null ? 1 : 0, Armor != null ? Armor.InventoryItem.Rare : 0, Armor != null ? Armor.InventoryItem.Upgrade : 0, UseSp ? Morph : 0, Reput, Compliment);
        }

        public string GenerateCMap()
        {
            return String.Format("c_map 0 {0} 1", MapId);
        }

        public string GenerateCond()
        {
            return String.Format("cond 1 {0} 0 0 {1}", CharacterId, Speed);
        }

        public string GenerateExts()
        {
            return String.Format("exts 0 {0} {0} {0}", 48 + BackPack * 12);

        }

        public string GenerateMv(int x, int y)
        {
            return String.Format("mv 1 {0} {1} {2} {3}", CharacterId, x, y, Speed);
        }

        public string GenerateCMode()
        {
            return String.Format("c_mode 1 {0} {1} {2} {3} {4}", CharacterId, UseSp || IsVehicled ? Morph : 0, UseSp ? MorphUpgrade : 0, UseSp ? MorphUpgrade2 : 0, ArenaWinner);
        }

        public string GenerateSay(string message, int type)
        {
            return String.Format("say 1 {0} {1} {2}", CharacterId, type, message);
        }

        public string GenerateIn()
        {
            int Color = HairColor;
            Inventory head = EquipmentList.LoadBySlotAndType((short)EquipmentType.Hat, (short)InventoryType.Equipment);
            if (head != null && ServerManager.GetItem(head.InventoryItem.ItemVNum).Colored)
                Color = head.InventoryItem.Color;
            Inventory fairy = EquipmentList.LoadBySlotAndType((short)EquipmentType.Fairy, (short)InventoryType.Equipment);


            return String.Format("in 1 {0} - {1} {2} {3} {4} {5} {6} {7} {8} {9} {17} {10} {11} {12} -1 {25} {22} 0 {23} 0 {19} {18} -1 - {13} {16} {20} 0 {21} {14} 0 {15} {26} {24}", Name, CharacterId, MapX, MapY, Direction, (Authority == 2 ? 2 : 0), Gender, HairStyle, Color, Class, (int)((Hp / HPLoad()) * 100), (int)((Mp / MPLoad()) * 100), _rested, (GetDigniteIco() == 1) ? GetReputIco() : -GetDigniteIco(), 0, ArenaWinner, _invisible, generateEqListForPacket(), generateEqRareUpgradeForPacket(), (UseSp ? Morph : 0), (UseSp ? MorphUpgrade : 0), (UseSp ? MorphUpgrade2 : 0), fairy != null ? ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element : 0, fairy != null ? ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Morph : 0, Size, fairy != null ? 2 : 0, Compliment);
        }

        public string GenerateRest()
        {
            return String.Format("rest 1 {0} {1}", CharacterId, Rested);
        }

        public string GenerateDir()
        {
            return String.Format("dir 1 {0} {1}", CharacterId, Direction);
        }

        public string GenerateMsg(string message, int v)
        {
            return String.Format("msg {0} {1}", v, message);
        }

        public string GenerateEquipment()
        {
            //equip 86 0 0.4903.6.8.0 2.340.0.0.0 3.4931.0.5.0 4.4845.3.5.0 5.4912.7.9.0 6.4848.1.0.0 7.4849.3.0.0 8.4850.2.0.0 9.227.0.0.0 10.281.0.0.0 11.347.0.0.0 13.4150.0.0.0 14.4076.0.0.0
            string eqlist = String.Empty;
            short WeaponRare = 0;
            short WeaponUpgrade = 0;
            short ArmorRare = 0;
            short ArmorUpgrade = 0;

            for (short i = 0; i < 15; i++)
            {
                Inventory inv = EquipmentList.LoadBySlotAndType(i, (short)InventoryType.Equipment);
                if (inv != null)
                {
                    Item iteminfo = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                    if (iteminfo.EquipmentSlot == (short)EquipmentType.Armor)
                    {
                        ArmorRare = inv.InventoryItem.Rare;
                        ArmorUpgrade = inv.InventoryItem.Upgrade;
                    }
                    else if (iteminfo.EquipmentSlot == (short)EquipmentType.MainWeapon)
                    {
                        WeaponRare = inv.InventoryItem.Rare;
                        WeaponUpgrade = inv.InventoryItem.Upgrade;
                    }
                    eqlist += String.Format(" {0}.{1}.{2}.{3}.{4}", i, iteminfo.VNum, inv.InventoryItem.Rare, iteminfo.Colored ? inv.InventoryItem.Color : inv.InventoryItem.Upgrade, 0);
                }
            }
            return String.Format("equip {0}{1} {2}{3}{4}", WeaponUpgrade, WeaponRare, ArmorUpgrade, ArmorRare, eqlist);
        }

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

            int WeaponUpgrade = 0;
            int MinHit = 0;
            int MaxHit = 0;
            int HitRate = 0;
            int HitCCRate = 0;
            int HitCC = 0;
            int SecondaryUpgrade = 0;
            int MinDist = 0;
            int MaxDist = 0;
            int DistRate = 0;
            int DistCCRate = 0;
            int DistCC = 0;
            int ArmorUpgrade = 0;
            int def = 0;
            int defrate = 0;
            int distdefrate = 0;
            int distdef = 0;
            int magic = 0;
            int fire = 0;
            int water = 0;
            int light = 0;
            int dark = 0;
            //TODO add base stats
            Inventory weapon = EquipmentList.LoadBySlotAndType((short)EquipmentType.MainWeapon, (short)InventoryType.Equipment);
            if (weapon != null)
            {
                Item iteminfo = ServerManager.GetItem(weapon.InventoryItem.ItemVNum);
                WeaponUpgrade = weapon.InventoryItem.Upgrade;
                MinHit += weapon.InventoryItem.DamageMinimum + iteminfo.DamageMinimum;
                MaxHit += weapon.InventoryItem.DamageMaximum + iteminfo.DamageMaximum;
                HitRate += weapon.InventoryItem.HitRate + iteminfo.HitRate;
                HitCCRate += weapon.InventoryItem.CriticalLuckRate + iteminfo.CriticalLuckRate;
                HitCC += weapon.InventoryItem.CriticalRate + iteminfo.CriticalRate;
                //maxhp-mp
            }

            Inventory weapon2 = EquipmentList.LoadBySlotAndType((short)EquipmentType.SecondaryWeapon, (short)InventoryType.Equipment);
            if (weapon2 != null)
            {
                Item iteminfo = ServerManager.GetItem(weapon2.InventoryItem.ItemVNum);
                SecondaryUpgrade = weapon2.InventoryItem.Upgrade;
                MinDist += weapon2.InventoryItem.DamageMinimum + iteminfo.DamageMinimum;
                MaxDist += weapon2.InventoryItem.DamageMaximum + iteminfo.DamageMaximum;
                DistRate += weapon2.InventoryItem.HitRate + iteminfo.HitRate;
                DistCCRate += weapon2.InventoryItem.CriticalLuckRate + iteminfo.CriticalLuckRate;
                DistCC += weapon2.InventoryItem.CriticalRate + iteminfo.CriticalRate;
                //maxhp-mp
            }

            Inventory armor = EquipmentList.LoadBySlotAndType((short)EquipmentType.Armor, (short)InventoryType.Equipment);
            if (armor != null)
            {
                Item iteminfo = ServerManager.GetItem(armor.InventoryItem.ItemVNum);
                ArmorUpgrade = armor.InventoryItem.Upgrade;
            }

            Inventory item = null;
            for (short i = 1; i < 14; i++)
            {
                if (i != 5)
                    item = EquipmentList.LoadBySlotAndType(i, (short)InventoryType.Equipment);

            }
            if (item != null)
            {
                Item iteminfo = ServerManager.GetItem(item.InventoryItem.ItemVNum);

                fire += item.InventoryItem.FireElement;
                light += item.InventoryItem.LightElement;
                water += item.InventoryItem.WaterElement;
                dark += item.InventoryItem.DarkElement;
                def += item.InventoryItem.RangeDefence + iteminfo.RangeDefence;
                defrate += item.InventoryItem.DefenceDodge + iteminfo.DefenceDodge;
                distdef += item.InventoryItem.DistanceDefence + iteminfo.DistanceDefence;
                distdefrate += item.InventoryItem.DistanceDefenceDodge + iteminfo.DistanceDefenceDodge;
                //maxhp-mp
            }
            return String.Format("sc {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19} {20} {21} {22} {23}", type, WeaponUpgrade, MinHit, MaxHit, HitRate, HitCCRate, HitCC, type2, SecondaryUpgrade, MinDist, MaxDist, DistRate, DistCCRate, DistCC, ArmorUpgrade, def, defrate, distdef, distdefrate, magic, fire, water, light, dark);
        }

        public string GeneratePairy()
        {
            Inventory fairy = EquipmentList.LoadBySlotAndType((short)EquipmentType.Fairy, (short)InventoryType.Equipment);

            if (fairy != null)
                return String.Format("pairy 1 {0} 4 {1} {2} {3}", CharacterId, ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element, fairy.InventoryItem.ElementRate, ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Morph);
            else
                return String.Format("pairy 1 {0} 0 0 0 40", CharacterId);

        }

        public string GenerateSpk(object message, int v)
        {
            return String.Format("spk 1 {0} {1} {2} {3}", CharacterId, v, Name, message);
        }

        public string GenerateInfo(string message)
        {
            return String.Format("info {0}", message);
        }

        public string GenerateStatInfo()
        {
            return String.Format("st 1 {0} {1} {2} {3} {4} {5}", CharacterId, Level, (int)((Hp / HPLoad()) * 100), (int)((Mp / MPLoad()) * 100), Hp, Mp);
        }
        public string generateEqListForPacket()
        {
            string[] invarray = new string[15];
            for (short i = 0; i < 15; i++)
            {
                Inventory inv = EquipmentList.LoadBySlotAndType(i, (short)InventoryType.Equipment);
                if (inv != null)
                {
                    invarray[i] = inv.InventoryItem.ItemVNum.ToString();

                }
                else invarray[i] = "-1";
            }

            return String.Format("{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}", invarray[(short)EquipmentType.Hat], invarray[(short)EquipmentType.Armor], invarray[(short)EquipmentType.MainWeapon], invarray[(short)EquipmentType.SecondaryWeapon], invarray[(short)EquipmentType.Mask], invarray[(short)EquipmentType.Fairy], invarray[(short)EquipmentType.CostumeSuite], invarray[(short)EquipmentType.CostumeHat]);
        }

        public string generateEqRareUpgradeForPacket()
        {
            short WeaponRare = 0;
            short WeaponUpgrade = 0;
            short ArmorRare = 0;
            short ArmorUpgrade = 0;
            for (short i = 0; i < 15; i++)
            {
                Inventory inv = EquipmentList.LoadBySlotAndType(i, (short)InventoryType.Equipment);
                if (inv != null)
                {
                    Item iteminfo = ServerManager.GetItem(inv.InventoryItem.ItemVNum);

                    if (iteminfo.EquipmentSlot == (short)EquipmentType.Armor)
                    {
                        ArmorRare = inv.InventoryItem.Rare;
                        ArmorUpgrade = inv.InventoryItem.Upgrade;
                    }
                    else if (iteminfo.EquipmentSlot == (short)EquipmentType.MainWeapon)
                    {
                        WeaponRare = inv.InventoryItem.Rare;
                        WeaponUpgrade = inv.InventoryItem.Upgrade;
                    }
                }
            }
            return String.Format("{0}{1} {2}{3}", WeaponUpgrade, WeaponRare, ArmorUpgrade, ArmorRare);
        }

        public string GenerateEq()
        {
            int color = HairColor;
            Inventory head = EquipmentList.LoadBySlotAndType((short)EquipmentType.Hat, (short)InventoryType.Equipment);
            if (head != null && ServerManager.GetItem(head.InventoryItem.ItemVNum).Colored)
                color = head.InventoryItem.Color;

            return String.Format("eq {0} {1} {2} {3} {4} {5} {6} {7}", CharacterId, (Authority == 2 ? 2 : 0), Gender, HairStyle, color, Class, generateEqListForPacket(), generateEqRareUpgradeForPacket());
        }

        public string GenerateFaction()
        {
            return String.Format("fs {0}", Faction);
        }

        public string Dance()
        {
            IsDancing = IsDancing == 0 ? 1 : 0;
            return String.Empty;
        }

        public string GenerateInventoryAdd(short vnum, short amount, short type, short slot, short rare, short color, short upgrade)
        {
            Item item = ServerManager.GetItem(vnum);
            switch (type)
            {
                case (short)InventoryType.Costume:
                    return String.Format("ivn 7 {0}.{1}.{2}.{3}", slot, vnum, rare, upgrade);
                case (short)InventoryType.Wear:
                    return String.Format("ivn 0 {0}.{1}.{2}.{3}", slot, vnum, rare, item != null ? (item.Colored ? color : upgrade) : upgrade);
                case (short)InventoryType.Main:
                    return String.Format("ivn 1 {0}.{1}.{2}", slot, vnum, amount);
                case (short)InventoryType.Etc:
                    return String.Format("ivn 2 {0}.{1}.{2}", slot, vnum, amount);
                case (short)InventoryType.Sp:
                    return String.Format("ivn 6 {0}.{1}.{2}.{3}", slot, vnum, rare, upgrade);
            }
            return String.Empty;
        }

        public IEnumerable<InventoryItem> LoadBySlotAllowed(short itemVNum, short amount)
        {
            foreach (Inventory inventoryitemobject in InventoryList.Inventory.Where(i => i.InventoryItem.ItemVNum.Equals(itemVNum) && i.InventoryItem.Amount + amount < 100))
            {
                yield return inventoryitemobject.InventoryItem;
            }
        }

        public string GenerateShopMemo(int type, string message)
        {
            return String.Format("s_memo {0} {1}", type, message);
        }

        public string GeneratePlayerFlag(long pflag)
        {
            return String.Format("pflag 1 {0} {1}", CharacterId, pflag);
        }
        public List<string> GeneratePlayerShopOnMap()
        {
            List<String> droplist = new List<String>();
            // foreach (KeyValuePair<long, MapShop> shop in ServerManager.GetMap(this.MapId).ShopUserList)
            //   droplist.Add(String.Format("pflag 1 {0} {1}", shop.Value.OwnerId, shop.Key+1));
            return droplist;
        }
        public string GenerateDialog(string dialog)
        {
            return String.Format("dlg {0}", dialog);
        }

        public string GenerateShop(string shopname)
        {
            return String.Format("shop 1 {0} 1 3 0 {1}", CharacterId, shopname);
        }

        public string GenerateGet(long id)
        {
            return String.Format("get 1 {0} {1} 0", CharacterId, id);
        }

        public string GenerateShopEnd()
        {
            return String.Format("shop 1 {0} 0 0", CharacterId);


        }

        public string GenerateModal(string message, int type)
        {
            return String.Format("modal {1} {0}", message, type);
        }

        public string GenerateSpPoint()
        {
            return String.Format("sp {0} 1000000 {1} 10000", SpAdditionPoint, SpPoint);
        }

        public string GenerateScal()
        {
            return String.Format("char_sc 1 {0} {1}", CharacterId, Size);
        }
        public string GenerateGender()
        {
            return String.Format("p_sex {0}", Gender);
        }

        public string GenerateEInfo(InventoryItem item)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
