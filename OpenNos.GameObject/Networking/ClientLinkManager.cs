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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ClientLinkManager : BroadcastableBase
    {
        #region Members

        public Boolean ShutdownStop = false;
        private static ClientLinkManager _instance;

        #endregion

        #region Instantiation

        private ClientLinkManager()
        {
            Groups = new List<Group>();

            Task autosave = new Task(SaveAllProcess);
            autosave.Start();

            Task GroupTask = new Task(() => GroupProcess());
            GroupTask.Start();

            Task TaskController = new Task(() => TaskLauncherProcess());
            TaskController.Start();
        }

        #endregion

        #region Properties

        public List<Group> Groups { get; set; }

        public static ClientLinkManager Instance => _instance ?? (_instance = new ClientLinkManager());

        public Task TaskShutdown { get; set; }

        #endregion

        #region Methods

        //PacketHandler -> with Callback?
        public void AskRevive(long Target)
        {
            ClientSession Session = Instance.Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == Target);
            if (Session != null && Session.Character != null)
            {
                Session.Client.SendPacket(Session.Character.GenerateDialog($"#revival^0 #revival^1 {(Session.Character.Level > 20 ? Language.Instance.GetMessageFromKey("ASK_REVIVE") : Language.Instance.GetMessageFromKey("ASK_REVIVE_FREE"))}"));
                Session.Character.Dignite -= (short)(Session.Character.Level < 50 ? Session.Character.Level : 50);
                if (Session.Character.Dignite < -1000)
                    Session.Character.Dignite = -1000;

                Session.Client.SendPacket(Session.Character.GenerateFd());
                Session.Client.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LOSE_DIGNITY"), (short)(Session.Character.Level < 50 ? Session.Character.Level : 50)), 11));

                Task.Factory.StartNew(async () =>
                {
                    for (int i = 1; i <= 30; i++)
                    {
                        await Task.Delay(1000);
                        if (Session.Character.Hp > 0)
                            return;
                    }
                    Instance.ReviveFirstPosition(Session.Character.CharacterId);
                });
            }
        }

        //PacketHandler
        public void BuyValidate(ClientSession clientSession, KeyValuePair<long, MapShop> shop, short slot, byte amount)
        {
            PersonalShopItem itemshop = clientSession.CurrentMap.UserShops[shop.Key].Items.FirstOrDefault(i => i.Slot.Equals(slot));
            if (itemshop == null)
                return;
            long id = itemshop.InventoryId;
            itemshop.Amount -= amount;
            if (itemshop.Amount <= 0)
                clientSession.CurrentMap.UserShops[shop.Key].Items.Remove(itemshop);

            ClientSession shopOwnerSession = Sessions.FirstOrDefault(s => s.Character.CharacterId.Equals(shop.Value.OwnerId));
            if (shopOwnerSession == null) return;

            shopOwnerSession.Character.Gold += itemshop.Price * amount;
            shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateGold());
            shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateShopMemo(1,
                string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM"), shopOwnerSession.Character.Name, (itemshop.ItemInstance as ItemInstance).Item.Name, amount)));
            clientSession.CurrentMap.UserShops[shop.Key].Sell += itemshop.Price * amount;
            shopOwnerSession.Client.SendPacket($"sell_list {shop.Value.Sell} {slot}.{amount}.{itemshop.Amount}");

            Inventory inv = shopOwnerSession.Character.InventoryList.RemoveItemAmountFromInventory(amount, id);

            if (inv != null)
            {
                // Send reduced-amount to owners inventory
                shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade));
            }
            else
            {
                // Send empty slot to owners inventory
                shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateInventoryAdd(-1, 0, itemshop.Type, itemshop.Slot, 0, 0, 0));
                if (clientSession.CurrentMap.UserShops[shop.Key].Items.Count == 0)
                {
                    clientSession.Client.SendPacket("shop_end 0");

                    ClientLinkManager.Instance.Broadcast(shopOwnerSession, shopOwnerSession.Character.GenerateShopEnd(), ReceiverType.All);
                    ClientLinkManager.Instance.Broadcast(shopOwnerSession, shopOwnerSession.Character.GeneratePlayerFlag(0), ReceiverType.AllExceptMe);
                    shopOwnerSession.Character.Speed = shopOwnerSession.Character.LastSpeed != 0 ? shopOwnerSession.Character.LastSpeed : shopOwnerSession.Character.Speed;
                    shopOwnerSession.Character.IsSitting = false;
                    shopOwnerSession.Client.SendPacket(shopOwnerSession.Character.GenerateCond());
                    ClientLinkManager.Instance.Broadcast(shopOwnerSession, shopOwnerSession.Character.GenerateRest(), ReceiverType.All);
                }
            }
        }

        //Both partly
        public void ChangeMap(long id)
        {
            ClientSession Session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == id);
            if (Session != null)
            {
                Session.CurrentMap.UnregisterSession(Session);
                Session.CurrentMap = ServerManager.GetMap(Session.Character.MapId);
                Session.CurrentMap.RegisterSession(Session);
                Session.Client.SendPacket(Session.Character.GenerateCInfo());
                Session.Client.SendPacket(Session.Character.GenerateCMode());
                Session.Client.SendPacket(Session.Character.GenerateFaction());
                Session.Client.SendPacket(Session.Character.GenerateFd());
                Session.Client.SendPacket(Session.Character.GenerateLev());
                Session.Client.SendPacket(Session.Character.GenerateStat());
                Session.Client.SendPacket(Session.Character.GenerateAt());
                Session.Client.SendPacket(Session.Character.GenerateCMap());
                if (Session.Character.Size != 10)
                    Session.Client.SendPacket(Session.Character.GenerateScal());
                foreach (String portalPacket in Session.Character.GenerateGp())
                    Session.Client.SendPacket(portalPacket);
                foreach (String npcPacket in Session.Character.GenerateIn2())
                    Session.Client.SendPacket(npcPacket);
                foreach (String monsterPacket in Session.Character.GenerateIn3())
                    Session.Client.SendPacket(monsterPacket);
                foreach (String ShopPacket in Session.Character.GenerateNPCShopOnMap())
                    Session.Client.SendPacket(ShopPacket);
                foreach (String droppedPacket in Session.Character.GenerateDroppedItem())
                    Session.Client.SendPacket(droppedPacket);

                Session.Client.SendPacket(Session.Character.GenerateStatChar());
                Session.Client.SendPacket(Session.Character.GenerateCond());
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GeneratePairy(), ReceiverType.All);
                Session.Client.SendPacket($"rsfi 1 1 0 9 0 9"); // Act completion
                ClientLinkManager.Instance.Sessions.Where(s => s.Character != null && s.Character.MapId.Equals(Session.Character.MapId) && s.Character.Name != Session.Character.Name && !Session.Character.InvisibleGm).ToList().ForEach(s => RequireBroadcastFromUser(Session, s.Character.CharacterId, "GenerateIn"));
                if (Session.Character.InvisibleGm == false)
                    ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                if (Session.CurrentMap.IsDancing == 2 && Session.Character.IsDancing == 0)
                    Session.CurrentMap.Broadcast("dance 2");
                else if (Session.CurrentMap.IsDancing == 0 && Session.Character.IsDancing == 1)
                {
                    Session.Character.IsDancing = 0;
                    Session.CurrentMap.Broadcast("dance");
                }
                foreach (String ShopPacket in Session.Character.GenerateShopOnMap())
                    Session.Client.SendPacket(ShopPacket);

                foreach (String ShopPacketChar in Session.Character.GeneratePlayerShopOnMap())
                    Session.Client.SendPacket(ShopPacketChar);

                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateEq(), ReceiverType.All);
                Session.Client.SendPacket(Session.Character.GenerateEquipment());
                string clinit = "clinit";
                string flinit = "flinit";
                string kdlinit = "kdlinit";

                foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopComplimented())
                {
                    clinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Compliment}|{character.Name}";
                }
                foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopReputation())
                {
                    flinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Reput}|{character.Name}";
                }
                foreach (CharacterDTO character in DAOFactory.CharacterDAO.GetTopPoints())
                {
                    kdlinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Act4Points}|{character.Name}";
                }

                Session.Client.SendPacket(clinit);
                Session.Client.SendPacket(flinit);
                Session.Client.SendPacket(kdlinit);

                foreach (Group g in Groups)
                {
                    foreach (ClientSession session in g.Characters)
                    {
                        ClientSession chara = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == session.Character.CharacterId && s.CurrentMap.MapId == Session.CurrentMap.MapId);
                        if (chara != null)
                        {
                            Session.Client.SendPacket($"pidx 1 1.{chara.Character.CharacterId}");
                        }
                        if (session.Character.CharacterId == Session.Character.CharacterId)
                        {
                            Broadcast(Session, $"pidx 1 1.{Session.Character.CharacterId}", ReceiverType.All);
                        }
                    }
                }
            }
        }

        //PacketHandler
        public void ClassChange(long id, byte Class)
        {
            foreach (ClientSession session in Sessions.Where(s => s.Character != null && s.Character.CharacterId == id))
            {
                session.Character.JobLevel = 1;
                session.Client.SendPacket("npinfo 0");
                session.Client.SendPacket("p_clear");

                session.Character.Class = Class;
                session.Character.Speed = ServersData.SpeedData[session.Character.Class];
                session.Client.SendPacket(session.Character.GenerateCond());
                session.Character.Hp = (int)session.Character.HPLoad();
                session.Character.Mp = (int)session.Character.MPLoad();
                session.Client.SendPacket(session.Character.GenerateTit());

                //eq 37 0 1 0 9 3 -1.120.46.86.-1.-1.-1.-1 0 0
                ClientLinkManager.Instance.Broadcast(session, session.Character.GenerateEq(), ReceiverType.All);

                //equip 0 0 0.46.0.0.0 1.120.0.0.0 5.86.0.0.0

                session.Client.SendPacket(session.Character.GenerateLev());
                session.Client.SendPacket(session.Character.GenerateStat());
                ClientLinkManager.Instance.Broadcast(session, session.Character.GenerateEff(8), ReceiverType.All);
                session.Client.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOB_CHANGED"), 0));
                ClientLinkManager.Instance.Broadcast(session, session.Character.GenerateEff(196), ReceiverType.All);
                Random rand = new Random();
                int faction = 1 + (int)rand.Next(0, 2);
                session.Character.Faction = faction;
                session.Client.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{faction}"), 0));
                session.Client.SendPacket("scr 0 0 0 0 0 0");

                session.Client.SendPacket(session.Character.GenerateFaction());
                session.Client.SendPacket(session.Character.GenerateStatChar());

                session.Client.SendPacket(session.Character.GenerateEff(4799 + faction));
                session.Client.SendPacket(session.Character.GenerateLev());
                ClientLinkManager.Instance.Broadcast(session, session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                ClientLinkManager.Instance.Broadcast(session, session.Character.GenerateEff(6), ReceiverType.All);
                ClientLinkManager.Instance.Broadcast(session, session.Character.GenerateEff(198), ReceiverType.All);

                session.Character.Skills = new List<CharacterSkill>();
                session.Character.Skills.Add(new CharacterSkill { SkillVNum = (short)(200 + 20 * session.Character.Class), CharacterId = session.Character.CharacterId });
                session.Character.Skills.Add(new CharacterSkill { SkillVNum = (short)(201 + 20 * session.Character.Class), CharacterId = session.Character.CharacterId });

                session.Client.SendPacket(session.Character.GenerateSki());

                // TODO Reset Quicklist (just add Rest-on-T Item)
                foreach (QuicklistEntryDTO quicklists in DAOFactory.QuicklistEntryDAO.Load(session.Character.CharacterId).Where(quicklists => session.Character.QuicklistEntries.Any(qle => qle.EntryId == quicklists.EntryId)))
                    DAOFactory.QuicklistEntryDAO.Delete(session.Character.CharacterId, quicklists.EntryId);
                session.Character.QuicklistEntries = new List<QuicklistEntry>
                {
                    new QuicklistEntry
                    {
                        CharacterId = session.Character.CharacterId,
                        Q1 = 0,
                        Q2 = 9,
                        Type = 1,
                        Slot = 3,
                        Pos = 1
                    }
                };

                if (Groups.FirstOrDefault(s => s.IsMemberOfGroup(session)) != null)
                    Instance.Broadcast(session, $"pidx 1 1.{session.Character.CharacterId}", ReceiverType.AllExceptMe);
            }
        }

        //PacketHandler
        public void ExchangeValidate(ClientSession c1Session, long charId)
        {
            ClientSession c2Session = Sessions.FirstOrDefault(s => s.Character.CharacterId.Equals(charId));
            {
                if (c2Session == null) return;

                foreach (ItemInstance item in c2Session.Character.ExchangeInfo.ExchangeList)
                {
                    Inventory invtemp = c2Session.Character.InventoryList.Inventory.FirstOrDefault(s => s.ItemInstance.ItemInstanceId == item.ItemInstanceId);
                    short slot = invtemp.Slot;
                    byte type = invtemp.Type;

                    Inventory inv = c2Session.Character.InventoryList.RemoveItemAmountFromInventory((byte)item.Amount, invtemp.InventoryId);
                    if (inv != null)
                    {
                        // Send reduced-amount to owners inventory
                        c2Session.Client.SendPacket(c2Session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade));
                    }
                    else
                    {
                        // Send empty slot to owners inventory
                        c2Session.Client.SendPacket(c2Session.Character.GenerateInventoryAdd(-1, 0, type, slot, 0, 0, 0));
                    }
                }

                foreach (ItemInstance item in c1Session.Character.ExchangeInfo.ExchangeList)
                {
                    ItemInstance item2 = item.DeepCopy();
                    item2.ItemInstanceId = c2Session.Character.InventoryList.GenerateItemInstanceId();
                    Inventory inv = c2Session.Character.InventoryList.AddToInventory(item2);
                    if (inv == null) continue;
                    if (inv.Slot == -1) continue;
                    c2Session.Client.SendPacket(c2Session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade));
                }

                c2Session.Character.Gold = c2Session.Character.Gold - c2Session.Character.ExchangeInfo.Gold + c1Session.Character.ExchangeInfo.Gold;
                c2Session.Client.SendPacket(c2Session.Character.GenerateGold());
                c1Session.Character.ExchangeInfo = null;
                c2Session.Character.ExchangeInfo = null;
            }
        }

        public T GetProperty<T>(string charName, string property)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(charName));
            if (session == null)
                return default(T);
            return (T)session?.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public T GetProperty<T>(long charId, string property)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(charId));
            if (session == null)
                return default(T);
            return (T)session?.Character.GetType().GetProperties().Single(pi => pi.Name == property).GetValue(session.Character, null);
        }

        public T GetUserMethod<T>(long characterId, string methodName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(characterId));
            if (session == null) return default(T);
            MethodInfo method = session.Character.GetType().GetMethod(methodName);

            return (T)method.Invoke(session.Character, null);
        }

        public void GroupLeave(ClientSession session)
        {
            Group grp = ClientLinkManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(session.Character.CharacterId));
            if (grp != null)
            {
                if (grp.Characters.Count() == 3)
                {
                    if (grp.Characters.ElementAt(0) == session)
                    {
                        Broadcast(session, session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("NEW_LEADER")), ReceiverType.OnlySomeone, "", grp.Characters.ElementAt(1).Character.CharacterId);
                    }
                    grp.LeaveGroup(session);
                    foreach (ClientSession groupSession in grp.Characters)
                    {
                        string str = $"pinit {grp.Characters.Count()}";
                        int i = 0;
                        foreach (ClientSession groupSessionForId in grp.Characters)
                        {
                            i++;
                            str += $" 1|{groupSessionForId.Character.CharacterId}|{i}|{groupSessionForId.Character.Level}|{groupSessionForId.Character.Name}|11|{groupSessionForId.Character.Gender}|{groupSessionForId.Character.Class}|{(groupSessionForId.Character.UseSp ? groupSessionForId.Character.Morph : 0)}";
                        }
                        foreach (ClientSession sess in Sessions.Where(s => s != null && s.Character != null && s.Character.CharacterId == groupSession.Character.CharacterId))
                        {
                            sess.Client.SendPacket(str);
                            sess.Client.SendPacket(sess.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("LEAVE_GROUP"), session.Character.Name), 0));
                        }
                    }
                    session.Client.SendPacket("pinit 0");
                    Broadcast(session, $"pidx -1 1.{session.Character.CharacterId}", ReceiverType.All);
                    session.Client.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_LEFT"), 0));
                }
                else
                {
                    foreach (ClientSession targetSession in grp.Characters)
                    {
                        foreach (ClientSession sess in Sessions.Where(s => s != null && s.Character != null && s.Character.CharacterId == targetSession.Character.CharacterId))
                        {
                            sess.Client.SendPacket("pinit 0");
                            sess.Client.SendPacket(sess.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_CLOSED"), 0));
                            Broadcast(sess, $"pidx -1 1.{targetSession.Character.CharacterId}", ReceiverType.All);
                        }
                    }
                    ClientLinkManager.Instance.Groups.Remove(grp);
                }

                session.Character.Group = null;
            }
        }

        //Server
        public bool Kick(string characterName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.Name.Equals(characterName));
            if (session == null) return false;

            session.Client.Disconnect();
            return true;
        }

        //Map
        public void MapOut(long id)
        {
            foreach (ClientSession Session in Sessions.Where(s => s.Character != null && s.Character.CharacterId == id))
            {
                Session.Client.SendPacket(Session.Character.GenerateMapOut());
                ClientLinkManager.Instance.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllExceptMe);
            }
        }

        public void RequireBroadcastFromUser(ClientSession client, long characterId, string methodName)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(characterId));
            if (session == null) return;

            MethodInfo method = session.Character.GetType().GetMethod(methodName);
            string result = (string)method.Invoke(session.Character, null);
            client.Client.SendPacket(result);
        }

        //Map
        public void ReviveFirstPosition(long characterId)
        {
            ClientSession Session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId == characterId && s.Character.Hp <= 0);
            if (Session != null)
            {
                MapOut(Session.Character.CharacterId);
                Session.Character.MapId = 1;
                Session.Character.MapX = 80;
                Session.Character.MapY = 116;
                Session.Character.Hp = 1;
                Session.Character.Mp = 1;
                ChangeMap(Session.Character.CharacterId);
                Broadcast(Session, Session.Character.GenerateTp(), ReceiverType.All);
                Broadcast(Session, Session.Character.GenerateRevive(), ReceiverType.All);
                Session.Client.SendPacket(Session.Character.GenerateStat());
            }
        }

        public void SetProperty(long charId, string property, object value)
        {
            ClientSession session = Sessions.FirstOrDefault(s => s.Character != null && s.Character.CharacterId.Equals(charId));
            if (session == null) return;

            PropertyInfo propertyinfo = session.Character.GetType().GetProperties().Single(pi => pi.Name == property);
            propertyinfo.SetValue(session.Character, value, null);
        }

        //Server
        public void UpdateGroup(long charId)
        {
            Group myGroup = Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId));
            if (myGroup == null)
                return;
            string str = $"pinit { myGroup.Characters.Count()}";

            int i = 0;
            foreach (ClientSession session in Groups.FirstOrDefault(s => s.IsMemberOfGroup(charId))?.Characters)
            {
                i++;
                str += $" 1|{session.Character.CharacterId}|{i}|{session.Character.Level}|{session.Character.Name}|11|{session.Character.Gender}|{session.Character.Class}|{(session.Character.UseSp ? session.Character.Morph : 0)}";
            }

            foreach (ClientSession session in myGroup.Characters)
            {
                session.Client.SendPacket(str);
            }
        }

        //Server
        private async void GroupProcess()
        {
            while (true)
            {
                foreach (Group grp in Groups)
                {
                    foreach (ClientSession session in grp.Characters)
                    {
                        foreach (string str in grp.GeneratePst())
                            session.Client.SendPacket(str);
                    }
                }
                await Task.Delay(2000);
            }
        }

        //Server
        private async void SaveAllProcess()
        {
            while (true)
            {
                Sessions.ForEach(s => s.Character?.Save());
                await Task.Delay(60000 * 4);
            }
        }

        //Map ??
        private async void TaskLauncherProcess()
        {
            Task TaskMap = null;
            while (true)
            {
                foreach (var GroupedSession in Sessions.Where(s => s.Character != null).GroupBy(s => s.Character.MapId))
                {
                    foreach (ClientSession Session in GroupedSession)
                    {
                        TaskMap = new Task(() => ServerManager.GetMap(Session.Character.MapId).MapTaskManager());
                        TaskMap.Start();
                    }
                }
                if (TaskMap != null)
                    await TaskMap;
                await Task.Delay(300);
            }
        }

        #endregion
    }
}