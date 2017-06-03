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

using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject.Helpers
{
    public class UserInterfaceHelper
    {
        #region Members

        private static UserInterfaceHelper instance;

        #endregion

        #region Properties

        public static UserInterfaceHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserInterfaceHelper();
                }
                return instance;
            }
        }

        #endregion

        #region Methods

        public string GenerateDelay(int delay, int type, string argument)
        {
            return $"delay {delay} {type} {argument}";
        }

        public string GenerateDialog(string dialog)
        {
            return $"dlg {dialog}";
        }

        public string GenerateFrank(byte type)
        {
            string packet = "frank_stc";
            int rank = 1;
            long savecount = 0;

            List<Family> familyordered = null;
            switch (type)
            {
                case 0:
                    familyordered = ServerManager.Instance.FamilyList.OrderByDescending(s => s.FamilyExperience).ToList();
                    break;

                case 1:
                    familyordered = ServerManager.Instance.FamilyList.OrderByDescending(s => s.FamilyLogs.Where(l => l.FamilyLogType == FamilyLogType.FamilyXP && l.Timestamp.AddDays(30) < DateTime.Now).ToList().Sum(c => long.Parse(c.FamilyLogData.Split('|')[1]))).ToList();//use month instead log
                    break;

                case 2:
                    familyordered = ServerManager.Instance.FamilyList.OrderByDescending(s => s.FamilyCharacters.Sum(c => c.Character.Reput)).ToList();//use month instead log
                    break;

                case 3:
                    familyordered = ServerManager.Instance.FamilyList.OrderByDescending(s => s.FamilyCharacters.Sum(c => c.Character.Reput)).ToList();
                    break;
            }
            int i = 0;
            if (familyordered != null)
            {
                foreach (Family fam in familyordered.Take(100))
                {
                    i++;
                    long sum = 0;
                    switch (type)
                    {
                        case 0:
                            if (savecount != fam.FamilyExperience)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = fam.FamilyExperience;
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{fam.FamilyExperience}";//replace by month log
                            break;

                        case 1:
                            if (savecount != fam.FamilyExperience)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = fam.FamilyExperience;
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{fam.FamilyExperience}";
                            break;

                        case 2:
                            sum = fam.FamilyCharacters.Sum(c => c.Character.Reput);
                            if (savecount != sum)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = sum;//replace by month log
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{savecount}";
                            break;

                        case 3:
                            sum = fam.FamilyCharacters.Sum(c => c.Character.Reput);
                            if (savecount != sum)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = sum;
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{savecount}";
                            break;
                    }
                }
            }
            return packet;
        }

        public string GenerateFStashRemove(short slot)
        {
            return $"f_stash {GenerateRemovePacket(slot)}";
        }

        public string GenerateGuri(byte type, byte argument, long CharacterId, int value = 0)
        {
            switch (type)
            {
                case 2:
                    return $"guri 2 {argument} {CharacterId}";

                case 6:
                    return $"guri 6 1 {CharacterId} 0 0";

                case 10:
                    return $"guri 10 {argument} {value} {CharacterId}";

                case 15:
                    return $"guri 15 {argument} 0 0";

                default:
                    return $"guri {type} {argument} {CharacterId} {value}";
            }
        }

        public string GenerateInbox(string value)
        {
            return $"inbox {value}";
        }

        public string GenerateInfo(string message)
        {
            return $"info {message}";
        }

        public string GenerateInventoryRemove(InventoryType Type, short Slot)
        {
            return $"ivn {(byte)Type} {GenerateRemovePacket(Slot)}";
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

        public string GeneratePClear()
        {
            return "p_clear";
        }

        public string GeneratePStashRemove(short slot)
        {
            return $"pstash {GenerateRemovePacket(slot)}";
        }

        public string GenerateRCBList(CBListPacket packet)
        {
            string itembazar = string.Empty;

            List<string> itemssearch = packet.ItemVNumFilter == "0" ? new List<string>() : packet.ItemVNumFilter.Split(' ').ToList();
            List<BazaarItemLink> bzlist = new List<BazaarItemLink>();
            BazaarItemLink[] billist = new BazaarItemLink[ServerManager.Instance.BazaarList.Count + 20];
            ServerManager.Instance.BazaarList.CopyTo(billist);
            foreach (BazaarItemLink bz in billist)
            {
                if (bz?.Item == null)
                {
                    continue;
                }

                switch (packet.TypeFilter)
                {
                    case BazaarListType.Weapon:
                        if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Weapon)
                            if (packet.SubTypeFilter == 0 || ((bz.Item.Item.Class + 1 >> packet.SubTypeFilter) & 1) == 1)
                                if (packet.LevelFilter == 0 || packet.LevelFilter == 11 && bz.Item.Item.IsHeroic || bz.Item.Item.LevelMinimum < packet.LevelFilter * 10 + 1 && bz.Item.Item.LevelMinimum >= packet.LevelFilter * 10 - 9)//Level filter
                                    if (packet.RareFilter == 0 || packet.RareFilter == bz.Item.Rare + 1)
                                        if (packet.UpgradeFilter == 0 || packet.UpgradeFilter == bz.Item.Upgrade + 1)
                                            bzlist.Add(bz);
                        break;

                    case BazaarListType.Armor:
                        if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Armor)
                            if (packet.SubTypeFilter == 0 || ((bz.Item.Item.Class + 1 >> packet.SubTypeFilter) & 1) == 1)
                                if (packet.LevelFilter == 0 || packet.LevelFilter == 11 && bz.Item.Item.IsHeroic || bz.Item.Item.LevelMinimum < packet.LevelFilter * 10 + 1 && bz.Item.Item.LevelMinimum >= packet.LevelFilter * 10 - 9)//Level filter
                                    if (packet.RareFilter == 0 || packet.RareFilter == bz.Item.Rare + 1)
                                        if (packet.UpgradeFilter == 0 || packet.UpgradeFilter == bz.Item.Upgrade + 1)
                                            bzlist.Add(bz);
                        break;

                    case BazaarListType.Equipment:
                        if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Fashion)
                            if (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 2 && bz.Item.Item.EquipmentSlot == EquipmentType.Mask || packet.SubTypeFilter == 1 && bz.Item.Item.EquipmentSlot == EquipmentType.Hat || packet.SubTypeFilter == 6 && bz.Item.Item.EquipmentSlot == EquipmentType.CostumeHat || packet.SubTypeFilter == 5 && bz.Item.Item.EquipmentSlot == EquipmentType.CostumeSuit || packet.SubTypeFilter == 3 && bz.Item.Item.EquipmentSlot == EquipmentType.Gloves || packet.SubTypeFilter == 4 && bz.Item.Item.EquipmentSlot == EquipmentType.Boots)
                                if (packet.LevelFilter == 0 || packet.LevelFilter == 11 && bz.Item.Item.IsHeroic || bz.Item.Item.LevelMinimum < packet.LevelFilter * 10 + 1 && bz.Item.Item.LevelMinimum >= packet.LevelFilter * 10 - 9)//Level filter
                                    bzlist.Add(bz);
                        break;

                    case BazaarListType.Jewelery:
                        if (bz.Item.Item.Type == InventoryType.Equipment && bz.Item.Item.ItemType == ItemType.Jewelery)
                            if (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 2 && bz.Item.Item.EquipmentSlot == EquipmentType.Ring || packet.SubTypeFilter == 1 && bz.Item.Item.EquipmentSlot == EquipmentType.Necklace || packet.SubTypeFilter == 5 && bz.Item.Item.EquipmentSlot == EquipmentType.Amulet || packet.SubTypeFilter == 3 && bz.Item.Item.EquipmentSlot == EquipmentType.Bracelet || packet.SubTypeFilter == 4 && (bz.Item.Item.EquipmentSlot == EquipmentType.Fairy || bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 5))
                                if (packet.LevelFilter == 0 || packet.LevelFilter == 11 && bz.Item.Item.IsHeroic || bz.Item.Item.LevelMinimum < packet.LevelFilter * 10 + 1 && bz.Item.Item.LevelMinimum >= packet.LevelFilter * 10 - 9)//Level filter
                                    bzlist.Add(bz);
                        break;

                    case BazaarListType.Specialist:
                        if (bz.Item.Item.Type == InventoryType.Equipment)
                            if (bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 2)
                            {
                                if (bz.Item is BoxInstance boxInstance)
                                {
                                    if (packet.SubTypeFilter == 0)
                                    {
                                        if (packet.LevelFilter == 0 || ((BoxInstance)bz.Item).SpLevel < packet.LevelFilter * 10 + 1 && ((BoxInstance)bz.Item).SpLevel >= packet.LevelFilter * 10 - 9)
                                            if (packet.UpgradeFilter == 0 || packet.UpgradeFilter == bz.Item.Upgrade + 1)
                                                if (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 1 && ((BoxInstance)bz.Item).HoldingVNum == 0 || packet.SubTypeFilter == 2 && ((BoxInstance)bz.Item).HoldingVNum != 0)
                                                    bzlist.Add(bz);
                                    }
                                    else if (boxInstance.HoldingVNum == 0)
                                    {
                                        if (packet.SubTypeFilter == 1)
                                        {
                                            if (packet.LevelFilter == 0 || ((BoxInstance)bz.Item).SpLevel < packet.LevelFilter * 10 + 1 && ((BoxInstance)bz.Item).SpLevel >= packet.LevelFilter * 10 - 9)
                                                if (packet.UpgradeFilter == 0 || packet.UpgradeFilter == bz.Item.Upgrade + 1)
                                                    if (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 1 && ((BoxInstance)bz.Item).HoldingVNum == 0 || packet.SubTypeFilter == 2 && ((BoxInstance)bz.Item).HoldingVNum != 0)
                                                        bzlist.Add(bz);
                                        }
                                    }
                                    else if (packet.SubTypeFilter == 2 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 10
                                        || packet.SubTypeFilter == 3 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 11
                                        || packet.SubTypeFilter == 4 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 2
                                        || packet.SubTypeFilter == 5 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 3
                                        || packet.SubTypeFilter == 6 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 13
                                        || packet.SubTypeFilter == 7 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 5
                                        || packet.SubTypeFilter == 8 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 12
                                        || packet.SubTypeFilter == 9 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 4
                                        || packet.SubTypeFilter == 10 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 7
                                        || packet.SubTypeFilter == 11 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 15
                                        || packet.SubTypeFilter == 12 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 6
                                        || packet.SubTypeFilter == 13 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 14
                                        || packet.SubTypeFilter == 14 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 9
                                        || packet.SubTypeFilter == 15 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 8
                                        || packet.SubTypeFilter == 16 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 1
                                        || packet.SubTypeFilter == 17 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 16
                                        || packet.SubTypeFilter == 18 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 17
                                        || packet.SubTypeFilter == 19 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 18
                                        || packet.SubTypeFilter == 20 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 19
                                        || packet.SubTypeFilter == 21 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 20
                                        || packet.SubTypeFilter == 22 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 21
                                        || packet.SubTypeFilter == 23 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 22
                                        || packet.SubTypeFilter == 24 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 23
                                        || packet.SubTypeFilter == 25 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 24
                                        || packet.SubTypeFilter == 26 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 25
                                        || packet.SubTypeFilter == 27 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 26
                                        || packet.SubTypeFilter == 28 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 27
                                        || packet.SubTypeFilter == 29 && ServerManager.Instance.GetItem(boxInstance.HoldingVNum).Morph == 28)
                                    {
                                        if (packet.LevelFilter == 0 || ((BoxInstance)bz.Item).SpLevel < packet.LevelFilter * 10 + 1 && ((BoxInstance)bz.Item).SpLevel >= packet.LevelFilter * 10 - 9)
                                            if (packet.UpgradeFilter == 0 || packet.UpgradeFilter == bz.Item.Upgrade + 1)
                                                if (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 1 && ((BoxInstance)bz.Item).HoldingVNum == 0 || packet.SubTypeFilter >= 2 && ((BoxInstance)bz.Item).HoldingVNum != 0)
                                                    bzlist.Add(bz);
                                    }
                                }
                            }
                        break;

                    case BazaarListType.Pet:
                        if (bz.Item.Item.Type == InventoryType.Equipment)
                            if (bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 0)
                            {
                                if (bz.Item is BoxInstance instance && (packet.LevelFilter == 0 || instance.SpLevel < packet.LevelFilter * 10 + 1 && instance.SpLevel >= packet.LevelFilter * 10 - 9))
                                    if (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 1 && ((BoxInstance)bz.Item).HoldingVNum == 0 || packet.SubTypeFilter == 2 && ((BoxInstance)bz.Item).HoldingVNum != 0)
                                        bzlist.Add(bz);
                            }
                        break;

                    case BazaarListType.Npc:
                        if (bz.Item.Item.Type == InventoryType.Equipment)
                            if (bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 1)
                            {
                                if (bz.Item is BoxInstance box && (packet.LevelFilter == 0 || box.SpLevel < packet.LevelFilter * 10 + 1 && box.SpLevel >= packet.LevelFilter * 10 - 9))
                                    if (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 1 && ((BoxInstance)bz.Item).HoldingVNum == 0 || packet.SubTypeFilter == 2 && ((BoxInstance)bz.Item).HoldingVNum != 0)
                                        bzlist.Add(bz);
                            }
                        break;

                    case BazaarListType.Shell:
                        if (bz.Item.Item.Type == InventoryType.Equipment)
                            if (bz.Item.Item.ItemType == ItemType.Shell)
                                if (packet.SubTypeFilter == 0 || bz.Item.Item.ItemSubType == bz.Item.Item.ItemSubType + 1)
                                    if (packet.RareFilter == 0 || packet.RareFilter == bz.Item.Rare + 1)
                                    {
                                        if (bz.Item is BoxInstance box && (packet.LevelFilter == 0 || box.SpLevel < packet.LevelFilter * 10 + 1 && box.SpLevel >= packet.LevelFilter * 10 - 9))
                                            bzlist.Add(bz);
                                    }
                        break;

                    case BazaarListType.Main:
                        if (bz.Item.Item.Type == InventoryType.Main)
                            if (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 1 && bz.Item.Item.ItemType == ItemType.Main || packet.SubTypeFilter == 2 && bz.Item.Item.ItemType == ItemType.Upgrade || packet.SubTypeFilter == 3 && bz.Item.Item.ItemType == ItemType.Production || packet.SubTypeFilter == 4 && bz.Item.Item.ItemType == ItemType.Special || packet.SubTypeFilter == 5 && bz.Item.Item.ItemType == ItemType.Potion || packet.SubTypeFilter == 6 && bz.Item.Item.ItemType == ItemType.Event)
                                bzlist.Add(bz);
                        break;

                    case BazaarListType.Usable:
                        if (bz.Item.Item.Type == InventoryType.Etc)
                            if (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 1 && bz.Item.Item.ItemType == ItemType.Food || packet.SubTypeFilter == 2 && bz.Item.Item.ItemType == ItemType.Snack || packet.SubTypeFilter == 3 && bz.Item.Item.ItemType == ItemType.Magical || packet.SubTypeFilter == 4 && bz.Item.Item.ItemType == ItemType.Part || packet.SubTypeFilter == 5 && bz.Item.Item.ItemType == ItemType.Teacher || packet.SubTypeFilter == 6 && bz.Item.Item.ItemType == ItemType.Sell)
                                bzlist.Add(bz);
                        break;

                    case BazaarListType.Other:
                        if (bz.Item.Item.Type == InventoryType.Equipment)
                            if (bz.Item.Item.ItemType == ItemType.Box && !bz.Item.Item.IsHolder)
                                bzlist.Add(bz);
                        break;

                    case BazaarListType.Vehicle:
                        if (bz.Item.Item.ItemType == ItemType.Box && bz.Item.Item.ItemSubType == 4)
                            if (bz.Item is BoxInstance box && (packet.SubTypeFilter == 0 || packet.SubTypeFilter == 1 && box.HoldingVNum == 0 || packet.SubTypeFilter == 2 && box.HoldingVNum != 0))
                                bzlist.Add(bz);
                        break;

                    default:
                        bzlist.Add(bz);
                        break;
                }
            }
            List<BazaarItemLink> bzlistsearched = bzlist.Where(s => itemssearch.Contains(s.Item.ItemVNum.ToString())).ToList();

            //price up price down quantity up quantity down
            List<BazaarItemLink> definitivelist = itemssearch.Any() ? bzlistsearched : bzlist;
            switch (packet.OrderFilter)
            {
                case 0:
                    definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ThenBy(s => s.BazaarItem.Price).ToList();
                    break;

                case 1:
                    definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ThenByDescending(s => s.BazaarItem.Price).ToList();
                    break;

                case 2:
                    definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ThenBy(s => s.BazaarItem.Amount).ToList();
                    break;

                case 3:
                    definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ThenByDescending(s => s.BazaarItem.Amount).ToList();
                    break;

                default:
                    definitivelist = definitivelist.OrderBy(s => s.Item.Item.Name).ToList();
                    break;
            }
            foreach (BazaarItemLink bzlink in definitivelist.Where(s => (s.BazaarItem.DateStart.AddHours(s.BazaarItem.Duration) - DateTime.Now).TotalMinutes > 0 && s.Item.Amount > 0).Skip(packet.Index * 50).Take(50))
            {
                long time = (long)(bzlink.BazaarItem.DateStart.AddHours(bzlink.BazaarItem.Duration) - DateTime.Now).TotalMinutes;
                string info = string.Empty;
                if (bzlink.Item.Item.Type == InventoryType.Equipment)
                    info = (bzlink.Item.Item.EquipmentSlot != EquipmentType.Sp ?
                        (bzlink.Item as WearableInstance).GenerateEInfo() : bzlink.Item.Item.SpType == 0 && bzlink.Item.Item.ItemSubType == 4 ?
                        (bzlink.Item as SpecialistInstance).GeneratePslInfo() : (bzlink.Item as SpecialistInstance).GenerateSlInfo()).Replace(' ', '^').Replace("slinfo^", "").Replace("e_info^", "");

                itembazar += $"{bzlink.BazaarItem.BazaarItemId}|{bzlink.BazaarItem.SellerId}|{bzlink.Owner}|{bzlink.Item.Item.VNum}|{bzlink.Item.Amount}|{(bzlink.BazaarItem.IsPackage ? 1 : 0)}|{bzlink.BazaarItem.Price}|{time}|2|0|{bzlink.Item.Rare}|{bzlink.Item.Upgrade}|{info} ";
            }

            return $"rc_blist {packet.Index} {itembazar} ";
        }

        public string GenerateRl(byte type)
        {
            string str = $"rl {type}";
            ServerManager.Instance.GroupList.ForEach(s=>
            {
                ClientSession leader = s.Characters.ElementAt(0);
                str += $" {s.Raid.Id}.{s.Raid?.LevelMinimum}.{s.Raid?.LevelMaximum}.{leader.Character.Name}.{leader.Character.Level}.{(leader.Character.UseSp ? leader.Character.Morph : -1)}.{(byte)leader.Character.Class}.{(byte)leader.Character.Gender}.{s.CharacterCount}.{leader.Character.HeroLevel}";
            });
            return str;
        }

        public string GenerateRemovePacket(short slot)
        {
            return $"{slot}.-1.0.0.0";
        }

        public string GenerateRp(int mapid, int x, int y, string param)
        {
            return $"rp {mapid} {x} {y} {param}";
        }

        public string GenerateShopMemo(int type, string message)
        {
            return $"s_memo {type} {message}";
        }

        public string GenerateStashRemove(short slot)
        {
            return $"stash {GenerateRemovePacket(slot)}";
        }

        public IEnumerable<string> GenerateVb()
        {
            return new[] { "vb 340 0 0", "vb 339 0 0", "vb 472 0 0", "vb 471 0 0" };
        }

        #endregion
    }
}