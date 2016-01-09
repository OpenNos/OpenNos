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
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Character : CharacterDTO, IGameObject
    {
        #region Members

        private int _lastPulse;
        private double _lastPortal;
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
        public int LastPulse
        {
            get { return _lastPulse; }
            set
            {
                _lastPulse = value;

            }
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

        public bool UseVehicule
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
                            , HitRate = inventoryItemDTO.HitRate,
                            Color = inventoryItemDTO.Color,
                            Concentrate = inventoryItemDTO.Concentrate,
                            CriticalLuckRate = inventoryItemDTO.CriticalLuckRate,
                            CriticalRate = inventoryItemDTO.CriticalRate,
                            DamageMaximum = inventoryItemDTO.DamageMaximum,
                            DamageMinimum = inventoryItemDTO.DamageMinimum,
                            DarkElement = inventoryItemDTO.DarkElement,
                            DistanceDefence = inventoryItemDTO.DistanceDefence,
                            Dodge = inventoryItemDTO.Dodge,
                            FireElement = inventoryItemDTO.FireElement,
                            InventoryItemId = inventoryItemDTO.InventoryItemId,
                            ItemVNum = inventoryItemDTO.ItemVNum,
                            LightElement = inventoryItemDTO.LightElement,
                            MagicDefence = inventoryItemDTO.MagicDefence,
                            RangeDefence = inventoryItemDTO.RangeDefence,
                            Rare = inventoryItemDTO.Rare,
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
                        inv0 += String.Format(" {0}.{1}.{2}.{3}", inv.Slot, inv.InventoryItem.ItemVNum, inv.InventoryItem.Rare, item.Colored ? inv.InventoryItem.Color:inv.InventoryItem.Upgrade);
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
                if (npc.Shop !=null)
                    droplist.Add(String.Format("shop 2 {0} {1} {2} {3} {4}", npc.NpcId, 1,  0, npc.Shop.MenuType, npc.Shop.Name));
            return droplist;
        }
        public List<string> GenerateShopOnMap()
        {
            List<String> droplist = new List<String>();
            foreach (KeyValuePair<long, MapShop> shop in ServerManager.GetMap(this.MapId).ShopUserList)
                droplist.Add(String.Format("shop 1 {0} 1 3 0 {1}", shop.Key+1, shop.Value.Name));  
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
            return String.Format("lev {0} {1} {2} {3} {4} {5} 0 2", Level, LevelXp, JobLevel, JobLevelXp, XPLoad(), JobXPLoad());
        }

        public string GenerateCInfo()
        {
            return String.Format("c_info {0} - {1} {2} - {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} ", Name, -1, -1, CharacterId, Authority, Gender, HairStyle, HairColor, Class, GetReputIco(), 0, Morph, Invisible, 0, MorphUpgrade, ArenaWinner);
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
            return Convert.ToInt64(Reput) <= 100 ? 1 : Convert.ToInt64(Reput) <= 300 ? 2 : Convert.ToInt64(Reput) <= 500 ? 3 : Convert.ToInt64(Reput) <= 1000 ? 4 : Convert.ToInt64(Reput) <= 1500 ? 5 : Convert.ToInt64(Reput) <= 2000 ? 6 : Convert.ToInt64(Reput) <= 4500 ? 7 :
            Convert.ToInt64(Reput) <= 7000 ? 8 : Convert.ToInt64(Reput) <= 10000 ? 9 : Convert.ToInt64(Reput) <= 19000 ? 10 : Convert.ToInt64(Reput) <= 38000 ? 11 : Convert.ToInt64(Reput) <= 50000 ? 12 : Convert.ToInt64(Reput) <= 80000 ? 13 : Convert.ToInt64(Reput) <= 120000 ? 14 :
                    Convert.ToInt64(Reput) <= 170000 ? 15 : Convert.ToInt64(Reput) <= 230000 ? 16 : Convert.ToInt64(Reput) <= 300000 ? 17 : Convert.ToInt64(Reput) <= 380000 ? 18 : Convert.ToInt64(Reput) <= 470000 ? 19 : Convert.ToInt64(Reput) <= 570000 ? 20 : Convert.ToInt64(Reput) <= 700000 ? 21 :
                    Convert.ToInt64(Reput) <= 1000000 ? 22 : Convert.ToInt64(Reput) <= 3000000 ? 23 : Convert.ToInt64(Reput) <= 5000000 ? 24 : Convert.ToInt64(Reput) <= 7000000 ? 25 : Convert.ToInt64(Reput) <= 10000000 ? 26 : Convert.ToInt64(Reput) <= 50000000 ? 27 : 30;

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
            Inventory Weapon2 = EquipmentList.LoadBySlotAndType((short)EquipmentType.SecondaryWeapon,(short) InventoryType.Equipment);
            Inventory Weapon = EquipmentList.LoadBySlotAndType((short)EquipmentType.MainWeapon, (short)InventoryType.Equipment);
            return String.Format("tc_info {0} {1} {2} {3} {4} {9} 0 {8} {5} {6} {10} {11} {12} {13} {14} {15} 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 {7}", Level, Name, Fairy != null ? ServerManager.GetItem(Fairy.InventoryItem.ItemVNum).Element : 0, Fairy !=null? Fairy.InventoryItem.ElementRate:0, Class, GetReputIco(), GetDigniteIco(),
                Language.Instance.GetMessageFromKey("NO_PREZ_MESSAGE"), Language.Instance.GetMessageFromKey("NO_FAMILY"),
                Gender, Weapon != null ? 1 : 0, Weapon != null ? Weapon.InventoryItem.Rare : 0, Weapon != null ? Weapon.InventoryItem.Upgrade : 0,
                Weapon2 != null ? 1 : 0, Weapon2 != null ? Weapon2.InventoryItem.Rare : 0, Weapon2 != null ? Weapon2.InventoryItem.Upgrade : 0,
                Armor != null ? 1 : 0, Armor != null ? Armor.InventoryItem.Rare : 0, Armor != null ? Armor.InventoryItem.Upgrade : 0);
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
            return String.Format("c_mode 1 {0} {1} {2} {3} {4}", CharacterId, Morph, MorphUpgrade, MorphUpgrade2, ArenaWinner);
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

            return String.Format("in 1 {0} - {1} {2} {3} {4} {5} {6} {7} {8} {9} {17} {10} {11} {12} -1 0 0 0 0 0 {19} {18} -1 - {13} {16} {20} 0 {21} {14} 0 {15} 0 10", Name, CharacterId, MapX, MapY, Direction, (Authority == 2 ? 2 : 0), Gender, HairStyle, Color, Class, (int)((Hp / HPLoad()) * 100), (int)((Mp / MPLoad()) * 100), _rested, (GetDigniteIco() == 1) ? GetReputIco() : -GetDigniteIco(), 0,ArenaWinner, _invisible, generateEqListForPacket(),generateEqRareUpgradeForPacket(), (UseSp == true? Morph : 0), (UseSp == true  ? MorphUpgrade : 0), (UseSp == true? MorphUpgrade2 : 0));
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
            
            for (short i=0;i<15;i++)
            {
                Inventory inv = EquipmentList.LoadBySlotAndType( i, (short)InventoryType.Equipment);
                if (inv !=null)
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
                    eqlist += String.Format(" {0}.{1}.{2}.{3}.{4}", i, iteminfo.VNum, inv.InventoryItem.Rare, iteminfo.Colored? inv.InventoryItem.Color : inv.InventoryItem.Upgrade, 0);
                }
            }
            return String.Format("equip {0}{1} {2}{3}{4}", WeaponUpgrade, WeaponRare, ArmorUpgrade, ArmorRare,eqlist);
        }

        public string GenerateStatChar()
        {
            //TODO sc packet
            //string charstat = String.Empty;
            return String.Empty;
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
                    invarray[i] = inv.InventoryItem.ItemVNum.ToString() ;
                  
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

        public void Save()
        {
            throw new NotImplementedException();
        }

        public string GenerateInventoryAdd(short vnum, short amount, short type, short slot, short rare, short color, short upgrade)
        {

            switch (type)
            {
                case (short)InventoryType.Costume:
                    return String.Format("ivn 7 {0}.{1}.{2}.{3}", slot, vnum, rare, upgrade);
                case (short)InventoryType.Wear:
                    return String.Format("ivn 0 {0}.{1}.{2}.{3}", slot, vnum, rare, color !=0 ?color : upgrade);
                case (short)InventoryType.Main:
                    return String.Format("ivn 1 {0}.{1}.{2}", slot, vnum, amount);
                case (short)InventoryType.Etc:
                    return String.Format("ivn 2 {0}.{1}.{2}", slot, vnum, amount);
                case (short)InventoryType.Sp:
                    return String.Format("ivn 6 {0}.{1}.{2}.{3}", slot,vnum, rare, upgrade);    
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
                 return String.Format("pflag 1 {0} {1}", CharacterId, pflag+1);
        }
        public string GenerateEndPlayerFlag()
        {
            return String.Format("pflag 1 {0} 0", CharacterId);
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
                return String.Format("shop 1 {0} 1 3 0 {1}", CharacterId,shopname); 
        }

        public string GenerateGet(long id)
        {
            return String.Format("get 1 {0} {1} 0", CharacterId,id);
        }

        public string GenerateShopEnd()
        {
            return String.Format("shop 1 {0} 0 0", CharacterId);


        }

        public string generateModal(string message, int type)
        {
            return String.Format("modal {1} {0}", message,type);
        }


        #endregion
    }
}
