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

        int MinHit
        {
            get; set;
        }
        int MaxHit
        {
            get; set;
        }
        int HitRate
        {
            get; set;
        }
        int HitCriticalRate
        {
            get; set;
        }
        int HitCritical
        {
            get; set;
        }
        int MinDistance
        {
            get; set;
        }
        int MaxDistance
        {
            get; set;
        }
        int DistanceRate
        {
            get; set;
        }
        int DistanceCriticalRate
        {
            get; set;
        }
        int DistanceCritical
        {
            get; set;
        }

        int Defence
        {
            get; set;
        }
        int DefenceRate
        {
            get; set;
        }
        int DistanceDefenceRate
        {
            get; set;
        }
        int DistanceDefence
        {
            get; set;
        }
        int MagicalDefence
        {
            get; set;
        }
        int FireResistance
        {
            get; set;
        }
        int WaterResistance
        {
            get; set;
        }
        int LightResistance
        {
            get; set;
        }
        int DarkResistance
        {
            get; set;
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
            return $"gold {Gold}";
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
                            ElementRate = inventoryItemDTO.ElementRate,
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
                        InventoryItem = new InventoryItem
                        {
                            Amount = inventoryItemDTO.Amount,
                            ElementRate = inventoryItemDTO.ElementRate,
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
                        inv7 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Rare}.{inv.InventoryItem.Upgrade}";
                        break;
                    case (short)InventoryType.Wear:
                        inv0 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Rare}.{(item.Colored ? inv.InventoryItem.Color : inv.InventoryItem.Upgrade)}";
                        break;
                    case (short)InventoryType.Main:
                        inv1 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Amount}.0";
                        break;
                    case (short)InventoryType.Etc:
                        inv2 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Amount}.0";
                        break;
                    case (short)InventoryType.Sp:
                        inv6 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Rare}.{inv.InventoryItem.Upgrade}";
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
            return $"eff 1 {CharacterId} {effectid}";
        }
        public List<string> GenerateDroppedItem()
        {
            List<String> droplist = new List<String>();
            foreach (KeyValuePair<long, MapItem> item in ServerManager.GetMap(this.MapId).DroppedList)
                droplist.Add($"drop {item.Value.ItemVNum} {item.Key} {item.Value.PositionX} {item.Value.PositionY} {item.Value.Amount} 0 -1");
            return droplist;
        }
        public List<string> GenerateNPCShopOnMap()
        {
            List<String> droplist = new List<String>();
            foreach (Npc npc in ServerManager.GetMap(this.MapId).Npcs)
                if (npc.Shop != null)
                    droplist.Add($"shop 2 {npc.NpcId} 1 0 {npc.Shop.MenuType} {npc.Shop.Name}");
            return droplist;
        }
        public List<string> GenerateShopOnMap()
        {
            List<String> droplist = new List<String>();
            foreach (KeyValuePair<long, MapShop> shop in ServerManager.GetMap(this.MapId).ShopUserList)
                droplist.Add($"shop 1 {shop.Key + 1} 1 3 0 {shop.Value.Name}");
            return droplist;
        }

        public List<String> GenerateGp()
        {
            List<String> gpList = new List<String>();
            int i = 0;
            foreach (Portal portal in ServerManager.GetMap(this.MapId).Portals)
            {    
                gpList.Add($"gp {portal.SourceX} {portal.SourceY} {portal.DestinationMapId} {portal.Type} {i} 0");
                i++;
            }
             
            return gpList;
        }

        public List<String> Generatein2()
        {
            List<String> in2List = new List<String>();
            foreach (Npc npc in ServerManager.GetMap(this.MapId).Npcs)
                in2List.Add($"in 2 {npc.Vnum} {npc.NpcId} {npc.MapX} {npc.MapY} {npc.Position} 100 100 9632 0 0 - 1 1 0 - 1 - 0 - 1 0 0 0 0 0 0 0 0");
            return in2List;
        }

        public string GenerateFd()
        {
            return $"fd {Reput} {GetReputIco()} {Dignite} {Math.Abs(GetDigniteIco())}";
        }

        public string GenerateMapOut()
        {
            return $"mapout";
        }

        public string GenerateOut()
        {
            return $"out 1 {CharacterId}";
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

            return $"lev {Level} {LevelXp} {(!UseSp || specialist == null ? JobLevel : specialist.InventoryItem.SpLevel)} {(!UseSp || specialist == null ? JobLevelXp : specialist.InventoryItem.SpXp)} {XPLoad()} {JobXPLoad()} {Reput} 2";
        }

        public string GenerateCInfo()
        {
            return $"c_info {Name} - -1 -1 - {CharacterId} {Authority} {Gender} {HairStyle} {HairColor} {Class} {GetReputIco()} {Compliment} {(UseSp || IsVehicled ? Morph : 0)} {Invisible} 0 {(UseSp ? MorphUpgrade : 0)} {ArenaWinner} ";
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
            return $"tit {Language.Instance.GetMessageFromKey(Class == (byte)ClassType.Adventurer ? ClassType.Adventurer.ToString().ToUpper() : Class == (byte)ClassType.Swordman ? ClassType.Swordman.ToString().ToUpper() : Class == (byte)ClassType.Archer ? ClassType.Archer.ToString().ToUpper() : ClassType.Magician.ToString().ToUpper())} {Name}";
        }

        public string GenerateStat()
        {
            double option =
                (WhisperBlocked ? Math.Pow(2, (int)ConfigType.WhisperBlocked - 1) : 0)
                + (FamilyRequestBlocked ? Math.Pow(2, (int)ConfigType.FamilyRequestBlocked - 1) : 0)
                + (!MouseAimLock ? Math.Pow(2, (int)ConfigType.MouseAimLock - 1) : 0)
                + (MinilandInviteBlocked ? Math.Pow(2, (int)ConfigType.MinilandInviteBlocked - 1) : 0)
                + (ExchangeBlocked ? Math.Pow(2, (int)ConfigType.ExchangeBlocked - 1) : 0)
                + (FriendRequestBlocked ? Math.Pow(2, (int)ConfigType.FriendRequestBlocked - 1) : 0)
                + (EmoticonsBlocked ? Math.Pow(2, (int)ConfigType.EmoticonsBlocked - 1) : 0)
                + (HpBlocked ? Math.Pow(2, (int)ConfigType.HpBlocked - 1) : 0)
                + (BuffBlocked ? Math.Pow(2, (int)ConfigType.BuffBlocked - 1) : 0)
                + (GroupRequestBlocked ? Math.Pow(2, (int)ConfigType.GroupRequestBlocked - 1) : 0)
                + (HeroChatBlocked ? Math.Pow(2, (int)ConfigType.HeroChatBlocked - 1) : 0)
                + (QuickGetUp ? Math.Pow(2, (int)ConfigType.QuickGetUp - 1) : 0);
            ;
            return $"stat {Hp} {HPLoad()} {Mp} {MPLoad()} 0 {option}";
        }

        public string GenerateAt()
        {
            return $"at {CharacterId} {MapId} {MapX} {MapY} 2 0 {ServerManager.GetMap(MapId).Music} 1";
        }
        public string GenerateReqInfo()
        {
            Inventory Fairy = EquipmentList.LoadBySlotAndType((short)EquipmentType.Fairy, (short)InventoryType.Equipment);
            Inventory Armor = EquipmentList.LoadBySlotAndType((short)EquipmentType.Armor, (short)InventoryType.Equipment);
            Inventory Weapon2 = EquipmentList.LoadBySlotAndType((short)EquipmentType.SecondaryWeapon, (short)InventoryType.Equipment);
            Inventory Weapon = EquipmentList.LoadBySlotAndType((short)EquipmentType.MainWeapon, (short)InventoryType.Equipment);
            return $"tc_info {Level} {Name} {(Fairy != null ? ServerManager.GetItem(Fairy.InventoryItem.ItemVNum).Element : 0)} {(Fairy != null ? Fairy.InventoryItem.ElementRate : 0)} {Class} {Gender} -1 - {GetReputIco()} {GetDigniteIco()} {(Weapon != null ? 1 : 0)} {(Weapon != null ? Weapon.InventoryItem.Rare : 0)} {(Weapon != null ? Weapon.InventoryItem.Upgrade : 0)} {(Weapon2 != null ? 1 : 0)} {(Weapon2 != null ? Weapon2.InventoryItem.Rare : 0)} {(Weapon2 != null ? Weapon2.InventoryItem.Upgrade : 0)} {(Armor != null ? 1 : 0)} {(Armor != null ? Armor.InventoryItem.Rare : 0)} {(Armor != null ? Armor.InventoryItem.Upgrade : 0)} 0 0 {Reput} 0 0 0 {(UseSp ? Morph : 0)} 0 0 0 0 0 {Compliment} 0 0 0 0 {Language.Instance.GetMessageFromKey("NO_PREZ_MESSAGE")}";
        }

        public string GenerateCMap()
        {
            return $"c_map 0 {MapId} 1";
        }

        public string GenerateCond()
        {
            return $"cond 1 {CharacterId} 0 0 {Speed}";
        }

        public string GenerateExts()
        {
            return $"exts 0 {48 + BackPack * 12} {48 + BackPack * 12} {48 + BackPack * 12}";
        }

        public string GenerateMv(int x, int y)
        {
            return $"mv 1 {CharacterId} {x} {y} {Speed}";
        }

        public string GenerateCMode()
        {
            return $"c_mode 1 {CharacterId} {(UseSp || IsVehicled ? Morph : 0)} {(UseSp ? MorphUpgrade : 0)} {(UseSp ? MorphUpgrade2 : 0)} {ArenaWinner}";
        }

        public string GenerateSay(string message, int type)
        {
            return $"say 1 {CharacterId} {type} {message}";
        }

        public string GenerateIn()
        {
            int Color = HairColor;
            Inventory head = EquipmentList.LoadBySlotAndType((short)EquipmentType.Hat, (short)InventoryType.Equipment);
            if (head != null && ServerManager.GetItem(head.InventoryItem.ItemVNum).Colored)
                Color = head.InventoryItem.Color;
            Inventory fairy = EquipmentList.LoadBySlotAndType((short)EquipmentType.Fairy, (short)InventoryType.Equipment);

            return $"in 1 {Name} - {CharacterId} {MapX} {MapY} {Direction} {(Authority == 2 ? 2 : 0)} {Gender} {HairStyle} {Color} {Class} {generateEqListForPacket()} {(int)((Hp / HPLoad()) * 100)} {(int)((Mp / MPLoad()) * 100)} {_rested} -1 {(fairy != null ? 2 : 0)} {(fairy != null ? ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element : 0)} 0 {(fairy != null ? ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Morph : 0)} 0 {(UseSp ? Morph : 0)} {generateEqRareUpgradeForPacket()} -1 - {((GetDigniteIco() == 1) ? GetReputIco() : -GetDigniteIco())} {_invisible} {(UseSp ? MorphUpgrade : 0)} 0 {(UseSp ? MorphUpgrade2 : 0)} 0 0 {ArenaWinner} {Compliment} {Size}";
        }

        public string GenerateRest()
        {
            return $"rest 1 {CharacterId} {Rested}";
        }

        public string GenerateDir()
        {
            return $"dir 1 {CharacterId} {Direction}";
        }

        public string GenerateMsg(string message, int v)
        {
            return $"msg {v} {message}";
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
                    eqlist += $" {i}.{iteminfo.VNum}.{inv.InventoryItem.Rare}.{(iteminfo.Colored ? inv.InventoryItem.Color : inv.InventoryItem.Upgrade)}.0";
                }
            }
            return $"equip {WeaponUpgrade}{WeaponRare} {ArmorUpgrade}{ArmorRare}{eqlist}";
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
            int SecondaryUpgrade = 0;
            int ArmorUpgrade = 0;
            MinHit = ServersData.MinHit(Class, Level);
            MaxHit = ServersData.MaxHit(Class, Level);
            HitRate = ServersData.HitRate(Class, Level);
            HitCriticalRate = ServersData.HitCriticalRate(Class, Level);
            HitCritical = ServersData.HitCritical(Class, Level);
            MinDistance = ServersData.MinDistance(Class, Level);
            MaxDistance = ServersData.MaxDistance(Class, Level);
            DistanceRate = ServersData.DistanceRate(Class, Level);
            DistanceCriticalRate = ServersData.DistCriticalRate(Class, Level);
            DistanceCritical = ServersData.DistCritical(Class, Level);
            FireResistance = ServersData.FireResistance(Class, Level);
            LightResistance = ServersData.LightResistance(Class, Level);
            WaterResistance = ServersData.WaterResistance(Class, Level);
            DarkResistance = ServersData.DarkResistance(Class, Level);
            Defence = ServersData.Defence(Class, Level);
            DefenceRate = ServersData.DefenceRate(Class, Level);
            DistanceDefence = ServersData.DistanceDefence(Class, Level);
            DistanceDefenceRate = ServersData.DistanceDefenceRate(Class, Level);
            MagicalDefence = ServersData.MagicalDefence(Class, Level);

            //TODO: add base stats
            Inventory weapon = EquipmentList.LoadBySlotAndType((short)EquipmentType.MainWeapon, (short)InventoryType.Equipment);
            if (weapon != null)
            {
                Item iteminfo = ServerManager.GetItem(weapon.InventoryItem.ItemVNum);
                WeaponUpgrade = weapon.InventoryItem.Upgrade;
                MinHit += weapon.InventoryItem.DamageMinimum + iteminfo.DamageMinimum;
                MaxHit += weapon.InventoryItem.DamageMaximum + iteminfo.DamageMaximum;
                HitRate += weapon.InventoryItem.HitRate + iteminfo.HitRate;
                HitCriticalRate += weapon.InventoryItem.CriticalLuckRate + iteminfo.CriticalLuckRate;
                HitCritical += weapon.InventoryItem.CriticalRate + iteminfo.CriticalRate;
                //maxhp-mp
            }

            Inventory weapon2 = EquipmentList.LoadBySlotAndType((short)EquipmentType.SecondaryWeapon, (short)InventoryType.Equipment);
            if (weapon2 != null)
            {
                Item iteminfo = ServerManager.GetItem(weapon2.InventoryItem.ItemVNum);
                SecondaryUpgrade = weapon2.InventoryItem.Upgrade;
                MinDistance += weapon2.InventoryItem.DamageMinimum + iteminfo.DamageMinimum;
                MaxDistance += weapon2.InventoryItem.DamageMaximum + iteminfo.DamageMaximum;
                DistanceRate += weapon2.InventoryItem.HitRate + iteminfo.HitRate;
                DistanceCriticalRate += weapon2.InventoryItem.CriticalLuckRate + iteminfo.CriticalLuckRate;
                DistanceCritical += weapon2.InventoryItem.CriticalRate + iteminfo.CriticalRate;
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

                FireResistance += item.InventoryItem.FireElement;
                LightResistance += item.InventoryItem.LightElement;
                WaterResistance += item.InventoryItem.WaterElement;
                DarkResistance += item.InventoryItem.DarkElement;
                Defence += item.InventoryItem.RangeDefence + iteminfo.RangeDefence;
                DefenceRate += item.InventoryItem.DefenceDodge + iteminfo.DefenceDodge;
                DistanceDefence += item.InventoryItem.DistanceDefence + iteminfo.DistanceDefence;
                DistanceDefenceRate += item.InventoryItem.DistanceDefenceDodge + iteminfo.DistanceDefenceDodge;
                //maxhp-mp
            }
            return $"sc {type} {WeaponUpgrade} {MinHit} {MaxHit} {HitRate} {HitCriticalRate} {HitCritical} {type2} {SecondaryUpgrade} {MinDistance} {MaxDistance} {DistanceRate} {DistanceCriticalRate} {DistanceCritical} {ArmorUpgrade} {Defence} {DefenceRate} {DistanceDefence} {DistanceDefenceRate} {MagicalDefence} {FireResistance} {WaterResistance} {LightResistance} {DarkResistance}";
        }

        public string GeneratePairy()
        {
            Inventory fairy = EquipmentList.LoadBySlotAndType((short)EquipmentType.Fairy, (short)InventoryType.Equipment);

            if (fairy != null)
                return $"pairy 1 {CharacterId} 4 {ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element} {fairy.InventoryItem.ElementRate} {ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Morph}";
            else
                return $"pairy 1 {CharacterId} 0 0 0 40";

        }

        public string GenerateSpk(object message, int v)
        {
            return $"spk 1 {CharacterId} {v} {Name} {message}";
        }

        public string GenerateInfo(string message)
        {
            return $"info {message}";
        }

        public string GenerateStatInfo()
        {
            return $"st 1 {CharacterId} {Level} {(int)((Hp / HPLoad()) * 100)} {(int)((Mp / MPLoad()) * 100)} {Hp} {Mp}";
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

            return $"{invarray[(short)EquipmentType.Hat]}.{invarray[(short)EquipmentType.Armor]}.{invarray[(short)EquipmentType.MainWeapon]}.{invarray[(short)EquipmentType.SecondaryWeapon]}.{invarray[(short)EquipmentType.Mask]}.{invarray[(short)EquipmentType.Fairy]}.{invarray[(short)EquipmentType.CostumeSuite]}.{invarray[(short)EquipmentType.CostumeHat]}";
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
            return $"{WeaponUpgrade}{WeaponRare} {ArmorUpgrade}{ArmorRare}";
        }

        public string GenerateEq()
        {
            int color = HairColor;
            Inventory head = EquipmentList.LoadBySlotAndType((short)EquipmentType.Hat, (short)InventoryType.Equipment);
            if (head != null && ServerManager.GetItem(head.InventoryItem.ItemVNum).Colored)
                color = head.InventoryItem.Color;

            return $"eq {CharacterId} {(Authority == 2 ? 2 : 0)} {Gender} {HairStyle} {color} {Class} {generateEqListForPacket()} {generateEqRareUpgradeForPacket()}";
        }

        public string GenerateFaction()
        {
            return $"fs {Faction}";
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
                    return $"ivn 7 {slot}.{vnum}.{rare}.{upgrade}";
                case (short)InventoryType.Wear:
                    return $"ivn 0 {slot}.{vnum}.{rare}.{(item != null ? (item.Colored ? color : upgrade) : upgrade)}";
                case (short)InventoryType.Main:
                    return $"ivn 1 {slot}.{vnum}.{amount}";
                case (short)InventoryType.Etc:
                    return $"ivn 2 {slot}.{vnum}.{amount}";
                case (short)InventoryType.Sp:
                    return $"ivn 6 {slot}.{vnum}.{rare}.{upgrade}";
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
            return $"s_memo {type} {message}";
        }

        public string GeneratePlayerFlag(long pflag)
        {
            return $"pflag 1 {CharacterId} {pflag}";
        }
        public List<string> GeneratePlayerShopOnMap()
        {
            List<String> droplist = new List<String>();
            /*foreach (KeyValuePair<long, MapShop> shop in ServerManager.GetMap(this.MapId).ShopUserList)
              droplist.Add($"pflag 1 {shop.Value.OwnerId} {shop.Key+1}");*/
            return droplist;
        }
        public string GenerateDialog(string dialog)
        {
            return $"dlg {dialog}";
        }

        public string GenerateShop(string shopname)
        {
            return $"shop 1 {CharacterId} 1 3 0 {shopname}";
        }

        public string GenerateGet(long id)
        {
            return $"get 1 {CharacterId} {id} 0";
        }

        public string GenerateShopEnd()
        {
            return $"shop 1 {CharacterId} 0 0";
        }

        public string GenerateModal(string message)
        {
            return $"modal {message}";
        }

        public string GenerateSpPoint()
        {
            return $"sp {SpAdditionPoint} 1000000 {SpPoint} 10000";
        }

        public string GenerateScal()
        {
            return $"char_sc 1 {CharacterId} {Size}";
        }
        public string GenerateGender()
        {
            return $"p_sex {Gender}";
        }

        public string GenerateEInfo(InventoryItem item)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            CharacterDTO tempsave = this;
            SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref tempsave);
            foreach (InventoryDTO inv in DAOFactory.InventoryDAO.LoadByCharacterId(CharacterId))
            {
                if (inv.Type == (short)InventoryType.Equipment)
                {
                    if (EquipmentList.LoadBySlotAndType(inv.Slot, inv.Type) == null)
                    {
                        DAOFactory.InventoryDAO.DeleteFromSlotAndType(CharacterId, inv.Slot, inv.Type);
                        DAOFactory.InventoryItemDAO.DeleteById(inv.InventoryItemId);

                    }

                }
                else
                {
                    if (InventoryList.LoadBySlotAndType(inv.Slot, inv.Type) == null)
                    {
                        DAOFactory.InventoryDAO.DeleteFromSlotAndType(CharacterId, inv.Slot, inv.Type);
                        DAOFactory.InventoryItemDAO.DeleteById(inv.InventoryItemId);
                    }
                }

            }
            foreach (Inventory inv in InventoryList.Inventory)
                inv.Save();
            foreach (Inventory inv in EquipmentList.Inventory)
                inv.Save();


        }

        public string GenerateSlInfo(InventoryItem inventoryItem,int type)
        {
            Item iteminfo = ServerManager.GetItem(inventoryItem.ItemVNum);
            int freepoint = ServersData.SpPoint(inventoryItem.SpLevel, inventoryItem.Upgrade) - inventoryItem.SlHit - inventoryItem.SlHP - inventoryItem.SlElement - inventoryItem.SlDefence;
          
            int SlElement = ServersData.SlPoint(inventoryItem.SlElement,2);
            int SlHP = ServersData.SlPoint(inventoryItem.SlHP,3); 
            int SlDefence = ServersData.SlPoint(inventoryItem.SlDefence,1);
            int SlHit = ServersData.SlPoint(inventoryItem.SlHit,0);
            string skill = "-1"; //sk.sk.sk.sk.sk...
            return $"slinfo {type} {inventoryItem.ItemVNum} {iteminfo.Morph} {inventoryItem.SpLevel} {iteminfo.LevelJobMinimum} {iteminfo.ReputationMinimum+1} 0 0 0 0 0 0 0 {iteminfo.FireResistance} {iteminfo.WaterResistance} {iteminfo.LightResistance} {iteminfo.DarkResistance} 0 {inventoryItem.SpXp} {ServersData.SpXPData[inventoryItem.SpLevel-1]} {skill} {inventoryItem.InventoryItemId} {freepoint} {SlHit} {SlDefence} {SlElement} {SlHP} {inventoryItem.Upgrade} - 1 12 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
        }


        #endregion
    }
}
