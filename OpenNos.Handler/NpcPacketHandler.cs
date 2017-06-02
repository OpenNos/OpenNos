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
using OpenNos.Core.Handling;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Packets.ClientPackets;
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

        private ClientSession Session => _session;

        #endregion

        #region Methods

        /// <summary>
        /// buy packet
        /// </summary>
        /// <param name="buyPacket"></param>
        public void BuyShop(BuyPacket buyPacket)
        {
            if (Session.Character.InExchangeOrTrade)
            {
                return;
            }
            Logger.Debug(Session.Character.GenerateIdentity(), buyPacket.ToString());

            byte amount = buyPacket.Amount;

            switch (buyPacket.Type)
            {
                case BuyShopType.CharacterShop:
                    {
                        // User shop
                        if (!Session.HasCurrentMapInstance)
                        {
                            return;
                        }
                        KeyValuePair<long, MapShop> shop = Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(buyPacket.OwnerId));
                        PersonalShopItem item = shop.Value?.Items.FirstOrDefault(i => i.ShopSlot.Equals(buyPacket.Slot));
                        if (item == null || amount <= 0 || amount > 99)
                        {
                            return;
                        }
                        if (amount > item.SellAmount)
                        {
                            amount = item.SellAmount;
                        }
                        if (item.Price * amount + ServerManager.Instance.GetProperty<long>(shop.Value.OwnerId, nameof(Character.Gold)) > ServerManager.Instance.MaxGold)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("MAX_GOLD")));
                            return;
                        }

                        if (item.Price * amount >= Session.Character.Gold)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                            return;
                        }

                        // check if the item has been removed successfully from previous owner and
                        // remove it
                        if (BuyValidate(Session, shop, buyPacket.Slot, amount))
                        {
                            Session.Character.Gold -= item.Price * amount;
                            Session.SendPacket(Session.Character.GenerateGold());

                            KeyValuePair<long, MapShop> shop2 = Session.CurrentMapInstance.UserShops.FirstOrDefault(s => s.Value.OwnerId.Equals(buyPacket.OwnerId));
                            LoadShopItem(buyPacket.OwnerId, shop2);
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                        }

                        break;
                    }

                case BuyShopType.ItemShop:
                    {
                        // load shop
                        if (!Session.HasCurrentMapInstance)
                        {
                            return;
                        }
                        MapNpc npc = Session.CurrentMapInstance.Npcs.FirstOrDefault(n => n.MapNpcId.Equals((short)buyPacket.OwnerId));
                        if (npc != null)
                        {
                            int dist = Map.GetDistance(new MapCell { X = Session.Character.PositionX, Y = Session.Character.PositionY }, new MapCell { X = npc.MapX, Y = npc.MapY });
                            if (npc.Shop == null || dist > 5)
                            {
                                return;
                            }
                            if (npc.Shop.ShopSkills.Any())
                            {
                                if (!npc.Shop.ShopSkills.Exists(s => s.SkillVNum == buyPacket.Slot))
                                {
                                    return;
                                }

                                // skill shop
                                if (Session.Character.UseSp)
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_SP"), 0));
                                    return;
                                }

                                if (Session.Character.Skills.GetAllItems().Any(s=>s.LastUse.AddMilliseconds(s.Skill.Cooldown *100) > DateTime.Now))
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_NEED_COOLDOWN"), 0));
                                    return;
                                }

                                Skill skillinfo = ServerManager.Instance.GetSkill(buyPacket.Slot);
                                if (Session.Character.Skills.GetAllItems().Any(s => s.SkillVNum == buyPacket.Slot) || skillinfo == null)
                                {
                                    return;
                                }

                                if (Session.Character.Gold < skillinfo.Price)
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 0));
                                }
                                else if (Session.Character.GetCP() < skillinfo.CPCost)
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_CP"), 0));
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
                                                case ClassType.Adventurer:
                                                    SkillMiniumLevel = skillinfo.MinimumAdventurerLevel;
                                                    break;

                                                case ClassType.Swordman:
                                                    SkillMiniumLevel = skillinfo.MinimumSwordmanLevel;
                                                    break;

                                                case ClassType.Archer:
                                                    SkillMiniumLevel = skillinfo.MinimumArcherLevel;
                                                    break;

                                                case ClassType.Magician:
                                                    SkillMiniumLevel = skillinfo.MinimumMagicianLevel;
                                                    break;
                                            }
                                        }
                                        if (SkillMiniumLevel == 0)
                                        {
                                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_CANT_LEARN"), 0));
                                            return;
                                        }
                                        if (Session.Character.Level < SkillMiniumLevel)
                                        {
                                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                                            return;
                                        }
                                        foreach (CharacterSkill skill in Session.Character.Skills.GetAllItems())
                                        {
                                            if (skillinfo.CastId == skill.Skill.CastId && skill.Skill.SkillVNum < 200)
                                            {
                                                Session.Character.Skills.Remove(skill.SkillVNum);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if ((byte)Session.Character.Class != skillinfo.Class)
                                        {
                                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_CANT_LEARN"), 0));
                                            return;
                                        }
                                        if (Session.Character.JobLevel < skillinfo.LevelMinimum)
                                        {
                                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 0));
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

                                    Session.Character.Skills[buyPacket.Slot] = new CharacterSkill { SkillVNum = buyPacket.Slot, CharacterId = Session.Character.CharacterId };

                                    Session.Character.Gold -= skillinfo.Price;
                                    Session.SendPacket(Session.Character.GenerateGold());
                                    Session.SendPacket(Session.Character.GenerateSki());
                                    Session.SendPackets(Session.Character.GenerateQuicklist());
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                                    Session.SendPacket(Session.Character.GenerateLev());
                                }
                            }
                            else if (npc.Shop.ShopItems.Any())
                            {
                                // npc shop
                                ShopItemDTO item = npc.Shop.ShopItems.FirstOrDefault(it => it.Slot == buyPacket.Slot);
                                if (item == null || amount <= 0 || amount > 99)
                                {
                                    return;
                                }
                                Item iteminfo = ServerManager.Instance.GetItem(item.ItemVNum);
                                long price = iteminfo.Price * amount;
                                long Reputprice = iteminfo.ReputPrice * amount;
                                double percent;
                                switch (Session.Character.GetDignityIco())
                                {
                                    case 3:
                                        percent = 1.10;
                                        break;

                                    case 4:
                                        percent = 1.20;
                                        break;

                                    case 5:
                                    case 6:
                                        percent = 1.5;
                                        break;

                                    default:
                                        percent = 1;
                                        break;
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
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                                        return;
                                    }
                                }
                                else
                                {
                                    if (Reputprice <= 0 || Reputprice > Session.Character.Reput)
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_REPUT")));
                                        return;
                                    }
                                    byte ra = (byte)ServerManager.Instance.RandomNumber();

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

                                List<ItemInstance> newItem = Session.Character.Inventory.AddNewToInventory(item.ItemVNum, amount, Rare: rare, Upgrade: item.Upgrade, Design: item.Color);
                                if (!newItem.Any())
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE")));
                                    return;
                                }

                                if (newItem.Any())
                                {
                                    if (iteminfo.ReputPrice == 0)
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALID"), iteminfo.Name, amount)));
                                        Session.Character.Gold -= (long)(price * percent);
                                        Session.SendPacket(Session.Character.GenerateGold());
                                    }
                                    else
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALID"), iteminfo.Name, amount)));
                                        Session.Character.Reput -= Reputprice;
                                        Session.SendPacket(Session.Character.GenerateFd());
                                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("REPUT_DECREASED"), 11));
                                    }
                                }
                                else
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                        break;
                    }
            }
        }

        [Packet("m_shop")]
        public void CreateShop(string packet)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), packet);
            string[] packetsplit = packet.Split(' ');
            InventoryType[] type = new InventoryType[20];
            long[] gold = new long[20];
            short[] slot = new short[20];
            byte[] qty = new byte[20];
            string shopname = string.Empty;
            if (packetsplit.Length > 2)
            {
                if (!short.TryParse(packetsplit[2], out short typePacket))
                {
                    return;
                }
                if (Session.Character.HasShopOpened && typePacket != 1 || !Session.HasCurrentMapInstance || Session.Character.IsExchanging || Session.Character.ExchangeInfo != null)
                {
                    return;
                }
                if (Session.CurrentMapInstance.Portals.Any(por => Session.Character.PositionX < por.SourceX + 6 && Session.Character.PositionX > por.SourceX - 6 && Session.Character.PositionY < por.SourceY + 6 && Session.Character.PositionY > por.SourceY - 6))
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NEAR_PORTAL"), 0));
                    return;
                }
                if (Session.Character.Group != null && Session.Character.Group?.GroupType != GroupType.Group)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NOT_ALLOWED_IN_RAID"), 0));
                    return;
                }
                if (!Session.CurrentMapInstance.ShopAllowed)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NOT_ALLOWED"), 0));
                    return;
                }
                if (typePacket == 2)
                {
                    Session.SendPacket("ishop");
                }
                else if (typePacket == 0)
                {
                    if (Session.CurrentMapInstance.UserShops.Count(s => s.Value.OwnerId == Session.Character.CharacterId) != 0)
                    {
                        return;
                    }
                    MapShop myShop = new MapShop();

                    if (packetsplit.Length > 82)
                    {
                        short shopSlot = 0;

                        for (short j = 3, i = 0; j < 82; j += 4, i++)
                        {
                            Enum.TryParse(packetsplit[j], out type[i]);
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
                                if (inv != null)
                                {
                                    if (inv.Amount < qty[i])
                                    {
                                        return;
                                    }
                                    if (!inv.Item.IsTradable || inv.IsBound)
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_ONLY_TRADABLE_ITEMS"), 0));
                                        Session.SendPacket("shop_end 0");
                                        return;
                                    }

                                    PersonalShopItem personalshopitem = new PersonalShopItem
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
                            shopname = shopname.TrimEnd(' ');

                            // create default shopname if it's empty
                            if (string.IsNullOrWhiteSpace(shopname) || string.IsNullOrEmpty(shopname))
                            {
                                shopname = Language.Instance.GetMessageFromKey("SHOP_PRIVATE_SHOP");
                            }

                            // truncate the string to a max-length of 20
                            shopname = shopname.Truncate(20);
                            myShop.OwnerId = Session.Character.CharacterId;
                            myShop.Name = shopname;
                            Session.CurrentMapInstance.UserShops.Add(Session.CurrentMapInstance.LastUserShopId++, myShop);

                            Session.Character.HasShopOpened = true;

                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GeneratePlayerFlag(Session.CurrentMapInstance.LastUserShopId), ReceiverType.AllExceptMe);
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateShop(shopname));
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("SHOP_OPEN")));

                            Session.Character.IsSitting = true;
                            Session.Character.IsShopping = true;

                            Session.Character.LoadSpeed();
                            Session.SendPacket(Session.Character.GenerateCond());
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateRest());
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
                    Session.Character.CloseShop();
                }
            }
        }

        /// <summary>
        /// n_run packet
        /// </summary>
        /// <param name="packet"></param>
        public void NpcRunFunction(NRunPacket packet)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), packet.ToString());
            Session.Character.LastNRunId = packet.NpcId;
            if (Session.Character.Hp > 0)
            {
                NRunHandler.NRun(Session, packet);
            }
        }

        /// <summary>
        /// pdtse packet
        /// </summary>
        /// <param name="pdtsePacket"></param>
        public void Pdtse(PdtsePacket pdtsePacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), pdtsePacket.ToString());
            if (!Session.HasCurrentMapInstance)
            {
                return;
            }
            short VNum = pdtsePacket.VNum;
            if (pdtsePacket.Type == 1)
            {
                MapNpc npc = Session.CurrentMapInstance.Npcs.FirstOrDefault(s => s.MapNpcId == Session.Character.LastNRunId);
                if (npc != null)
                {
                    int distance = Map.GetDistance(new MapCell
                    {
                        X = Session.Character.PositionX,
                        Y = Session.Character.PositionY
                    }, new MapCell
                    {
                        X = npc.MapX,
                        Y = npc.MapY
                    });
                    if (npc.MapInstance == Session.CurrentMapInstance && distance <= 5)
                    {
                        Recipe rec = npc.Recipes.FirstOrDefault(s => s.ItemVNum == VNum);
                        if (rec != null && rec.Amount > 0)
                        {
                            string rece = $"m_list 3 {rec.Amount}";
                            foreach (RecipeItemDTO ite in rec.Items)
                            {
                                if (ite.Amount > 0) rece = rece + $" {ite.ItemVNum} {ite.Amount}";
                            }
                            rece += " -1";
                            Session.SendPacket(rece);
                        }
                    }
                }
            }
            else
            {
                MapNpc npc = Session.CurrentMapInstance.Npcs.FirstOrDefault(s => s.MapNpcId == Session.Character.LastNRunId);
                if (npc != null)
                {
                    int distance = Map.GetDistance(new MapCell
                    {
                        X = Session.Character.PositionX,
                        Y = Session.Character.PositionY
                    }, new MapCell
                    {
                        X = npc.MapX,
                        Y = npc.MapY
                    });
                    if (npc.MapInstance == Session.CurrentMapInstance && distance <= 5)
                    {
                        Recipe rec = npc.Recipes.FirstOrDefault(s => s.ItemVNum == VNum);
                        if (rec != null)
                        {
                            if (rec.Amount <= 0)
                            {
                                return;
                            }
                            if (rec.Items.Any(ite => Session.Character.Inventory.CountItem(ite.ItemVNum) < ite.Amount))
                            {
                                return;
                            }

                            ItemInstance inv = Session.Character.Inventory.AddNewToInventory(rec.ItemVNum, rec.Amount).FirstOrDefault();
                            if (inv != null)
                            {
                                if (inv.GetType() == typeof(WearableInstance))
                                {
                                    if (inv is WearableInstance item && (item.Item.EquipmentSlot == EquipmentType.Armor || item.Item.EquipmentSlot == EquipmentType.MainWeapon || item.Item.EquipmentSlot == EquipmentType.SecondaryWeapon))
                                    {
                                        item.SetRarityPoint();
                                    }
                                }
                                foreach (RecipeItemDTO ite in rec.Items)
                                {
                                    Session.Character.Inventory.RemoveItemAmount(ite.ItemVNum, ite.Amount);
                                }
                                Session.SendPacket($"pdti 11 {inv.ItemVNum} {rec.Amount} 29 {inv.Upgrade} 0");
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(19, 1, Session.Character.CharacterId, 1324));
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("CRAFTED_OBJECT"), inv.Item.Name, rec.Amount), 0));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ptctl packet
        /// </summary>
        /// <param name="packet"></param>
        public void PetMove(PtCtlPacket packet)
        {
            string[] packetsplit = packet.PacketEnd.Split(' ');
            for (int i = 0; i < packet.Amount * 3; i += 3)
            {
                if (packetsplit.Count() >= packet.Amount * 3)
                {
                    int PetId = int.Parse(packetsplit[i]);
                    short PositionX = short.Parse(packetsplit[i + 1]);
                    short PositionY = short.Parse(packetsplit[i + 2]);

                    Mate mate = Session.Character.Mates.FirstOrDefault(s => s.MateTransportId == PetId);
                    if (mate != null)
                    {
                        mate.PositionX = PositionX;
                        mate.PositionY = PositionY;
                        Session.CurrentMapInstance.Broadcast($"mv 2 {PetId} {PositionX} {PositionY} {mate.Monster.Speed}");
                    }
                }
            }
            /*
            packet.Users.ForEach(u =>
            {
                Mate mate = Session.Character.Mates.FirstOrDefault(s => s.MateTransportId == u.UserId);
                if (mate != null)
                {
                    mate.PositionX = u.UserX;
                    mate.PositionY = u.UserY;
                    Session.CurrentMapInstance.Broadcast($"mv 2 {u.UserId} { u.UserX} { u.UserY} {mate.Monster.Speed}");
                }
            });
             */
        }

        /// <summary>
        /// say_p packet
        /// </summary>
        /// <param name="sayPPacket"></param>
        public void PetTalk(SayPPacket sayPPacket)
        {
            if (string.IsNullOrEmpty(sayPPacket.Message))
            {
                return;
            }
            Mate mate = Session.Character.Mates.FirstOrDefault(s => s.MateTransportId == sayPPacket.PetId);
            if (mate != null)
            {
                Session.CurrentMapInstance.Broadcast(mate.GenerateSay(sayPPacket.Message, 2));
            }
        }

        /// <summary>
        /// sell packet
        /// </summary>
        /// <param name="sellPacket"></param>
        public void SellShop(SellPacket sellPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), sellPacket.ToString());
            if (Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo.ExchangeList.Any() || Session.Character.IsShopping)
            {
                return;
            }
            if (sellPacket.Amount.HasValue && sellPacket.Slot.HasValue)
            {
                InventoryType type = (InventoryType)sellPacket.Data;
                byte amount = sellPacket.Amount.Value, slot = sellPacket.Slot.Value;

                if (type == InventoryType.Bazaar)
                {
                    return;
                }
                ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(slot, type);
                if (inv == null || amount > inv.Amount)
                {
                    return;
                }
                if (Session.Character.MinilandObjects.Any(s => s.ItemInstanceId == inv.Id))
                {
                    return;
                }
                if (!inv.Item.IsSoldable)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(2, string.Format(Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"))));
                    return;
                }
                long price = inv.Item.ItemType == ItemType.Sell ? inv.Item.Price : inv.Item.Price / 20;

                if (Session.Character.Gold + price * amount > ServerManager.Instance.MaxGold)
                {
                    string message = UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                    Session.SendPacket(message);
                    return;
                }
                Session.Character.Gold += price * amount;
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("SELL_ITEM_VALIDE"), inv.Item.Name, amount)));

                Session.Character.Inventory.RemoveItemAmountFromInventory(amount, inv.Id);
                Session.SendPacket(Session.Character.GenerateGold());
            }
            else
            {
                short vnum = sellPacket.Data;
                CharacterSkill skill = Session.Character.Skills[vnum];
                if (skill == null || vnum == 200 + 20 * (byte)Session.Character.Class || vnum == 201 + 20 * (byte)Session.Character.Class)
                {
                    return;
                }
                Session.Character.Gold -= skill.Skill.Price;
                Session.SendPacket(Session.Character.GenerateGold());

                foreach (CharacterSkill loadedSkill in Session.Character.Skills.GetAllItems())
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

        /// <summary>
        /// shopping packet
        /// </summary>
        /// <param name="shoppingPacket"></param>
        public void Shopping(ShoppingPacket shoppingPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), shoppingPacket.ToString());
            byte type = shoppingPacket.Type, typeshop = 0;
            int NpcId = shoppingPacket.NpcId;
            if (Session.Character.IsShopping || !Session.HasCurrentMapInstance)
            {
                return;
            }
            MapNpc mapnpc = Session.CurrentMapInstance.Npcs.FirstOrDefault(n => n.MapNpcId.Equals(NpcId));
            if (mapnpc?.Shop == null)
            {
                return;
            }
            string shoplist = string.Empty;
            foreach (ShopItemDTO item in mapnpc.Shop.ShopItems.Where(s => s.Type.Equals(type)))
            {
                Item iteminfo = ServerManager.Instance.GetItem(item.ItemVNum);
                typeshop = 100;
                double percent = 1;
                switch (Session.Character.GetDignityIco())
                {
                    case 3:
                        percent = 1.1;
                        typeshop = 110;
                        break;

                    case 4:
                        percent = 1.2;
                        typeshop = 120;
                        break;

                    case 5:
                        percent = 1.5;
                        typeshop = 150;
                        break;

                    case 6:
                        percent = 1.5;
                        typeshop = 150;
                        break;

                    default:
                        if (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4))
                        {
                            percent *= 1.5;
                            typeshop = 150;
                        }
                        break;
                }
                if (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4 && Session.Character.GetDignityIco() == 3))
                {
                    percent = 1.6;
                    typeshop = 160;
                }
                else if (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4 && Session.Character.GetDignityIco() == 4))
                {
                    percent = 1.7;
                    typeshop = 170;
                }
                else if (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4 && Session.Character.GetDignityIco() == 5))
                {
                    percent = 2;
                    typeshop = 200;
                }
                else if
                    (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4 && Session.Character.GetDignityIco() == 6))
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
                    shoplist += $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.-1.{iteminfo.ReputPrice}";
                }
                else if (iteminfo.Type != 0)
                {
                    shoplist += $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.-1.{iteminfo.Price * percent}";
                }
                else
                {
                    shoplist += $" {(byte)iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{item.Rare}.{(iteminfo.IsColored ? item.Color : item.Upgrade)}.{iteminfo.Price * percent}";
                }
            }

            foreach (ShopSkillDTO skill in mapnpc.Shop.ShopSkills.Where(s => s.Type.Equals(type)))
            {
                Skill skillinfo = ServerManager.Instance.GetSkill(skill.SkillVNum);

                if (skill.Type != 0)
                {
                    typeshop = 1;
                    if (skillinfo.Class == (byte)Session.Character.Class)
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

        /// <summary>
        /// npc_req packet
        /// </summary>
        /// <param name="requestNpcPacket"></param>
        public void ShowShop(RequestNpcPacket requestNpcPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), requestNpcPacket.ToString());
            long owner = requestNpcPacket.Owner;
            if (!Session.HasCurrentMapInstance)
            {
                return;
            }
            if (requestNpcPacket.Type == 1)
            {
                // User Shop
                KeyValuePair<long, MapShop> shopList = Session.CurrentMapInstance.UserShops.FirstOrDefault(s => s.Value.OwnerId.Equals(owner));
                LoadShopItem(owner, shopList);
            }
            else
            {
                // Npc Shop , ignore if has drop
                MapNpc npc = Session.CurrentMapInstance.Npcs.FirstOrDefault(n => n.MapNpcId.Equals((int)requestNpcPacket.Owner));
                if (npc == null)
                {
                    return;
                }
                if (npc.Npc.Drops.Any(s => s.MonsterVNum != null) && npc.Npc.Race == 8 && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(5000, 4, $"#guri^400^{npc.MapNpcId}"));
                }
                else if (npc.Npc.VNumRequired > 0 && npc.Npc.Race == 8 && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(6000, 4, $"#guri^400^{npc.MapNpcId}"));
                }
                else if (npc.Npc.MaxHP == 0 && npc.Npc.Drops.All(s => s.MonsterVNum == null) && npc.Npc.Race == 8 && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(5000, 1, $"#guri^710^162^85^{npc.MapNpcId}")); // #guri^710^X^Y^MapNpcId
                }
                else if (!string.IsNullOrEmpty(npc.GetNpcDialog()))
                {
                    Session.SendPacket(npc.GetNpcDialog());
                }
            }
        }

        private bool BuyValidate(ClientSession clientSession, KeyValuePair<long, MapShop> shop, short slot, byte amount)
        {
            if (!clientSession.HasCurrentMapInstance)
            {
                return false;
            }
            PersonalShopItem shopitem = clientSession.CurrentMapInstance.UserShops[shop.Key].Items.FirstOrDefault(i => i.ShopSlot.Equals(slot));
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
            List<ItemInstance> inv = shopitem.ItemInstance.Type == InventoryType.Equipment
                   ? clientSession.Character.Inventory.AddToInventory(shopitem.ItemInstance)
                   : clientSession.Character.Inventory.AddNewToInventory(shopitem.ItemInstance.ItemVNum, amount, shopitem.ItemInstance.Type);

            if (!inv.Any())
            {
                return false;
            }

            shopOwnerSession.Character.Gold += shopitem.Price * amount;
            shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateGold());
            shopOwnerSession.SendPacket(UserInterfaceHelper.Instance.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM"), Session.Character.Name, shopitem.ItemInstance.Item.Name, amount)));
            clientSession.CurrentMapInstance.UserShops[shop.Key].Sell += shopitem.Price * amount;

            if (shopitem.ItemInstance.Type != InventoryType.Equipment)
            {
                // remove sold amount of items
                shopOwnerSession.Character.Inventory.RemoveItemAmountFromInventory(amount, id);

                // remove sold amount from sellamount
                shopitem.SellAmount -= amount;
            }
            else
            {
                // remove equipment
                shopOwnerSession.Character.Inventory.Remove(shopitem.ItemInstance.Id);

                // send empty slot to owners inventory
                shopOwnerSession.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(shopitem.ItemInstance.Type, shopitem.ItemInstance.Slot));

                // remove the sell amount
                shopitem.SellAmount = 0;
            }

            // remove item from shop if the amount the user wanted to sell has been sold
            if (shopitem.SellAmount == 0)
            {
                clientSession.CurrentMapInstance.UserShops[shop.Key].Items.Remove(shopitem);
            }

            // update currently sold item
            shopOwnerSession.SendPacket($"sell_list {shop.Value.Sell} {slot}.{amount}.{shopitem.SellAmount}");

            // end shop
            if (!clientSession.CurrentMapInstance.UserShops[shop.Key].Items.Any(s => s.SellAmount > 0))
            {
                shopOwnerSession.Character.CloseShop();
            }

            return true;
        }

        private void LoadShopItem(long owner, KeyValuePair<long, MapShop> shop)
        {
            string packetToSend = $"n_inv 1 {owner} 0 0";

            if (shop.Value?.Items != null)
            {
                foreach (PersonalShopItem item in shop.Value.Items)
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