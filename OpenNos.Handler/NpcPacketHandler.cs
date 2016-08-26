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
using OpenNos.Domain;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.Handler
{
    public class NpcPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public NpcPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        public ClientSession Session { get { return _session; } }

        #endregion

        #region Methods

        [Packet("buy")]
        public void BuyShop(string packet)
        {
            if (Session.Character.InExchangeOrTrade)
                return;

            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length < 5)
                return;
            long owner; long.TryParse(packetsplit[3], out owner);
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[4], out slot);
            byte amount = 0;
            if (packetsplit.Length == 6)
                byte.TryParse(packetsplit[5], out amount);

            if (type == 1) // User shop
            {
                KeyValuePair<long, MapShop> shop = Session.CurrentMap.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(owner));
                PersonalShopItem item = shop.Value.Items.FirstOrDefault(i => i.Slot.Equals(slot));
                if (item == null || amount <= 0) return;

                if (amount > item.Amount)
                    amount = item.Amount;

                if (item.Price * amount + ServerManager.Instance.GetProperty<long>(shop.Value.OwnerId, "Gold") > 1000000000)
                {
                    Session.Client.SendPacket(Session.Character.GenerateShopMemo(3,
                        Language.Instance.GetMessageFromKey("MAX_GOLD")));
                    return;
                }

                if (item.Price * amount >= Session.Character.Gold)
                {
                    Session.Client.SendPacket(Session.Character.GenerateShopMemo(3,
                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                    return;
                }

                ItemInstance item2 = (item.ItemInstance as ItemInstance).DeepCopy();
                item2.Amount = amount;
                item2.ItemInstanceId = Session.Character.InventoryList.GenerateItemInstanceId();
                Inventory inv = Session.Character.InventoryList.AddToInventory(item2);

                if (inv != null)
                {
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum,
                        inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade, 0));
                    Session.Character.Gold -= item.Price * amount;
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    ServerManager.Instance.BuyValidate(Session, shop, slot, amount);
                    KeyValuePair<long, MapShop> shop2 = Session.CurrentMap.UserShops.FirstOrDefault(s => s.Value.OwnerId.Equals(owner));
                    LoadShopItem(owner, shop2);
                }
                else
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
            }
            else if (packetsplit.Length == 5) // skill shop
            {
                if (Session.Character.UseSp)
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_SP"), 0));
                    return;
                }
                Skill skillinfo = ServerManager.GetSkill(slot);

                if (skillinfo == null)
                    return;
                if (Session.Character.Gold < skillinfo.Price)
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 0));
                else if (Session.Character.GetCP() < skillinfo.CPCost)
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_CP"), 0));
                else
                {
                    if (skillinfo.SkillVNum < 200)
                    {
                        int SkillMiniumLevel = 0;
                        if (skillinfo.MinimumSwordmanLevel == 0 && skillinfo.MinimumArcherLevel == 0 && skillinfo.MinimumMagicianLevel == 0)
                            SkillMiniumLevel = skillinfo.MinimumAdventurerLevel;
                        else
                        {
                            switch (Session.Character.Class)
                            {
                                case (byte)ClassType.Adventurer:
                                    SkillMiniumLevel = skillinfo.MinimumAdventurerLevel;
                                    break;

                                case (byte)ClassType.Swordman:
                                    SkillMiniumLevel = skillinfo.MinimumSwordmanLevel;
                                    break;

                                case (byte)ClassType.Archer:
                                    SkillMiniumLevel = skillinfo.MinimumArcherLevel;
                                    break;

                                case (byte)ClassType.Magician:
                                    SkillMiniumLevel = skillinfo.MinimumMagicianLevel;
                                    break;
                            }
                        }
                        if (SkillMiniumLevel == 0)
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_CANT_LEARN"), 0));
                            return;
                        }
                        else if (Session.Character.Level < SkillMiniumLevel)
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                            return;
                        }
                        for (int i = Session.Character.Skills.Count - 1; i >= 0; i--)
                        {
                            Skill skinfo = ServerManager.GetSkill(Session.Character.Skills[i].SkillVNum);
                            if ((skillinfo.CastId == skinfo.CastId) && (skinfo.SkillVNum < 200))
                                Session.Character.Skills.Remove(Session.Character.Skills[i]);
                        }
                    }
                    else
                    {
                        if (Session.Character.Class != skillinfo.Class)
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_CANT_LEARN"), 0));
                            return;
                        }
                        else if (Session.Character.JobLevel < skillinfo.LevelMinimum)
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 0));
                            return;
                        }
                    }

                    if (Session.Character.Skills.FirstOrDefault(s => s.SkillVNum == slot) != null)
                        return;

                    if (skillinfo.UpgradeSkill != 0)
                    {
                        CharacterSkill oldupgrade = Session.Character.Skills.FirstOrDefault(s => s.Skill.UpgradeSkill == skillinfo.UpgradeSkill && s.Skill.UpgradeType == skillinfo.UpgradeType && s.Skill.UpgradeSkill != 0);
                        if (oldupgrade != null)
                        {
                            Session.Character.Skills.Remove(oldupgrade);
                        }
                    }

                    Session.Character.Skills.Add(new CharacterSkill() { SkillVNum = slot, CharacterId = Session.Character.CharacterId });

                    Session.Character.Gold -= skillinfo.Price;
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    Session.Client.SendPacket(Session.Character.GenerateSki());

                    string[] quicklistpackets = Session.Character.GenerateQuicklist();
                    foreach (string quicklist in quicklistpackets)
                        Session.Client.SendPacket(quicklist);

                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                }
            }
            else
            {
                MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(n => n.MapNpcId.Equals((short)owner));

                ShopItem item = npc?.Shop.ShopItems.FirstOrDefault(it => it.Slot == slot);
                if (item == null) return;
                Item iteminfo = ServerManager.GetItem(item.ItemVNum);
                long price = iteminfo.Price * amount;
                long Reputprice = iteminfo.ReputPrice * amount;
                double percent = 1;
                if (Session.Character.GetDignityIco() == 3)
                    percent = 1.10;
                else if (Session.Character.GetDignityIco() == 4)
                    percent = 1.20;
                else if (Session.Character.GetDignityIco() == 5 || Session.Character.GetDignityIco() == 6)
                    percent = 1.5;
                sbyte rare = (sbyte)item.Rare;
                if (iteminfo.Type == 0)
                    amount = 1;

                if (iteminfo.ReputPrice == 0)
                {
                    if (price < 0 || price * percent > Session.Character.Gold)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                        return;
                    }
                }
                else
                {
                    if (Reputprice <= 0 || Reputprice > Session.Character.Reput)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_REPUT")));
                        return;
                    }
                    Random rnd = new Random();
                    byte ra = (byte)rnd.Next(0, 100);

                    int[] rareprob = { 100, 100, 70, 50, 30, 15, 5, 1 };
                    if (iteminfo.ReputPrice != 0)
                        for (int i = 0; i < rareprob.Length; i++)
                        {
                            if (ra <= rareprob[i])
                                rare = (sbyte)i;
                        }
                }

                Inventory newItem = Session.Character.InventoryList.AddNewItemToInventory(item.ItemVNum, amount);
                newItem.ItemInstance.Rare = rare;
                newItem.ItemInstance.Upgrade = item.Upgrade;
                newItem.ItemInstance.Design = item.Color;

                if (newItem != null && newItem.Slot != -1)
                {
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(newItem.ItemInstance.ItemVNum,
                        newItem.ItemInstance.Amount, newItem.Type, newItem.Slot, newItem.ItemInstance.Rare, newItem.ItemInstance.Design, newItem.ItemInstance.Upgrade, 0));
                    if (iteminfo.ReputPrice == 0)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALID"), ServerManager.GetItem(item.ItemVNum).Name, amount)));
                        Session.Character.Gold -= (long)(price * percent);
                        Session.Client.SendPacket(Session.Character.GenerateGold());
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALID"), ServerManager.GetItem(item.ItemVNum).Name, amount)));
                        Session.Character.Reput -= (long)(Reputprice);
                        Session.Client.SendPacket(Session.Character.GenerateFd());
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("REPUT_DECREASED"), 11));
                    }
                }
                else
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
            }
        }

        [Packet("m_shop")]
        public void CreateShop(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte[] type = new byte[20];
            long[] gold = new long[20];
            short[] slot = new short[20];
            byte[] qty = new byte[20];

            string shopname = String.Empty;
            if (packetsplit.Length > 2)
            {
                short typePacket; short.TryParse(packetsplit[2], out typePacket);
                if (Session.Character.InExchangeOrTrade && typePacket != 1)
                    return;
                foreach (Portal por in Session.CurrentMap.Portals)
                {
                    if (Session.Character.MapX < por.SourceX + 6 && Session.Character.MapX > por.SourceX - 6 && Session.Character.MapY < por.SourceY + 6 && Session.Character.MapY > por.SourceY - 6)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NEAR_PORTAL"), 0));
                        return;
                    }
                }

                if (typePacket == 2)
                {
                    Session.Client.SendPacket("ishop");
                }
                else if (typePacket == 0)
                {
                    if (Session.CurrentMap.UserShops.Where(s => s.Value.OwnerId == Session.Character.CharacterId).Count() != 0)
                    {
                        return;
                    }
                    MapShop myShop = new MapShop();

                    if (packetsplit.Length > 2)
                        for (short j = 3, i = 0; j <= packetsplit.Length - 5; j += 4, i++)
                        {
                            byte.TryParse(packetsplit[j], out type[i]);
                            short.TryParse(packetsplit[j + 1], out slot[i]);
                            byte.TryParse(packetsplit[j + 2], out qty[i]);

                            long.TryParse(packetsplit[j + 3], out gold[i]);
                            if (gold[i] < 0)
                                return;
                            if (qty[i] > 0)
                            {
                                Inventory inv = Session.Character.InventoryList.LoadInventoryBySlotAndType(slot[i], type[i]);

                                if (inv.ItemInstance.Amount < qty[i])
                                    return;
                                if (!((ItemInstance)inv.ItemInstance).Item.IsTradable)
                                {
                                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_ONLY_TRADABLE_ITEMS"), 0));
                                    return;
                                }

                                PersonalShopItem personalshopitem = new PersonalShopItem()
                                {
                                    Slot = slot[i],
                                    Type = type[i],
                                    Price = gold[i],
                                    InventoryId = inv.InventoryId,
                                    CharacterId = inv.CharacterId,
                                    Amount = qty[i],
                                    ItemInstance = inv.ItemInstance
                                };
                                myShop.Items.Add(personalshopitem);
                            }
                        }
                    if (myShop.Items.Count != 0)
                    {
                        for (int i = 83; i < packetsplit.Length; i++)
                            shopname += $"{packetsplit[i]} ";

                        //trim shopname
                        shopname.TrimEnd(' ');

                        //create default shopname if it's empty
                        if (String.IsNullOrWhiteSpace(shopname) || String.IsNullOrEmpty(shopname))
                        {
                            shopname = Language.Instance.GetMessageFromKey("SHOP_PRIVATE_SHOP");
                        }

                        //truncate the string to a max-length of 20
                        shopname = StringHelper.Truncate(shopname, 20);

                        myShop.OwnerId = Session.Character.CharacterId;
                        myShop.Name = shopname;

                        Session.CurrentMap.UserShops.Add(Session.CurrentMap.UserShops.Count(), myShop);
                        Session.Character.HasShopOpened = true;

                        Session.CurrentMap?.Broadcast(Session, Session.Character.GeneratePlayerFlag(Session.CurrentMap.UserShops.Count()), ReceiverType.AllExceptMe);
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateShop(shopname));

                        Session.Client.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("SHOP_OPEN")));
                        Session.Character.IsSitting = true;
                        Session.Character.LastSpeed = Session.Character.Speed;
                        Session.Character.Speed = 0;
                        Session.Client.SendPacket(Session.Character.GenerateCond());

                        Session.CurrentMap?.Broadcast(Session.Character.GenerateRest());
                    }
                    else
                    {
                        Session.Client.SendPacket("shop_end 0");
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SHOP_EMPTY"), 10));
                    }
                }
                else if (typePacket == 1)
                {
                    KeyValuePair<long, MapShop> shop = Session.CurrentMap.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId));
                    Session.CurrentMap.UserShops.Remove(shop.Key);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateShopEnd());
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GeneratePlayerFlag(0), ReceiverType.AllExceptMe);
                    Session.Character.Speed = Session.Character.LastSpeed != 0 ? Session.Character.LastSpeed : Session.Character.Speed;
                    Session.Character.IsSitting = false;
                    Session.Client.SendPacket(Session.Character.GenerateCond());
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateRest());
                }
            }
        }

        [Packet("n_run")]
        public void NpcRunFunction(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length <= 5) return;

            byte type; byte.TryParse(packetsplit[3], out type);
            short runner; short.TryParse(packetsplit[2], out runner);
            short data3; short.TryParse(packetsplit[4], out data3);
            short npcid; short.TryParse(packetsplit[5], out npcid);
            Session.Character.LastNRunId = npcid;
            if (Session.Character.Hp > 0)
                NRunHandler.NRun(Session, type, runner, data3, npcid);
        }

        [Packet("pdtse")]
        public void Pdtse(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Count() < 4)
                return;

            byte type = 0;
            byte.TryParse(packetsplit[2], out type);
            if (type == 1)
            {
                MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(s => s.MapNpcId == Session.Character.LastNRunId);
                if (npc != null)
                {
                    Recipe rec = npc.Recipes.FirstOrDefault(s => s.ItemVNum == short.Parse(packetsplit[3]));
                    if (rec != null && rec.Amount > 0)
                    {
                        String rece = $"m_list 3 {rec.Amount}";
                        foreach (RecipeItem ite in rec.Items)
                        {
                            if (ite.Amount > 0)
                                rece += String.Format($" {ite.ItemVNum} {ite.Amount}");
                        }
                        rece += " -1";
                        Session.Client.SendPacket(rece);
                    }
                }
            }
            else
            {
                MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(s => s.MapNpcId == Session.Character.LastNRunId);
                if (npc != null)
                {
                    Recipe rec = npc.Recipes.FirstOrDefault(s => s.ItemVNum == short.Parse(packetsplit[3]));
                    if (rec != null)
                    {
                        if (rec.Amount <= 0)
                            return;
                        foreach (RecipeItem ite in rec.Items)
                        {
                            if (Session.Character.InventoryList.CountItem(ite.ItemVNum) < ite.Amount)
                                return;
                        }

                        Inventory inv = Session.Character.InventoryList.AddNewItemToInventory(rec.ItemVNum, rec.Amount);
                        if (inv.ItemInstance.GetType().Equals(typeof(WearableInstance)))
                        {
                            WearableInstance item = inv.ItemInstance as WearableInstance;
                            item.SetRarityPoint();
                        }

                        if (inv != null)
                        {
                            short Slot = inv.Slot;
                            if (Slot != -1)
                            {
                                foreach (RecipeItem ite in rec.Items)
                                {
                                    Session.Character.InventoryList.RemoveItemAmount(ite.ItemVNum, ite.Amount);
                                }
                                Session.Character.GenerateStartupInventory();

                                Session.Client.SendPacket($"pdti 11 {inv.ItemInstance.ItemVNum} {rec.Amount} 29 {inv.ItemInstance.Upgrade} 0");
                                Session.Client.SendPacket($"guri 19 1 {Session.Character.CharacterId} 1324");

                                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("CRAFTED_OBJECT"), (inv.ItemInstance as ItemInstance).Item.Name, rec.Amount), 0));
                            }
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                        }
                    }
                }
            }
        }

        [Packet("sell")]
        public void SellShop(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                return;
            if (packetsplit.Length > 6)
            {
                byte type, amount, slot;
                if (!byte.TryParse(packetsplit[4], out type) || !byte.TryParse(packetsplit[5], out slot) || !byte.TryParse(packetsplit[6], out amount)) return;

                Inventory inv = Session.Character.InventoryList.LoadInventoryBySlotAndType(slot, type);
                if (inv == null || amount > inv.ItemInstance.Amount) return;

                if (!(inv.ItemInstance as ItemInstance).Item.IsSoldable)
                {
                    Session.Client.SendPacket(Session.Character.GenerateShopMemo(2, string.Format(Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"))));
                    return;
                }

                if (Session.Character.Gold + (inv.ItemInstance as ItemInstance).Item.Price * amount > 1000000000)
                {
                    string message = Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                    Session.Client.SendPacket(message);
                    return;
                }
                Session.Character.Gold += ((inv.ItemInstance as ItemInstance).Item.Price / 20) * amount;
                Session.Client.SendPacket(Session.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("SELL_ITEM_VALIDE"), (inv.ItemInstance as ItemInstance).Item.Name, amount)));

                inv = Session.Character.InventoryList.RemoveItemAmountFromInventory(amount, inv.InventoryId);
                if (inv != null)
                {
                    // Send reduced-amount to owners inventory
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade, 0));
                }
                else
                {
                    // Send empty slot to owners inventory
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0, 0));
                }
                Session.Client.SendPacket(Session.Character.GenerateGold());
            }
            else if (packetsplit.Length == 5)
            {
                short vnum = -1;
                short.TryParse(packetsplit[4], out vnum);
                Skill skillinfo = ServerManager.GetSkill(vnum);
                CharacterSkill skill = Session.Character.Skills.FirstOrDefault(s => s.SkillVNum == vnum);
                if (skill == null || skillinfo == null || vnum == (200 + 20 * Session.Character.Class) || vnum == (201 + 20 * Session.Character.Class))
                    return;
                Session.Character.Gold -= skillinfo.Price;
                Session.Client.SendPacket(Session.Character.GenerateGold());

                for (int i = Session.Character.Skills.Count - 1; i >= 0; i--)
                {
                    Skill skinfo = ServerManager.GetSkill(Session.Character.Skills[i].SkillVNum);
                    if (skillinfo.SkillVNum == skinfo.UpgradeSkill)
                        Session.Character.Skills.Remove(Session.Character.Skills[i]);
                }

                Session.Character.Skills.Remove(skill);
                Session.Client.SendPacket(Session.Character.GenerateSki());
                string[] quicklistpackets = Session.Character.GenerateQuicklist();
                foreach (string quicklist in quicklistpackets)
                    Session.Client.SendPacket(quicklist);
                Session.Client.SendPacket(Session.Character.GenerateLev());
            }
        }

        [Packet("shopping")]
        public void Shopping(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            // n_inv 2 1834 0 100 0.13.13.0.0.330 0.14.15.0.0.2299 0.18.120.0.0.3795 0.19.107.0.0.3795 0.20.94.0.0.3795 0.37.95.0.0.5643 0.38.97.0.0.11340 0.39.99.0.0.18564 0.48.108.0.0.5643 0.49.110.0.0.11340 0.50.112.0.0.18564 0.59.121.0.0.5643 0.60.123.0.0.11340 0.61.125.0.0.18564
            string[] packetsplit = packet.Split(' ');
            byte type;
            int NpcId;
            byte typeshop = 0;
            if (!int.TryParse(packetsplit[5], out NpcId) || !byte.TryParse(packetsplit[2], out type)) return;
            if (Session.Character.Speed == 0)
                return;
            MapNpc mapnpc = Session.CurrentMap.Npcs.FirstOrDefault(n => n.MapNpcId.Equals(NpcId));
            NpcMonster npc = ServerManager.GetNpc(mapnpc.NpcVNum);
            if (mapnpc?.Shop == null) return;

            string shoplist = "";
            foreach (ShopItem item in mapnpc.Shop.ShopItems.Where(s => s.Type.Equals(type)))
            {
                Item iteminfo = ServerManager.GetItem(item.ItemVNum);
                double percent = 1;
                if (Session.Character.GetDignityIco() == 3)
                    percent = 1.10;
                else if (Session.Character.GetDignityIco() == 4)
                    percent = 1.20;
                else if (Session.Character.GetDignityIco() == 5 || Session.Character.GetDignityIco() == 6)
                    percent = 1.5;

                if (iteminfo.ReputPrice > 0 && iteminfo.Type == 0)
                    shoplist += $" {iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{item.Rare}.{(iteminfo.IsColored ? item.Color : item.Upgrade)}.{ServerManager.GetItem(item.ItemVNum).ReputPrice}";
                else if (iteminfo.ReputPrice > 0 && iteminfo.Type != 0)
                    shoplist += $" {iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{-1}.{ServerManager.GetItem(item.ItemVNum).ReputPrice}";
                else if (iteminfo.Type != 0)
                    shoplist += $" {iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{-1}.{ServerManager.GetItem(item.ItemVNum).Price * percent}";
                else
                    shoplist += $" {iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{item.Rare}.{(iteminfo.IsColored ? item.Color : item.Upgrade)}.{ServerManager.GetItem(item.ItemVNum).Price * percent}";
            }

            foreach (ShopSkill skill in mapnpc.Shop.ShopSkills.Where(s => s.Type.Equals(type)))
            {
                Skill skillinfo = ServerManager.GetSkill(skill.SkillVNum);

                if (skill.Type != 0)
                {
                    typeshop = 1;
                    if (skillinfo.Class == Session.Character.Class)
                        shoplist += $" {skillinfo.SkillVNum}";
                }
                else
                    shoplist += $" {skillinfo.SkillVNum}";
            }

            Session.Client.SendPacket($"n_inv 2 {mapnpc.MapNpcId} 0 {typeshop}{shoplist}");
        }

        [Packet("npc_req")]
        public void ShowShop(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            // n_inv 1 2 0 0 0.0.302.7.0.990000. 0.1.264.5.6.2500000. 0.2.69.7.0.650000. 0.3.4106.0.0.4200000. -1 0.5.4240.0.0.11200000. 0.6.4240.0.5.24000000. 0.7.4801.0.0.6200000. 0.8.4240.0.10.32000000. 0.9.712.0.3.250000. 0.10.997.0.4.250000. 1.11.1895.4.16000.-1.-1 1.12.1897.6.18000.-1.-1 -1 1.14.1902.3.35000.-1.-1 1.15.1237.2.12000.-1.-1 -1 -1 1.18.1249.3.92000.-1.-1 0.19.4240.0.1.10500000. -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                int mode;
                if (!int.TryParse(packetsplit[2], out mode)) return;

                if (mode == 1)
                {
                    // User Shop
                    if (packetsplit.Length <= 3) return;

                    long owner;
                    if (!long.TryParse(packetsplit[3], out owner)) return;

                    KeyValuePair<long, MapShop> shopList = Session.CurrentMap.UserShops.FirstOrDefault(s => s.Value.OwnerId.Equals(owner));
                    LoadShopItem(owner, shopList);
                }
                else
                {
                    // Npc Shop , ignore if has drop
                    MapNpc npc = ServerManager.GetMap(Session.Character.MapId).Npcs.FirstOrDefault(n => n.MapNpcId.Equals(Convert.ToInt16(packetsplit[3])));
                    NpcMonster mapobject = ServerManager.GetNpc(npc.NpcVNum);

                    if (mapobject.Drops.Any()) // mining mapobjects
                    {
                        Session.Client.SendPacket(Session.Character.GenerateDelay(5000,4, $"#guri^400^{npc.MapNpcId}"));
                    }
                    else if (mapobject.VNumRequired > 0) // mapobject with required item to use
                    {
                        Session.Client.SendPacket(Session.Character.GenerateDelay(6000, 4, $"#guri^400^{npc.MapNpcId}"));
                    }
                    else if (mapobject.MaxHP == 0 && !mapobject.Drops.Any()) // mapobject teleporter
                    {
                        // #guri^710^X^Y^MapNpcId
                        Session.Client.SendPacket(Session.Character.GenerateDelay(5000, 1, $"#guri^710^162^85^{npc.MapNpcId}"));
                    }
                    else if (!string.IsNullOrEmpty(npc?.GetNpcDialog()))
                    {
                        Session.Client.SendPacket(npc.GetNpcDialog());
                    }
                }
            }
        }

        private void LoadShopItem(long owner, KeyValuePair<long, MapShop> shop)
        {
            string packetToSend = $"n_inv 1 {owner} 0 0";
            for (short i = 0; i < 20; i++)
            {
                PersonalShopItem item = shop.Value.Items.Count() > i ? shop.Value.Items.ElementAt(i) : null;
                if (item != null)
                {
                    if ((item.ItemInstance as ItemInstance).Item.Type == 0)
                        packetToSend += $" 0.{i}.{item.ItemInstance.ItemVNum}.{item.ItemInstance.Rare}.{item.ItemInstance.Upgrade}.{item.Price}.";
                    else
                        packetToSend += $" {(item.ItemInstance as ItemInstance).Item.Type}.{i}.{item.ItemInstance.ItemVNum}.{item.Amount}.{item.Price}.-1.";
                }
                else
                {
                    packetToSend += " -1";
                }
            }
            packetToSend += " -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1";

            Session.Client.SendPacket(packetToSend);
        }

        #endregion
    }
}