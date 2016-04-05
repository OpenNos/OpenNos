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
using OpenNos.GameObject;
using OpenNos.ServiceRef.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class WorldPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public WorldPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        public ClientSession Session { get { return _session; } }

        #endregion

        #region Methods

        [Packet("#req_exc")]
        public void AcceptExchange(string packet)
        {
            string[] packetsplit = packet.Split(' ', '^');
            byte mode;
            long charId;
            if (!byte.TryParse(packetsplit[2], out mode) || !long.TryParse(packetsplit[3], out charId)) return;

            Session.Character.ExchangeInfo = new ExchangeInfo
            {
                CharId = charId,
                Confirm = false
            };

            if (mode == 2)
            {
                if (charId == Session.Character.CharacterId) return;
                Session.Client.SendPacket($"exc_list 1 {charId} -1");
                ClientLinkManager.Instance.Broadcast(Session, $"exc_list 1 {Session.Character.CharacterId} -1", ReceiverType.OnlySomeone, "", charId);
            }
            else if (mode == 5)
            {
                Session.Client.SendPacket(Session.Character.GenerateModal($"refused {Language.Instance.GetMessageFromKey("REFUSED")}"));
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateModal($"refused {Language.Instance.GetMessageFromKey("REFUSED")}"), ReceiverType.OnlySomeone, "", charId);
            }
        }

        [Packet("$AddMonster")]
        public void AddMonster(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short vnum = 0, move = 0;

            Random rnd = new Random();

            if (packetsplit.Length == 4 && short.TryParse(packetsplit[2], out vnum) && short.TryParse(packetsplit[3], out move))
            {
                NpcMonster npcmonster = ServerManager.GetNpc(vnum);
                if (npcmonster == null)
                    return;
                MapMonsterDTO monst = new MapMonsterDTO() { MonsterVNum = vnum, MapY = Session.Character.MapY, MapX = Session.Character.MapX, MapId = Session.Character.MapId, Position = (byte)Session.Character.Direction, Move = move == 1 ? true : false, MapMonsterId = MapMonster.generateMapMonsterId() };
                MapMonster monster = null;
                if (DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) == null)
                {
                    DAOFactory.MapMonsterDAO.Insert(monst);
                    monster = new MapMonster() { MonsterVNum = vnum, MapY = monst.MapY, Alive = true, CurrentHp = npcmonster.MaxHP, CurrentMp = npcmonster.MaxMP, MapX = monst.MapX, MapId = Session.Character.MapId, firstX = monst.MapX, firstY = monst.MapY, MapMonsterId = monst.MapMonsterId, Position = 1, Move = move == 1 ? true : false };
                    ServerManager.Monsters.Add(monster);
                    ServerManager.GetMap(Session.Character.MapId).Monsters.Add(monster);
                    ClientLinkManager.Instance.Broadcast(Session, monster.GenerateIn3(), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 10));
        }

        [Packet("#b_i")]
        public void answerToDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ', '^');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);

            if (Convert.ToInt32(packetsplit[4]) == 1)
            {
                Session.Client.SendPacket(Session.Character.GenerateDialog($"#b_i^{type}^{slot}^2 #b_i^0^0^5 {Language.Instance.GetMessageFromKey("SURE_TO_DELETE")}"));
            }
            else if (Convert.ToInt32(packetsplit[4]) == 2)
            {
                if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                    return;
                DeleteItem(type, slot);
            }
        }

        [Packet("b_i")]
        public void askToDelete(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            Session.Client.SendPacket(Session.Character.GenerateDialog($"#b_i^{type}^{slot}^1 #b_i^0^0^5 {Language.Instance.GetMessageFromKey("ASK_TO_DELETE")}"));
        }

        [Packet("$Ban")]
        public void Ban(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                ClientLinkManager.Instance.Kick(packetsplit[2]);
                if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                {
                    DAOFactory.AccountDAO.ToggleBan(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId);
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
                else
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME", 10));
        }

        [Packet("buy")]
        public void BuyShop(string packet)
        {
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
                KeyValuePair<long, MapShop> shop = Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(owner));
                PersonalShopItem item = shop.Value.Items.FirstOrDefault(i => i.Slot.Equals(slot));
                if (item == null) return;

                if (amount > item.Amount)
                    amount = item.Amount;

                if (item.Price * amount + ClientLinkManager.Instance.GetProperty<long>(shop.Value.OwnerId, "Gold") > 1000000000)
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

                Session.Character.Gold -= item.Price * amount;
                Session.Client.SendPacket(Session.Character.GenerateGold());
                InventoryItem newItem = new InventoryItem
                {
                    InventoryItemId = Session.Character.InventoryList.generateInventoryItemId(),
                    Amount = amount,
                    ItemVNum = item.InventoryItem.ItemVNum,
                    Rare = item.InventoryItem.Rare,
                    Upgrade = item.InventoryItem.Upgrade,
                    Design = item.InventoryItem.Design,
                    Concentrate = item.InventoryItem.Concentrate,
                    CriticalLuckRate = item.InventoryItem.CriticalLuckRate,
                    CriticalRate = item.InventoryItem.CriticalRate,
                    DamageMaximum = item.InventoryItem.DamageMaximum,
                    DamageMinimum = item.InventoryItem.DamageMinimum,
                    DarkElement = item.InventoryItem.DarkElement,
                    DistanceDefence = item.InventoryItem.DistanceDefence,
                    DistanceDefenceDodge = item.InventoryItem.DistanceDefenceDodge,
                    DefenceDodge = item.InventoryItem.DefenceDodge,
                    ElementRate = item.InventoryItem.ElementRate,
                    FireElement = item.InventoryItem.FireElement,
                    HitRate = item.InventoryItem.HitRate,
                    LightElement = item.InventoryItem.LightElement,
                    MagicDefence = item.InventoryItem.MagicDefence,
                    CloseDefence = item.InventoryItem.CloseDefence,
                    SlDefence = item.InventoryItem.SlDefence,
                    SpXp = item.InventoryItem.SpXp,
                    SpLevel = item.InventoryItem.SpLevel,
                    SlElement = item.InventoryItem.SlElement,
                    SlDamage = item.InventoryItem.SlDamage,
                    SlHP = item.InventoryItem.SlHP,
                    WaterElement = item.InventoryItem.WaterElement,
                };

                Inventory inv = Session.Character.InventoryList.CreateItem(newItem, Session.Character);
                if (inv != null && inv.Slot != -1)
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(newItem.ItemVNum,
                        inv.InventoryItem.Amount, inv.Type, inv.Slot, newItem.Rare, newItem.Design, newItem.Upgrade));

                ClientLinkManager.Instance.BuyValidate(Session, shop, slot, amount);
                KeyValuePair<long, MapShop> shop2 = Session.CurrentMap.ShopUserList.FirstOrDefault(s => s.Value.OwnerId.Equals(owner));
                loadShopItem(owner, shop2);
            }
            else if (packetsplit.Length == 5) // skill shop
            {
                Skill skillinfo = ServerManager.GetSkill(slot);

                if (skillinfo == null)
                    return;
                if (Session.Character.Gold >= skillinfo.Cost && Session.Character.getCP() >= skillinfo.CPCost && Session.Character.Level >= skillinfo.LevelMinimum)
                {
                    switch (Session.Character.Class)
                    {
                        case (byte)ClassType.Adventurer:
                            if (Session.Character.JobLevel < skillinfo.MinimumAdventurerLevel)
                                return;
                            break;
                        case (byte)ClassType.Swordman:
                            if (Session.Character.JobLevel < skillinfo.MinimumSwordmanLevel)
                                return;
                            break;
                        case (byte)ClassType.Archer:
                            if (Session.Character.JobLevel < skillinfo.MinimumArcherLevel)
                                return;
                            break;
                        case (byte)ClassType.Magician:
                            if (Session.Character.JobLevel < skillinfo.MinimumMagicianLevel)
                                return;
                            break;
                    }
                    if (Session.Character.Skills.FirstOrDefault(s => s.SkillVNum == slot) != null)
                        return;
                    Session.Character.Gold -= skillinfo.Cost;
                    Session.Client.SendPacket(Session.Character.GenerateGold());

                    Session.Character.Skills.Add(new CharacterSkill() { SkillVNum = slot, CharacterId = Session.Character.CharacterId });
                    Session.Client.SendPacket(Session.Character.GenerateSki());
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateLev());//replace by the correct one

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
                double pourcent = 1;
                if (Session.Character.GetDigniteIco() == 3)
                    pourcent = 1.10;
                else if (Session.Character.GetDigniteIco() == 4)
                    pourcent = 1.20;
                else if (Session.Character.GetDigniteIco() == 5 || Session.Character.GetDigniteIco() == 6)
                    pourcent = 1.5;

                if (iteminfo.ReputPrice == 0)
                {
                    if (price <= 0 || price * pourcent > Session.Character.Gold)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                        return;
                    }

                    Session.Client.SendPacket(Session.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALIDE"), ServerManager.GetItem(item.ItemVNum).Name, amount)));
                    Session.Character.Gold -= (long)(price * pourcent);
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                }
                else
                {
                    if (Reputprice <= 0 || Reputprice > Session.Character.Reput)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateShopMemo(3, Language.Instance.GetMessageFromKey("NOT_ENOUGH_REPUT")));
                        return;
                    }

                    Session.Client.SendPacket(Session.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM_VALIDE"), ServerManager.GetItem(item.ItemVNum).Name, amount)));
                    Session.Character.Reput -= (long)(Reputprice);
                    Session.Client.SendPacket(Session.Character.GenerateFd());
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("REPUT_DECREASE"), 11));
                }

                InventoryItem newItem = new InventoryItem
                {
                    InventoryItemId = Session.Character.InventoryList.generateInventoryItemId(),
                    Amount = amount,
                    ItemVNum = item.ItemVNum,
                    Rare = item.Rare,
                    Upgrade = item.Upgrade,
                    Design = item.Color,
                    Concentrate = 0,
                    CriticalLuckRate = 0,
                    CriticalRate = 0,
                    DamageMaximum = 0,
                    DamageMinimum = 0,
                    DarkElement = 0,
                    DistanceDefence = 0,
                    DistanceDefenceDodge = 0,
                    DefenceDodge = 0,
                    ElementRate = 0,
                    FireElement = 0,
                    HitRate = 0,
                    LightElement = 0,
                    MagicDefence = 0,
                    CloseDefence = 0,
                    SpXp = 0,
                    SpLevel = ServerManager.GetItem(item.ItemVNum).EquipmentSlot.Equals((byte)EquipmentType.Sp) ? (byte)1 : (byte)0,
                    SlDefence = 0,
                    SlElement = 0,
                    SlDamage = 0,
                    SlHP = 0,
                    WaterElement = 0,
                };

                Inventory inv = Session.Character.InventoryList.CreateItem(newItem, Session.Character);
                if (inv != null && inv.Slot != -1)
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(newItem.ItemVNum,
                        inv.InventoryItem.Amount, inv.Type, inv.Slot, newItem.Rare, newItem.Design, newItem.Upgrade));
            }

        }
        [Packet("c_close")]
        public void CClose(string packet)
        {
            // idk
        }

        [Packet("$ChangeClass")]
        public void ChangeClass(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte Class;
            if (packetsplit.Length > 2)
            {
                if (byte.TryParse(packetsplit[2], out Class) && Class < 4)
                {
                    ClientLinkManager.Instance.ClassChange(Session.Character.CharacterId, Class);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 10));
        }

        public void ChangeSP()
        {
            Session.Client.SendPacket("delay 5000 3 #sl^1");
            ClientLinkManager.Instance.Broadcast(Session, $"guri 2 1 {Session.Character.CharacterId}", ReceiverType.AllOnMap);
            Thread.Sleep(5000);

            Inventory sp = Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
            Inventory fairy = Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Fairy, (byte)InventoryType.Equipment);
            if (sp == null)
                return;

            if (Session.Character.GetReputIco() < ServerManager.GetItem(sp.InventoryItem.ItemVNum).ReputationMinimum)
            {
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_REP"), 0));
                return;
            }

            if (fairy != null && ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element != ServerManager.GetItem(sp.InventoryItem.ItemVNum).Element && ServerManager.GetItem(fairy.InventoryItem.ItemVNum).Element != ServerManager.GetItem(sp.InventoryItem.ItemVNum).SecondaryElement)
            {
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                return;
            }

            Session.Character.UseSp = true;
            Session.Character.Morph = ServerManager.GetItem(sp.InventoryItem.ItemVNum).Morph;
            Session.Character.MorphUpgrade = sp.InventoryItem.Upgrade;
            Session.Character.MorphUpgrade2 = sp.InventoryItem.Design;
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);

            // TODO: Send SP Skills here

            /*s = "ski 833 833 833 834 835 836 837 838 839 840 841 21 25 28 37 41 44 49 53 56 340 341 345 352";
            MainFile.maps.SendMap(chara, s, true);
            /*
                qslot 0 1.1.2 1.1.1 1.1.3 0.7.-1 1.1.0 0.7.-1 0.7.-1 0.1.10 1.3.2 1.3.1

                qslot 1 1.1.2 1.1.3 1.1.4 1.1.5 1.1.6 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1

                qslot 2 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1 7.7.-1
                */

            // lev 40 2288403 14 72745 3221180 145000 20086 5

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(196), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, $"guri 6 1 {Session.Character.CharacterId} 0 0", ReceiverType.AllOnMap);
            Session.Client.SendPacket(Session.Character.GenerateSpPoint());
            Session.Character.Speed += ServerManager.GetItem(sp.InventoryItem.ItemVNum).Speed;
            Session.Client.SendPacket(Session.Character.GenerateCond());
            Session.Client.SendPacket(Session.Character.GenerateLev());
            Session.Client.SendPacket(Session.Character.GenerateStat());
            Session.Client.SendPacket(Session.Character.GenerateStatChar());
        }

        public void ChangeVehicle(Item item)
        {
            Session.Client.SendPacket("delay 2500 3 #sl^1");
            ClientLinkManager.Instance.Broadcast(Session, $"guri 2 1 {Session.Character.CharacterId}", ReceiverType.AllOnMap);

            Thread.Sleep(2500);
            Session.Character.IsVehicled = true;
            Session.Character.Speed = item.Speed;
            Session.Client.SendPacket(Session.Character.GenerateCond());
            Session.Character.Morph = item.Morph + Session.Character.Gender;
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, $"guri 6 1 {Session.Character.CharacterId} 0 0", ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(196), ReceiverType.AllOnMap);
        }

        [Packet("$Help")]
        public void Command(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateSay("-----------Commands Info--------------", 10));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Shout MESSAGE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport Map X Y", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$TeleportToMe CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Gold AMOUNT", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Stat", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$JLvl JOBLEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeSex", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeRep REPUTATION", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Kick CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$MapDance", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Effect EFFECTID", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$PlayMusic MUSIC", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Invisible", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Position", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY PORTALTYPE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("$Shutdown", 6));
            Session.Client.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 10));
        }

        [Packet("compl")]
        public void Compliment(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long complimentCharacterId = 0;
            if (long.TryParse(packetsplit[3], out complimentCharacterId))
            {
                if (Session.Character.Level >= 30)
                {
                    if (Session.Character.LastLogin.AddMinutes(60) <= DateTime.Now)
                    {
                        if (Session.Account.LastCompliment.Date.AddDays(1) <= DateTime.Now.Date)
                        {
                            short? compliment = ClientLinkManager.Instance.GetProperty<short?>(complimentCharacterId, "Compliment");
                            compliment++;
                            ClientLinkManager.Instance.SetProperty(complimentCharacterId, "Compliment", compliment);
                            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_GIVEN"), ClientLinkManager.Instance.GetProperty<string>(complimentCharacterId, "Name")), 12));
                            AccountDTO account = Session.Account;
                            account.LastCompliment = DateTime.Now;
                            DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_RECEIVED"), Session.Character.Name), 12), ReceiverType.OnlySomeone, packetsplit[1].Substring(1));
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_COOLDOWN"), 11));
                        }
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_LOGIN_COOLDOWN"), (Session.Character.LastLogin.AddMinutes(60) - DateTime.Now).Minutes), 11));
                    }
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_NOT_MINLVL"), 11));
                }
            }
        }

        [Packet("Char_NEW")]
        public void CreateCharacter(string packet)
        {
            if (Session.CurrentMap != null)
                return;
            // TODO: Hold Account Information in Authorized object
            long accountId = Session.Account.AccountId;
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2].Length > 3 && packetsplit[2].Length < 15)
            {
                bool isIllegalCharacter = false;
                for (int i = 0; i < packetsplit[2].Length; i++)
                {
                    if (packetsplit[2][i] < 0x30 || packetsplit[2][i] > 0x7E)
                    {
                        isIllegalCharacter = true;
                    }
                }

                if (!isIllegalCharacter)
                {
                    if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) == null)
                    {
                        if (Convert.ToByte(packetsplit[3]) > 2)
                            return;
                        Random r = new Random();
                        CharacterDTO newCharacter = new CharacterDTO()
                        {
                            Class = (byte)ClassType.Adventurer,
                            Gender = (Convert.ToByte(packetsplit[4]) >= 0 && Convert.ToByte(packetsplit[4]) <= 1 ? Convert.ToByte(packetsplit[4]) : Convert.ToByte(0)),
                            Gold = 0,
                            HairColor = Enum.IsDefined(typeof(HairColorType), Convert.ToByte(packetsplit[6])) ? Convert.ToByte(packetsplit[6]) : Convert.ToByte(0),
                            HairStyle = Enum.IsDefined(typeof(HairStyleType), Convert.ToByte(packetsplit[5])) ? Convert.ToByte(packetsplit[5]) : Convert.ToByte(0),
                            Hp = 221,
                            JobLevel = 1,
                            JobLevelXp = 0,
                            Level = 1,
                            LevelXp = 0,
                            MapId = 1,
                            MapX = (short)(r.Next(78, 81)),
                            MapY = (short)(r.Next(114, 118)),
                            Mp = 221,
                            Name = packetsplit[2],
                            Slot = Convert.ToByte(packetsplit[3]),
                            AccountId = accountId,
                            StateEnum = CharacterState.Active,
                            WhisperBlocked = false,
                            FamilyRequestBlocked = false,
                            ExchangeBlocked = false,
                            BuffBlocked = false,
                            EmoticonsBlocked = false,
                            FriendRequestBlocked = false,
                            GroupRequestBlocked = false,
                            MinilandInviteBlocked = false,
                            HeroChatBlocked = false,
                            QuickGetUp = false,
                            MouseAimLock = false,
                            HpBlocked = false,
                        };

                        SaveResult insertResult = DAOFactory.CharacterDAO.InsertOrUpdate(ref newCharacter);
                        CharacterSkillDTO sk1 = new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 200 };
                        CharacterSkillDTO sk2 = new CharacterSkillDTO { CharacterId = newCharacter.CharacterId, SkillVNum = 201 };
                        DAOFactory.CharacterSkillDAO.InsertOrUpdate(ref sk1);
                        DAOFactory.CharacterSkillDAO.InsertOrUpdate(ref sk2);
                        LoadCharacters(packet);
                    }
                    else Session.Client.SendPacketFormat($"info {Language.Instance.GetMessageFromKey("ALREADY_TAKEN")}");
                }
                else Session.Client.SendPacketFormat($"info {Language.Instance.GetMessageFromKey("INVALID_CHARNAME")}");
            }
        }

        [Packet("$CreateItem")]
        public void CreateItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte amount = 1, rare = 0, upgrade = 0, level = 0;
            short vnum, design = 0;
            ItemDTO iteminfo = null;
            if (packetsplit.Length != 5 && packetsplit.Length != 4 && packetsplit.Length != 3)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 10));
                Session.Client.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 10));
            }
            else if (short.TryParse(packetsplit[2], out vnum))
            {
                iteminfo = ServerManager.GetItem(vnum);
                if (iteminfo != null)
                {
                    if (iteminfo.IsColored)
                    {
                        if (packetsplit.Count() > 3)
                            short.TryParse(packetsplit[3], out design);
                    }
                    else if (iteminfo.Type == 0)
                    {
                        if (packetsplit.Length == 4)
                        {
                            byte.TryParse(packetsplit[3], out rare);
                        }
                        else if (packetsplit.Length == 5)
                        {
                            if (iteminfo.EquipmentSlot == Convert.ToByte((byte)EquipmentType.Sp))
                            {
                                byte.TryParse(packetsplit[3], out upgrade);
                                short.TryParse(packetsplit[4], out design);
                            }
                            else
                            {
                                byte.TryParse(packetsplit[3], out rare);
                                byte.TryParse(packetsplit[4], out upgrade);
                                if (upgrade == 0)
                                    if (iteminfo.BasicUpgrade != 0)
                                    {
                                        upgrade = iteminfo.BasicUpgrade;
                                    }
                            }
                        }
                    }
                    else
                    {
                        if (packetsplit.Length > 3)
                            byte.TryParse(packetsplit[3], out amount);
                    }
                    if (iteminfo.EquipmentSlot == Convert.ToByte((byte)EquipmentType.Sp))
                        level = 1;
                    InventoryItem newItem = new InventoryItem()
                    {
                        InventoryItemId = Session.Character.InventoryList.generateInventoryItemId(),
                        Amount = amount,
                        ItemVNum = vnum,
                        Rare = rare,
                        Upgrade = upgrade,
                        Design = design,
                        Concentrate = 0,
                        CriticalLuckRate = 0,
                        CriticalRate = 0,
                        DamageMaximum = 0,
                        DamageMinimum = 0,
                        DarkElement = 0,
                        DistanceDefence = 0,
                        DistanceDefenceDodge = 0,
                        DefenceDodge = 0,
                        ElementRate = 0,
                        FireElement = 0,
                        HitRate = 0,
                        LightElement = 0,
                        IsFixed = false,
                        Ammo = 0,
                        MagicDefence = 0,
                        CloseDefence = 0,
                        SpXp = 0,
                        SpLevel = level,
                        SlDefence = 0,
                        SlElement = 0,
                        SlDamage = 0,
                        SlHP = 0,
                        WaterElement = 0,
                    };
                    Inventory inv = Session.Character.InventoryList.CreateItem(newItem, Session.Character);
                    ServersData.SetRarityPoint(ref inv);
                    if (inv != null)
                    {
                        Session.Character.InventoryList.LoadByInventoryItem(inv.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;

                        short Slot = inv.Slot;
                        if (Slot != -1)
                        {
                            Session.Client.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {amount}", 12));
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(vnum, inv.InventoryItem.Amount, iteminfo.Type, Slot, rare, design, upgrade));
                        }
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                    }
                }
                else
                {
                    Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_ITEM"), 0);
                }
            }
        }

        [Packet("$PortalTo")]
        public void CreatePortal(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short mapid, destx, desty = 0;
            sbyte portaltype = -1;
            if (packetsplit.Length > 4 && short.TryParse(packetsplit[2], out mapid) && short.TryParse(packetsplit[3], out destx) && short.TryParse(packetsplit[4], out desty))
            {
                if (ServerManager.GetMap(mapid) == null)
                    return;

                short mapId = Session.Character.MapId;
                short mapX = Session.Character.MapX;
                short mapY = Session.Character.MapY;
                if (packetsplit.Length > 5)
                    sbyte.TryParse(packetsplit[5], out portaltype);
                Portal portal = new Portal() { SourceMapId = mapId, SourceX = mapX, SourceY = mapY, DestinationMapId = mapid, DestinationX = destx, DestinationY = desty, Type = portaltype };
                ServerManager.GetMap(Session.Character.MapId).Portals.Add(portal);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateGp(portal), ReceiverType.AllOnMap);
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY PORTALTYPE", 10));
        }

        [Packet("m_shop")]
        public void createShop(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte[] type = new byte[20];
            long[] gold = new long[20];
            short[] slot = new short[20];
            byte[] qty = new byte[20];

            string shopname = "";
            if (packetsplit.Length > 2)
            {
                foreach (Portal por in Session.CurrentMap.Portals)
                {
                    if (Session.Character.MapX < por.SourceX + 6 && Session.Character.MapX > por.SourceX - 6 && Session.Character.MapY < por.SourceY + 6 && Session.Character.MapY > por.SourceY - 6)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NEAR_PORTAL"), 0));
                        return;
                    }
                }
                short typePacket; short.TryParse(packetsplit[2], out typePacket);
                if (typePacket == 2)
                {
                    Session.Client.SendPacket("ishop");
                }
                else if (typePacket == 0)
                {
                    MapShop myShop = new MapShop();

                    if (packetsplit.Length > 2)
                        for (short j = 3, i = 0; j <= packetsplit.Length - 5; j += 4, i++)
                        {
                            byte.TryParse(packetsplit[j], out type[i]);
                            short.TryParse(packetsplit[j + 1], out slot[i]);
                            byte.TryParse(packetsplit[j + 2], out qty[i]);
                            long.TryParse(packetsplit[j + 3], out gold[i]);
                            if (qty[i] != 0)
                            {
                                Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot[i], type[i]);
                                PersonalShopItem personalshopitem = new PersonalShopItem()
                                {
                                    Slot = slot[i],
                                    Type = type[i],
                                    Price = gold[i],
                                    InventoryId = inv.InventoryId,
                                    CharacterId = inv.CharacterId,
                                    Amount = qty[i],
                                    InventoryItem = new InventoryItem()
                                    {
                                        Design = inv.InventoryItem.Design,
                                        Concentrate = inv.InventoryItem.Concentrate,
                                        CriticalLuckRate = inv.InventoryItem.CriticalLuckRate,
                                        CriticalRate = inv.InventoryItem.CriticalRate,
                                        DamageMaximum = inv.InventoryItem.DamageMaximum,
                                        DamageMinimum = inv.InventoryItem.DamageMinimum,
                                        DarkElement = inv.InventoryItem.DarkElement,
                                        DistanceDefence = inv.InventoryItem.DistanceDefence,
                                        DistanceDefenceDodge = inv.InventoryItem.DistanceDefenceDodge,
                                        DefenceDodge = inv.InventoryItem.DefenceDodge,
                                        ElementRate = inv.InventoryItem.ElementRate,
                                        FireElement = inv.InventoryItem.FireElement,
                                        HitRate = inv.InventoryItem.HitRate,
                                        ItemVNum = inv.InventoryItem.ItemVNum,
                                        LightElement = inv.InventoryItem.LightElement,
                                        MagicDefence = inv.InventoryItem.MagicDefence,
                                        CloseDefence = inv.InventoryItem.CloseDefence,
                                        Rare = inv.InventoryItem.Rare,
                                        SpXp = inv.InventoryItem.SpXp,
                                        SpLevel = inv.InventoryItem.SpLevel,
                                        SlDefence = inv.InventoryItem.SlDefence,
                                        SlElement = inv.InventoryItem.SlElement,
                                        SlDamage = inv.InventoryItem.SlDamage,
                                        SlHP = inv.InventoryItem.SlHP,
                                        Upgrade = inv.InventoryItem.Upgrade,
                                        WaterElement = inv.InventoryItem.WaterElement,
                                    }
                                };
                                myShop.Items.Add(personalshopitem);
                            }
                        }
                    if (myShop.Items.Count != 0)
                    {
                        for (int i = 83; i < packetsplit.Length; i++)
                            shopname += $"{packetsplit[i]} ";

                        shopname.TrimEnd(' ');

                        myShop.OwnerId = Session.Character.CharacterId;
                        myShop.Name = shopname;

                        Session.CurrentMap.ShopUserList.Add(Session.CurrentMap.ShopUserList.Count(), myShop);

                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePlayerFlag(Session.CurrentMap.ShopUserList.Count()), ReceiverType.AllOnMapExceptMe);

                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateShop(shopname), ReceiverType.AllOnMap);

                        Session.Client.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("SHOP_OPEN")));
                        Session.Character.IsSitting = true;
                        Session.Character.LastSpeed = Session.Character.Speed;
                        Session.Character.Speed = 0;
                        Session.Client.SendPacket(Session.Character.GenerateCond());

                        ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateRest(), ReceiverType.AllOnMap);
                    }
                    else
                    {
                        Session.Client.SendPacket("shop_end 0");
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SHOP_EMPTY"), 10));
                    }
                }
                else if (typePacket == 1)
                {
                    KeyValuePair<long, MapShop> shop = Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId));
                    Session.CurrentMap.ShopUserList.Remove(shop.Key);

                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateShopEnd(), ReceiverType.AllOnMap);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePlayerFlag(0), ReceiverType.AllOnMapExceptMe);
                    Session.Character.Speed = Session.Character.LastSpeed != 0 ? Session.Character.LastSpeed : Session.Character.Speed;
                    Session.Character.IsSitting = false;
                    Session.Client.SendPacket(Session.Character.GenerateCond());
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateRest(), ReceiverType.AllOnMap);
                }
            }
        }

        [Packet("Char_DEL")]
        public void DeleteCharacter(string packet)
        {
            if (Session.CurrentMap != null)
                return;
            string[] packetsplit = packet.Split(' ');
            AccountDTO account = DAOFactory.AccountDAO.LoadBySessionId(Session.SessionId);
            if (packetsplit.Length <= 3)
                return;
            if (account != null && account.Password == OpenNos.Core.EncryptionBase.sha256(packetsplit[3]))
            {
                DAOFactory.GeneralLogDAO.SetCharIdNull((long?)Convert.ToInt64(DAOFactory.CharacterDAO.LoadBySlot(account.AccountId, Convert.ToByte(packetsplit[2])).CharacterId));
                DAOFactory.CharacterDAO.Delete(account.AccountId, Convert.ToByte(packetsplit[2]));
                LoadCharacters(packet);
            }
            else
            {
                Session.Client.SendPacket($"info {Language.Instance.GetMessageFromKey("BAD_PASSWORD")}");
            }
        }

        public void DeleteItem(byte type, short slot)
        {
            Session.Character.InventoryList.DeleteFromSlotAndType(slot, type);
            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));
        }

        public void deleteTimeout()
        {
            for (int i = Session.Character.InventoryList.Inventory.Count() - 1; i >= 0; i--)
            {
                Inventory item = Session.Character.InventoryList.Inventory[i];
                if (item != null)
                {
                    if (item.InventoryItem.IsUsed && item.InventoryItem.ItemDeleteTime != null && item.InventoryItem.ItemDeleteTime < DateTime.Now)
                    {
                        Session.Character.InventoryList.DeleteByInventoryItemId(item.InventoryItem.InventoryItemId);
                        Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, item.Type, item.Slot, 0, 0, 0));
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                    }
                }
            }
            for (int i = Session.Character.EquipmentList.Inventory.Count() - 1; i >= 0; i--)
            {
                Inventory item = Session.Character.EquipmentList.Inventory[i];
                if (item != null)
                {
                    if (item.InventoryItem.IsUsed && item.InventoryItem.ItemDeleteTime != null && item.InventoryItem.ItemDeleteTime < DateTime.Now)
                    {
                        Session.Character.EquipmentList.DeleteByInventoryItemId(item.InventoryItem.InventoryItemId);
                        Session.Client.SendPacket(Session.Character.GenerateEquipment());
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                    }
                }
            }
        }

        [Packet("dir")]
        public void Dir(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if (Convert.ToInt32(packetsplit[4]) == Session.Character.CharacterId)
            {
                Session.Character.Direction = Convert.ToInt32(packetsplit[2]);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateDir(), ReceiverType.AllOnMap);
            }
        }

        [Packet("$Effect")]
        public void Effect(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short arg = 0;
            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out arg);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(arg), ReceiverType.AllOnMap);
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Effect EFFECT", 10));
        }

        [Packet("eqinfo")]
        public void EqInfo(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length <= 3) return;

            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            Inventory inventory = null;
            switch (type)
            {
                case 0:
                    inventory = Session.Character.EquipmentList.LoadBySlotAndType(slot, (byte)InventoryType.Equipment);
                    break;

                case 1:
                    inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, (byte)InventoryType.Wear);
                    break;

                case 2:
                    Item item = ServerManager.GetItem(slot);
                    inventory = new Inventory()
                    {
                        InventoryItem = new InventoryItem()
                        {
                            InventoryItemId = Session.Character.InventoryList.generateInventoryItemId(),
                            Amount = 1,
                            ItemVNum = item.VNum,
                            Rare = 0,
                            Upgrade = 0,
                            Design = 0,
                            Concentrate = 0,
                            CriticalLuckRate = 0,
                            CriticalRate = 0,
                            DamageMaximum = 0,
                            DamageMinimum = 0,
                            DarkElement = 0,
                            DistanceDefence = 0,
                            DistanceDefenceDodge = 0,
                            DefenceDodge = 0,
                            ElementRate = 0,
                            FireElement = 0,
                            HitRate = 0,
                            LightElement = 0,
                            IsFixed = false,
                            Ammo = 0,
                            MagicDefence = 0,
                            CloseDefence = 0,
                            SpXp = 0,
                            SpLevel = 0,
                            SlDefence = 0,
                            SlElement = 0,
                            SlDamage = 0,
                            SlHP = 0,
                            WaterElement = 0
                        }
                    };
                    break;

                case 10:
                    inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, (byte)InventoryType.Sp);
                    break;

                case 11:
                    inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, (byte)InventoryType.Costume);
                    break;
            }

            if (inventory != null)
            {
                Session.Client.SendPacket(
                    ServerManager.GetItem(inventory.InventoryItem.ItemVNum).EquipmentSlot != (byte)EquipmentType.Sp
                        ? Session.Character.GenerateEInfo(new InventoryItem(inventory.InventoryItem))
                        : Session.Character.GenerateSlInfo(new InventoryItem(inventory.InventoryItem), 0));
            }
        }

        [Packet("req_exc")]
        public void Exchange(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short mode;
            if (!short.TryParse(packetsplit[2], out mode)) return;

            long charId = -1;
            string charName;
            bool Blocked;

            if (mode == 1)
            {
                if (!long.TryParse(packetsplit[3], out charId)) return;
                Blocked = ClientLinkManager.Instance.GetProperty<bool>(charId, "ExchangeBlocked");

                if (Session.Character.Speed == 0 || ClientLinkManager.Instance.GetProperty<byte>(charId, "Speed") == 0)
                    Blocked = true;
                if (Blocked)
                {
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRADE_BLOCKED"), 11), ReceiverType.OnlyMe);
                }
                else
                {
                    Session.Character.ExchangeInfo = new ExchangeInfo { CharId = charId, Confirm = false };

                    charName = (string)ClientLinkManager.Instance.GetProperty<string>(charId, "Name");
                    Session.Client.SendPacket(Session.Character.GenerateModal($"{Language.Instance.GetMessageFromKey("YOU_ASK_FOR_EXCHANGE")} {charName}"));
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateDialog($"#req_exc^2^{Session.Character.CharacterId} #req_exc^5^{Session.Character.CharacterId} {String.Format(Language.Instance.GetMessageFromKey("INCOMING_EXCHANGE"), Session.Character.Name)}"), ReceiverType.OnlySomeone, charName);
                }
            }
            else if (mode == 3)
            {
                if (Session.Character.ExchangeInfo.CharId == Session.Character.CharacterId) return;
                ExchangeInfo exchange = ClientLinkManager.Instance.GetProperty<ExchangeInfo>(Session.Character.ExchangeInfo.CharId, "ExchangeInfo");
                long gold = ClientLinkManager.Instance.GetProperty<long>(Session.Character.ExchangeInfo.CharId, "Gold");
                int backpack = ClientLinkManager.Instance.GetProperty<int>(Session.Character.ExchangeInfo.CharId, "BackPack");
                InventoryList inventory = ClientLinkManager.Instance.GetProperty<InventoryList>(Session.Character.ExchangeInfo.CharId, "InventoryList");
                if (Session.Character.ExchangeInfo.Validate && exchange.Validate)
                {
                    Session.Character.ExchangeInfo.Confirm = true;
                    if (exchange.Confirm)
                    {
                        Session.Client.SendPacket("exc_close 1");
                        ClientLinkManager.Instance.Broadcast(Session, "exc_close 1", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                        bool continu = true;
                        bool goldmax = false;
                        bool notsold = false;
                        if (!Session.Character.InventoryList.getFreePlaceAmount(Session.Character.ExchangeInfo.ExchangeList, Session.Character.BackPack))
                            continu = false;

                        if (!inventory.getFreePlaceAmount(exchange.ExchangeList, backpack))
                            continu = false;

                        if (Session.Character.ExchangeInfo.Gold + gold > 1000000000)
                            goldmax = true;
                        if (Session.Character.ExchangeInfo.Gold > Session.Character.Gold)
                            return;
                        if (exchange.Gold + Session.Character.Gold > 1000000000)
                            goldmax = true;

                        if (continu == false)
                        {
                            string message = Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0);
                            Session.Client.SendPacket(message);
                            ClientLinkManager.Instance.Broadcast(Session, message, ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                            Session.Client.SendPacket("exc_close 0");
                            ClientLinkManager.Instance.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                            ClientLinkManager.Instance.SetProperty(Session.Character.ExchangeInfo.CharId, "ExchangeInfo", null);
                            Session.Character.ExchangeInfo = null;
                        }
                        else if (goldmax == true)
                        {
                            string message = Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                            Session.Client.SendPacket(message);
                            ClientLinkManager.Instance.Broadcast(Session, message, ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                            Session.Client.SendPacket("exc_close 0");
                            ClientLinkManager.Instance.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                            ClientLinkManager.Instance.SetProperty(Session.Character.ExchangeInfo.CharId, "ExchangeInfo", null);
                            Session.Character.ExchangeInfo = null;
                        }
                        else
                        {
                            foreach (InventoryItem item in Session.Character.ExchangeInfo.ExchangeList)
                            {
                                Inventory inv = Session.Character.InventoryList.getInventoryByInventoryItemId(item.InventoryItemId);
                                if (inv != null && ServerManager.GetItem(inv.InventoryItem.ItemVNum).IsTradable != true)
                                {
                                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_TRADABLE"), 0));
                                    Session.Client.SendPacket("exc_close 0");
                                    ClientLinkManager.Instance.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                                    ClientLinkManager.Instance.SetProperty(Session.Character.ExchangeInfo.CharId, "ExchangeInfo", null);
                                    Session.Character.ExchangeInfo = null;
                                    notsold = true;
                                    break;
                                }
                            }
                            if (!notsold)
                            {
                                foreach (InventoryItem item in Session.Character.ExchangeInfo.ExchangeList)
                                {
                                    // Delete items from their owners
                                    Inventory inv = Session.Character.InventoryList.getInventoryByInventoryItemId(item.InventoryItemId);
                                    Session.Character.InventoryList.DeleteByInventoryItemId(item.InventoryItemId);
                                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0));
                                }

                                foreach (InventoryItem item in exchange.ExchangeList)
                                {
                                    // Add items to their new owners
                                    Inventory inv = Session.Character.InventoryList.CreateItem(item, Session.Character);
                                    if (inv != null && inv.Slot != -1)
                                        Session.Client.SendPacket(
                                            Session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum,
                                                inv.InventoryItem.Amount, inv.Type, inv.Slot, inv.InventoryItem.Rare,
                                                inv.InventoryItem.Design, inv.InventoryItem.Upgrade));
                                }

                                Session.Character.Gold = Session.Character.Gold - Session.Character.ExchangeInfo.Gold + exchange.Gold;
                                Session.Client.SendPacket(Session.Character.GenerateGold());
                                ClientLinkManager.Instance.ExchangeValidate(Session, Session.Character.ExchangeInfo.CharId);

                                // TODO: Maybe log exchanges to a (new) table, so that the server admins could trace cheaters
                            }
                        }
                    }
                    else
                    {
                        charName = ClientLinkManager.Instance.GetProperty<string>(charId, "Name");

                        Session.Client.SendPacket(Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("IN_WAITING_FOR"), charName)));
                    }
                }
            }
            else if (mode == 4)
            {
                Session.Client.SendPacket("exc_close 0");
                ClientLinkManager.Instance.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);

                ClientLinkManager.Instance.SetProperty(Session.Character.ExchangeInfo.CharId, "ExchangeInfo", null);
                Session.Character.ExchangeInfo = null;
            }
        }

        [Packet("exc_list")]
        public void ExchangeList(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long Gold = 0; long.TryParse(packetsplit[2], out Gold);
            byte[] type = new byte[10];
            short[] slot = new short[10];
            byte[] qty = new byte[10];
            string packetList = "";
            for (int j = 6, i = 0; j <= packetsplit.Length; j += 3, i++)
            {
                byte.TryParse(packetsplit[j - 3], out type[i]);
                short.TryParse(packetsplit[j - 2], out slot[i]);
                byte.TryParse(packetsplit[j - 1], out qty[i]);
                Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot[i], type[i]);
                InventoryItem item = new InventoryItem(inv.InventoryItem);
                Session.Character.ExchangeInfo.ExchangeList.Add(item);
                item.Amount = qty[i];
                if (type[i] != 0)
                    packetList += $"{i}.{type[i]}.{item.ItemVNum}.{qty[i]} ";
                else
                    packetList += $"{i}.{type[i]}.{item.ItemVNum}.{inv.InventoryItem.Rare}.{inv.InventoryItem.Upgrade} ";
            }
            Session.Character.ExchangeInfo.Gold = Gold;
            ClientLinkManager.Instance.Broadcast(Session, $"exc_list 1 {Session.Character.CharacterId} {Gold} {packetList}", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.CharId);
            Session.Character.ExchangeInfo.Validate = true;
        }

        [Packet("f_stash_end")]
        public void FStashEnd(string packet)
        {
            // idk
        }

        [Packet("$ChangeSex")]
        public void Gender(string packet)
        {
            Session.Character.Gender = Session.Character.Gender == 1 ? (byte)0 : (byte)1;
            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SEX_CHANGED"), 0));
            Session.Client.SendPacket(Session.Character.GenerateEq());
            Session.Client.SendPacket(Session.Character.GenerateGender());
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
        }

        public string generatePidx(long CharId)
        {
            string stri = "pidx 1";
            foreach (long Id in ClientLinkManager.Instance.Groups.FirstOrDefault(s => s.Characters.Contains(CharId)).Characters)
            {
                stri += $" 1.{Id} ";
            }
            if (stri == "pidx 1")
                stri = "";
            return stri;
        }

        [Packet("get")]
        public void GetItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long DropId; long.TryParse(packetsplit[4], out DropId);
            MapItem mapitem;
            if (Session.CurrentMap.DroppedList.TryGetValue(DropId, out mapitem))
            {
                int Amount = mapitem.Amount;
                if (mapitem.PositionX < Session.Character.MapX + 3 && mapitem.PositionX > Session.Character.MapX - 3 && mapitem.PositionY < Session.Character.MapY + 3 && mapitem.PositionY > Session.Character.MapY - 3)
                {
                    if (mapitem.ItemVNum != 1046)
                    {
                        Inventory newInv = Session.Character.InventoryList.CreateItem((InventoryItem)mapitem, Session.Character);
                        if (newInv != null)
                        {
                            Session.CurrentMap.DroppedList.Remove(DropId);
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateGet(DropId), ReceiverType.AllOnMap);
                            Item iteminfo = ServerManager.GetItem(newInv.InventoryItem.ItemVNum);
                            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(newInv.InventoryItem.ItemVNum, newInv.InventoryItem.Amount, newInv.Type, newInv.Slot, newInv.InventoryItem.Rare, newInv.InventoryItem.Design, newInv.InventoryItem.Upgrade));
                            Session.Client.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {Amount}", 12));
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                        }
                    }
                    else
                    {
                        if (Session.Character.Gold + mapitem.Amount <= 1000000000)
                        {
                            Session.Character.Gold += mapitem.Amount;
                            Session.CurrentMap.DroppedList.Remove(DropId);
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateGet(DropId), ReceiverType.AllOnMap);
                            Session.Client.SendPacket(Session.Character.GenerateGold());
                        }
                        else
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                        }
                    }
                }
            }
        }

        [Packet("ncif")]
        public void GetNamedCharacterInformation(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if (packetsplit[2] == "1")
            {
                ClientLinkManager.Instance.RequiereBroadcastFromUser(Session, Convert.ToInt64(packetsplit[3]), "GenerateStatInfo");
            }
            if (packetsplit[2] == "2")
            {
                foreach (MapNpc npc in ServerManager.GetMap(Session.Character.MapId).Npcs)
                    if (npc.MapNpcId == Convert.ToInt32(packetsplit[3]))
                    {
                        NpcMonster npcinfo = ServerManager.GetNpc(npc.NpcVNum);
                        if (npcinfo == null)
                            return;
                        ClientLinkManager.Instance.Broadcast(Session, $"st 2 {packetsplit[3]} {npcinfo.Level} 100 100 50000 50000", ReceiverType.OnlyMe);
                    }
            }
            if (packetsplit[2] == "3")
            {
                foreach (MapMonster monster in ServerManager.GetMap(Session.Character.MapId).Monsters)
                    if (monster.MapMonsterId == Convert.ToInt32(packetsplit[3]))
                    {
                        NpcMonster monsterinfo = ServerManager.GetNpc(monster.MonsterVNum);
                        if (monsterinfo == null)
                            return;
                        ClientLinkManager.Instance.Broadcast(Session, $"st 3 {packetsplit[3]} {monsterinfo.Level} {monster.CurrentHp / monsterinfo.MaxHP * 100} {monster.CurrentMp / monsterinfo.MaxMP * 100} {monster.CurrentHp} {monster.CurrentMp}", ReceiverType.OnlyMe);
                    }
            }
        }

        public void GetStartupInventory()
        {
            foreach (String inv in Session.Character.GenerateStartupInventory())
            {
                Session.Client.SendPacket(inv);
            }
        }

        [Packet("npinfo")]
        public void GetStats(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateStatChar());
        }

        [Packet("$Gold")]
        public void Gold(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long gold;
            if (packetsplit.Length > 2)
            {
                if (Int64.TryParse(packetsplit[2], out gold))
                {
                    if (gold <= 1000000000 && gold >= 0)
                    {
                        Session.Character.Gold = gold;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_SET"), 0));
                        Session.Client.SendPacket(Session.Character.GenerateGold());
                    }
                    else
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Gold AMOUNT", 10));
        }

        [Packet(";")]
        public void GroupTalk(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            for (int i = 1; i < packetsplit.Length; i++)
                message += packetsplit[i] + " ";
            message = message.Substring(1).Trim();

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, 3), ReceiverType.Group);
        }

        [Packet("guri")]
        public void Guri(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2] == "10" && Convert.ToInt32(packetsplit[5]) >= 973 && Convert.ToInt32(packetsplit[5]) <= 999 && !Session.Character.EmoticonsBlocked)
            {
                Session.Client.SendPacket(Session.Character.GenerateEff(Convert.ToInt32(packetsplit[5]) + 4099));
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(Convert.ToInt32(packetsplit[5]) + 4099),
                    ReceiverType.AllOnMapNoEmoBlocked);
            }
            if (packetsplit[2] == "2")
                ClientLinkManager.Instance.Broadcast(Session, $"guri 2 1 {Session.Character.CharacterId}", ReceiverType.AllOnMap);
        }

        public async void healthTask()
        {
            int x = 1;
            while (true)
            {
                bool change = false;
                if (Session.Character.IsSitting)
                    await Task.Delay(1500);
                else
                    await Task.Delay(2000);
                if (Session.healthStop == true)
                {
                    Session.healthStop = false;
                    return;
                }
                if (x == 0)
                    x = 1;

                if (Session.Character.Hp + Session.Character.HealthHPLoad() < Session.Character.HPLoad())
                {
                    change = true;
                    Session.Character.Hp += Session.Character.HealthHPLoad();
                }
                else
                {
                    if (Session.Character.Hp != (int)Session.Character.HPLoad())
                        change = true;
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
                            change = true;
                        Session.Character.Mp = (int)Session.Character.MPLoad();
                    }
                    x = 0;
                }
                if (change)
                {
                    Session.Client.SendPacket(Session.Character.GenerateStat());
                }
            }
        }

        [Packet("hero")]
        public void Hero(string packet)
        {
            if (DAOFactory.CharacterDAO.IsReputHero(Session.Character.CharacterId) >= 3)
            {
                string[] packetsplit = packet.Split(' ');
                string message = String.Empty;
                for (int i = 2; i < packetsplit.Length; i++)
                    message += packetsplit[i] + " ";
                message.Trim();

                ClientLinkManager.Instance.Broadcast(Session, $"msg 5 [{Session.Character.Name}]:{message}", ReceiverType.AllNoHeroBlocked);
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_HERO"), 11));
            }
        }

        [Packet("$Invisible")]
        public void Invisible(string packet)
        {
            Session.Character.Invisible = Session.Character.Invisible == 0 ? 1 : 0;
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateInvisible(), ReceiverType.AllOnMap);
            Session.Character.InvisibleGm = Session.Character.InvisibleGm ? false : true;
            if (Session.Character.InvisibleGm == true)
            {
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllOnMapExceptMe);
            }
            else
            {
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
            }
        }

        [Packet("$JLvl")]
        public void JLvl(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte joblevel;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out joblevel) && ((Session.Character.Class == 0 && joblevel <= 20) || (Session.Character.Class != 0 && joblevel <= 80)) && joblevel > 0)
                {
                    Session.Character.JobLevel = joblevel;
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOBLEVEL_CHANGED"), 0));
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(8), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$JLvl JOBLEVEL", 10));
        }

        [Packet("$Kick")]
        public void Kick(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
                ClientLinkManager.Instance.Kick(packetsplit[2]);
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Kick CHARACTERNAME", 10));
        }

        [Packet("lbs")]
        public void Lbs(string packet)
        {
            // idk
        }

        /// <summary>
        /// Load Characters, this is the Entrypoint for the Client, Wait for 3 Packets.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        [Packet("OpenNos.EntryPoint", 3)]
        public void LoadCharacters(string packet)
        {
            string[] loginPacketParts = packet.Split(' ');

            // Load account by given SessionId
            if (Session.Account == null)
            {
                bool hasRegisteredAccountLogin = true;
                try
                {
                    hasRegisteredAccountLogin = ServiceFactory.Instance.CommunicationService.HasRegisteredAccountLogin(loginPacketParts[4], Session.SessionId);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                }

                if (loginPacketParts.Length > 4 && hasRegisteredAccountLogin)
                {
                    AccountDTO accountDTO = DAOFactory.AccountDAO.LoadByName(loginPacketParts[4]);

                    if (accountDTO != null)
                    {
                        if (accountDTO.Password.Equals(EncryptionBase.sha256(loginPacketParts[6])))
                        {
                            var account = new GameObject.Account()
                            {
                                AccountId = accountDTO.AccountId,
                                Name = accountDTO.Name,
                                Password = accountDTO.Password,
                                Authority = accountDTO.Authority
                            };

                            Session.InitializeAccount(account);
                        }
                        else
                        {
                            Logger.Log.ErrorFormat($"Client {Session.Client.ClientId} forced Disconnection, invalid Password or SessionId.");
                            Session.Client.Disconnect();
                        }
                    }
                    else
                    {
                        Logger.Log.ErrorFormat($"Client {Session.Client.ClientId} forced Disconnection, invalid AccountName.");
                        Session.Client.Disconnect();
                    }
                }
                else
                {
                    Logger.Log.ErrorFormat($"Client {Session.Client.ClientId} forced Disconnection, login has not been registered or Account is already logged in.");
                    Session.Client.Disconnect();
                    Session.Destroy();
                    return;
                }
            }

            IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(Session.Account.AccountId);
            Logger.Log.InfoFormat(Language.Instance.GetMessageFromKey("ACCOUNT_ARRIVED"), Session.SessionId);
            Session.Client.SendPacket("clist_start 0");
            foreach (CharacterDTO character in characters)
            {
                // Move to character
                InventoryItemDTO[] item = new InventoryItemDTO[15];
                for (short i = 0; i < 15; i++)
                {
                    InventoryDTO inv = DAOFactory.InventoryDAO.LoadBySlotAndType(character.CharacterId, i, (byte)InventoryType.Equipment);
                    if (inv != null)
                    {
                        inv.InventoryItem = DAOFactory.InventoryItemDAO.LoadByInventoryId(inv.InventoryId);
                        item[i] = inv.InventoryItem;
                    }
                }
                Session.Client.SendPacket($"clist {character.Slot} {character.Name} 0 {character.Gender} {character.HairStyle} {character.HairColor} 0 {character.Class} {character.Level} {(item[(byte)EquipmentType.Hat] != null ? item[(byte)EquipmentType.Hat].ItemVNum : 0)}.{(item[(byte)EquipmentType.Armor] != null ? item[(byte)EquipmentType.Armor].ItemVNum : 0)}.{(item[(byte)EquipmentType.MainWeapon] != null ? item[(byte)EquipmentType.MainWeapon].ItemVNum : 0)}.{(item[(byte)EquipmentType.SecondaryWeapon] != null ? item[(byte)EquipmentType.SecondaryWeapon].ItemVNum : 0)}.{(item[(byte)EquipmentType.Mask] != null ? item[(byte)EquipmentType.Mask].ItemVNum : 0)}.{(item[(byte)EquipmentType.Fairy] != null ? item[(byte)EquipmentType.Fairy].ItemVNum : 0)}.{(item[(byte)EquipmentType.CostumeSuit] != null ? item[(byte)EquipmentType.CostumeSuit].ItemVNum : 0)}.{(item[(byte)EquipmentType.CostumeHat] != null ? item[(byte)EquipmentType.CostumeHat].ItemVNum : 0)} 1 0 0 -1.-1 {(item[(byte)EquipmentType.Hat] != null ? (ServerManager.GetItem(item[(byte)EquipmentType.Hat].ItemVNum).IsColored ? item[(byte)EquipmentType.Hat].Design : character.HairColor) : character.HairColor)} 0");
            }
            Session.Client.SendPacket("clist_end");
        }

        public void loadShopItem(long owner, KeyValuePair<long, MapShop> shop)
        {
            string packetToSend = $"n_inv 1 {owner} 0 0";
            for (short i = 0; i < 20; i++)
            {
                PersonalShopItem item = shop.Value.Items.FirstOrDefault(it => it.Slot.Equals(i));
                if (item != null)
                {
                    if (ServerManager.GetItem(item.InventoryItem.ItemVNum).Type == 0)
                        packetToSend += $" 0.{i}.{item.InventoryItem.ItemVNum}.{item.InventoryItem.Rare}.{item.InventoryItem.Upgrade}.{item.Price}.";
                    else
                        packetToSend += $" {ServerManager.GetItem(item.InventoryItem.ItemVNum).Type}.{i}.{item.InventoryItem.ItemVNum}.{item.Amount}.{item.Price}.-1.";
                }
                else
                {
                    packetToSend += " -1";
                }
            }
            packetToSend += " -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1";

            Session.Client.SendPacket(packetToSend);
        }

        [Packet("$Lvl")]
        public void Lvl(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte level;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out level) && level < 100 && level > 0)
                {
                    Session.Character.Level = level;
                    Session.Character.Hp = (int)Session.Character.HPLoad();
                    Session.Character.Mp = (int)Session.Character.MPLoad();
                    Session.Client.SendPacket(Session.Character.GenerateStat());
                    // sc 0 0 31 39 31 4 70 1 0 33 35 43 2 70 0 17 35 19 35 17 0 0 0 0
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LEVEL_CHANGED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
                    this.GetStats(String.Empty);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 10));
        }

        [Packet("$MapDance")]
        public void MapDance(string packet)
        {
            Session.CurrentMap.IsDancing = Session.CurrentMap.IsDancing == 0 ? 2 : 0;
            if (Session.CurrentMap.IsDancing == 2)
            {
                Session.Character.Dance();
                ClientLinkManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "Dance");
                ClientLinkManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance 2");
            }
            else
            {
                Session.Character.Dance();
                ClientLinkManager.Instance.RequiereBroadcastFromAllMapUsers(Session, "Dance");
                ClientLinkManager.Instance.RequiereBroadcastFromMap(Session.Character.MapId, "dance");
            }
        }

        [Packet("$Morph")]
        public void Morph(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short[] arg = new short[4];
            bool verify = false;
            if (packetsplit.Length > 5)
            {
                verify = (short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && short.TryParse(packetsplit[5], out arg[3]));
            }
            switch (packetsplit.Length)
            {
                case 6:
                    if (verify)
                    {
                        if (arg[0] != 0)
                        {
                            Session.Character.UseSp = true;
                            Session.Character.Morph = arg[0];
                            Session.Character.MorphUpgrade = arg[1];
                            Session.Character.MorphUpgrade2 = arg[2];
                            Session.Character.ArenaWinner = arg[3];
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
                        }
                        else
                        {
                            Session.Character.UseSp = false;

                            Session.Client.SendPacket(Session.Character.GenerateCond());
                            Session.Client.SendPacket(Session.Character.GenerateLev());

                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
                            ClientLinkManager.Instance.Broadcast(Session, $"guri 6 1 {Session.Character.CharacterId} 0 0", ReceiverType.AllOnMap);
                        }
                    }
                    break;

                default:
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 10));
                    break;
            }
        }

        [Packet("mve")]
        public void MoveInventory(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            byte desttype; byte.TryParse(packetsplit[4], out desttype);
            short destslot; short.TryParse(packetsplit[5], out destslot);
            if (destslot > 48 + (Session.Character.BackPack * 12))
                return;
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                return;
            Inventory inv = Session.Character.InventoryList.moveInventory(type, slot, desttype, destslot);
            if (inv != null)
            {
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, desttype, inv.Slot, inv.InventoryItem.Rare, inv.InventoryItem.Design, inv.InventoryItem.Upgrade));
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));
            }
        }

        [Packet("mvi")]
        public void MoveItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            byte amount; byte.TryParse(packetsplit[4], out amount);
            short destslot; short.TryParse(packetsplit[5], out destslot);
            Inventory LastInventory;
            Inventory NewInventory;
            if (destslot > 48 + (Session.Character.BackPack * 12))
                return;
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                return;
            Session.Character.InventoryList.MoveItem(Session.Character, type, slot, amount, destslot, out LastInventory, out NewInventory);
            if (NewInventory == null) return;
            Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(NewInventory.InventoryItem.ItemVNum, NewInventory.InventoryItem.Amount, type, NewInventory.Slot, NewInventory.InventoryItem.Rare, NewInventory.InventoryItem.Design, NewInventory.InventoryItem.Upgrade));
            if (LastInventory != null)
                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(LastInventory.InventoryItem.ItemVNum, LastInventory.InventoryItem.Amount, type, LastInventory.Slot, LastInventory.InventoryItem.Rare, LastInventory.InventoryItem.Design, LastInventory.InventoryItem.Upgrade));
            else
            {
                DeleteItem(type, slot);
            }
        }

        [Packet("n_run")]
        public void npcRunFunction(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length <= 5) return;

            byte type; byte.TryParse(packetsplit[3], out type);
            short runner; short.TryParse(packetsplit[2], out runner);
            short data3; short.TryParse(packetsplit[4], out data3);
            short npcid; short.TryParse(packetsplit[5], out npcid);
            Session.Character.LastNRunId = npcid;
            NRunHandler.NRun(Session, type, runner, data3, npcid);
        }

        [Packet("gop")]
        public void option(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 4)
            {
                switch (int.Parse(packetsplit[2]))
                {
                    case (int)ConfigType.BuffBlocked:
                        Session.Character.BuffBlocked = int.Parse(packetsplit[3]) == 1;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.BuffBlocked
                                ? "BUFF_BLOCKED"
                                : "BUFF_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.EmoticonsBlocked:
                        Session.Character.EmoticonsBlocked = int.Parse(packetsplit[3]) == 1;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.EmoticonsBlocked
                                ? "EMO_BLOCKED"
                                : "EMO_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.ExchangeBlocked:
                        Session.Character.ExchangeBlocked = int.Parse(packetsplit[3]) == 0;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.ExchangeBlocked
                                ? "EXCHANGE_BLOCKED"
                                : "EXCHANGE_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.FriendRequestBlocked:
                        Session.Character.FriendRequestBlocked = int.Parse(packetsplit[3]) == 0;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.FriendRequestBlocked
                                ? "FRIEND_REQ_BLOCKED"
                                : "FRIEND_REQ_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.GroupRequestBlocked:
                        Session.Character.GroupRequestBlocked = int.Parse(packetsplit[3]) == 0;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.GroupRequestBlocked
                                ? "GROUP_REQ_BLOCKED"
                                : "GROUP_REQ_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.HeroChatBlocked:
                        Session.Character.HeroChatBlocked = int.Parse(packetsplit[3]) == 1;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.HeroChatBlocked
                                ? "HERO_CHAT_BLOCKED"
                                : "HERO_CHAT_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.HpBlocked:
                        Session.Character.HpBlocked = int.Parse(packetsplit[3]) == 1;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.HpBlocked
                                ? "HP_BLOCKED"
                                : "HP_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.MinilandInviteBlocked:
                        Session.Character.MinilandInviteBlocked = int.Parse(packetsplit[3]) == 1;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.MinilandInviteBlocked
                                ? "MINI_INV_BLOCKED"
                                : "MINI_INV_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.MouseAimLock:
                        Session.Character.MouseAimLock = int.Parse(packetsplit[3]) == 1;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.MouseAimLock
                                ? "MOUSE_LOCKED"
                                : "MOUSE_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.QuickGetUp:
                        Session.Character.QuickGetUp = int.Parse(packetsplit[3]) == 1;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.QuickGetUp
                                ? "QUICK_GET_UP_ENABLED"
                                : "QUICK_GET_UP_DISABLED"
                            ), 0));
                        break;

                    case (int)ConfigType.WhisperBlocked:
                        Session.Character.WhisperBlocked = int.Parse(packetsplit[3]) == 0;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.WhisperBlocked
                                ? "WHISPER_BLOCKED"
                                : "WHISPER_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.FamilyRequestBlocked:
                        Session.Character.FamilyRequestBlocked = int.Parse(packetsplit[3]) == 0;
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey(
                            Session.Character.FamilyRequestBlocked
                                ? "FAMILY_REQ_LOCKED"
                                : "FAMILY_REQ_UNLOCKED"
                            ), 0));
                        break;

                    case (int)ConfigType.GroupSharing:
                        Group grp = ClientLinkManager.Instance.Groups.FirstOrDefault(s => s.Characters.Contains(Session.Character.CharacterId));
                        if (grp == null)
                            return;
                        if (grp.Characters.ElementAt(0) != Session.Character.CharacterId)
                        {
                            Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_MASTER"), 0));
                            return;
                        }
                        if (int.Parse(packetsplit[3]) == 0)
                        {
                            ClientLinkManager.Instance.Groups.FirstOrDefault(s => s.Characters.Contains(Session.Character.CharacterId)).SharingMode = 1;
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING"), 0), ReceiverType.Group);

                        }
                        else
                        {
                            ClientLinkManager.Instance.Groups.FirstOrDefault(s => s.Characters.Contains(Session.Character.CharacterId)).SharingMode = 0;
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING_BY_ORDER"), 0), ReceiverType.Group);
                        }

                        break;
                }
            }
            Session.Client.SendPacket(Session.Character.GenerateStat());
        }

        [Packet("pdtse")]
        public void pdtse(string packet)
        {
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

                        InventoryItem newItem = new InventoryItem()
                        {
                            InventoryItemId = Session.Character.InventoryList.generateInventoryItemId(),
                            Amount = rec.Amount,
                            ItemVNum = rec.ItemVNum,
                            Rare = 0,
                            Upgrade = 0,
                            Design = 0,
                            Concentrate = 0,
                            CriticalLuckRate = 0,
                            CriticalRate = 0,
                            DamageMaximum = 0,
                            DamageMinimum = 0,
                            DarkElement = 0,
                            DistanceDefence = 0,
                            DistanceDefenceDodge = 0,
                            DefenceDodge = 0,
                            ElementRate = 0,
                            FireElement = 0,
                            HitRate = 0,
                            LightElement = 0,
                            IsFixed = false,
                            Ammo = 0,
                            MagicDefence = 0,
                            CloseDefence = 0,
                            SpXp = 0,
                            SpLevel = 0,
                            SlDefence = 0,
                            SlElement = 0,
                            SlDamage = 0,
                            SlHP = 0,
                            WaterElement = 0,
                        };
                        Item iteminfo = ServerManager.GetItem(rec.ItemVNum);
                        Inventory inv = Session.Character.InventoryList.CreateItem(newItem, Session.Character);
                        ServersData.SetRarityPoint(ref inv);
                        if (inv != null)
                        {
                            Session.Character.InventoryList.LoadByInventoryItem(inv.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;

                            short Slot = inv.Slot;
                            if (Slot != -1)
                            {
                                foreach (RecipeItem ite in rec.Items)
                                {
                                    Session.Character.InventoryList.RemoveItemAmount(ite.ItemVNum, ite.Amount);
                                }
                                GetStartupInventory();

                                Session.Client.SendPacket($"pdti 11 {inv.InventoryItem.ItemVNum} {rec.Amount} 29 {inv.InventoryItem.Upgrade} 0");
                                Session.Client.SendPacket($"guri 19 1 {Session.Character.CharacterId} 1324");

                                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("MADE_OBJECT"), iteminfo.Name, rec.Amount), 0));
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

        public void PerfectSP(Inventory item, InventoryItem.UpgradeProtection protect)
        {
            short[] upsuccess = { 50, 40, 30, 20, 10 };

            int[] goldprice = { 5000, 1000, 20000, 50000, 100000 };
            short[] stoneprice = { 1, 2, 3, 4, 5 };
            short stonevnum;
            byte upmode = 1;

            switch (ServerManager.GetItem(item.InventoryItem.ItemVNum).Morph)
            {
                case 2:
                    stonevnum = 2514;
                    break;

                case 6:
                    stonevnum = 2514;
                    break;

                case 9:
                    stonevnum = 2514;
                    break;

                case 12:
                    stonevnum = 2514;
                    break;

                case 3:
                    stonevnum = 2515;
                    break;

                case 4:
                    stonevnum = 2515;
                    break;

                case 14:
                    stonevnum = 2515;
                    break;

                case 5:
                    stonevnum = 2516;
                    break;

                case 11:
                    stonevnum = 2516;
                    break;

                case 15:
                    stonevnum = 2516;
                    break;

                case 10:
                    stonevnum = 2517;
                    break;

                case 13:
                    stonevnum = 2517;
                    break;

                case 7:
                    stonevnum = 2517;
                    break;

                case 17:
                    stonevnum = 2518;
                    break;

                case 18:
                    stonevnum = 2518;
                    break;

                case 19:
                    stonevnum = 2518;
                    break;

                case 20:
                    stonevnum = 2519;
                    break;

                case 21:
                    stonevnum = 2519;
                    break;

                case 22:
                    stonevnum = 2519;
                    break;

                case 23:
                    stonevnum = 2520;
                    break;

                case 24:
                    stonevnum = 2520;
                    break;

                case 25:
                    stonevnum = 2520;
                    break;

                case 26:
                    stonevnum = 2521;
                    break;

                case 27:
                    stonevnum = 2521;
                    break;

                case 28:
                    stonevnum = 2521;
                    break;

                default:
                    return;
            }
            if (item.InventoryItem.SpStoneUpgrade > 99)
                return;
            if (item.InventoryItem.SpStoneUpgrade > 80)
            {
                upmode = 5;
            }
            if (item.InventoryItem.SpStoneUpgrade > 60)
            {
                upmode = 4;
            }
            if (item.InventoryItem.SpStoneUpgrade > 40)
            {
                upmode = 3;
            }
            if (item.InventoryItem.SpStoneUpgrade > 20)
            {
                upmode = 2;
            }

            if (item.InventoryItem.IsFixed)
                return;
            if (Session.Character.Gold < goldprice[upmode])
                return;
            if (Session.Character.InventoryList.CountItem(stonevnum) < stoneprice[upmode])
                return;

            Random r = new Random();
            int rnd = r.Next(100);
            if (rnd <= upsuccess[upmode])
            {
                byte type = (byte)r.Next(16), count = 1;

                if (upmode == 4)
                {
                    count = 2;
                }
                if (count == 5)
                {
                    count = (byte)r.Next(3, 6);
                }

                Session.Client.SendPacket(Session.Character.GenerateEff(3005));

                if (type < 3)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.SpDamage += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ATTACK"), count), 0));
                }
                else if (type < 6)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.SpDefence += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_DEFENSE"), count), 0));
                }
                else if (type < 9)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.SpElement += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_ELEMENT"), count), 0));
                }
                else if (type < 12)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.SpHP += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_HPMP"), count), 0));
                }
                else if (type == 12)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.SpFire += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_FIRE"), count), 0));
                }
                else if (type == 13)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.SpWater += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_WATER"), count), 0));
                }
                else if (type == 14)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.SpLight += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_LIGHT"), count), 0));
                }
                else if (type == 15)
                {
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.SpDark += count;
                    Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("PERFECTSP_SUCCESS"), Language.Instance.GetMessageFromKey("PERFECTSP_SHADOW"), count), 0));
                }
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.SpStoneUpgrade++;
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PERFECTSP_FAILED"), 11));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("PERFECTSP_FAILED"), 0));
            }
            Session.Character.Gold = Session.Character.Gold - goldprice[upmode];
            Session.Client.SendPacket(Session.Character.GenerateGold());
            Session.Character.InventoryList.RemoveItemAmount(stonevnum, stoneprice[upmode]);
            GetStartupInventory();
        }

        [Packet("pjoin")]
        public void pjoin(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                Boolean Blocked = false;
                string charName;
                long charId = -1;
                if (!long.TryParse(packetsplit[3], out charId))
                    return;
                Boolean grouped1 = false;
                Boolean grouped2 = false;
                foreach (Group group in ClientLinkManager.Instance.Groups)
                {
                    if ((group.Characters.Contains(charId) || group.Characters.Contains(Session.Character.CharacterId)) && group.Characters.Count == 3)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                        return;
                    }
                    if (group.Characters.Contains(charId))
                    {
                        grouped1 = true;
                    }
                    if (group.Characters.Contains(Session.Character.CharacterId))
                    {
                        grouped2 = true;
                    }
                }

                if (grouped1 && grouped2)
                {
                    Session.Client.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_GROUP")));
                    return;
                }
                if (Convert.ToInt32(packetsplit[2]) == 0 || Convert.ToInt32(packetsplit[2]) == 1)
                {
                    if (Session.Character.CharacterId != charId)
                    {
                        if (!long.TryParse(packetsplit[3], out charId)) return;
                        Blocked = ClientLinkManager.Instance.GetProperty<bool>(charId, "GroupRequestBlocked");

                        if (Blocked)
                        {
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GROUP_BLOCKED"), 11), ReceiverType.OnlyMe);
                        }
                        else
                        {
                            Session.Character.ExchangeInfo = new ExchangeInfo { CharId = charId, Confirm = false };

                            charName = (string)ClientLinkManager.Instance.GetProperty<string>(charId, "Name");
                            Session.Client.SendPacket(Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("GROUP_REQUEST"), charName)));
                            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateDialog($"#pjoin^3^{ Session.Character.CharacterId} #pjoin^4^{Session.Character.CharacterId} {String.Format(Language.Instance.GetMessageFromKey("INVITED_YOU"), Session.Character.Name)}"), ReceiverType.OnlySomeone, charName);
                        }
                    }
                }
            }
        }

        [Packet("$PlayMusic")]
        public void PlayMusic(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                if (packetsplit.Length <= 1) return;

                short arg;
                short.TryParse(packetsplit[2], out arg);
                if (arg > -1)
                    ClientLinkManager.Instance.Broadcast(Session, $"bgm {arg}", ReceiverType.AllOnMap);
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$PlayMusic BGMUSIC", 10));
        }

        [Packet("pleave")]
        public void pleave(string packet)
        {
            ClientLinkManager.Instance.GroupLeave(Session);
        }

        [Packet("$Position")]
        public void Position(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateSay($"Map:{Session.Character.MapId} - X:{Session.Character.MapX} - Y:{Session.Character.MapY}", 12));
        }

        [Packet("preq")]
        public void Preq(string packet)
        {
            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
            double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
            if (!(timeSpanSinceLastPortal >= 4))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                return;
            }

            foreach (Portal portal in ServerManager.GetMap(Session.Character.MapId).Portals)
            {
                if (Session.Character.MapY >= portal.SourceY - 1 && Session.Character.MapY <= portal.SourceY + 1
                    && Session.Character.MapX >= portal.SourceX - 1 && Session.Character.MapX <= portal.SourceX + 1)
                {
                    switch (portal.Type)
                    {
                        case (sbyte)PortalType.MapPortal:
                        case (sbyte)PortalType.TSNormal:
                        case (sbyte)PortalType.Open:
                        case (sbyte)PortalType.Miniland:
                        case (sbyte)PortalType.TSEnd:
                        case (sbyte)PortalType.End:
                        case (sbyte)PortalType.Effect:
                        case (sbyte)PortalType.ShopTeleport:
                            break;

                        default:
                            Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"), 10));
                            return;
                    }

                    ClientLinkManager.Instance.MapOut(Session.Character.CharacterId);
                    Session.Character.MapId = portal.DestinationMapId;
                    Session.Character.MapX = portal.DestinationX;
                    Session.Character.MapY = portal.DestinationY;

                    Session.Character.LastPortal = currentRunningSeconds;
                    Session.Client.SendPacket(Session.Character.GenerateCMap());
                    ClientLinkManager.Instance.ChangeMap(Session.Character.CharacterId);
                    break;
                }
            }
        }

        [Packet("pulse")]
        public void Pulse(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            Session.Character.LastPulse += 60;
            if (Convert.ToInt32(packetsplit[2]) != Session.Character.LastPulse)
            {
                Session.Client.Disconnect();
            }
            deleteTimeout();
        }

        [Packet("put")]
        public void PutItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[2], out type);
            short slot; short.TryParse(packetsplit[3], out slot);
            byte amount; byte.TryParse(packetsplit[4], out amount);
            Inventory inv;
            Inventory invitem = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
            if (invitem != null && ServerManager.GetItem(invitem.InventoryItem.ItemVNum).IsDroppable == true && ServerManager.GetItem(invitem.InventoryItem.ItemVNum).IsTradable == true && (Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null && (Session.Character.ExchangeInfo == null || Session.Character.ExchangeInfo?.ExchangeList.Count() == 0)))
            {
                if (amount > 0 && amount < 100)
                {
                    MapItem DroppedItem = Session.Character.InventoryList.PutItem(Session, type, slot, amount, out inv);
                    if (DroppedItem == null)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE_HERE"), 0)); ;
                        return;
                    }
                    Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(inv.InventoryItem.ItemVNum, inv.InventoryItem.Amount, type, inv.Slot, inv.InventoryItem.Rare, inv.InventoryItem.Design, inv.InventoryItem.Upgrade));

                    if (inv.InventoryItem.Amount == 0)
                        DeleteItem(type, inv.Slot);
                    if (DroppedItem != null)
                        ClientLinkManager.Instance.Broadcast(Session, $"drop {DroppedItem.ItemVNum} {DroppedItem.InventoryItemId} {DroppedItem.PositionX} {DroppedItem.PositionY} {DroppedItem.Amount} 0 -1", ReceiverType.AllOnMap);
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_DROP_AMOUNT"), 0));
                }
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE"), 0));
            }
        }

        [Packet("$Rarify")]
        public void Rarify(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 10));
            }
            else
            {
                short itemslot = -1;
                short mode = -1;
                short protection = -1;
                short.TryParse(packetsplit[2], out itemslot);
                short.TryParse(packetsplit[3], out mode);
                short.TryParse(packetsplit[4], out protection);

                if (itemslot > -1 && mode > -1 && protection > -1)
                {
                    Inventory inventoryDTO = Session.Character.InventoryList.LoadBySlotAndType(itemslot, 0);
                    if (inventoryDTO != null)
                    {
                        RarifyItem(inventoryDTO, (InventoryItem.RarifyMode)mode, (InventoryItem.RarifyProtection)protection);
                    }
                }
            }
        }

        public void RarifyItem(Inventory item, InventoryItem.RarifyMode mode, InventoryItem.RarifyProtection protection)
        {
            double rare1 = 50;
            double rare2 = 35;
            double rare3 = 25;
            double rare4 = 10;
            double rare5 = 10;
            double rare6 = 5;
            double rare7 = 1;
            double reducedchancefactor = 1.1;

            //short itempricevnum = 0;
            short goldprice = 500;
            double reducedpricefactor = 0.5;

            byte cella = 5;
            int cellavnum = 1014;

            if (protection == InventoryItem.RarifyProtection.RedAmulet)
            {
                rare1 = rare1 * reducedchancefactor;
                rare2 = rare2 * reducedchancefactor;
                rare3 = rare3 * reducedchancefactor;
                rare4 = rare4 * reducedchancefactor;
                rare5 = rare5 * reducedchancefactor;
                rare6 = rare6 * reducedchancefactor;
                rare7 = rare7 * reducedchancefactor;
            }
            switch (mode)
            {
                case InventoryItem.RarifyMode.Free:
                    break;

                case InventoryItem.RarifyMode.Reduced:
                    // TODO: Reduced Item Amount
                    if (Session.Character.Gold < goldprice * reducedpricefactor)
                        return;
                    Session.Character.Gold = Session.Character.Gold - (long)(goldprice * reducedpricefactor);
                    if (Session.Character.InventoryList.CountItem(cellavnum) < cella * reducedpricefactor)
                        return;
                    Session.Character.InventoryList.RemoveItemAmount(cellavnum, (int)(cella * reducedpricefactor));
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    break;

                case InventoryItem.RarifyMode.Normal:
                    // TODO: Normal Item Amount
                    if (Session.Character.Gold < goldprice)
                        return;
                    Session.Character.Gold = Session.Character.Gold - goldprice;
                    if (Session.Character.InventoryList.CountItem(cellavnum) < cella)
                        return;
                    Session.Character.InventoryList.RemoveItemAmount(cellavnum, cella);
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    break;
            }

            Random r = new Random();
            int rnd = r.Next(100);
            if (rnd <= rare7 && !(protection == InventoryItem.RarifyProtection.Scroll && item.InventoryItem.Rare >= 7))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 7), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 7), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.Rare = 7;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare6 && !(protection == InventoryItem.RarifyProtection.Scroll && item.InventoryItem.Rare >= 6))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 6), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 6), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.Rare = 6;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare5 && !(protection == InventoryItem.RarifyProtection.Scroll && item.InventoryItem.Rare >= 5))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 5), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 5), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.Rare = 5;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare4 && !(protection == InventoryItem.RarifyProtection.Scroll && item.InventoryItem.Rare >= 4))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 4), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 4), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.Rare = 4;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare3 && !(protection == InventoryItem.RarifyProtection.Scroll && item.InventoryItem.Rare >= 3))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 3), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 3), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.Rare = 3;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare2 && !(protection == InventoryItem.RarifyProtection.Scroll && item.InventoryItem.Rare >= 2))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 2), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 2), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.Rare = 2;

                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else if (rnd <= rare1 && !(protection == InventoryItem.RarifyProtection.Scroll && item.InventoryItem.Rare >= 1))
            {
                Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 1), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("RARIFY_SUCCESS"), 1), 0));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.Rare = 1;
                Inventory inv = Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId);
                ServersData.SetRarityPoint(ref inv);
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem = inv.InventoryItem;
            }
            else
            {
                if (protection == InventoryItem.RarifyProtection.None)
                {
                    DeleteItem(item.Type, item.Slot);
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED"), 0));
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("RARIFY_FAILED_ITEM_SAVED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.IsFixed = true;
                }
            }
            GetStartupInventory();
            Session.Client.SendPacket("shop_end 1");
        }

        [Packet("remove")]
        public void remove(string packet)
        {
            // Undress Equipment
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3 && Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null && (Session.Character.ExchangeInfo == null || Session.Character.ExchangeInfo?.ExchangeList.Count() == 0))
            {
                short slot;
                if (!short.TryParse(packetsplit[2], out slot)) return; // Invalid Number

                Inventory inventory = Session.Character.EquipmentList.LoadBySlotAndType(slot, (byte)InventoryType.Equipment);
                if (inventory == null) return; // This eqslot is not equipped

                if (slot == (byte)EquipmentType.Sp && Session.Character.UseSp)
                {
                    Session.Character.LastSp = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                    new Task(() => RemoveSP(inventory.InventoryItem.ItemVNum)).Start();
                }

                // Put item back to inventory
                Inventory inv = Session.Character.InventoryList.CreateItem(new InventoryItem(inventory.InventoryItem), Session.Character);
                if (inv == null) return;

                if (inv.Slot != -1)
                    Session.Client.SendPacket(
                        Session.Character.GenerateInventoryAdd(inventory.InventoryItem.ItemVNum,
                            inv.InventoryItem.Amount, inv.Type, inv.Slot, inventory.InventoryItem.Rare,
                            inventory.InventoryItem.Design, inventory.InventoryItem.Upgrade));

                Session.Character.EquipmentList.DeleteFromSlotAndType(slot, (byte)InventoryType.Equipment);

                Session.Client.SendPacket(Session.Character.GenerateStatChar());
                Thread.Sleep(100);
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.AllOnMap);
                Session.Client.SendPacket(Session.Character.GenerateEquipment());
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
            }
        }

        public async void RemoveSP(short vnum)
        {
            Inventory sp = Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
            Session.Character.Speed -= ServerManager.GetItem(vnum).Speed;
            Session.Character.UseSp = false;

            /* string s2 = "c_info " + chara.name + " - -1 -1 - " + chara.id + " " + ((chara.isGm) ? 2 : 0) + " " + +chara.sex + " " + +chara.Hair.style + " " + +chara.Hair.color + " " + chara.user_class + " " + Stats.GetReput(chara.Reput, chara.dignite.ToString()) + " " + (chara.Sp.inUsing ? chara.Sp.sprite : 0) + " 0 - " + (chara.Sp.inUsing ? chara.Sp.upgrade == 15 ? chara.Sp.wings > 4 ? 0 : 15 : chara.Sp.upgrade : 0) + " " + (chara.Sp.inUsing ? (chara.Sp.wings > 4) ? chara.Sp.wings - 4 : chara.Sp.wings : 0) + " " + (chara.Sp.wings_arena ? 1 : 0);
            chara.Send(s2);
            s2 = "at " + chara.id + " " + chara.MapPoint.map + " " + chara.MapPoint.x + " " + +chara.MapPoint.y + " 2 0 0 1";
            chara.Send(s2); */

            Session.Client.SendPacket(Session.Character.GenerateCond());
            Session.Client.SendPacket(Session.Character.GenerateLev());

            /* string s="sl 0";
               chara.Send(s); */

            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("STAY_TIME")), 11));
            Session.Client.SendPacket("sd 30");

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, $"guri 6 1 {Session.Character.CharacterId} 0 0", ReceiverType.AllOnMap);

            /* s="ms_c";
            chara.Send(s); */

            // lev 40 2288403 23 47450 3221180 113500 20086 5

            Session.Client.SendPacket(Session.Character.GenerateStat());
            Session.Client.SendPacket(Session.Character.GenerateStatChar());
            await Task.Delay(30000);
            if (Session == null || Session.Client == null)
                return;
            Session.Client.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPEAR")), 11));
            Session.Client.SendPacket("sd 0");
        }

        public void RemoveVehicle()
        {
            Session.Character.IsVehicled = false;
            Session.Character.Speed = ServersData.SpeedData[Session.Character.Class];
            Session.Client.SendPacket(Session.Character.GenerateCond());
            if (Session.Character.UseSp)
            {
                Session.Character.Morph = ServerManager.GetItem(Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment).InventoryItem.ItemVNum).Morph;
            }
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateCMode(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, $"guri 6 1 {Session.Character.CharacterId} 0 0", ReceiverType.AllOnMap);
        }

        [Packet("$ChangeRep")]
        public void Rep(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            long reput;
            if (packetsplit.Length != 3)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$ChangeRep REPUTATION", 10));
                return;
            }

            if (Int64.TryParse(packetsplit[2], out reput) && reput > 0)
            {
                Session.Character.Reput = reput;
                Session.Client.SendPacket(Session.Character.GenerateFd());
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REP_CHANGED"), 0));
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
            }
        }

        [Packet("req_info")]
        public void ReqInfo(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit[2] == "5")
            {
                NpcMonster npc = ServerManager.GetNpc(short.Parse(packetsplit[3]));
                if (npc != null)
                {
                    Session.Client.SendPacket(npc.GenerateEInfo());
                }
            }
            else
                ClientLinkManager.Instance.RequiereBroadcastFromUser(Session, Convert.ToInt64(packetsplit[3]), "GenerateReqInfo");
        }

        [Packet("$Resize")]
        public void Resize(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short arg = -1;

            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out arg);

                if (arg > -1)

                {
                    Session.Character.Size = arg;
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateScal(), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 10));
        }

        [Packet("rest")]
        public void Rest(string packet)
        {
            Session.Character.IsSitting = !Session.Character.IsSitting;
            if (Session.Character.IsVehicled)
                Session.Character.IsSitting = false;
            if (Session.Character.ThreadCharChange != null && Session.Character.ThreadCharChange.IsAlive)
                Session.Character.ThreadCharChange.Abort();

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateRest(), ReceiverType.AllOnMap);
        }

        [Packet("say")]
        public void Say(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = "";
            for (int i = 2; i < packetsplit.Length; i++)
                message += packetsplit[i] + " ";

            ClientLinkManager.Instance.Broadcast(Session,
                Session.Character.GenerateSay(message.Trim(), 0),
                ReceiverType.AllOnMapExceptMe);
        }

        [Packet("select")]
        public void SelectCharacter(string packet)
        {
            try
            {
                string[] packetsplit = packet.Split(' ');
                CharacterDTO characterDTO = DAOFactory.CharacterDAO.LoadBySlot(Session.Account.AccountId, Convert.ToByte(packetsplit[2]));
                if (characterDTO != null)
                    Session.Character = new Character
                    {
                        AccountId = characterDTO.AccountId,
                        CharacterId = characterDTO.CharacterId,
                        Class = characterDTO.Class,
                        Dignite = characterDTO.Dignite,
                        Gender = characterDTO.Gender,
                        Gold = characterDTO.Gold,
                        HairColor = characterDTO.HairColor,
                        HairStyle = characterDTO.HairStyle,
                        Hp = characterDTO.Hp,
                        JobLevel = characterDTO.JobLevel,
                        JobLevelXp = characterDTO.JobLevelXp,
                        Level = characterDTO.Level,
                        LevelXp = characterDTO.LevelXp,
                        MapId = characterDTO.MapId,
                        MapX = characterDTO.MapX,
                        MapY = characterDTO.MapY,
                        Mp = characterDTO.Mp,
                        State = characterDTO.State,
                        Faction = characterDTO.Faction,
                        Name = characterDTO.Name,
                        Reput = characterDTO.Reput,
                        Slot = characterDTO.Slot,
                        Authority = Session.Account.Authority,
                        SpAdditionPoint = characterDTO.SpAdditionPoint,
                        SpPoint = characterDTO.SpPoint,
                        LastPulse = 0,
                        LastPortal = 0,
                        LastSp = 0,
                        Invisible = 0,
                        InvisibleGm = false,
                        ArenaWinner = characterDTO.ArenaWinner,
                        Morph = 0,
                        MorphUpgrade = 0,
                        MorphUpgrade2 = 0,
                        Direction = 0,
                        IsSitting = false,
                        BackPack = characterDTO.Backpack,
                        Speed = ServersData.SpeedData[characterDTO.Class],
                        Compliment = characterDTO.Compliment,
                        Backpack = characterDTO.Backpack,
                        BuffBlocked = characterDTO.BuffBlocked,
                        EmoticonsBlocked = characterDTO.EmoticonsBlocked,
                        WhisperBlocked = characterDTO.WhisperBlocked,
                        FamilyRequestBlocked = characterDTO.FamilyRequestBlocked,
                        ExchangeBlocked = characterDTO.ExchangeBlocked,
                        FriendRequestBlocked = characterDTO.FriendRequestBlocked,
                        GroupRequestBlocked = characterDTO.GroupRequestBlocked,
                        HeroChatBlocked = characterDTO.HeroChatBlocked,
                        HpBlocked = characterDTO.HpBlocked,
                        MinilandInviteBlocked = characterDTO.MinilandInviteBlocked,
                        QuickGetUp = characterDTO.QuickGetUp,
                        MouseAimLock = characterDTO.MouseAimLock,
                        LastLogin = DateTime.Now,
                        SnackHp = 0,
                        SnackMp = 0,
                        SnackAmount = 0,
                        MaxSnack = 0,
                    };

                Session.Character.Update();
                Session.Character.LoadInventory();
                Session.Character.LoadSkills();
                Session.Character.LoadQuicklists();
                DAOFactory.AccountDAO.WriteGeneralLog(Session.Character.AccountId, Session.Client.RemoteEndPoint.ToString(), Session.Character.CharacterId, "Connection", "World");
                Session.Client.SendPacket("OK");
                Session.HealthTask = new Task(() => healthTask());

                Session.HealthTask.Start();

                // Inform everyone about connected character
                ServiceFactory.Instance.CommunicationService.ConnectCharacter(Session.Character.Name, Session.Account.Name);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }
        }

        [Packet("sell")]
        public void SellShop(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 6)
            {
                byte type, amount, slot;
                if (!byte.TryParse(packetsplit[4], out type) || !byte.TryParse(packetsplit[5], out slot) || !byte.TryParse(packetsplit[6], out amount)) return;

                Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                if (inv == null || amount > inv.InventoryItem.Amount) return;

                if (ServerManager.GetItem(inv.InventoryItem.ItemVNum).IsSoldable != true)
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"), 0));
                    // TODO: Need to see if on global it's a MSG packet
                    return;
                }

                Item item = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                if (Session.Character.Gold + item.Price * amount > 1000000000)
                {
                    string message = Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                    Session.Client.SendPacket(message);
                    return;
                }
                Session.Character.Gold += (item.Price / 20) * amount;
                DeleteItem(type, slot);
                Session.Client.SendPacket(Session.Character.GenerateGold());
                Session.Client.SendPacket(Session.Character.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("SELL_ITEM_VALIDE"), item.Name, amount)));
            }
            else if (packetsplit.Length == 5)
            {
                short vnum = -1;
                short.TryParse(packetsplit[4], out vnum);
                Skill skillinfo = ServerManager.GetSkill(vnum);
                CharacterSkill skill = Session.Character.Skills.FirstOrDefault(s => s.SkillVNum == vnum);
                if (skill == null || skillinfo == null)
                    return;
                Session.Character.Gold -= skillinfo.Cost;
                Session.Client.SendPacket(Session.Character.GenerateGold());

                foreach (CharacterSkill sk in Session.Character.Skills)
                {
                    Skill skinfo = ServerManager.GetSkill(sk.SkillVNum);
                    if (skillinfo.UpgradeSkill == skinfo.UpgradeSkill)
                        Session.Character.Skills.Remove(sk);
                }

                Session.Character.Skills.Remove(skill);
                Session.Client.SendPacket(Session.Character.GenerateSki());
                Session.Client.SendPacket(Session.Character.GenerateLev());

            }
        }

        [Packet("shopping")]
        public void Shopping(string packet)
        {
            // n_inv 2 1834 0 100 0.13.13.0.0.330 0.14.15.0.0.2299 0.18.120.0.0.3795 0.19.107.0.0.3795 0.20.94.0.0.3795 0.37.95.0.0.5643 0.38.97.0.0.11340 0.39.99.0.0.18564 0.48.108.0.0.5643 0.49.110.0.0.11340 0.50.112.0.0.18564 0.59.121.0.0.5643 0.60.123.0.0.11340 0.61.125.0.0.18564
            string[] packetsplit = packet.Split(' ');
            byte type;
            int NpcId;
            byte typeshop = 0;
            if (!int.TryParse(packetsplit[5], out NpcId) || !byte.TryParse(packetsplit[2], out type)) return;

            MapNpc mapnpc = Session.CurrentMap.Npcs.FirstOrDefault(n => n.MapNpcId.Equals(NpcId));
            NpcMonster npc = ServerManager.GetNpc(mapnpc.NpcVNum);
            if (mapnpc?.Shop == null) return;

            string shoplist = "";
            foreach (ShopItem item in mapnpc.Shop.ShopItems.Where(s => s.Type.Equals(type)))
            {
                Item iteminfo = ServerManager.GetItem(item.ItemVNum);
                double pourcent = 1;
                if (Session.Character.GetDigniteIco() == 3)
                    pourcent = 1.10;
                else if (Session.Character.GetDigniteIco() == 4)
                    pourcent = 1.20;
                else if (Session.Character.GetDigniteIco() == 5 || Session.Character.GetDigniteIco() == 6)
                    pourcent = 1.5;

                if (iteminfo.ReputPrice > 0 && iteminfo.Type == 0)
                    shoplist += $" {iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{item.Rare}.{(iteminfo.IsColored ? item.Color : item.Upgrade)}.{ServerManager.GetItem(item.ItemVNum).ReputPrice}";
                else if (iteminfo.ReputPrice > 0 && iteminfo.Type != 0)
                    shoplist += $" {iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{-1}.{ServerManager.GetItem(item.ItemVNum).ReputPrice}";
                else if (iteminfo.Type != 0)
                    shoplist += $" {iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{-1}.{ServerManager.GetItem(item.ItemVNum).Price * pourcent}";
                else
                    shoplist += $" {iteminfo.Type}.{item.Slot}.{item.ItemVNum}.{item.Rare}.{(iteminfo.IsColored ? item.Color : item.Upgrade)}.{ServerManager.GetItem(item.ItemVNum).Price * pourcent}";
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

        [Packet("$Shout")]
        public void Shout(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            if (packetsplit.Length > 2)
                for (int i = 2; i < packetsplit.Length; i++)
                    message += packetsplit[i] + " ";
            message.Trim();

            ClientLinkManager.Instance.Broadcast(Session, $"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}", ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
        }

        [Packet("npc_req")]
        public void ShowShop(string packet)
        {
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

                    KeyValuePair<long, MapShop> shopList = Session.CurrentMap.ShopUserList.FirstOrDefault(s => s.Value.OwnerId.Equals(owner));
                    loadShopItem(owner, shopList);
                }
                else
                {
                    // Npc Shop
                    MapNpc npc = ServerManager.GetMap(Session.Character.MapId).Npcs.FirstOrDefault(n => n.MapNpcId.Equals(Convert.ToInt16(packetsplit[3])));
                    if (!string.IsNullOrEmpty(npc?.GetNpcDialog()))
                        Session.Client.SendPacket(npc.GetNpcDialog());
                }
            }
        }

        [Packet("$Shutdown")]
        public void Shutdown(string packet)
        {
            if (ClientLinkManager.Instance.taskShutdown != null)
            {
                ClientLinkManager.Instance.ShutdownStop = true;
                ClientLinkManager.Instance.taskShutdown = null;
            }
            else
            {
                ClientLinkManager.Instance.taskShutdown = new Task(ShutdownTask);
                ClientLinkManager.Instance.taskShutdown.Start();
            }
        }

        public async void ShutdownTask()
        {
            string message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 5);
            ClientLinkManager.Instance.Broadcast(Session, $"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}", ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            for (int i = 0; i < 60 * 4; i++)
            {
                await Task.Delay(1000);
                if (ClientLinkManager.Instance.ShutdownStop == true)
                {
                    ClientLinkManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 1);
            ClientLinkManager.Instance.Broadcast(Session, $"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}", ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (ClientLinkManager.Instance.ShutdownStop == true)
                {
                    ClientLinkManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 30);
            ClientLinkManager.Instance.Broadcast(Session, $"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}", ReceiverType.All);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMsg(message, 2), ReceiverType.All);
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (ClientLinkManager.Instance.ShutdownStop == true)
                {
                    ClientLinkManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            ClientLinkManager.Instance.SaveAll();
            Environment.Exit(0);
        }

        [Packet("#sl")]
        public void sl(string packet)
        {
            // idk
        }

        [Packet("snap")]
        public void Snap(string packet)
        {
            // Not needed for now. (pictures)
        }
        [Packet("shopclose")]
        public void shopclose(string packet)
        {
            // Not needed for now. 
        }
        [Packet("sortopen")]
        public void sortopen(string packet)
        {
            Boolean gravity = true;
            byte type;
            while (gravity)
            {
                gravity = false;
                for (short x = 0; x < 44; x++)
                {
                    for (short i = 0; i < 2; i++)
                    {
                        type = (i == 0) ? (byte)InventoryType.Sp : (byte)InventoryType.Costume;
                        if (Session.Character.InventoryList.LoadBySlotAndType(x, type) == null)
                        {
                            if (Session.Character.InventoryList.LoadBySlotAndType((short)(x + 1), type) != null)
                            {
                                Inventory invdest = new Inventory();
                                Inventory inv = new Inventory();
                                Session.Character.InventoryList.MoveItem(Session.Character, type, (short)(x + 1), 1, x, out inv, out invdest);
                                Session.Client.SendPacket(Session.Character.GenerateInventoryAdd(invdest.InventoryItem.ItemVNum, invdest.InventoryItem.Amount, type, invdest.Slot, invdest.InventoryItem.Rare, invdest.InventoryItem.Design, invdest.InventoryItem.Upgrade));
                                DeleteItem(type, (short)(x + 1));
                                gravity = true;
                            }
                        }
                    }
                }
            }
        }

        [Packet("$Speed")]
        public void Speed(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte arg = 0;
            bool verify = false;
            if (packetsplit.Length > 2)
            {
                verify = (byte.TryParse(packetsplit[2], out arg));
            }
            switch (packetsplit.Length)
            {
                case 3:
                    if (verify && arg < 60)
                    {
                        Session.Character.Speed = arg;
                        Session.Client.SendPacket(Session.Character.GenerateCond());
                    }
                    break;

                default:
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 10));
                    break;
            }
        }

        [Packet("$SPLvl")]
        public void SPLvl(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte splevel;
            Inventory sp = Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);
            if (sp != null && packetsplit.Length > 2 && Session.Character.UseSp)
            {
                if (Byte.TryParse(packetsplit[2], out splevel) && splevel <= 99 && splevel > 0)
                {
                    sp.InventoryItem.SpLevel = splevel;
                    Session.Client.SendPacket(Session.Character.GenerateLev());
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SPLEVEL_CHANGED"), 0));
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllOnMapExceptMe);
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(8), ReceiverType.AllOnMap);
                }
            }
            else

                Session.Client.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 10));
        }

        [Packet("sl")]
        public void SpTransform(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            Inventory spInventory = Session.Character.EquipmentList.LoadBySlotAndType((byte)EquipmentType.Sp, (byte)InventoryType.Equipment);

            if (packetsplit.Length == 10 && packetsplit[2] == "10")
            {
                // There you go, SP!

                if (!Session.Character.UseSp || spInventory == null || int.Parse(packetsplit[5]) != spInventory.InventoryItem.InventoryItemId)
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SPUSE_NEEDED"), 0));
                    return;
                }

                if (ServersData.SpPoint(spInventory.InventoryItem.SpLevel, spInventory.InventoryItem.Upgrade)
                    - spInventory.InventoryItem.SlDamage - spInventory.InventoryItem.SlHP
                    - spInventory.InventoryItem.SlElement - spInventory.InventoryItem.SlDefence
                    - short.Parse(packetsplit[6]) - short.Parse(packetsplit[7])
                    - short.Parse(packetsplit[8]) - short.Parse(packetsplit[9]) < 0)
                    return;

                spInventory.InventoryItem.SlDamage += short.Parse(packetsplit[6]);
                spInventory.InventoryItem.SlDefence += short.Parse(packetsplit[7]);
                spInventory.InventoryItem.SlElement += short.Parse(packetsplit[8]);
                spInventory.InventoryItem.SlHP += short.Parse(packetsplit[9]);

                int slElement = ServersData.SlPoint(spInventory.InventoryItem.SlElement, 2);
                int slHp = ServersData.SlPoint(spInventory.InventoryItem.SlHP, 3);
                int slDefence = ServersData.SlPoint(spInventory.InventoryItem.SlDefence, 1);
                int slHit = ServersData.SlPoint(spInventory.InventoryItem.SlDamage, 0);

                //so add upgrade to sp
                //slhit
                spInventory.InventoryItem.DamageMinimum = 0;
                spInventory.InventoryItem.DamageMaximum = 0;
                spInventory.InventoryItem.HitRate = 0;
                spInventory.InventoryItem.CriticalLuckRate = 0;
                spInventory.InventoryItem.CriticalRate = 0;
                spInventory.InventoryItem.DefenceDodge = 0;
                spInventory.InventoryItem.DistanceDefenceDodge = 0;
                spInventory.InventoryItem.ElementRate = 0;
                spInventory.InventoryItem.DarkResistance = 0;
                spInventory.InventoryItem.LightResistance = 0;
                spInventory.InventoryItem.FireResistance = 0;
                spInventory.InventoryItem.WaterResistance = 0;
                spInventory.InventoryItem.CriticalDodge = 0;
                spInventory.InventoryItem.MagicDefence = 0;
                spInventory.InventoryItem.HP = 0;
                spInventory.InventoryItem.MP = 0;

                if (slHit >= 1)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slHit >= 10)
                {
                    spInventory.InventoryItem.HitRate += 10;
                }
                if (slHit >= 20)
                {
                    spInventory.InventoryItem.CriticalLuckRate += 2;
                }
                if (slHit >= 30)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                    spInventory.InventoryItem.HitRate += 10;
                }
                if (slHit >= 40)
                {
                    spInventory.InventoryItem.CriticalRate += 10;
                }
                if (slHit >= 50)
                {
                    spInventory.InventoryItem.HP += 200;
                    spInventory.InventoryItem.MP += 200;
                }
                if (slHit >= 60)
                {
                    spInventory.InventoryItem.HitRate += 15;
                }
                if (slHit >= 70)
                {
                    spInventory.InventoryItem.HitRate += 15;
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slHit >= 80)
                {
                    spInventory.InventoryItem.CriticalLuckRate += 2;
                }
                if (slHit >= 90)
                {
                    spInventory.InventoryItem.CriticalRate += 20;
                }
                //sldef
                if (slDefence >= 20)
                {
                    spInventory.InventoryItem.DefenceDodge += 2;
                    spInventory.InventoryItem.DistanceDefenceDodge += 2;
                }
                if (slDefence >= 30)
                {
                    spInventory.InventoryItem.HP += 100;
                }
                if (slDefence >= 40)
                {
                    spInventory.InventoryItem.DefenceDodge += 2;
                    spInventory.InventoryItem.DistanceDefenceDodge += 2;
                }
                if (slDefence >= 60)
                {
                    spInventory.InventoryItem.HP += 200;
                }
                if (slDefence >= 70)
                {
                    spInventory.InventoryItem.DefenceDodge += 3;
                    spInventory.InventoryItem.DistanceDefenceDodge += 3;
                }
                if (slDefence >= 75)
                {
                    spInventory.InventoryItem.FireResistance += 2;
                    spInventory.InventoryItem.WaterResistance += 2;
                    spInventory.InventoryItem.LightResistance += 2;
                    spInventory.InventoryItem.DarkResistance += 2;
                }
                if (slDefence >= 80)
                {
                    spInventory.InventoryItem.DefenceDodge += 3;
                    spInventory.InventoryItem.DistanceDefenceDodge += 3;
                }
                if (slDefence >= 90)
                {
                    spInventory.InventoryItem.FireResistance += 3;
                    spInventory.InventoryItem.WaterResistance += 3;
                    spInventory.InventoryItem.LightResistance += 3;
                    spInventory.InventoryItem.DarkResistance += 3;
                }
                if (slDefence >= 95)
                {
                    spInventory.InventoryItem.HP += 300;
                }
                //slele
                if (slElement >= 1)
                {
                    spInventory.InventoryItem.ElementRate += 2;
                }
                if (slElement >= 10)
                {
                    spInventory.InventoryItem.MP += 100;
                }
                if (slElement >= 20)
                {
                    spInventory.InventoryItem.MagicDefence += 5;
                }
                if (slElement >= 30)
                {
                    spInventory.InventoryItem.FireResistance += 2;
                    spInventory.InventoryItem.WaterResistance += 2;
                    spInventory.InventoryItem.LightResistance += 2;
                    spInventory.InventoryItem.DarkResistance += 2;
                    spInventory.InventoryItem.ElementRate += 2;
                }
                if (slElement >= 40)
                {
                    spInventory.InventoryItem.MP += 100;
                }
                if (slElement >= 50)
                {
                    spInventory.InventoryItem.MagicDefence += 5;
                }
                if (slElement >= 60)
                {
                    spInventory.InventoryItem.FireResistance += 3;
                    spInventory.InventoryItem.WaterResistance += 3;
                    spInventory.InventoryItem.LightResistance += 3;
                    spInventory.InventoryItem.DarkResistance += 3;
                    spInventory.InventoryItem.ElementRate += 2;
                }
                if (slElement >= 70)
                {
                    spInventory.InventoryItem.MP += 100;
                }
                if (slElement >= 80)
                {
                    spInventory.InventoryItem.MagicDefence += 5;
                }
                if (slElement >= 90)
                {
                    spInventory.InventoryItem.FireResistance += 4;
                    spInventory.InventoryItem.WaterResistance += 4;
                    spInventory.InventoryItem.LightResistance += 4;
                    spInventory.InventoryItem.DarkResistance += 4;
                }
                if (slElement == 100)
                {
                    spInventory.InventoryItem.FireResistance += 6;
                    spInventory.InventoryItem.WaterResistance += 6;
                    spInventory.InventoryItem.LightResistance += 6;
                    spInventory.InventoryItem.DarkResistance += 6;
                }
                //slhp
                if (slElement >= 5)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 10)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 15)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 20)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                    spInventory.InventoryItem.CloseDefence += 10;
                    spInventory.InventoryItem.DistanceDefence += 10;
                    spInventory.InventoryItem.MagicDefence += 10;
                }
                if (slElement >= 25)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 30)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 35)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                }
                if (slElement >= 40)
                {
                    spInventory.InventoryItem.DamageMinimum += 5;
                    spInventory.InventoryItem.DamageMaximum += 5;
                    spInventory.InventoryItem.CloseDefence += 15;
                    spInventory.InventoryItem.DistanceDefence += 15;
                    spInventory.InventoryItem.MagicDefence += 15;
                }
                if (slElement >= 45)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                }
                if (slElement >= 50)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                    spInventory.InventoryItem.FireResistance += 2;
                    spInventory.InventoryItem.WaterResistance += 2;
                    spInventory.InventoryItem.LightResistance += 2;
                    spInventory.InventoryItem.DarkResistance += 2;
                }
                if (slElement >= 60)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                }
                if (slElement >= 65)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                }
                if (slElement >= 70)
                {
                    spInventory.InventoryItem.DamageMinimum += 10;
                    spInventory.InventoryItem.DamageMaximum += 10;
                    spInventory.InventoryItem.CloseDefence += 45;
                    spInventory.InventoryItem.DistanceDefence += 45;
                    spInventory.InventoryItem.MagicDefence += 45;
                }
                if (slElement >= 75)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                }
                if (slElement >= 80)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                }
                if (slElement >= 85)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                    spInventory.InventoryItem.CriticalDodge += 1;
                }
                if (slElement >= 86)
                {
                    spInventory.InventoryItem.CriticalDodge += 1;
                }
                if (slElement >= 87)
                {
                    spInventory.InventoryItem.CriticalDodge += 1;
                }
                if (slElement >= 88)
                {
                    spInventory.InventoryItem.CriticalDodge += 1;
                }
                if (slElement >= 90)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                    spInventory.InventoryItem.DefenceDodge += (short)((slElement - 90) * 2);
                    spInventory.InventoryItem.DistanceDefenceDodge += (short)((slElement - 90) * 2);
                }
                if (slElement >= 95)
                {
                    spInventory.InventoryItem.DamageMinimum += 15;
                    spInventory.InventoryItem.DamageMaximum += 15;
                }
                if (slElement >= 100)
                {
                    spInventory.InventoryItem.DamageMinimum += 20;
                    spInventory.InventoryItem.DamageMaximum += 20;
                    spInventory.InventoryItem.FireResistance += 3;
                    spInventory.InventoryItem.WaterResistance += 3;
                    spInventory.InventoryItem.LightResistance += 3;
                    spInventory.InventoryItem.DarkResistance += 3;
                    spInventory.InventoryItem.CloseDefence += 30;
                    spInventory.InventoryItem.DistanceDefence += 30;
                    spInventory.InventoryItem.MagicDefence += 30;
                    spInventory.InventoryItem.CriticalDodge += 3;
                }
                Session.Client.SendPacket(Session.Character.GenerateStatChar());
                Session.Client.SendPacket(Session.Character.GenerateStat());
                Session.Client.SendPacket(Session.Character.GenerateSlInfo(new InventoryItem(spInventory.InventoryItem), 2));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_SET"), 0));
            }
            else if (!Session.Character.IsSitting)
            {
                if (spInventory == null)
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    return;
                }

                double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;

                if (Session.Character.UseSp)
                {
                    Session.Character.LastSp = currentRunningSeconds;
                    new Task(() => RemoveSP(spInventory.InventoryItem.ItemVNum)).Start();
                }
                else
                {
                    double timeSpanSinceLastSpUsage = currentRunningSeconds - Session.Character.LastSp;
                    if (timeSpanSinceLastSpUsage >= 30)
                    {
                        if (Session.Character.ThreadCharChange?.IsAlive == true)
                            Session.Character.ThreadCharChange.Abort();
                        Session.Character.ThreadCharChange = new Thread(ChangeSP);
                        Session.Character.ThreadCharChange.Start();
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), 30 - (int)Math.Round(timeSpanSinceLastSpUsage, 0)), 0));
                    }
                }
            }
        }

        [Packet("game_start")]
        public void StartGame(string packet)
        {
            if (Session.CurrentMap != null)
                return;
            Session.CurrentMap = ServerManager.GetMap(Session.Character.MapId);

            if (System.Configuration.ConfigurationManager.AppSettings["SceneOnCreate"].ToLower() == "true" & DAOFactory.GeneralLogDAO.LoadByLogType("Connection", Session.Character.CharacterId).Count() == 1) Session.Client.SendPacket("scene 40");
            if (System.Configuration.ConfigurationManager.AppSettings["WorldInformation"].ToLower() == "true")
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("---------- World Information ----------", 10));
                Assembly assembly = Assembly.GetEntryAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                Session.Client.SendPacket(Session.Character.GenerateSay($"OpenNos by OpenNos Team\nVersion: v{fileVersionInfo.ProductVersion}", 6));
                Session.Client.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 10));
            }

            Session.Client.SendPacket(Session.Character.GenerateTit());

            ClientLinkManager.Instance.ChangeMap(Session.Character.CharacterId);
            Session.Client.SendPacket("rank_cool 0 0 18000");

            Session.Client.SendPacket("scr 0 0 0 0 0 0");

            Session.Client.SendPacket($"bn 0 {Language.Instance.GetMessageFromKey("BN0")}");
            Session.Client.SendPacket($"bn 1 {Language.Instance.GetMessageFromKey("BN1")}");
            Session.Client.SendPacket($"bn 2 {Language.Instance.GetMessageFromKey("BN2")}");
            Session.Client.SendPacket($"bn 3 {Language.Instance.GetMessageFromKey("BN3")}");
            Session.Client.SendPacket($"bn 4 {Language.Instance.GetMessageFromKey("BN4")}");
            Session.Client.SendPacket($"bn 5 {Language.Instance.GetMessageFromKey("BN5")}");
            Session.Client.SendPacket($"bn 6 {Language.Instance.GetMessageFromKey("BN6")}");

            Session.Client.SendPacket(Session.Character.GenerateExts());
            Session.Client.SendPacket(Session.Character.GenerateGold());

            string[] quicklistpackets = Session.Character.GenerateQuicklist();
            Session.Client.SendPacket(quicklistpackets[0]);
            Session.Client.SendPacket(quicklistpackets[1]);
            Session.Client.SendPacket(quicklistpackets[2]);

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.AllOnMap);
            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSpPoint(), ReceiverType.AllOnMap);
            GetStartupInventory();
            // gidx
            Session.Client.SendPacket($"mlinfo 3800 2000 100 0 0 10 0 {Language.Instance.GetMessageFromKey("WELCOME_MUSIC_INFO")}");
            // cond
            Session.Client.SendPacket("p_clear");
            // sc_p pet
            Session.Client.SendPacket("pinit 0");
            Session.Client.SendPacket("zzim");
            Session.Client.SendPacket($"twk 1 {Session.Character.CharacterId} {Session.Account.Name} {Session.Character.Name} shtmxpdlfeoqkr");
            deleteTimeout();
        }

        [Packet("$Stat")]
        public void Stat(string packet)
        {
            Session.Client.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("TOTAL_SESSION")}: {ClientLinkManager.Instance.GetNumberOfAllASession()} ", 13));
        }

        public void SumItem(Inventory item, Inventory item2)
        {
            short[] upsuccess = { 100, 100, 85, 70, 50, 20 };
            int[] goldprice = { 1500, 3000, 6000, 12000, 24000, 48000 };
            short[] sand = { 5, 10, 15, 20, 25, 30 };
            int sandVnum = 1027;
            Item iteminfo = ServerManager.GetItem(item.InventoryItem.ItemVNum);
            Item iteminfo2 = ServerManager.GetItem(item2.InventoryItem.ItemVNum);
            if ((item.InventoryItem.Upgrade + item2.InventoryItem.Upgrade) < 6 && ((iteminfo2.EquipmentSlot == (byte)EquipmentType.Gloves) && (iteminfo2.EquipmentSlot == (byte)EquipmentType.Gloves)) || ((iteminfo.EquipmentSlot == (byte)EquipmentType.Boots) && (iteminfo2.EquipmentSlot == (byte)EquipmentType.Boots)))
            {
                if (Session.Character.Gold < goldprice[item.InventoryItem.Upgrade])
                    return;
                Session.Character.Gold = Session.Character.Gold - (goldprice[item.InventoryItem.Upgrade]);
                if (Session.Character.InventoryList.CountItem(sandVnum) < sand[item.InventoryItem.Upgrade])
                    return;
                Session.Character.InventoryList.RemoveItemAmount(sandVnum, (byte)(sand[item.InventoryItem.Upgrade]));

                Random r = new Random();
                int rnd = r.Next(100);
                if (rnd <= upsuccess[item.InventoryItem.Upgrade + item2.InventoryItem.Upgrade])
                {
                    item.InventoryItem.Upgrade += (byte)(item2.InventoryItem.Upgrade + 1);
                    item.InventoryItem.DarkResistance += (byte)(item2.InventoryItem.DarkResistance + iteminfo2.DarkResistance);
                    item.InventoryItem.LightResistance += (byte)(item2.InventoryItem.LightResistance + iteminfo2.LightResistance);
                    item.InventoryItem.WaterResistance += (byte)(item2.InventoryItem.WaterResistance + iteminfo2.WaterResistance);
                    item.InventoryItem.FireResistance += (byte)(item2.InventoryItem.FireResistance + iteminfo2.FireResistance);
                    DeleteItem(item2.Type, item2.Slot);
                    Session.Client.SendPacket($"pdti 10 {item.InventoryItem.ItemVNum} 1 27 {item.InventoryItem.Upgrade} 0");
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_SUCCESS"), 12));
                    Session.Client.SendPacket($"guri 19 1 {Session.Character.CharacterId} 1324");
                    Session.Client.SendPacket(Session.Character.GenerateGold());
                    GetStartupInventory();
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SUM_FAILED"), 0));
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SUM_FAILED"), 11));
                    Session.Client.SendPacket($"guri 19 1 {Session.Character.CharacterId} 1332");
                    DeleteItem(item2.Type, item2.Slot);
                    DeleteItem(item.Type, item.Slot);
                }
                Session.Client.SendPacket("shop_end 1");
            }
        }

        [Packet("$Summon")]
        public void Summon(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short vnum = 0;
            byte qty = 1, move = 0;
            Random rnd = new Random();
            if (packetsplit.Length == 5 && short.TryParse(packetsplit[2], out vnum) && byte.TryParse(packetsplit[3], out qty) && byte.TryParse(packetsplit[4], out move))
            {
                NpcMonster npcmonster = ServerManager.GetNpc(vnum);
                if (npcmonster == null)
                    return;
                for (int i = 0; i < qty; i++)
                {
                    short mapx = (short)rnd.Next((Session.Character.MapX - qty) % Session.CurrentMap.XLength, (Session.Character.MapX + qty / 3) % Session.CurrentMap.YLength);
                    short mapy = (short)rnd.Next((Session.Character.MapY - qty) % Session.CurrentMap.XLength, (Session.Character.MapY + qty / 3) % Session.CurrentMap.YLength);
                    for (int j = 100; j > 0 && Session.CurrentMap != null && Session.CurrentMap.IsBlockedZone(mapx, mapy); j--)
                    {
                        mapx = (short)rnd.Next((Session.Character.MapX - qty) % Session.CurrentMap.XLength, (Session.Character.MapX + qty / 3) % Session.CurrentMap.YLength);
                        mapy = (short)rnd.Next((Session.Character.MapY - qty) % Session.CurrentMap.XLength, (Session.Character.MapY + qty / 3) % Session.CurrentMap.YLength);
                    }
                    MapMonster monst = new MapMonster() { MonsterVNum = vnum, Alive = true, CurrentHp = npcmonster.MaxHP, CurrentMp = npcmonster.MaxMP, MapY = mapy, MapX = mapx, MapId = Session.Character.MapId, firstX = mapx, firstY = mapy, MapMonsterId = MapMonster.generateMapMonsterId(), Position = 1, Move = move != 0 ? true : false };
                    ServerManager.GetMap(Session.Character.MapId).Monsters.Add(monst);
                    ServerManager.Monsters.Add(monst);
                    ClientLinkManager.Instance.Broadcast(Session, monst.GenerateIn3(), ReceiverType.AllOnMap);
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 10));
        }

        [Packet("$Teleport")]
        public void Teleport(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            short[] arg = new short[3];
            bool verify = false;

            if (packetsplit.Length > 4)
            {
                verify = (short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && DAOFactory.MapDAO.LoadById(arg[0]) != null);
            }
            switch (packetsplit.Length)
            {
                case 3:
                    string name = packetsplit[2];
                    short? mapy = ClientLinkManager.Instance.GetProperty<short?>(name, "MapY");
                    short? mapx = ClientLinkManager.Instance.GetProperty<short?>(name, "MapX");
                    short? mapId = ClientLinkManager.Instance.GetProperty<short?>(name, "MapId");
                    if (mapy != null && mapx != null && mapId != null)
                    {
                        ClientLinkManager.Instance.MapOut(Session.Character.CharacterId);
                        Session.Character.MapId = (short)mapId;
                        Session.Character.MapX = (short)((short)(mapx) + 1);
                        Session.Character.MapY = (short)((short)(mapy) + 1);

                        ClientLinkManager.Instance.ChangeMap(Session.Character.CharacterId);
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                    break;

                case 5:
                    if (verify)
                    {
                        ClientLinkManager.Instance.MapOut(Session.Character.CharacterId);
                        Session.Character.MapId = arg[0];
                        Session.Character.MapX = arg[1];
                        Session.Character.MapY = arg[2];

                        ClientLinkManager.Instance.ChangeMap(Session.Character.CharacterId);
                    }
                    break;

                default:
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport MAP X Y", 10));
                    Session.Client.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 10));
                    break;
            }
        }

        [Packet("$TeleportToMe")]
        public void TeleportToMe(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if (packetsplit.Length == 3)
            {
                string name = packetsplit[2];

                long? id = ClientLinkManager.Instance.GetProperty<long?>(name, "CharacterId");

                if (id != null)
                {
                    ClientLinkManager.Instance.MapOut((long)id);
                    ClientLinkManager.Instance.SetProperty(name, "MapY", (short)((Session.Character.MapY) + (short)1));
                    ClientLinkManager.Instance.SetProperty(name, "MapX", (short)((Session.Character.MapX) + (short)1));
                    ClientLinkManager.Instance.SetProperty(name, "MapId", Session.Character.MapId);
                    ClientLinkManager.Instance.ChangeMap((long)id);
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                }
            }
            else
                Session.Client.SendPacket(Session.Character.GenerateSay("$TeleportToMe CHARACTERNAME", 10));
        }

        [Packet("up_gr")]
        public void upgr(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                return;
            if (packetsplit.Count() > 4)
            {
                byte uptype, type, slot, type2 = 0, slot2 = 0;
                byte.TryParse(packetsplit[2], out uptype);
                byte.TryParse(packetsplit[3], out type);
                byte.TryParse(packetsplit[4], out slot);

                if (packetsplit.Count() > 6)
                {
                    byte.TryParse(packetsplit[5], out type2);
                    byte.TryParse(packetsplit[6], out slot2);
                }
                Inventory inventory;
                switch (uptype)
                {
                    case 1:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        if (inventory != null)
                        {
                            Item iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                            if (iteminfo.EquipmentSlot == (byte)EquipmentType.Armor || iteminfo.EquipmentSlot == (byte)EquipmentType.MainWeapon || iteminfo.EquipmentSlot == (byte)EquipmentType.SecondaryWeapon)
                                UpgradeItem(inventory, InventoryItem.UpgradeMode.Normal, InventoryItem.UpgradeProtection.None);
                        }
                        break;

                    case 7:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        if (inventory != null)
                        {
                            Item iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                            if (iteminfo.EquipmentSlot == (byte)EquipmentType.Armor || iteminfo.EquipmentSlot == (byte)EquipmentType.MainWeapon || iteminfo.EquipmentSlot == (byte)EquipmentType.SecondaryWeapon)

                                RarifyItem(inventory, InventoryItem.RarifyMode.Normal, InventoryItem.RarifyProtection.None);
                        }
                        break;

                    case 8:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        Inventory inventory2 = Session.Character.InventoryList.LoadBySlotAndType(slot2, type2);

                        if (inventory != null && inventory2 != null && inventory != inventory2)
                        {
                            SumItem(inventory, inventory2);
                        }
                        break;

                    case 9:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        if (inventory != null)
                        {
                            Item iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                            if (iteminfo.EquipmentSlot == (byte)EquipmentType.Sp)

                                UpgradeSp(inventory, InventoryItem.UpgradeProtection.None);
                        }
                        break;

                    case 41:
                        inventory = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                        if (inventory != null)
                        {
                            Item iteminfo = ServerManager.GetItem(inventory.InventoryItem.ItemVNum);
                            if (iteminfo.EquipmentSlot == (byte)EquipmentType.Sp)

                                PerfectSP(inventory, InventoryItem.UpgradeProtection.None);
                        }
                        break;
                }
            }
        }

        [Packet("$Upgrade")]
        public void Upgrade(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.Client.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 10));
            }
            else
            {
                short itemslot = -1;
                short mode = -1;
                short protection = -1;
                short.TryParse(packetsplit[2], out itemslot);
                short.TryParse(packetsplit[3], out mode);
                short.TryParse(packetsplit[4], out protection);

                if (itemslot > -1 && mode > -1 && protection > -1)
                {
                    Inventory inventoryDTO = Session.Character.InventoryList.LoadBySlotAndType(itemslot, 0);
                    if (inventoryDTO != null)
                    {
                        UpgradeItem(inventoryDTO, (InventoryItem.UpgradeMode)mode, (InventoryItem.UpgradeProtection)protection);
                    }
                }
            }
        }

        public void UpgradeItem(Inventory item, InventoryItem.UpgradeMode mode, InventoryItem.UpgradeProtection protection)
        {
            if (item.InventoryItem.Upgrade < 10)
            {
                short[] upsuccess = { 100, 100, 100, 95, 80, 60, 40, 30, 20, 11 };
                short[] upfix = { 0, 0, 10, 15, 20, 20, 20, 20, 15, 10 };

                //short itempricevnum1 = 0;
                //short itempricevnum2 = 0;
                int[] goldprice = { 500, 1500, 3000, 10000, 30000, 80000, 150000, 400000, 700000, 1000000 };
                short[] cella = { 20, 50, 80, 120, 160, 220, 280, 380, 480, 600 };
                short[] gem = { 1, 1, 2, 2, 3, 3, 1, 2, 2, 3 };

                int cellaVnum = 1014;
                int gemVnum = 1015;
                int gemFullVnum = 1016;
                double reducedpricefactor = 0.5;

                if (item.InventoryItem.IsFixed)
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_IS_FIXED"), 10));

                    GetStartupInventory();
                    Session.Client.SendPacket("shop_end 1");
                    return;
                }
                switch (mode)
                {
                    case InventoryItem.UpgradeMode.Free:
                        break;

                    case InventoryItem.UpgradeMode.Reduced:
                        // TODO: Reduced Item Amount
                        if (Session.Character.Gold < goldprice[item.InventoryItem.Upgrade] * reducedpricefactor)
                            return;
                        Session.Character.Gold = Session.Character.Gold - (long)(goldprice[item.InventoryItem.Upgrade] * reducedpricefactor);
                        if (Session.Character.InventoryList.CountItem(cellaVnum) < cella[item.InventoryItem.Upgrade] * reducedpricefactor)
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(cellaVnum, (int)(cella[item.InventoryItem.Upgrade] * reducedpricefactor));
                        if (item.InventoryItem.Upgrade <= 5)
                        {
                            if (Session.Character.InventoryList.CountItem(gemVnum) < gem[item.InventoryItem.Upgrade] * reducedpricefactor)
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemVnum, (int)(gem[item.InventoryItem.Upgrade] * reducedpricefactor));
                        }
                        else
                        {
                            if (Session.Character.InventoryList.CountItem(gemFullVnum) < gem[item.InventoryItem.Upgrade] * reducedpricefactor)
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemFullVnum, (int)(gem[item.InventoryItem.Upgrade] * reducedpricefactor));
                        }
                        Session.Client.SendPacket(Session.Character.GenerateGold());
                        break;

                    case InventoryItem.UpgradeMode.Normal:
                        // TODO: Normal Item Amount
                        if (Session.Character.Gold < goldprice[item.InventoryItem.Upgrade])
                            return;
                        Session.Character.Gold = Session.Character.Gold - goldprice[item.InventoryItem.Upgrade];
                        if (Session.Character.InventoryList.CountItem(cellaVnum) < cella[item.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(cellaVnum, (cella[item.InventoryItem.Upgrade]));
                        if (item.InventoryItem.Upgrade < 5)
                        {
                            if (Session.Character.InventoryList.CountItem(gemVnum) < gem[item.InventoryItem.Upgrade])
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemVnum, (gem[item.InventoryItem.Upgrade]));
                        }
                        else
                        {
                            if (Session.Character.InventoryList.CountItem(gemFullVnum) < gem[item.InventoryItem.Upgrade])
                                return;
                            Session.Character.InventoryList.RemoveItemAmount(gemFullVnum, (gem[item.InventoryItem.Upgrade]));
                        }
                        Session.Client.SendPacket(Session.Character.GenerateGold());
                        break;
                }

                Random r = new Random();
                int rnd = r.Next(100);
                if (rnd <= upfix[item.InventoryItem.Upgrade])
                {
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.IsFixed = true;
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FIXED"), 0));
                }
                else if (rnd <= upsuccess[item.InventoryItem.Upgrade])
                {
                    Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 12));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_SUCCESS"), 0));
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.Upgrade++;
                }
                else
                {
                    if (protection == InventoryItem.UpgradeProtection.None)
                    {
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 11));
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED"), 0));
                        DeleteItem(item.Type, item.Slot);
                    }
                    else
                    {
                        Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                        Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 11));
                        Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
                    }
                }
            }
            else
            {
                Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 11));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADE_FAILED_ITEM_SAVED"), 0));
            }
            GetStartupInventory();
            Session.Client.SendPacket("shop_end 1");
        }

        public void UpgradeSp(Inventory item, InventoryItem.UpgradeProtection protect)
        {
            short[] upsuccess = { 100, 100, 95, 90, 85, 80, 75, 70, 65, 60, 55, 50, 45, 40, 30 };
            short[] upfail = { 20, 25, 25, 30, 35, 40, 40, 40, 40, 40, 45, 43, 40, 37, 29 };

            int[] goldprice = { 200000, 200000, 200000, 200000, 200000, 500000, 500000, 500000, 500000, 500000, 1000000, 1000000, 1000000, 1000000, 1000000 };
            short[] feather = { 3, 5, 8, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 70 };
            short[] fullmoon = { 1, 3, 5, 7, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30 };
            short[] soul = { 2, 4, 6, 8, 10, 1, 2, 3, 4, 5, 1, 2, 3, 4, 5 };
            short featherVnum = 2282;
            short fullmoonVnum = 1030;
            short greenSoulVnum = 2283;
            short redSoulVnum = 2284;
            short blueSoulVnum = 2285;
            short dragonSkinVnum = 2511;
            short dragonBloodVnum = 2512;
            short dragonHeartVnum = 2513;

            if (item.InventoryItem.IsFixed)
                return;
            if (Session.Character.Gold < goldprice[item.InventoryItem.Upgrade])
                return;
            if (Session.Character.InventoryList.CountItem(fullmoonVnum) < fullmoon[item.InventoryItem.Upgrade])
                return;
            if (Session.Character.InventoryList.CountItem(featherVnum) < feather[item.InventoryItem.Upgrade])
                return;

            if (item.InventoryItem.Upgrade < 5)
            {
                if (item.InventoryItem.SpLevel > 20)
                {
                    if (ServerManager.GetItem(item.InventoryItem.ItemVNum).Morph <= 15)
                    {
                        if (Session.Character.InventoryList.CountItem(greenSoulVnum) < soul[item.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(greenSoulVnum, (soul[item.InventoryItem.Upgrade]));
                    }
                    else
                    {
                        if (Session.Character.InventoryList.CountItem(dragonSkinVnum) < soul[item.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(dragonSkinVnum, (soul[item.InventoryItem.Upgrade]));
                    }
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIERED"), 21), 11));

                    return;
                }
            }
            else if (item.InventoryItem.Upgrade < 10)
            {
                if (item.InventoryItem.SpLevel > 40)
                {
                    if (ServerManager.GetItem(item.InventoryItem.ItemVNum).Morph <= 15)
                    {
                        if (Session.Character.InventoryList.CountItem(redSoulVnum) < soul[item.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(redSoulVnum, (soul[item.InventoryItem.Upgrade]));
                    }
                    else
                    {
                        if (Session.Character.InventoryList.CountItem(dragonBloodVnum) < soul[item.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(dragonBloodVnum, (soul[item.InventoryItem.Upgrade]));
                    }
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIERED"), 40), 11));

                    return;
                }
            }
            else
            {
                if (item.InventoryItem.SpLevel > 50)
                {
                    if (ServerManager.GetItem(item.InventoryItem.ItemVNum).Morph <= 15)
                    {
                        if (Session.Character.InventoryList.CountItem(blueSoulVnum) < soul[item.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(blueSoulVnum, (soul[item.InventoryItem.Upgrade]));
                    }
                    else
                    {
                        if (Session.Character.InventoryList.CountItem(dragonHeartVnum) < soul[item.InventoryItem.Upgrade])
                            return;
                        Session.Character.InventoryList.RemoveItemAmount(dragonHeartVnum, (soul[item.InventoryItem.Upgrade]));
                    }
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LVL_REQUIERED"), 51), 11));

                    return;
                }
            }
            Random r = new Random();
            int rnd = r.Next(100);
            if (rnd <= upfail[item.InventoryItem.Upgrade])
            {
                if (protect == InventoryItem.UpgradeProtection.Protected)
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));

                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 11));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED"), 0));
            }
            else if (rnd <= upsuccess[item.InventoryItem.Upgrade])
            {
                if (protect == InventoryItem.UpgradeProtection.Protected)
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                Session.Client.SendPacket(Session.Character.GenerateEff(3005));
                Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 12));
                Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_SUCCESS"), 0));
                Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.Upgrade++;
            }
            else
            {
                if (protect == InventoryItem.UpgradeProtection.Protected)
                {
                    Session.Client.SendPacket(Session.Character.GenerateEff(3004));
                    Session.Character.InventoryList.LoadByInventoryItem(item.InventoryItem.InventoryItemId).InventoryItem.IsFixed = true;
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_FAILED_SAVED"), 0));
                }
                else
                {
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROY"), 11));
                    Session.Client.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("UPGRADESP_DESTROY"), 0));
                }
            }
            Session.Character.Gold = Session.Character.Gold - goldprice[item.InventoryItem.Upgrade];
            Session.Client.SendPacket(Session.Character.GenerateGold());
            Session.Character.InventoryList.RemoveItemAmount(featherVnum, (feather[item.InventoryItem.Upgrade]));
            Session.Character.InventoryList.RemoveItemAmount(fullmoonVnum, (fullmoon[item.InventoryItem.Upgrade]));
            GetStartupInventory();
            Session.Client.SendPacket("shop_end 1");
        }

        [Packet("u_i")]
        public void UseItem(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            byte type; byte.TryParse(packetsplit[4], out type);
            short slot; short.TryParse(packetsplit[5], out slot);
            Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
            if (inv != null)
            {
                Item iteminfo = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                iteminfo.Use(Session, ref inv);
            }
        }

        [Packet("u_s")]
        public void UseSkill(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            TargetHit(Convert.ToInt32(packetsplit[2]), Convert.ToInt32(packetsplit[3]), Convert.ToInt32(packetsplit[4]));


        }

        public void TargetHit(int Castingid, int targetobj, int targetid)
        {
            int skillsize = Session.Character.Skills.Count();
            for (int i = 0; i < skillsize; i++)
            {
                CharacterSkill ski = Session.Character.Skills.ElementAt(Castingid);
                MapMonster mmon = Session.CurrentMap.Monsters.FirstOrDefault(s => s.MapMonsterId == targetid);
                if (mmon != null)
                {
                    NpcMonster monsterinfo = ServerManager.GetNpc(mmon.MonsterVNum);
                    if (ski != null && monsterinfo != null && !ski.Used)
                    {
                        Skill skill = ServerManager.GetSkill(ski.SkillVNum);
                        short dX = (short)(Session.Character.MapX - mmon.MapX);
                          short dY = (short)(Session.Character.MapY - mmon.MapY);
                        if (Math.Pow(dX,2) + Math.Pow(dY,2) <= Math.Pow(skill.Range+2,2))
                        {
                            Random random = new Random();
                            int hitmode = 0;
                            short damage = 5000;
                            int generated = random.Next(0, 100);
                            int critical_chance = 10;
                            int miss_chance = 20;
                            int criticalhit = 0;
                            int AtkType = 0;
                            switch (AtkType)
                            {
                                case 0:
                                    critical_chance *= Session.Character.HitCriticalRate / 100;
                                    criticalhit *= Session.Character.HitCritical / 100;
                                    miss_chance /= (int)(1 + Session.Character.HitRate / 100.0);
                                    break;
                                case 1:
                                    critical_chance *= Session.Character.DistanceCriticalRate / 100;
                                    criticalhit *= Session.Character.DistanceCritical / 100;
                                    miss_chance /= (int)(1 + Session.Character.DistanceRate / 100.0);
                                    break;
                                case 2:
                                    critical_chance = 0;
                                    miss_chance = 0;
                                    break;

                            }


                            if (generated < critical_chance)
                            {
                                hitmode = 3;
                                damage *= 2;
                            }
                            if (generated > 100 - miss_chance) { hitmode = 1; damage = 0; }

                            if (mmon.CurrentHp <= damage)
                            {
                                mmon.Alive = false;
                                mmon.CurrentHp = 0;
                                mmon.CurrentMp = 0;
                                mmon.Death = DateTime.Now;
                                Random rnd;
                                foreach (DropDTO drop in monsterinfo.Drops)
                                {
                                    rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                                    double rndamount = rnd.Next(0, 100) * rnd.NextDouble();
                                    if (rndamount <= (double)drop.DropChance / 1000.000)
                                    {
                                        Session.CurrentMap.ItemSpawn(drop, mmon.MapX, mmon.MapY);
                                    }

                                }
                                rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                                short gold = Convert.ToInt16((rnd.Next(1, 5) >= 4 ? 1 : 0) * rnd.Next(6 * monsterinfo.Level, 12 * monsterinfo.Level));
                                if (gold != 0)
                                {
                                    DropDTO drop2 = new DropDTO()
                                    {
                                        Amount = gold,
                                        ItemVNum = 1046
                                    };
                                    Session.CurrentMap.ItemSpawn(drop2, mmon.MapX, mmon.MapY);
                                }
                                if ((int)(Session.Character.LevelXp / (Session.Character.XPLoad() / 10)) < (int)((Session.Character.LevelXp + monsterinfo.XP) / (Session.Character.XPLoad() / 10)))
                                {
                                    Session.Character.Hp = (int)Session.Character.HPLoad();
                                    Session.Character.Mp = (int)Session.Character.MPLoad();
                                    Session.Client.SendPacket(Session.Character.GenerateStatInfo());
                                    Session.Client.SendPacket(Session.Character.GenerateEff(5));
                                }
                                Session.Character.LevelXp += monsterinfo.XP;
                                Session.Character.JobLevelXp += monsterinfo.JobXP;
                                if (Session.Character.LevelXp >= Session.Character.XPLoad())
                                {
                                    Session.Character.LevelXp -= (int)Session.Character.XPLoad();
                                    Session.Character.Level++;
                                    Session.Character.Hp = (int)Session.Character.HPLoad();
                                    Session.Character.Mp = (int)Session.Character.MPLoad();
                                    Session.Client.SendPacket(Session.Character.GenerateStatInfo());
                                    Session.Client.SendPacket($"levelup {Session.Character.CharacterId}");
                                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
                                }
                                if (Session.Character.JobLevelXp >= Session.Character.JobXPLoad())
                                {
                                    Session.Character.JobLevelXp -= (int)Session.Character.JobXPLoad();
                                    Session.Character.JobLevel++;
                                    Session.Character.Hp = (int)Session.Character.HPLoad();
                                    Session.Character.Mp = (int)Session.Character.MPLoad();
                                    Session.Client.SendPacket(Session.Character.GenerateStatInfo());
                                    Session.Client.SendPacket($"levelup {Session.Character.CharacterId}");
                                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(6), ReceiverType.AllOnMap);
                                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEff(198), ReceiverType.AllOnMap);
                                }
                                Session.Client.SendPacket(Session.Character.GenerateLev());

                            }
                            else
                            {
                                mmon.CurrentHp -= damage;
                            }
                            mmon.Target = Session.Character.CharacterId;
                            string packet = $"su {1} {Session.Character.CharacterId} {3} {mmon.MapMonsterId} {skill.Effect} 6 {skill.AttackAnimation} {skill.Effect} 0 0 {(mmon.Alive ? 1 : 0)} {mmon.CurrentHp / monsterinfo.MaxHP * 100} {damage} {hitmode} {skill.Type}";
                            Session.Client.SendPacket(packet);
                            Task t = Task.Factory.StartNew(async () =>
                            {
                                ski.Used = true;
                                await Task.Delay(skill.Cooldown * 100);
                                Session.Client.SendPacket("sr 0");
                                ski.Used = false;
                            });
                        }

                    }
                }
            }
            Session.Client.SendPacket("cancel 0 0");
        }



        [Packet("#pjoin")]
        public void validpjoin(string packet)
        {
            string[] packetsplit = packet.Split(' ', '^');
            int type = -1;
            long CharId = -1;
            int newgroup = 1;
            Boolean blocked1 = false;
            Boolean blocked2 = false;
            if (packetsplit.Length > 3)
            {
                if (!int.TryParse(packetsplit[2], out type))
                    return;
                long.TryParse(packetsplit[3], out CharId);

                if (type == 3 && ClientLinkManager.Instance.GetProperty<string>(CharId, "Name") != null)
                {
                    foreach (Group group in ClientLinkManager.Instance.Groups)
                    {
                        if (group.Characters.Contains(Session.Character.CharacterId))
                        {
                            blocked1 = true;
                        }
                        if (group.Characters.Contains(CharId))
                        {
                            blocked2 = true;
                        }
                    }
                    foreach (Group group in ClientLinkManager.Instance.Groups)
                    {
                        if (group.Characters.Count == 3)
                        {
                            Session.Client.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));

                            return;
                        }
                        else if (blocked2 == true && blocked1 == true)
                            return;
                        else if (group.Characters.Contains(CharId))
                        {
                            group.Characters.Add(Session.Character.CharacterId);
                            newgroup = 0;
                        }
                        else if (group.Characters.Contains(Session.Character.CharacterId))
                        {
                            group.Characters.Add(CharId);
                            newgroup = 0;
                        }

                    }
                    if (newgroup == 1)
                    {
                        Group group = new Group()
                        {
                            Characters = new List<long>()
                        };
                        group.Characters.Add(CharId);
                        group.Characters.Add(Session.Character.CharacterId);
                        ClientLinkManager.Instance.Groups.Add(group);
                    }

                    //player join group
                    string str = $"pinit { ClientLinkManager.Instance.Groups.FirstOrDefault(s => s.Characters.Contains(CharId)).Characters.Count()}";

                    int i = 0;
                    foreach (long Id in ClientLinkManager.Instance.Groups.FirstOrDefault(s => s.Characters.Contains(CharId)).Characters)
                    {
                        i++;
                        str += $" 1|{ClientLinkManager.Instance.GetProperty<long>(Id, "CharacterId")}|{i}|{ClientLinkManager.Instance.GetProperty<byte>(Id, "Level")}|{ClientLinkManager.Instance.GetProperty<string>(Id, "Name")}|11|{ClientLinkManager.Instance.GetProperty<byte>(Id, "Gender")}|{ClientLinkManager.Instance.GetProperty<byte>(Id, "Class")}|{(ClientLinkManager.Instance.GetProperty<bool>(Id, "UseSp") ? ClientLinkManager.Instance.GetProperty<int>(Id, "Morph") : 0)}";
                    }

                    foreach (long Id in ClientLinkManager.Instance.Groups.FirstOrDefault(s => s.Characters.Contains(CharId)).Characters)
                    {
                        ClientLinkManager.Instance.Broadcast(Session, str, ReceiverType.OnlySomeone, "", Id);
                    }

                    string p = generatePidx(Session.Character.CharacterId);
                    if (p != "")
                        ClientLinkManager.Instance.Broadcast(Session, p, ReceiverType.AllOnMap);
                }
                else if (type == 4)
                {
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("REFUSED_REQUEST"), Session.Character.Name)), ReceiverType.OnlySomeone, "", CharId);
                }
            }
        }

        [Packet("walk")]
        public void Walk(string packet)
        {
            if (Session.Character.ThreadCharChange != null && Session.Character.ThreadCharChange.IsAlive)
                Session.Character.ThreadCharChange.Abort();
            string[] packetsplit = packet.Split(' ');
            Session.Character.MapX = Convert.ToInt16(packetsplit[2]);
            Session.Character.MapY = Convert.ToInt16(packetsplit[3]);

            if (Session.Character.Speed.Equals(Convert.ToByte(packetsplit[5])) || Convert.ToByte(packetsplit[5]) == 10)
            {
                if (Session.Character.MapX > Convert.ToInt16(packetsplit[2]) + 10 || Session.Character.MapX < Convert.ToInt16(packetsplit[2]) - 10
                   || Session.Character.MapY > Convert.ToInt16(packetsplit[3]) + 10 || Session.Character.MapY < Convert.ToInt16(packetsplit[3]) - 10)
                    Session.Client.Disconnect();
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateMv(), ReceiverType.AllOnMapExceptMe);
                Session.Client.SendPacket(Session.Character.GenerateCond());
            }
            else
            {
                Session.Client.Disconnect();
                // TODO : need to see why sometime Session.Character.Speed == 1
                //Session.Client.SendPacket(Session.Character.GenerateMsg(("!Warning! Speed manipulation detected"), 0));
                //Logger.Log.Warn("Speed manipulation detected, Speed : " + Session.Character.Speed);
            }
        }

        [Packet("wear")]
        public void Wear(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if ((Session.Character.ExchangeInfo != null && Session.Character.ExchangeInfo?.ExchangeList.Count() != 0) || Session.Character.Speed == 0)
                return;
            if (packetsplit.Length > 3 && Session.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null)
            {
                byte type;
                short slot;

                if (!byte.TryParse(packetsplit[3], out type) || !short.TryParse(packetsplit[2], out slot)) return;
                Inventory inv = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
                if (inv != null)
                {
                    Item iteminfo = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                    iteminfo.Use(Session, ref inv);
                }
            }
        }

        [Packet("/")]
        public void Whisper(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            for (int i = 2; i < packetsplit.Length; i++)
                message += packetsplit[i] + " ";
            message.Trim();

            ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, 5), ReceiverType.OnlyMe);

            bool? Blocked = ClientLinkManager.Instance.GetProperty<bool?>(packetsplit[1].Substring(1), "WhisperBlocked");
            if (!Blocked.Equals(null))
            {
                if (!Convert.ToBoolean(Blocked))
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(message, 5), ReceiverType.OnlySomeone, packetsplit[1].Substring(1));
                else
                    Session.Client.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("WHISPERED_BLOCKED"), 11));
            }
            else ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED")), ReceiverType.OnlyMe);
        }

        [Packet("qset")]
        public void QuicklistSet(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 4)
            {
                short type, q1, q2, data1 = 0, data2 = 0;
                if (!short.TryParse(packetsplit[2], out type) ||
                    !short.TryParse(packetsplit[3], out q1) || !short.TryParse(packetsplit[4], out q2))
                    return;
                if (packetsplit.Length > 6)
                {
                    short.TryParse(packetsplit[5], out data1);
                    short.TryParse(packetsplit[6], out data2);
                }
                // qset type q1 q2 data1 data2

                switch (type)
                {
                    case 0:
                    case 1:
                        // client says  qset 0 1 3 2 6
                        // answer    -> qset 1 3 0.2.6.0

                        Session.Character.QuicklistEntries.Add(new QuicklistEntry
                        {
                            CharacterId = Session.Character.CharacterId,
                            Type = type,
                            Q1 = q1,
                            Q2 = q2,
                            Slot = data1,
                            Pos = data2
                        });

                        Session.Client.SendPacket(string.Format("qset {0} {1} {2}.{3}.{4}.0", q1, q2, type, data1, data2));

                        break;
                    case 2:
                        // DragDrop / Reorder

                        // qset type to1 to2 from1 from2
                        // vars ->   q1  q2  data1 data2

                        QuicklistEntry qlFrom = Session.Character.QuicklistEntries.Single(n => n.Q1 == data1 && n.Q2 == data2);
                        QuicklistEntry qlTo = Session.Character.QuicklistEntries.SingleOrDefault(n => n.Q1 == q1 && n.Q2 == q2);

                        qlFrom.Q1 = q1;
                        qlFrom.Q2 = q2;

                        if (qlTo == null)
                        {
                            // Put 'from' to new position (datax)
                            Session.Client.SendPacket(string.Format("qset {0} {1} {2}.{3}.{4}.0", qlFrom.Q1, qlFrom.Q2, qlFrom.Type, qlFrom.Slot, qlFrom.Pos));
                            // old 'from' is now empty.
                            Session.Client.SendPacket(string.Format("qset {0} {1} 7.7.-1.0", data1, data2));
                        }
                        else
                        {
                            // Put 'from' to new position (datax)
                            Session.Client.SendPacket(string.Format("qset {0} {1} {2}.{3}.{4}.0", qlFrom.Q1, qlFrom.Q2, qlFrom.Type, qlFrom.Slot, qlFrom.Pos));
                            // 'from' is now 'to' because they exchanged
                            qlTo.Q1 = data1;
                            qlTo.Q2 = data2;
                            Session.Client.SendPacket(string.Format("qset {0} {1} {2}.{3}.{4}.0", qlTo.Q1, qlTo.Q2, qlTo.Type, qlTo.Slot, qlTo.Pos));
                        }

                        break;
                    case 3:
                        // Remove from Quicklist

                        Session.Character.QuicklistEntries.RemoveAll(n => n.Q1 == q1 && n.Q2 == q2);

                        Session.Client.SendPacket(string.Format("qset {0} {1} 7.7.-1.0", q1, q2));

                        break;
                    default:
                        return;
                }
            }
        }


        #endregion
    }
}