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
using OpenNos.Data;
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

        public ClientSession Session
        {
            get
            {
                return _session;
            }
        }

        #endregion

        #region Methods

        public void BuyShop(BuyPacket buyPacket)
        {
            if (Session.Character.InExchangeOrTrade)
            {
                return;
            }
            Logger.Debug(buyPacket.ToString(), Session.SessionId);

            Random random = new Random();
            byte amount = buyPacket.Amount;

            switch (buyPacket.Type)
            {
                case BuyShopType.CharacterShop:
                    {
                        // User shop
                        KeyValuePair<long, MapShop> shop = Session.CurrentMap.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(buyPacket.OwnerId));

                        PersonalShopItem item = shop.Value.Items.FirstOrDefault(i => i.ShopSlot.Equals(buyPacket.Slot));
                        if (item == null || amount <= 0 || amount > 99)
                        {
                            return;
                        }
                        if (amount > item.SellAmount)
                        {
                            amount = item.SellAmount;
                        }
                        if (item.Price * amount + ServerManager.Instance.GetProperty<long>(shop.Value.OwnerId, nameof(Character.Gold)) > 1000000000)
                        {
                            Session.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("MAX_GOLD")));
                            return;
                        }

                        if (item.Price * amount >= Session.Character.Gold)
                        {
                            Session.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                            return;
                        }

                        // check if the item has been removed successfully from previous owner and remove it
                        if (BuyValidate(Session, shop, buyPacket.Slot, amount))
                        {
                            ItemInstance inv = item.ItemInstance.Type == InventoryType.Equipment 
                                               ? Session.Character.Inventory.AddToInventory(item.ItemInstance)
                                               : Session.Character.Inventory.AddNewToInventory(item.ItemInstance.ItemVNum, amount, item.ItemInstance.Type);

                            if (inv != null)
                            {
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, 0));
                                Session.Character.Gold -= item.Price * amount;
                                Session.SendPacket(Session.Character.GenerateGold());

                                KeyValuePair<long, MapShop> shop2 = Session.CurrentMap.UserShops.FirstOrDefault(s => s.Value.OwnerId.Equals(buyPacket.OwnerId));
                                LoadShopItem(buyPacket.OwnerId, shop2);
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                            }
                        }

                        break;
                    }
                case BuyShopType.ItemShop:
                    {
                        //load shop
                        MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(n => n.MapNpcId.Equals((short)buyPacket.OwnerId));


                        if (npc == null || npc.Shop == null || Map.GetDistance(new MapCell() { MapId = Session.CurrentMap.MapId, X = Session.Character.MapX, Y = Session.Character.MapY }, new MapCell() { MapId = npc.MapId, X = npc.MapX, Y = npc.MapY }) > 5);
                        {
                            return;
                        }

                        if (npc.Shop.ShopSkills.Any())
                        {
                            // skill shop
                            if (Session.Character.UseSp)
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_SP"), 0));
                                return;
                            }
                            Skill skillinfo = ServerManager.GetSkill(buyPacket.Slot);
                            if (Session.Character.Skills.GetAllItems().Any(s => s.SkillVNum == buyPacket.Slot) || skillinfo == null)
                            {
                                return;
                            }
    
                            if (Session.Character.Gold < skillinfo.Price)
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 0));
                                return;
                            }
                            else if (Session.Character.GetCP() < skillinfo.CPCost)
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_CP"), 0));
                            }
                            else
                            {
                                if (skillinfo.SkillVNum < 200)
                                {
                                    int SkillMiniumLevel = 0;
                                    if (skillinfo.MinimumSwordmanLevel == 0 && skillinfo.MinimumArcherLevel == 0 && skillinfo.MinimumMagicianLevel == 0)
                                    {
                                        SkillMiniumLevel = skillinfo.MinimumAdventurerLevel;
                                    }
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
                                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_CANT_LEARN"), 0));
                                        return;
                                    }
                                    else if (Session.Character.Level < SkillMiniumLevel)
                                    {
                                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                                        return;
                                    }
                                    foreach (var skill in Session.Character.Skills.GetAllItems())
                                    {
                                        if ((skillinfo.CastId == skill.Skill.CastId) && skill.Skill.SkillVNum < 200)
                                        {
                                            Session.Character.Skills.Remove(skill.SkillVNum);
                                        }
                                    }
                                }
                                else
                                {
                                    if (Session.Character.Class != skillinfo.Class)
                                    {
                                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_CANT_LEARN"), 0));
                                        return;
                                    }
                                    else if (Session.Character.JobLevel < skillinfo.LevelMinimum)
                                    {
                                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 0));
                                        return;
                                    }
                                    if (skillinfo.UpgradeSkill != 0)
                                    {
                                        CharacterSkill oldupgrade = Session.Character.Skills.GetAllItems().FirstOrDefault(s => s.Skill.UpgradeSkill == skillinfo.UpgradeSkill && s.Skill.UpgradeType == skillinfo.UpgradeType && s.Skill.UpgradeSkill != 0);
                                        if (oldupgrade != null)
                                        {
                                            Session.Character.Skills.Remove(oldupgrade.SkillVNum);
                                        }
                                    }
                                }

                                Session.Character.Skills[buyPacket.Slot] = new CharacterSkill() { SkillVNum = buyPacket.Slot, CharacterId = Session.Character.CharacterId };

                                Session.Character.Gold -= skillinfo.Price;
                                Session.SendPacket(Session.Character.GenerateGold());
                                Session.SendPacket(Session.Character.GenerateSki());
                                Session.SendPackets(Session.Character.GenerateQuicklist());
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                                Session.SendPacket(Session.Character.GenerateLev());
                            }
                        }
                        else if (npc.Shop.ShopItems.Any())
                        {
                            // npc shop
                            ShopItemDTO item = npc?.Shop.ShopItems.FirstOrDefault(it => it.Slot == buyPacket.Slot);
                            if (item == null || amount <= 0 || amount > 99)
                            {
                                return;
                            }
                            Item iteminfo = ServerManager.GetItem(item.ItemVNum);
                            long price = iteminfo.Price * amount;
                            long Reputprice = iteminfo.ReputPrice * amount;
                            double percent = 1;
                            if (Session.Character.GetDignityIco() == 3)
                            {
                                percent = 1.10;
                            }
                            else if (Session.Character.GetDignityIco() == 4)
                            {
                                percent = 1.20;
                            }
                            else if (Session.Character.GetDignityIco() == 5 || Session.Character.GetDignityIco() == 6)
                            {
                                percent = 1.5;
                            }
                            sbyte rare = item.Rare;
                            if (iteminfo.Type == 0)
                            {
                                amount = 1;
                            }
                            if (iteminfo.ReputPrice == 0)
                            {
                                if (price < 0 || price * percent > Session.Character.Gold)
                                {
                                    Session.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                                    return;
                                }
                            }
                            else
                            {
                                if (Reputprice <= 0 || Reputprice > Session.Character.Reput)
                                {
                                    Session.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_REPUT")));
                                    return;
                                }
                                byte ra = (byte)random.Next(0, 100);

                                int[] rareprob = { 100, 100, 70, 50, 30, 15, 5, 1 };
                                if (iteminfo.ReputPrice != 0)
                                {
                                    for (int i = 0; i < rareprob.Length; i++)
                                    {
                                        if (ra <= rareprob[i])
                                        {
                                            rare = (sbyte)i;
                                        }
                                    }
                                }
                            }

                            ItemInstance newItem = Session.Character.Inventory.AddNewToInventory(item.ItemVNum, amount);
                            if (newItem == null)
                            {
                                return;
                            }

                            newItem.Rare = rare;
                            newItem.Upgrade = item.Upgrade;
                            newItem.Design = item.Color;

                            if (newItem != null && newItem.Slot != -1)
                            {
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(newItem.ItemVNum, newItem.Amount, newItem.Type, newItem.Slot, newItem.Rare, newItem.Design, newItem.Upgrade, 0));
                                if (iteminfo.ReputPrice == 0)
                                {
                                    Session.SendPacket(Session.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALID"), iteminfo.Name, amount)));
                                    Session.Character.Gold -= (long)(price * percent);
                                    Session.SendPacket(Session.Character.GenerateGold());
                                }
                                else
                                {
                                    Session.SendPacket(Session.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALID"), iteminfo.Name, amount)));
                                    Session.Character.Reput -= Reputprice;
                                    Session.SendPacket(Session.Character.GenerateFd());
                                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("REPUT_DECREASED"), 11));
                                }
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                            }
                        }
                        break;
                    }
            }
        }

        private bool BuyValidate(ClientSession clientSession, KeyValuePair<long, MapShop> shop, short slot, byte amount)
        {
            PersonalShopItem shopitem = clientSession.CurrentMap.UserShops[shop.Key].Items.FirstOrDefault(i => i.ShopSlot.Equals(slot));
            if (shopitem == null)
            {
                return false;
            }
            Guid id = shopitem.ItemInstance.Id;

            ClientSession shopOwnerSession = ServerManager.Instance.GetSessionByCharacterId(shop.Value.OwnerId);
            if (shopOwnerSession == null)
            {
                return false;
            }

            if (amount > shopitem.SellAmount)
            {
                amount = shopitem.SellAmount;
            }

            shopOwnerSession.Character.Gold += shopitem.Price * amount;
            shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateGold());
            shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM"), Session.Character.Name, shopitem.ItemInstance.Item.Name, amount)));
            clientSession.CurrentMap.UserShops[shop.Key].Sell += shopitem.Price * amount;

            if (shopitem.ItemInstance.Type != InventoryType.Equipment)
            {
                // remove sold amount of items
                ItemInstance inv = shopOwnerSession.Character.Inventory.RemoveItemAmountFromInventory(amount, id);

                // remove sold amount from sellamount
                shopitem.SellAmount -= amount;

                // Send reduced-amount to owners inventory
                if (inv == null)
                {
                    // Send empty slot to owners inventory
                    shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(-1, 0, shopitem.ItemInstance.Type, shopitem.ItemInstance.Slot, 0, 0, 0, 0));
                }
                else
                {
                    // remove items from inventory
                    shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, 0));
                }
            }
            else
            {
                // remove equipment
                shopOwnerSession.Character.Inventory.Remove(shopitem.ItemInstance);

                // send empty slot to owners inventory
                shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(-1, 0, shopitem.ItemInstance.Type, shopitem.ItemInstance.Slot, 0, 0, 0, 0));

                // remove the sell amount
                shopitem.SellAmount = 0;
            }

            // remove item from shop if the amount the user wanted to sell has been sold
            if (shopitem.SellAmount == 0)
            {
                clientSession.CurrentMap.UserShops[shop.Key].Items.Remove(shopitem);
            }

            // update currently sold item
            shopOwnerSession.SendPacket($"sell_list {shop.Value.Sell} {slot}.{amount}.{shopitem.SellAmount}");

            // end shop 
            if (!clientSession.CurrentMap.UserShops[shop.Key].Items.Any(s => s.SellAmount > 0))
            {
                shopOwnerSession.Character.CloseShop();
            }

            return true;
        }

        [Packet("m_shop")]
        public void CreateShop(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            InventoryType[] type = new InventoryType[20];
            long[] gold = new long[20];
            short[] slot = new short[20];
            byte[] qty = new byte[20];
            short typePacket;
            string shopname = String.Empty;
            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out typePacket);
                if (Session.Character.HasShopOpened && typePacket != 1)
                {
                    return;
                }
                foreach (PortalDTO por in Session.CurrentMap.Portals)
                {
                    if (Session.Character.MapX < por.SourceX + 6 && Session.Character.MapX > por.SourceX - 6 && Session.Character.MapY < por.SourceY + 6 && Session.Character.MapY > por.SourceY - 6)
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NEAR_PORTAL"), 0));
                        return;
                    }
                }
                if (!Session.CurrentMap.ShopAllowed)
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NOT_ALLOWED"), 0));
                    return;
                }
                if (typePacket == 2)
                {
                    Session.SendPacket("ishop");
                }
                else if (typePacket == 0)
                {
                    if (Session.CurrentMap.UserShops.Where(s => s.Value.OwnerId == Session.Character.CharacterId).Count() != 0)
                    {
                        return;
                    }
                    MapShop myShop = new MapShop();

                    if (packetsplit.Length > 82)
                    {
                        short shopSlot = 0;

                        for (short j = 3, i = 0; j < 82; j += 4, i++)
                        {
                            Enum.TryParse<InventoryType>(packetsplit[j], out type[i]);
                            short.TryParse(packetsplit[j + 1], out slot[i]);
                            byte.TryParse(packetsplit[j + 2], out qty[i]);

                            long.TryParse(packetsplit[j + 3], out gold[i]);
                            if (gold[i] < 0)
                            {
                                return;
                            }
                            if (qty[i] > 0)
                            {
                                ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(slot[i], type[i]);
                                if (inv.Amount < qty[i])
                                {
                                    return;
                                }
                                if (!inv.Item.IsTradable || inv.IsBound)
                                {
                                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_ONLY_TRADABLE_ITEMS"), 0));
                                    Session.SendPacket("shop_end 0");
                                    return;
                                }

                                PersonalShopItem personalshopitem = new PersonalShopItem()
                                {
                                    ShopSlot = shopSlot,
                                    Price = gold[i],
                                    ItemInstance = inv,
                                    SellAmount = qty[i]
                                };
                                myShop.Items.Add(personalshopitem);
                                shopSlot++;
                            }
                        }
                    }
                    if (myShop.Items.Count != 0)
                    {
                        if (!myShop.Items.Any(s => !s.ItemInstance.Item.IsSoldable || s.ItemInstance.IsBound))
                        {
                            for (int i = 83; i < packetsplit.Length; i++)
                            {
                                shopname += $"{packetsplit[i]} ";
                            }

                            // trim shopname
                            shopname.TrimEnd(' ');

                            // create default shopname if it's empty
                            if (String.IsNullOrWhiteSpace(shopname) || String.IsNullOrEmpty(shopname))
                            {
                                shopname = Language.Instance.GetMessageFromKey("SHOP_PRIVATE_SHOP");
                            }

                            // truncate the string to a max-length of 20
                            shopname = StringHelper.Truncate(shopname, 20);
                            myShop.OwnerId = Session.Character.CharacterId;
                            myShop.Name = shopname;
                            Session.CurrentMap.UserShops.Add(Session.CurrentMap.UserShops.Count(), myShop);

                            Session.Character.HasShopOpened = true;

                            Session.CurrentMap?.Broadcast(Session, Session.Character.GeneratePlayerFlag(Session.CurrentMap.UserShops.Count()), ReceiverType.AllExceptMe);
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateShop(shopname));
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("SHOP_OPEN")));

                            Session.Character.IsSitting = true;
                            Session.Character.IsShopping = true;

                            Session.Character.LoadSpeed();
                            Session.SendPacket(Session.Character.GenerateCond());
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateRest());
                        }
                        else
                        {
                            Session.SendPacket("shop_end 0");
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"), 10));
                        }
                    }
                    else
                    {
                        Session.SendPacket("shop_end 0");
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SHOP_EMPTY"), 10));
                    }
                }
                else if (typePacket == 1)
                {
                    Session.Character.CloseShop(true);
                }
            }
        }

        [Packet("n_run")]
        public void NpcRunFunction(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length <= 5)
            {
                return;
            }
            byte type;
            short runner;
            short data3;
            short npcid;
            byte.TryParse(packetsplit[3], out type);
            short.TryParse(packetsplit[2], out runner);
            short.TryParse(packetsplit[4], out data3);
            short.TryParse(packetsplit[5], out npcid);
            Session.Character.LastNRunId = npcid;
            if (Session.Character.Hp > 0)
            {
                NRunHandler.NRun(Session, type, runner, data3, npcid);
            }
        }

        [Packet("pdtse")]
        public void Pdtse(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Count() < 4)
            {
                return;
            }
            byte type = 0;
            short VNum = 0;
            if (!byte.TryParse(packetsplit[2], out type) || !short.TryParse(packetsplit[3], out VNum))
            {
                return;
            }
            if (type == 1)
            {
                MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(s => s.MapNpcId == Session.Character.LastNRunId);
                if (npc != null)
                {
                    Recipe rec = npc.Recipes.FirstOrDefault(s => s.ItemVNum == VNum);
                    if (rec != null && rec.Amount > 0)
                    {
                        string rece = $"m_list 3 {rec.Amount}";
                        foreach (RecipeItemDTO ite in rec.Items)
                        {
                            if (ite.Amount > 0)
                            {
                                rece += $" {ite.ItemVNum} {ite.Amount}";
                            }
                        }
                        rece += " -1";
                        Session.SendPacket(rece);
                    }
                }
            }
            else
            {
                MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(s => s.MapNpcId == Session.Character.LastNRunId);
                if (npc != null)
                {
                    Recipe rec = npc.Recipes.FirstOrDefault(s => s.ItemVNum == VNum);
                    if (rec != null)
                    {
                        if (rec.Amount <= 0)
                        {
                            return;
                        }
                        foreach (RecipeItemDTO ite in rec.Items)
                        {
                            if (Session.Character.Inventory.CountItem(ite.ItemVNum) < ite.Amount)
                            {
                                return;
                            }
                        }

                        ItemInstance inv = Session.Character.Inventory.AddNewToInventory(rec.ItemVNum, rec.Amount);
                        if (inv.GetType().Equals(typeof(WearableInstance)))
                        {
                            WearableInstance item = inv as WearableInstance;
                            if (item != null && (item.Item.EquipmentSlot == EquipmentType.Armor || item.Item.EquipmentSlot == EquipmentType.MainWeapon || item.Item.EquipmentSlot == EquipmentType.SecondaryWeapon))
                            {
                                item.SetRarityPoint();
                            }
                        }

                        if (inv != null)
                        {
                            short Slot = inv.Slot;
                            if (Slot != -1)
                            {
                                foreach (RecipeItemDTO ite in rec.Items)
                                {
                                    Session.Character.Inventory.RemoveItemAmount(ite.ItemVNum, ite.Amount);
                                }
                                Session.SendPacket(Session.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, 0, inv.Rare, inv.Upgrade, 0));

                                Session.SendPacket($"pdti 11 {inv.ItemVNum} {rec.Amount} 29 {inv.Upgrade} 0");
                                Session.SendPacket(Session.Character.GenerateGuri(19, 1, 1324));

                                Session.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("CRAFTED_OBJECT"), inv.Item.Name, rec.Amount), 0));
                            }
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
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
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.IsShopping)
            {
                return;
            }
            if (packetsplit.Length > 6)
            {
                InventoryType type;
                byte amount, slot;

                if (!Enum.TryParse<InventoryType>(packetsplit[4], out type) || !byte.TryParse(packetsplit[5], out slot) || !byte.TryParse(packetsplit[6], out amount))
                {
                    return;
                }
                ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(slot, type);
                if (inv == null || amount > inv.Amount)
                {
                    return;
                }
                if (!inv.Item.IsSoldable)
                {
                    Session.SendPacket(Session.Character.GenerateShopMemo(2, string.Format(Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"))));
                    return;
                }
                long price = inv.Item.Type == InventoryType.Equipment ? inv.Item.Price / 20 : inv.Item.Price;

                if (Session.Character.Gold + price * amount > 1000000000)
                {
                    string message = Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                    Session.SendPacket(message);
                    return;
                }
                Session.Character.Gold += price * amount;
                Session.SendPacket(Session.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("SELL_ITEM_VALIDE"), inv.Item.Name, amount)));

                inv = Session.Character.Inventory.RemoveItemAmountFromInventory(amount, inv.Id);
                if (inv != null)
                {
                    // Send reduced-amount to owners inventory
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, 0));
                }
                else
                {
                    // Send empty slot to owners inventory
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0, 0));
                }
                Session.SendPacket(Session.Character.GenerateGold());
            }
            else if (packetsplit.Length == 5)
            {
                short vnum = -1;
                short.TryParse(packetsplit[4], out vnum);
                CharacterSkill skill = Session.Character.Skills[vnum];
                if (skill == null || vnum == (200 + 20 * Session.Character.Class) || vnum == (201 + 20 * Session.Character.Class))
                {
                    return;
                }
                Session.Character.Gold -= skill.Skill.Price;
                Session.SendPacket(Session.Character.GenerateGold());

                foreach (var loadedSkill in Session.Character.Skills.GetAllItems())
                {
                    if (skill.Skill.SkillVNum == loadedSkill.Skill.UpgradeSkill)
                    {
                        Session.Character.Skills.Remove(loadedSkill.SkillVNum);
                    }
                }

                Session.Character.Skills.Remove(skill.SkillVNum);
                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
                Session.SendPacket(Session.Character.GenerateLev());
            }
        }

        [Packet("shopping")]
        public void Shopping(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte type, typeshop = 0;
            int NpcId;
            if (!int.TryParse(packetsplit[5], out NpcId) || !byte.TryParse(packetsplit[2], out type))
            {
                return;
            }
            if (Session.Character.IsShopping)
            {
                return;
            }
            MapNpc mapnpc = Session.CurrentMap.Npcs.FirstOrDefault(n => n.MapNpcId.Equals(NpcId));
            if (mapnpc == null)
            {
                return;
            }
            NpcMonster npc = ServerManager.GetNpc(mapnpc.NpcVNum);
            if (mapnpc?.Shop == null)
            {
                return;
            }
            string shoplist = String.Empty;
            foreach (ShopItemDTO item in mapnpc.Shop.ShopItems.Where(s => s.Type.Equals(type)))
            {
                Item iteminfo = ServerManager.GetItem(item.ItemVNum);
                typeshop = 100;
                double percent = 1;
                if (Session.Character.GetDignityIco() == 3)
                {
                    percent = 1.1;
                    typeshop = 110;
                }
                else if (Session.Character.GetDignityIco() == 4)
                {
                    percent = 1.2;
                    typeshop = 120;
                }
                else if (Session.Character.GetDignityIco() == 5)
                {
                    percent = 1.5;
                    typeshop = 150;
                }
                else if
                    (Session.Character.GetDignityIco() == 6)
                {
                    percent = 1.5;
                    typeshop = 150;
                }
                else if (Session.CurrentMap.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4))
                {
                    percent *= 1.5;
                    typeshop = 150;
                }
                if (Session.CurrentMap.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4 && Session.Character.GetDignityIco() == 3))
                {
                    percent = 1.6;
                    typeshop = 160;
                }
                else if (Session.CurrentMap.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4 && Session.Character.GetDignityIco() == 4))
                {
                    percent = 1.7;
                    typeshop = 170;
                }
                else if (Session.CurrentMap.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4 && Session.Character.GetDignityIco() == 5))
                {
                    percent = 2;
                    typeshop = 200;
                }
                else if
                    (Session.CurrentMap.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4 && Session.Character.GetDignityIco() == 6))
                {
                    percent = 2;
                    typeshop = 200;
                }
                if (iteminfo.ReputPrice > 0 && iteminfo.Type == 0)
                {
                    shoplist += $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{item.Rare}.{(iteminfo.IsColored ? item.Color : item.Upgrade)}.{iteminfo.ReputPrice}";
                }
                else if (iteminfo.ReputPrice > 0 && iteminfo.Type != 0)
                {
                    shoplist += $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{-1}.{iteminfo.ReputPrice}";
                }
                else if (iteminfo.Type != 0)
                {
                    shoplist += $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{-1}.{iteminfo.Price * percent}";
                }
                else
                {
                    shoplist += $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{item.Rare}.{(iteminfo.IsColored ? item.Color : item.Upgrade)}.{iteminfo.Price * percent}";
                }
            }

            foreach (ShopSkillDTO skill in mapnpc.Shop.ShopSkills.Where(s => s.Type.Equals(type)))
            {
                Skill skillinfo = ServerManager.GetSkill(skill.SkillVNum);

                if (skill.Type != 0)
                {
                    typeshop = 1;
                    if (skillinfo.Class == Session.Character.Class)
                    {
                        shoplist += $" {skillinfo.SkillVNum}";
                    }
                }
                else
                {
                    shoplist += $" {skillinfo.SkillVNum}";
                }
            }

            Session.SendPacket($"n_inv 2 {mapnpc.MapNpcId} 0 {typeshop}{shoplist}");
        }

        [Packet("npc_req")]
        public void ShowShop(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            long owner;
            int mode;
            if (packetsplit.Length > 2)
            {
                if (!int.TryParse(packetsplit[2], out mode))
                {
                    return;
                }
                if (mode == 1)
                {
                    // User Shop
                    if (packetsplit.Length <= 3)
                    {
                        return;
                    }
                    if (!long.TryParse(packetsplit[3], out owner))
                    {
                        return;
                    }
                    KeyValuePair<long, MapShop> shopList = Session.CurrentMap.UserShops.FirstOrDefault(s => s.Value.OwnerId.Equals(owner));
                    LoadShopItem(owner, shopList);
                }
                else
                {
                    // Npc Shop , ignore if has drop
                    short MapNpcId = -1;
                    if (!short.TryParse(packetsplit[3], out MapNpcId))
                    {
                        return;
                    }
                    MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(n => n.MapNpcId.Equals(MapNpcId));
                    if (npc == null)
                    {
                        return;
                    }
                    if (npc.Npc.Drops.Any(s => s.MonsterVNum != null) && npc.Npc.Race == 8 && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                    {
                        Session.SendPacket(Session.Character.GenerateDelay(5000, 4, $"#guri^400^{npc.MapNpcId}"));
                    }
                    else if (npc.Npc.VNumRequired > 0 && npc.Npc.Race == 8 && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                    {
                        Session.SendPacket(Session.Character.GenerateDelay(6000, 4, $"#guri^400^{npc.MapNpcId}"));
                    }
                    else if (npc.Npc.MaxHP == 0 && !npc.Npc.Drops.Any(s => s.MonsterVNum != null) && npc.Npc.Race == 8 && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                    {
                        // #guri^710^X^Y^MapNpcId
                        Session.SendPacket(Session.Character.GenerateDelay(5000, 1, $"#guri^710^162^85^{npc.MapNpcId}"));
                    }
                    else if (!string.IsNullOrEmpty(npc?.GetNpcDialog()))
                    {
                        Session.SendPacket(npc.GetNpcDialog());
                    }
                }
            }
        }

        private void LoadShopItem(long owner, KeyValuePair<long, MapShop> shop)
        {
            string packetToSend = $"n_inv 1 {owner} 0 0";

            if(shop.Value != null && shop.Value.Items != null)
            {
                foreach(PersonalShopItem item in shop.Value.Items)
                { 
                    if (item != null)
                    {
                        if (item.ItemInstance.Item.Type == InventoryType.Equipment)
                        {
                            packetToSend += $" 0.{item.ShopSlot}.{item.ItemInstance.ItemVNum}.{item.ItemInstance.Rare}.{item.ItemInstance.Upgrade}.{item.Price}";
                        }
                        else
                        {
                            packetToSend += $" {(byte)item.ItemInstance.Item.Type}.{item.ShopSlot}.{item.ItemInstance.ItemVNum}.{item.SellAmount}.{item.Price}.-1";
                        }
                    }
                    else
                    {
                        packetToSend += " -1";
                    }
                }
            }

            packetToSend += " -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1";

            Session.SendPacket(packetToSend);
        }

        #endregion
    }
}