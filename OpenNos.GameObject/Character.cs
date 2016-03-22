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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenNos.GameObject
{
    public class Character : CharacterDTO, IGameObject
    {
        #region Members

        private int _authority;
        private int _backpack;
        private int _direction;
        private InventoryList _equipmentlist;
        private InventoryList _inventorylist;
        private int _invisible;
        private int _isDancing;
        private bool _issitting;
        private double _lastPortal;
        private int _lastPulse;
        private int _morph;
        private int _morphUpgrade;
        private int _morphUpgrade2;
        private int _size = 10;

        #endregion

        #region Instantiation

        public Character()
        {
            Mapper.CreateMap<CharacterDTO, Character>();
            Mapper.CreateMap<Character, CharacterDTO>();
        }

        #endregion

        #region Properties

        public int Authority { get { return _authority; } set { _authority = value; } }
        public int BackPack { get { return _backpack; } set { _backpack = value; } }
        public int Direction { get { return _direction; } set { _direction = value; } }
        public InventoryList EquipmentList { get { return _equipmentlist; } set { _equipmentlist = value; } }
        public ExchangeInfo ExchangeInfo { get; set; }
        public InventoryList InventoryList { get { return _inventorylist; } set { _inventorylist = value; } }
        public int Invisible { get { return _invisible; } set { _invisible = value; } }
        public bool InvisibleGm { get; set; }
        public int IsDancing { get { return _isDancing; } set { _isDancing = value; } }
        public bool IsSitting { get { return _issitting; } set { _issitting = value; } }
        public bool IsVehicled { get; set; }
        public DateTime LastLogin { get; set; }
        public double LastPortal { get { return _lastPortal; } set { _lastPortal = value; } }
        public int LastPulse { get { return _lastPulse; } set { _lastPulse = value; } }
        public double LastSp { get; set; }
        public byte LastSpeed { get { return Speed; } set { if (value > 59) { Speed = 59; } else { Speed = value; } } }
        public int MaxSnack { get; set; }
        public int Morph { get { return _morph; } set { _morph = value; } }
        public int MorphUpgrade { get { return _morphUpgrade; } set { _morphUpgrade = value; } }
        public int MorphUpgrade2 { get { return _morphUpgrade2; } set { _morphUpgrade2 = value; } }
        public int Size { get { return _size; } set { _size = value; } }
        public int SnackAmount { get; set; }
        public int SnackHp { get; set; }
        public int SnackMp { get; set; }
        public byte Speed { get; set; }
        public Thread ThreadCharChange { get; set; }
        public bool UseSp { get; set; }
        private int DarkResistance { get; set; }
        private int Defence { get; set; }
        private int DefenceRate { get; set; }
        private int DistanceCritical { get; set; }
        private int DistanceCriticalRate { get; set; }
        private int DistanceDefence { get; set; }
        private int DistanceDefenceRate { get; set; }
        private int DistanceRate { get; set; }
        private int Element { get; set; }
        private int ElementRate { get; set; }
        private int FireResistance { get; set; }
        private int HitCritical { get; set; }
        private int HitCriticalRate { get; set; }
        private int HitRate { get; set; }
        private int LightResistance { get; set; }
        private int MagicalDefence { get; set; }
        private int MaxDistance { get; set; }
        private int MaxHit { get; set; }
        private int MinDistance { get; set; }
        private int MinHit { get; set; }
        private int WaterResistance { get; set; }

        #endregion

        #region Methods

        public string Dance()
        {
            IsDancing = IsDancing == 0 ? 1 : 0;
            return string.Empty;
        }

        public string GenerateAt()
        {
            return $"at {CharacterId} {MapId} {MapX} {MapY} 2 0 {ServerManager.GetMap(MapId).Music} 1";
        }

        public string GenerateCInfo()
        {
            return $"c_info {Name} - -1 -1 - {CharacterId} {Authority} {Gender} {HairStyle} {HairColor} {Class} {GetReputIco()} {Compliment} {(UseSp || IsVehicled ? Morph : 0)} {Invisible} 0 {(UseSp ? MorphUpgrade : 0)} {ArenaWinner} ";
        }

        public string GenerateCMap()
        {
            return $"c_map 0 {MapId} 1";
        }

        public string GenerateCMode()
        {
            return $"c_mode 1 {CharacterId} {(UseSp || IsVehicled ? Morph : 0)} {(UseSp ? MorphUpgrade : 0)} {(UseSp ? MorphUpgrade2 : 0)} {ArenaWinner}";
        }

        public string GenerateCond()
        {
            return $"cond 1 {CharacterId} 0 0 {Speed}";
        }

        public string GenerateDialog(string dialog)
        {
            return $"dlg {dialog}";
        }

        public string GenerateDir()
        {
            return $"dir 1 {CharacterId} {Direction}";
        }

        public List<string> GenerateDroppedItem()
        {
            return ServerManager.GetMap(MapId).DroppedList.Select(item => $"in 9 {item.Value.ItemVNum} {item.Key} {item.Value.PositionX} {item.Value.PositionY} {item.Value.Amount} 0 0 -1").ToList();
        }

        public string GenerateEff(int effectid)
        {
            return $"eff 1 {CharacterId} {effectid}";
        }

        public string GenerateEInfo(InventoryItem item)
        {
            Item iteminfo = ServerManager.GetItem(item.ItemVNum);
            byte equipmentslot = iteminfo.EquipmentSlot;
            byte itemType = iteminfo.ItemType;
            byte classe = iteminfo.Class;
            byte subtype = iteminfo.ItemSubType;
            DateTime test = item.ItemDeleteTime != null ? (DateTime)item.ItemDeleteTime : DateTime.Now;
            long time = item.ItemDeleteTime != null ? (long)test.Subtract(DateTime.Now).TotalSeconds : 0;
            long seconds = item.IsUsed ? time : iteminfo.ItemValidTime;
            if (seconds < 0)
                seconds = 0;
            switch (itemType)
            {
                case (byte)ItemType.Weapon:
                    switch (equipmentslot)
                    {
                        case (byte)EquipmentType.MainWeapon:
                            switch (classe)
                            {
                                case 4:
                                    return $"e_info 1 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0"; // -1 = {item.ShellEffectValue} {item.FirstShell}...
                                case 8:
                                    return $"e_info 5 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0";

                                default:
                                    return $"e_info 0 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0";
                            }
                        case (byte)EquipmentType.SecondaryWeapon:
                            switch (classe)
                            {
                                case 4:
                                    return $"e_info 1 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0";

                                default:
                                    return $"e_info 0 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.DamageMinimum + item.DamageMinimum} {iteminfo.DamageMaximum + item.DamageMaximum} {iteminfo.HitRate + item.HitRate} {iteminfo.CriticalLuckRate + item.CriticalLuckRate} {iteminfo.CriticalRate + item.CriticalRate} {item.Ammo} {iteminfo.MaximumAmmo} {iteminfo.Price} -1 0 0 0";
                            }
                    }
                    break;

                case (byte)ItemType.Armor:
                    return $"e_info 2 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.Price} -1 0 0 0";

                case (byte)ItemType.Fashion:
                    switch (equipmentslot)
                    {
                        case (byte)EquipmentType.CostumeHat:
                            return $"e_info 3 {item.ItemVNum} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.FireResistance + item.FireResistance} {iteminfo.WaterResistance + item.WaterResistance} {iteminfo.LightResistance + item.LightResistance} {iteminfo.DarkResistance + item.DarkResistance} {iteminfo.Price} 0 1 {(seconds / (3600))}";

                        case (byte)EquipmentType.CostumeSuit:
                            return $"e_info 2 {item.ItemVNum} {item.Rare} {item.Upgrade} {(item.IsFixed ? 1 : 0)} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.Price} -1 1 {(seconds / (3600))}"; // 1 = IsCosmetic -1 = no shells
                        default:
                            return $"e_info 3 {item.ItemVNum} {iteminfo.LevelMinimum} {iteminfo.CloseDefence + item.CloseDefence} {iteminfo.DistanceDefence + item.DistanceDefence} {iteminfo.MagicDefence + item.MagicDefence} {iteminfo.DefenceDodge + item.DefenceDodge} {iteminfo.FireResistance + item.FireResistance} {iteminfo.WaterResistance + item.WaterResistance} {iteminfo.LightResistance + item.LightResistance} {iteminfo.DarkResistance + item.DarkResistance} {iteminfo.Price} 0 0 -1"; // after iteminfo.Price theres TimesConnected {(iteminfo.ItemValidTime == 0 ? -1 : iteminfo.ItemValidTime / (3600))}
                    }
                case (byte)ItemType.Jewelery:
                    switch (equipmentslot)
                    {
                        case (byte)EquipmentType.Amulet:
                            return $"e_info 4 {item.ItemVNum} {iteminfo.LevelMinimum}  {seconds * 10} 0 0 {iteminfo.Price}";

                        case (byte)EquipmentType.Fairy:
                            return $"e_info 4 {item.ItemVNum} {iteminfo.Element} {item.ElementRate + iteminfo.ElementRate} 0 0 0 0 0"; // last IsNosmall
                        default:
                            return $"e_info 4 {item.ItemVNum} {iteminfo.LevelMinimum} {iteminfo.MaxCellonLvl} {iteminfo.MaxCellon} {item.Cellon} {iteminfo.Price}";
                    }
                case (byte)ItemType.Box:
                    switch (subtype) //0 = NOSMATE pearl 1= npc pearl 2 = sp box 3 = raid box 4= VEHICLE pearl 5=fairy pearl
                    {
                        case 2:
                            return $"e_info 7 {item.ItemVNum} {(item.IsEmpty ? 1 : 0)} {item.Design} {item.SpLevel} {item.SpXp} {ServersData.SpXPData[JobLevel - 1]} {item.Upgrade} {item.SlDamage} {item.SlDefence} {item.SlElement} {item.SlHP} 10 {item.FireResistance} {item.WaterResistance} {item.LightResistance} {item.DarkResistance} {item.SpStoneUpgrade} {item.SpDamage} {item.SpDefence} {item.SpElement} {item.SpHP} {item.SpFire} {item.SpWater} {item.SpLight} {item.SpDark}";

                        default:
                            return $"e_info 8 {item.ItemVNum} {item.Design} {item.Rare}";
                    }
                case (byte)ItemType.Shell:
                    return $"e_info 4 {item.ItemVNum} {iteminfo.LevelMinimum} {item.Rare} {iteminfo.Price} 0"; //0 = Number of effects
            }
            return string.Empty;
        }

        public string GenerateEq()
        {
            int color = HairColor;
            Inventory head = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Hat, (byte)InventoryType.Equipment);
            if (head != null && ServerManager.GetItem(head.InventoryItem.ItemVNum).IsColored)
                color = head.InventoryItem.Design;

            return $"eq {CharacterId} {(Authority == 2 ? 2 : 0)} {Gender} {HairStyle} {color} {Class} {generateEqListForPacket()} {generateEqRareUpgradeForPacket()}";
        }

        public string generateEqListForPacket()
        {
            string[] invarray = new string[15];
            for (short i = 0; i < 15; i++)
            {
                Inventory inv = EquipmentList.LoadBySlotAndType(i, (byte)InventoryType.Equipment);
                if (inv != null)
                {
                    invarray[i] = inv.InventoryItem.ItemVNum.ToString();
                }
                else invarray[i] = "-1";
            }

            return $"{invarray[(byte)EquipmentType.Hat]}.{invarray[(byte)EquipmentType.Armor]}.{invarray[(byte)EquipmentType.MainWeapon]}.{invarray[(byte)EquipmentType.SecondaryWeapon]}.{invarray[(byte)EquipmentType.Mask]}.{invarray[(byte)EquipmentType.Fairy]}.{invarray[(byte)EquipmentType.CostumeSuit]}.{invarray[(byte)EquipmentType.CostumeHat]}";
        }

        public string generateEqRareUpgradeForPacket()
        {
            byte weaponRare = 0;
            byte weaponUpgrade = 0;
            byte armorRare = 0;
            byte armorUpgrade = 0;
            for (short i = 0; i < 15; i++)
            {
                Inventory inv = EquipmentList.LoadBySlotAndType(i, (byte)InventoryType.Equipment);
                if (inv != null)
                {
                    Item iteminfo = ServerManager.GetItem(inv.InventoryItem.ItemVNum);

                    if (iteminfo.EquipmentSlot == (byte)EquipmentType.Armor)
                    {
                        armorRare = inv.InventoryItem.Rare;
                        armorUpgrade = inv.InventoryItem.Upgrade;
                    }
                    else if (iteminfo.EquipmentSlot == (byte)EquipmentType.MainWeapon)
                    {
                        weaponRare = inv.InventoryItem.Rare;
                        weaponUpgrade = inv.InventoryItem.Upgrade;
                    }
                }
            }
            return $"{weaponUpgrade}{weaponRare} {armorUpgrade}{armorRare}";
        }

        public string GenerateEquipment()
        {
            //equip 86 0 0.4903.6.8.0 2.340.0.0.0 3.4931.0.5.0 4.4845.3.5.0 5.4912.7.9.0 6.4848.1.0.0 7.4849.3.0.0 8.4850.2.0.0 9.227.0.0.0 10.281.0.0.0 11.347.0.0.0 13.4150.0.0.0 14.4076.0.0.0
            string eqlist = string.Empty;
            byte weaponRare = 0;
            byte weaponUpgrade = 0;
            byte armorRare = 0;
            byte armorUpgrade = 0;

            for (short i = 0; i < 15; i++)
            {
                Inventory inv = EquipmentList.LoadBySlotAndType(i, (byte)InventoryType.Equipment);
                if (inv != null)
                {
                    Item iteminfo = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                    if (iteminfo.EquipmentSlot == (byte)EquipmentType.Armor)
                    {
                        armorRare = inv.InventoryItem.Rare;
                        armorUpgrade = inv.InventoryItem.Upgrade;
                    }
                    else if (iteminfo.EquipmentSlot == (byte)EquipmentType.MainWeapon)
                    {
                        weaponRare = inv.InventoryItem.Rare;
                        weaponUpgrade = inv.InventoryItem.Upgrade;
                    }
                    eqlist += $" {i}.{iteminfo.VNum}.{inv.InventoryItem.Rare}.{(iteminfo.IsColored ? inv.InventoryItem.Design : inv.InventoryItem.Upgrade)}.0";
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
            return $"fd {Reput} {GetReputIco()} {Dignite} {Math.Abs(GetDigniteIco())}";
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
            return $"gold {Gold}";
        }

        public List<string> GenerateGp()
        {
            List<string> gpList = new List<string>();
            int i = 0;
            foreach (Portal portal in ServerManager.GetMap(MapId).Portals)
            {
                gpList.Add($"gp {portal.SourceX} {portal.SourceY} {portal.DestinationMapId} {portal.Type} {i} {(portal.IsDisabled ? 1 : 0)}");
                i++;
            }

            return gpList;
        }

        public string GenerateIn()
        {
            int color = HairColor;
            Inventory head = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Hat, (byte)InventoryType.Equipment);
            if (head != null && ServerManager.GetItem(head.InventoryItem.ItemVNum).IsColored)
                color = head.InventoryItem.Design;
            Inventory fairy = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Fairy, (byte)InventoryType.Equipment);

            return $"in 1 {Name} - {CharacterId} {MapX} {MapY} {Direction} {(Authority == 2 ? 2 : 0)} {Gender} {HairStyle} {color} {Class} {generateEqListForPacket()} {(int)(Hp / HPLoad() * 100)} {(int)(Mp / MPLoad() * 100)} {(IsSitting ? 1 : 0)} -1 {(fairy != null ? 2 : 0)} {(fairy != null ? ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element : 0)} 0 {(fairy != null ? ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Morph : 0)} 0 {(UseSp ? Morph : 0)} {generateEqRareUpgradeForPacket()} -1 - {((GetDigniteIco() == 1) ? GetReputIco() : -GetDigniteIco())} {_invisible} {(UseSp ? MorphUpgrade : 0)} 0 {(UseSp ? MorphUpgrade2 : 0)} {Level} 0 {ArenaWinner} {Compliment} {Size}";
        }

        public List<string> Generatein2()
        {
            return ServerManager.GetMap(MapId).Npcs.Select(npc => $"in 2 {npc.NpcVNum} {npc.MapNpcId} {npc.MapX} {npc.MapY} {npc.Position} 100 100 {npc.Dialog} 0 0 - {(npc.IsSitting ? 0 : 1)} 0 0 - 1 - 0 - 1 0 0 0 0 0 0 0 0").ToList();
        }

        public List<string> Generatein3()
        {
            return ServerManager.GetMap(MapId).Monsters.Select(monster => monster.GenerateIn3()).ToList();
        }

        public string GenerateInfo(string message)
        {
            return $"info {message}";
        }

        public string GenerateInventoryAdd(short vnum, byte amount, byte type, short slot, byte rare, short color, byte upgrade)
        {
            Item item = ServerManager.GetItem(vnum);
            switch (type)
            {
                case (byte)InventoryType.Costume:
                    return $"ivn 7 {slot}.{vnum}.{rare}.{upgrade}";

                case (byte)InventoryType.Wear:
                    return $"ivn 0 {slot}.{vnum}.{rare}.{(item != null ? (item.IsColored ? color : upgrade) : upgrade)}";

                case (byte)InventoryType.Main:
                    return $"ivn 1 {slot}.{vnum}.{amount}";

                case (byte)InventoryType.Etc:
                    return $"ivn 2 {slot}.{vnum}.{amount}";

                case (byte)InventoryType.Sp:
                    return $"ivn 6 {slot}.{vnum}.{rare}.{upgrade}";
            }
            return string.Empty;
        }

        public string GenerateInvisible()
        {
            return $"cl {CharacterId} {Invisible} 0";
        }

        public string GenerateLev()
        {
            Inventory specialist = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);

            return $"lev {Level} {LevelXp} {(!UseSp || specialist == null ? JobLevel : specialist.InventoryItem.SpLevel)} {(!UseSp || specialist == null ? JobLevelXp : specialist.InventoryItem.SpXp)} {(!UseSp || specialist == null ? XPLoad() : SPXPLoad())} {JobXPLoad()} {Reput} 2";
        }

        public string GenerateMapOut()
        {
            return "mapout";
        }

        public string GenerateModal(string message)
        {
            return $"modal {message}";
        }

        public string GenerateMsg(string message, int v)
        {
            return $"msg {v} {message}";
        }

        public string GenerateMv()
        {
            return $"mv 1 {CharacterId} {MapX} {MapY} {Speed}";
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
            Inventory fairy = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Fairy, (byte)InventoryType.Equipment);
            Item iteminfo = null;
            ElementRate = 0;
            Element = 0;
            if (fairy != null)
            {
                iteminfo = ServerManager.GetItem(fairy.InventoryItem.ItemVNum);
                ElementRate += fairy.InventoryItem.ElementRate + iteminfo.ElementRate;
                Element = iteminfo.Element;
            }

            return fairy != null
                ? $"pairy 1 {CharacterId} 4 {iteminfo.Element} {fairy.InventoryItem.ElementRate + iteminfo.ElementRate} {iteminfo.Morph}"
                : $"pairy 1 {CharacterId} 0 0 0 40";
        }

        public string GeneratePlayerFlag(long pflag)
        {
            return $"pflag 1 {CharacterId} {pflag}";
        }

        public List<string> GeneratePlayerShopOnMap()
        {
            return ServerManager.GetMap(MapId).ShopUserList.Select(shop => $"pflag 1 {shop.Value.OwnerId} {shop.Key + 1}").ToList();
        }

        public string GenerateRc(int v)
        {
            return $"rc 1 {CharacterId} {v} 0";
        }

        public string GenerateReqInfo()
        {
            Inventory fairy = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Fairy, (byte)InventoryType.Equipment);
            Inventory armor = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Armor, (byte)InventoryType.Equipment);
            Inventory weapon2 = EquipmentList.LoadBySlotAndType((byte)EquipmentType.SecondaryWeapon, (byte)InventoryType.Equipment);
            Inventory weapon = EquipmentList.LoadBySlotAndType((byte)EquipmentType.MainWeapon, (byte)InventoryType.Equipment);
            return $"tc_info {Level} {Name} {(fairy != null ? ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element : 0)} {(Element != 0 ? ElementRate : 0)} {Class} {Gender} -1 - {GetReputIco()} {GetDigniteIco()} {(weapon != null ? 1 : 0)} {weapon?.InventoryItem.Rare ?? 0} {weapon?.InventoryItem.Upgrade ?? 0} {(weapon2 != null ? 1 : 0)} {weapon2?.InventoryItem.Rare ?? 0} {weapon2?.InventoryItem.Upgrade ?? 0} {(armor != null ? 1 : 0)} {armor?.InventoryItem.Rare ?? 0} {armor?.InventoryItem.Upgrade ?? 0} 0 0 {Reput} 0 0 0 {(UseSp ? Morph : 0)} 0 0 0 0 0 {Compliment} 0 0 0 0 {Language.Instance.GetMessageFromKey("NO_PREZ_MESSAGE")}";
        }

        public string GenerateRest()
        {
            return $"rest 1 {CharacterId} {(IsSitting ? 1 : 0)}";
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

        public List<string> GenerateShopOnMap()
        {
            return ServerManager.GetMap(MapId).ShopUserList.Select(shop => $"shop 1 {shop.Key + 1} 1 3 0 {shop.Value.Name}").ToList();
        }

        public string GenerateSlInfo(InventoryItem inventoryItem, int type)
        {
            Item iteminfo = ServerManager.GetItem(inventoryItem.ItemVNum);
            int freepoint = ServersData.SpPoint(inventoryItem.SpLevel, inventoryItem.Upgrade) - inventoryItem.SlDamage - inventoryItem.SlHP - inventoryItem.SlElement - inventoryItem.SlDefence;

            int slElement = ServersData.SlPoint(inventoryItem.SlElement, 2);
            int slHp = ServersData.SlPoint(inventoryItem.SlHP, 3);
            int slDefence = ServersData.SlPoint(inventoryItem.SlDefence, 1);
            int slHit = ServersData.SlPoint(inventoryItem.SlDamage, 0);

            string skill = "-1"; //sk.sk.sk.sk.sk...

            return $"slinfo {type} {inventoryItem.ItemVNum} {iteminfo.Morph} {inventoryItem.SpLevel} {iteminfo.LevelJobMinimum} {iteminfo.ReputationMinimum + 1} 0 0 0 0 0 0 0 0 {iteminfo.FireResistance} {iteminfo.WaterResistance} {iteminfo.LightResistance} {iteminfo.DarkResistance} {inventoryItem.SpXp} {ServersData.SpXPData[inventoryItem.SpLevel - 1]} {skill} {inventoryItem.InventoryItemId} {freepoint} {slHit} {slDefence} {slElement} {slHp} {inventoryItem.Upgrade} - 1 12 0 0 0 0 0 {inventoryItem.SpDamage} {inventoryItem.SpDefence} {inventoryItem.SpElement} {inventoryItem.SpHP} {inventoryItem.SpFire} {inventoryItem.SpWater} {inventoryItem.SpLight} {inventoryItem.SpDark} 0";
        }

        public string GenerateSpk(object message, int v)
        {
            return $"spk 1 {CharacterId} {v} {Name} {message}";
        }

        public string GenerateSpPoint()
        {
            return $"sp {SpAdditionPoint} 1000000 {SpPoint} 10000";
        }

        public List<string> GenerateStartupInventory()
        {
            List<string> inventoriesStringPacket = new List<string>();
            string inv0 = "inv 0", inv1 = "inv 1", inv2 = "inv 2", inv6 = "inv 6", inv7 = "inv 7";

            foreach (Inventory inv in InventoryList.Inventory)
            {
                Item item = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                switch (inv.Type)
                {
                    case (byte)InventoryType.Costume:
                        inv7 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Rare}.{inv.InventoryItem.Upgrade}";
                        break;

                    case (byte)InventoryType.Wear:
                        inv0 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Rare}.{(item.IsColored ? inv.InventoryItem.Design : inv.InventoryItem.Upgrade)}";
                        break;

                    case (byte)InventoryType.Main:
                        inv1 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Amount}.0";
                        break;

                    case (byte)InventoryType.Etc:
                        inv2 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Amount}.0";
                        break;

                    case (byte)InventoryType.Sp:
                        inv6 += $" {inv.Slot}.{inv.InventoryItem.ItemVNum}.{inv.InventoryItem.Rare}.{inv.InventoryItem.Upgrade}";
                        break;

                    case (byte)InventoryType.Equipment:
                        break;
                }
            }
            inventoriesStringPacket.Add(inv0);
            inventoriesStringPacket.Add(inv1);
            inventoriesStringPacket.Add(inv2);
            inventoriesStringPacket.Add(inv6);
            inventoriesStringPacket.Add(inv7);
            return inventoriesStringPacket;
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
            Element = ServersData.Element(Class, Level);
            DistanceDefence = ServersData.DistanceDefence(Class, Level);
            DistanceDefenceRate = ServersData.DistanceDefenceRate(Class, Level);
            MagicalDefence = ServersData.MagicalDefence(Class, Level);
            if (UseSp)
            {
                Inventory inventory = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
                if (inventory != null)
                {
                    int point = ServersData.SlPoint(inventory.InventoryItem.SlDamage, 0);
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

                    point = ServersData.SlPoint(inventory.InventoryItem.SlDefence, 1);
                    p = 0;
                    if (point <= 50)
                        p = point;
                    else
                        p = 50 + (point - 50) * 2;
                    Defence += p;
                    MagicalDefence += p;
                    DistanceDefence += p;

                    point = ServersData.SlPoint(inventory.InventoryItem.SlElement, 2);
                    p = 0;
                    if (point <= 50)
                        p = point;
                    else
                        p = 50 + (point - 50) * 2;
                    Element += p;
                }
            }
            //TODO: add base stats
            Inventory weapon = EquipmentList.LoadBySlotAndType((byte)EquipmentType.MainWeapon, (byte)InventoryType.Equipment);
            if (weapon != null)
            {
                Item iteminfo = ServerManager.GetItem(weapon.InventoryItem.ItemVNum);
                weaponUpgrade = weapon.InventoryItem.Upgrade;
                MinHit += (int)((weapon.InventoryItem.DamageMinimum + iteminfo.DamageMinimum));
                MaxHit += (int)((weapon.InventoryItem.DamageMaximum + iteminfo.DamageMaximum));
                HitRate += weapon.InventoryItem.HitRate + iteminfo.HitRate;
                HitCriticalRate += weapon.InventoryItem.CriticalLuckRate + iteminfo.CriticalLuckRate;
                HitCritical += weapon.InventoryItem.CriticalRate + iteminfo.CriticalRate;
                //maxhp-mp
            }

            Inventory weapon2 = EquipmentList.LoadBySlotAndType((byte)EquipmentType.SecondaryWeapon, (byte)InventoryType.Equipment);
            if (weapon2 != null)
            {
                Item iteminfo = ServerManager.GetItem(weapon2.InventoryItem.ItemVNum);
                secondaryUpgrade = weapon2.InventoryItem.Upgrade;
                MinDistance += (int)((weapon2.InventoryItem.DamageMinimum + iteminfo.DamageMinimum));
                MaxDistance += (int)((weapon2.InventoryItem.DamageMaximum + iteminfo.DamageMaximum));
                DistanceRate += weapon2.InventoryItem.HitRate + iteminfo.HitRate;
                DistanceCriticalRate += weapon2.InventoryItem.CriticalLuckRate + iteminfo.CriticalLuckRate;
                DistanceCritical += weapon2.InventoryItem.CriticalRate + iteminfo.CriticalRate;
                //maxhp-mp
            }

            Inventory armor = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Armor, (byte)InventoryType.Equipment);
            if (armor != null)
            {
                Item iteminfo = ServerManager.GetItem(armor.InventoryItem.ItemVNum); // unused variable
                armorUpgrade = armor.InventoryItem.Upgrade;
                Defence += (int)((armor.InventoryItem.CloseDefence + iteminfo.CloseDefence));
                DistanceDefence += (int)((armor.InventoryItem.DistanceDefence + iteminfo.DistanceDefence));
                MagicalDefence += (int)((armor.InventoryItem.MagicDefence + iteminfo.MagicDefence));
                DefenceRate += armor.InventoryItem.DefenceDodge + iteminfo.DefenceDodge;
                DistanceDefenceRate += armor.InventoryItem.DistanceDefenceDodge + iteminfo.DistanceDefenceDodge;
            }
            Inventory item = null;
            for (short i = 1; i < 14; i++)
            {
                item = EquipmentList.LoadBySlotAndType(i, (byte)InventoryType.Equipment);

                if (item != null)
                {
                    Item iteminfo = ServerManager.GetItem(item.InventoryItem.ItemVNum);
                    if ((iteminfo.EquipmentSlot != (byte)EquipmentType.MainWeapon) && (iteminfo.EquipmentSlot != (byte)EquipmentType.SecondaryWeapon) && iteminfo.EquipmentSlot != (byte)EquipmentType.Armor)
                    {
                        FireResistance += item.InventoryItem.FireResistance + iteminfo.FireResistance;
                        LightResistance += item.InventoryItem.LightResistance + iteminfo.LightResistance;
                        WaterResistance += item.InventoryItem.WaterResistance + iteminfo.WaterResistance;
                        DarkResistance += item.InventoryItem.DarkResistance + iteminfo.DarkResistance;
                        Defence += item.InventoryItem.CloseDefence + iteminfo.CloseDefence;
                        DefenceRate += item.InventoryItem.DefenceDodge + iteminfo.DefenceDodge;
                        DistanceDefence += item.InventoryItem.DistanceDefence + iteminfo.DistanceDefence;
                        DistanceDefenceRate += item.InventoryItem.DistanceDefenceDodge + iteminfo.DistanceDefenceDodge;
                        //maxhp-mp
                    }
                }
            }
            return $"sc {type} {weaponUpgrade} {MinHit} {MaxHit} {HitRate} {HitCriticalRate} {HitCritical} {type2} {secondaryUpgrade} {MinDistance} {MaxDistance} {DistanceRate} {DistanceCriticalRate} {DistanceCritical} {armorUpgrade} {Defence} {DefenceRate} {DistanceDefence} {DistanceDefenceRate} {MagicalDefence} {FireResistance} {WaterResistance} {LightResistance} {DarkResistance}";
        }

        public string GenerateStatInfo()
        {
            return $"st 1 {CharacterId} {Level} {(int)(Hp / HPLoad() * 100)} {(int)(Mp / MPLoad() * 100)} {Hp} {Mp}";
        }

        public string GenerateTit()
        {
            return $"tit {Language.Instance.GetMessageFromKey(Class == (byte)ClassType.Adventurer ? ClassType.Adventurer.ToString().ToUpper() : Class == (byte)ClassType.Swordman ? ClassType.Swordman.ToString().ToUpper() : Class == (byte)ClassType.Archer ? ClassType.Archer.ToString().ToUpper() : ClassType.Magician.ToString().ToUpper())} {Name}";
        }

        public int GetDigniteIco()
        {
            int icoDignite = 1;
            if (Dignite <= -100)
                icoDignite = 2;
            if (Dignite <= -200)
                icoDignite = 3;
            if (Dignite <= -400)
                icoDignite = 4;
            if (Dignite <= -600)
                icoDignite = 5;
            if (Dignite <= -800)
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

        public int HealthHPLoad()
        {
            if (IsSitting)
                return ServersData.HpHealth[Class];
            return ServersData.HpHealthStand[Class];
        }

        public int HealthMPLoad()
        {
            if (IsSitting)
                return ServersData.MpHealth[Class];
            return ServersData.MpHealthStand[Class];
        }

        public double HPLoad()
        {
            double multiplicator = 1.0;
            int hp = 0;
            if (UseSp)
            {
                Inventory inventory = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
                if (inventory != null)
                {
                    int point = ServersData.SlPoint(inventory.InventoryItem.SlHP, 3);
                    if (point <= 50)
                        multiplicator += point / 100.0;
                    else
                        multiplicator += 0.5 + (point - 50.00) / 50.00;

                    hp = inventory.InventoryItem.HP;
                }
            }
            return (int)((ServersData.HPData[Class, Level] + hp) * multiplicator);
        }

        public double JobXPLoad()
        {
            if (Class == (byte)ClassType.Adventurer)
                return ServersData.FirstJobXPData[JobLevel - 1];
            return ServersData.SecondJobXPData[JobLevel - 1];
        }

        public IEnumerable<InventoryItem> LoadBySlotAllowed(short itemVNum, byte amount)
        {
            return InventoryList.Inventory.Where(i => i.InventoryItem.ItemVNum.Equals(itemVNum) && i.InventoryItem.Amount + amount < 100).Select(inventoryitemobject => new InventoryItem(inventoryitemobject.InventoryItem));
        }

        public void LoadInventory()
        {
            IEnumerable<InventoryDTO> inventorysDTO = DAOFactory.InventoryDAO.LoadByCharacterId(CharacterId);

            InventoryList = new InventoryList();
            EquipmentList = new InventoryList();
            foreach (InventoryDTO inventory in inventorysDTO)
            {
                inventory.InventoryItem = DAOFactory.InventoryItemDAO.LoadByInventoryId(inventory.InventoryId);
                InventoryItem invitem = new InventoryItem
                {
                    Amount = inventory.InventoryItem.Amount,
                    ElementRate = inventory.InventoryItem.ElementRate,
                    HitRate = inventory.InventoryItem.HitRate,
                    Design = inventory.InventoryItem.Design,
                    Concentrate = inventory.InventoryItem.Concentrate,
                    CriticalLuckRate = inventory.InventoryItem.CriticalLuckRate,
                    CriticalRate = inventory.InventoryItem.CriticalRate,
                    DamageMaximum = inventory.InventoryItem.DamageMaximum,
                    DamageMinimum = inventory.InventoryItem.DamageMinimum,
                    DarkElement = inventory.InventoryItem.DarkElement,
                    DistanceDefence = inventory.InventoryItem.DistanceDefence,
                    DistanceDefenceDodge = inventory.InventoryItem.DistanceDefenceDodge,
                    DefenceDodge = inventory.InventoryItem.DefenceDodge,
                    FireElement = inventory.InventoryItem.FireElement,
                    InventoryItemId = inventory.InventoryItem.InventoryItemId,
                    ItemVNum = inventory.InventoryItem.ItemVNum,
                    LightElement = inventory.InventoryItem.LightElement,
                    MagicDefence = inventory.InventoryItem.MagicDefence,
                    CloseDefence = inventory.InventoryItem.CloseDefence,
                    Rare = inventory.InventoryItem.Rare,
                    SpXp = inventory.InventoryItem.SpXp,
                    SpLevel = inventory.InventoryItem.SpLevel,
                    SlDefence = inventory.InventoryItem.SlDefence,
                    SlElement = inventory.InventoryItem.SlElement,
                    SlDamage = inventory.InventoryItem.SlDamage,
                    SlHP = inventory.InventoryItem.SlHP,
                    Upgrade = inventory.InventoryItem.Upgrade,
                    WaterElement = inventory.InventoryItem.WaterElement,
                    Ammo = inventory.InventoryItem.Ammo,
                    Cellon = inventory.InventoryItem.Cellon,
                    CriticalDodge = inventory.InventoryItem.CriticalDodge,
                    DarkResistance = inventory.InventoryItem.DarkResistance,
                    FireResistance = inventory.InventoryItem.FireResistance,
                    HP = inventory.InventoryItem.HP,
                    IsEmpty = inventory.InventoryItem.IsEmpty,
                    IsFixed = inventory.InventoryItem.IsFixed,
                    IsUsed = inventory.InventoryItem.IsUsed,
                    ItemDeleteTime = inventory.InventoryItem.ItemDeleteTime,
                    LightResistance = inventory.InventoryItem.LightResistance,
                    MP = inventory.InventoryItem.MP,
                    SpDamage = inventory.InventoryItem.SpDamage,
                    SpDark = inventory.InventoryItem.SpDark,
                    SpDefence = inventory.InventoryItem.SpDefence,
                    SpElement = inventory.InventoryItem.SpElement,
                    SpFire = inventory.InventoryItem.SpFire,
                    SpHP = inventory.InventoryItem.SpHP,
                    SpLight = inventory.InventoryItem.SpLight,
                    SpStoneUpgrade = inventory.InventoryItem.SpStoneUpgrade,
                    SpWater = inventory.InventoryItem.SpWater,
                    WaterResistance = inventory.InventoryItem.WaterResistance
                };
                if (inventory.Type != (byte)InventoryType.Equipment)
                    InventoryList.Inventory.Add(new Inventory
                    {
                        CharacterId = inventory.CharacterId,
                        Slot = inventory.Slot,
                        InventoryId = inventory.InventoryId,
                        Type = inventory.Type,
                        InventoryItem = invitem
                    });
                else
                    EquipmentList.Inventory.Add(new Inventory
                    {
                        CharacterId = inventory.CharacterId,
                        Slot = inventory.Slot,
                        InventoryId = inventory.InventoryId,
                        Type = inventory.Type,
                        InventoryItem = invitem
                    });
            }
        }

        public double MPLoad()
        {
            int mp = 0;
            double multiplicator = 1.0;
            if (UseSp)
            {
                Inventory inventory = EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
                if (inventory != null)
                {
                    int point = ServersData.SlPoint(inventory.InventoryItem.SlHP, 3);
                    if (point <= 50)
                        multiplicator += point / 100.0;
                    else
                        multiplicator += 0.5 + (point - 50.00) / 50.00; ;

                    mp = inventory.InventoryItem.MP;
                }
            }
            return (int)((ServersData.MPData[Class, Level] + mp) * multiplicator);
        }

        public void Save()
        {
            CharacterDTO tempsave = this;
            SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref tempsave); // unused variable
            foreach (InventoryDTO inv in DAOFactory.InventoryDAO.LoadByCharacterId(CharacterId))
            {
                if (inv.Type == (byte)InventoryType.Equipment)
                {
                    if (EquipmentList.LoadBySlotAndType(inv.Slot, inv.Type) == null)
                    {
                        DAOFactory.InventoryDAO.DeleteFromSlotAndType(CharacterId, inv.Slot, inv.Type);
                    }
                }
                else
                {
                    if (InventoryList.LoadBySlotAndType(inv.Slot, inv.Type) == null)
                    {
                        DAOFactory.InventoryDAO.DeleteFromSlotAndType(CharacterId, inv.Slot, inv.Type);
                    }
                }
            }

            for (int i = 0; i < InventoryList.Inventory.Count(); i++)
                InventoryList.Inventory[i].Save();
            for (int i = 0; i < EquipmentList.Inventory.Count(); i++)
                EquipmentList.Inventory[i].Save();
        }

        public double SPXPLoad()
        {
            return ServersData.SpXPData[JobLevel - 1];
        }

        public bool Update()
        {
            try
            {
                CharacterDTO characterToUpdate = Mapper.Map<CharacterDTO>(this);
                DAOFactory.CharacterDAO.InsertOrUpdate(ref characterToUpdate);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public double XPLoad()
        {
            return ServersData.XPData[Level - 1];
        }

        #endregion
    }
}