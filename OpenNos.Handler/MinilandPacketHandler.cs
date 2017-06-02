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
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.Handler
{
    public class MinilandPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public MinilandPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session => _session;

        #endregion

        #region Methods

        /// <summary>
        /// mjoin packet
        /// </summary>
        /// <param name="mJoinPacket"></param>
        public void JoinMiniland(MJoinPacket mJoinPacket)
        {
            ClientSession sess = ServerManager.Instance.GetSessionByCharacterId(mJoinPacket.CharacterId);
            if (sess?.Character != null)
            {
                if (sess.Character.MinilandState == MinilandState.OPEN)
                {
                    ServerManager.Instance.JoinMiniland(Session, sess);
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("MINILAND_CLOSED_BY_FRIEND")));
                }
            }
        }

        /// <summary>
        /// mg packet
        /// </summary>
        /// <param name="packet"></param>
        public void MinigamePlay(MinigamePacket packet)
        {
            ClientSession client = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Miniland == Session.Character.MapInstance);
            MinilandObject mlobj = client?.Character.MinilandObjects.FirstOrDefault(s => s.ItemInstance.ItemVNum == packet.MinigameVNum);
            if (mlobj != null)
            {
                bool full = false;
                byte game = (byte)(mlobj.ItemInstance.Item.EquipmentSlot == 0 ? 4 + mlobj.ItemInstance.ItemVNum % 10 : (int)mlobj.ItemInstance.Item.EquipmentSlot / 3);
                switch (packet.Type)
                {
                    //play
                    case 1:
                        if (mlobj.ItemInstance.DurabilityPoint <= 0)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_DURABILITY_POINT"), 0));
                            return;
                        }
                        if (Session.Character.MinilandPoint <= 0)
                        {
                            Session.SendPacket($"qna #mg^1^7^3125^1^1 {Language.Instance.GetMessageFromKey("NOT_ENOUGH_MINILAND_POINT")}");
                        }
                        Session.Character.MapInstance.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(2, 1, Session.Character.CharacterId));
                        Session.Character.CurrentMinigame = (short)(game == 0 ? 5102 : game == 1 ? 5103 : game == 2 ? 5105 : game == 3 ? 5104 : game == 4 ? 5113 : 5112);
                        Session.SendPacket($"mlo_st {game}");
                        break;

                    //stop
                    case 2:
                        Session.Character.CurrentMinigame = 0;
                        Session.Character.MapInstance.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(6, 1, Session.Character.CharacterId));
                        break;

                    case 3:
                        Session.Character.CurrentMinigame = 0;
                        Session.Character.MapInstance.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(6, 1, Session.Character.CharacterId));
                        int Level = -1;
                        for (short i = 0; i < GetMinilandMaxPoint(game).Count(); i++)
                        {
                            if (packet.Point > GetMinilandMaxPoint(game)[i])
                            {
                                Level = i;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Session.SendPacket(Level != -1
                            ? $"mlo_lv {Level}"
                            : $"mg 3 {game} {packet.MinigameVNum} 0 0");
                        break;

                    // select gift
                    case 4:
                        if (Session.Character.MinilandPoint >= 100)
                        {
                            Gift obj = GetMinilandGift(packet.MinigameVNum, (int)packet.Point);
                            if (obj != null)
                            {
                                Session.SendPacket($"mlo_rw {obj.VNum} {obj.Amount}");
                                Session.SendPacket(Session.Character.GenerateMinilandPoint());
                                List<ItemInstance> inv = Session.Character.Inventory.AddNewToInventory(obj.VNum, obj.Amount);
                                Session.Character.MinilandPoint -= 100;
                                if (!inv.Any())
                                {
                                    Session.Character.SendGift(Session.Character.CharacterId, obj.VNum, obj.Amount, 0, 0, false);
                                }

                                if (client != Session)
                                {
                                    switch (packet.Point)
                                    {
                                        case 0:
                                            mlobj.Level1BoxAmount++;
                                            break;

                                        case 1:
                                            mlobj.Level2BoxAmount++;
                                            break;

                                        case 2:
                                            mlobj.Level3BoxAmount++;
                                            break;

                                        case 3:
                                            mlobj.Level4BoxAmount++;
                                            break;

                                        case 4:
                                            mlobj.Level5BoxAmount++;
                                            break;
                                    }
                                }
                            }
                        }
                        break;

                    case 5:
                        Session.SendPacket(Session.Character.GenerateMloMg(mlobj, packet));
                        break;

                    //refill
                    case 6:
                        if (packet.Point == null)
                        {
                            return;
                        }
                        if (Session.Character.Gold > packet.Point)
                        {
                            Session.Character.Gold -= (int)packet.Point;
                            Session.SendPacket(Session.Character.GenerateGold());
                            mlobj.ItemInstance.DurabilityPoint += (int)(packet.Point / 100);
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey(string.Format("REFILL_MINIGAME", (int)packet.Point / 100))));
                            Session.SendPacket(Session.Character.GenerateMloMg(mlobj, packet));
                        }
                        break;

                    //gift
                    case 7:
                        Session.SendPacket($"mlo_pmg {packet.MinigameVNum} {Session.Character.MinilandPoint} {(mlobj.ItemInstance.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} {(mlobj.Level1BoxAmount > 0 ? $"392 {mlobj.Level1BoxAmount}" : "0 0")} {(mlobj.Level2BoxAmount > 0 ? $"393 {mlobj.Level2BoxAmount}" : "0 0")} {(mlobj.Level3BoxAmount > 0 ? $"394 {mlobj.Level3BoxAmount}" : "0 0")} {(mlobj.Level4BoxAmount > 0 ? $"395 {mlobj.Level4BoxAmount}" : "0 0")} {(mlobj.Level5BoxAmount > 0 ? $"396 {mlobj.Level5BoxAmount}" : "0 0")} 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0");
                        break;

                    //get gift
                    case 8:
                        int amount = 0;
                        switch (packet.Point)
                        {
                            case 0:
                                amount = mlobj.Level1BoxAmount;
                                break;

                            case 1:
                                amount = mlobj.Level2BoxAmount;
                                break;

                            case 2:
                                amount = mlobj.Level3BoxAmount;
                                break;

                            case 3:
                                amount = mlobj.Level4BoxAmount;
                                break;

                            case 4:
                                amount = mlobj.Level5BoxAmount;
                                break;
                        }
                        List<Gift> gifts = new List<Gift>();
                        for (int i = 0; i < amount; i++)
                        {
                            Gift gift = GetMinilandGift(packet.MinigameVNum, (int)packet.Point);
                            if (gift != null)
                            {
                                if (gifts.Any(o => o.VNum == gift.VNum))
                                {
                                    gifts.First(o => o.Amount == gift.Amount).Amount += gift.Amount;
                                }
                                else
                                {
                                    gifts.Add(gift);
                                }
                            }
                        }
                        string str = string.Empty;
                        for (int i = 0; i < 9; i++)
                        {
                            if (gifts.Count > i)
                            {
                                List<ItemInstance> inv = Session.Character.Inventory.AddNewToInventory(gifts.ElementAt(i).VNum, gifts.ElementAt(i).Amount);
                                if (inv.Any())
                                {
                                    Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.Instance.GetItem(gifts.ElementAt(i).VNum).Name} x {gifts.ElementAt(i).Amount}", 12));
                                }
                                else
                                {
                                    Session.Character.SendGift(Session.Character.CharacterId, gifts.ElementAt(i).VNum, gifts.ElementAt(i).Amount, 0, 0, false);
                                }
                                str += $" {gifts.ElementAt(i).VNum} {gifts.ElementAt(i).Amount}";
                            }
                            else
                            {
                                str += " 0 0";
                            }
                        }
                        Session.SendPacket($"mlo_pmg {packet.MinigameVNum} {Session.Character.MinilandPoint} {(mlobj.ItemInstance.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} {(mlobj.Level1BoxAmount > 0 ? $"392 {mlobj.Level1BoxAmount}" : "0 0")} {(mlobj.Level2BoxAmount > 0 ? $"393 {mlobj.Level2BoxAmount}" : "0 0")} {(mlobj.Level3BoxAmount > 0 ? $"394 {mlobj.Level3BoxAmount}" : "0 0")} {(mlobj.Level4BoxAmount > 0 ? $"395 {mlobj.Level4BoxAmount}" : "0 0")} {(mlobj.Level5BoxAmount > 0 ? $"396 {mlobj.Level5BoxAmount}" : "0 0")}{str}");
                        break;

                    //coupon
                    case 9:
                        List<ItemInstance> items = Session.Character.Inventory.GetAllItems().Where(s => s.ItemVNum == 1269 || s.ItemVNum == 1271).OrderBy(s => s.Slot).ToList();
                        if (items.Count > 0)
                        {
                            Session.Character.Inventory.RemoveItemAmount(items.ElementAt(0).ItemVNum);
                            int point = items.ElementAt(0).ItemVNum == 1269 ? 300 : 500;
                            mlobj.ItemInstance.DurabilityPoint += point;
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey(string.Format("REFILL_MINIGAME", point))));
                            Session.SendPacket(Session.Character.GenerateMloMg(mlobj, packet));
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// addobj packet
        /// </summary>
        /// <param name="packet"></param>
        public void MinilandAddObject(AddobjPacket packet)
        {
            ItemInstance minilandobject = Session.Character.Inventory.LoadBySlotAndType<ItemInstance>(packet.Slot, InventoryType.Miniland);
            if (minilandobject != null)
            {
                if (Session.Character.MinilandObjects.All(s => s.ItemInstanceId != minilandobject.Id))
                {
                    if (Session.Character.MinilandState == MinilandState.LOCK)
                    {
                        MinilandObject minilandobj = new MinilandObject
                        {
                            CharacterId = Session.Character.CharacterId,
                            ItemInstance = minilandobject,
                            ItemInstanceId = minilandobject.Id,
                            MapX = packet.PositionX,
                            MapY = packet.PositionY,
                            Level1BoxAmount = 0,
                            Level2BoxAmount = 0,
                            Level3BoxAmount = 0,
                            Level4BoxAmount = 0,
                            Level5BoxAmount = 0
                        };

                        if (minilandobject.Item.ItemType == ItemType.House)
                        {
                            switch (minilandobject.Item.ItemSubType)
                            {
                                case 2:
                                    minilandobj.MapX = 31;
                                    minilandobj.MapY = 3;
                                    break;

                                case 0:
                                    minilandobj.MapX = 24;
                                    minilandobj.MapY = 7;
                                    break;

                                case 1:
                                    minilandobj.MapX = 21;
                                    minilandobj.MapY = 4;
                                    break;
                            }

                            MinilandObject min = Session.Character.MinilandObjects.FirstOrDefault(s => s.ItemInstance.Item.ItemType == ItemType.House && s.ItemInstance.Item.ItemSubType == minilandobject.Item.ItemSubType);
                            if (min != null)
                            {
                                MinilandRemoveObject(new RmvobjPacket { Slot = min.ItemInstance.Slot });
                            }
                        }

                        if (minilandobject.Item.IsMinilandObject)
                        {
                            Session.Character.WareHouseSize = minilandobject.Item.MinilandObjectPoint;
                        }
                        Session.Character.MinilandObjects.Add(minilandobj);
                        Session.SendPacket(minilandobj.GenerateMinilandEffect(false));
                        Session.SendPacket(Session.Character.GenerateMinilandPoint());
                        Session.SendPacket(minilandobj.GenerateMinilandObject(false));
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_NEED_LOCK"), 0));
                    }
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_THIS_MINILANDOBJECT"), 0));
                }
            }
        }

        /// <summary>
        /// mledit packet
        /// </summary>
        /// <param name="mlEditPacket"></param>
        public void MinilandEdit(MLEditPacket mlEditPacket)
        {
            switch (mlEditPacket.Type)
            {
                case 1:
                    Session.SendPacket($"mlintro {mlEditPacket.Parameters.Replace(' ', '^')}");
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("MINILAND_INFO_CHANGED")));
                    break;

                case 2:
                    MinilandState state;
                    Enum.TryParse(mlEditPacket.Parameters, out state);

                    switch (state)
                    {
                        case MinilandState.PRIVATE:
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_PRIVATE"), 0));

                            //Need to be review to permit one friend limit on the miniland
                            Session.Character.Miniland.Sessions.Where(s => s.Character != Session.Character).ToList().ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.MapX, s.Character.MapY));
                            break;

                        case MinilandState.LOCK:
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_LOCK"), 0));
                            Session.Character.Miniland.Sessions.Where(s => s.Character != Session.Character).ToList().ForEach(s => ServerManager.Instance.ChangeMap(s.Character.CharacterId, s.Character.MapId, s.Character.MapX, s.Character.MapY));
                            break;

                        case MinilandState.OPEN:
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_PUBLIC"), 0));
                            break;
                    }

                    Session.Character.MinilandState = state;
                    break;
            }
        }

        /// <summary>
        /// rmvobj packet
        /// </summary>
        /// <param name="packet"></param>
        public void MinilandRemoveObject(RmvobjPacket packet)
        {
            ItemInstance minilandobject = Session.Character.Inventory.LoadBySlotAndType<ItemInstance>(packet.Slot, InventoryType.Miniland);
            if (minilandobject != null)
            {
                if (Session.Character.MinilandState == MinilandState.LOCK)
                {
                    MinilandObject minilandObject = Session.Character.MinilandObjects.FirstOrDefault(s => s.ItemInstanceId == minilandobject.Id);
                    if (minilandObject != null)
                    {
                        if (minilandobject.Item.IsMinilandObject)
                        {
                            Session.Character.WareHouseSize = 0;
                        }
                        Session.Character.MinilandObjects.Remove(minilandObject);
                        Session.SendPacket(minilandObject.GenerateMinilandEffect(true));
                        Session.SendPacket(Session.Character.GenerateMinilandPoint());
                        Session.SendPacket(minilandObject.GenerateMinilandObject(true));
                    }
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_NEED_LOCK"), 0));
                }
            }
        }

        /// <summary>
        /// useobj packet
        /// </summary>
        /// <param name="packet"></param>
        public void UseMinilandObject(UseobjPacket packet)
        {
            ClientSession client = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Miniland == Session.Character.MapInstance);
            ItemInstance minilandObjectItem = client?.Character.Inventory.LoadBySlotAndType<ItemInstance>(packet.Slot, InventoryType.Miniland);
            if (minilandObjectItem != null)
            {
                MinilandObject minilandObject = client.Character.MinilandObjects.FirstOrDefault(s => s.ItemInstanceId == minilandObjectItem.Id);
                if (minilandObject != null)
                {
                    if (!minilandObjectItem.Item.IsMinilandObject)
                    {
                        byte game = (byte)(minilandObject.ItemInstance.Item.EquipmentSlot == 0 ? 4 + minilandObject.ItemInstance.ItemVNum % 10 : (int)minilandObject.ItemInstance.Item.EquipmentSlot / 3);
                        bool full = false;
                        Session.SendPacket($"mlo_info {(client == Session ? 1 : 0)} {minilandObjectItem.ItemVNum} {packet.Slot} {Session.Character.MinilandPoint} {(minilandObjectItem.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} 0 {GetMinilandMaxPoint(game)[0]} {GetMinilandMaxPoint(game)[0] + 1} {GetMinilandMaxPoint(game)[1]} {GetMinilandMaxPoint(game)[1] + 1} {GetMinilandMaxPoint(game)[2]} {GetMinilandMaxPoint(game)[2] + 2} {GetMinilandMaxPoint(game)[3]} {GetMinilandMaxPoint(game)[3] + 1} {GetMinilandMaxPoint(game)[4]} {GetMinilandMaxPoint(game)[4] + 1} {GetMinilandMaxPoint(game)[5]}");
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateStashAll());
                    }
                }
            }
        }

        private static Gift GetMinilandGift(short game, int point)
        {
            List<Gift> gifts = new List<Gift>();
            Random rand = new Random();
            switch (game)
            {
                case 3117:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2099, 3));
                            gifts.Add(new Gift(2100, 3));
                            gifts.Add(new Gift(2102, 3));
                            break;

                        case 1:
                            gifts.Add(new Gift(1031, 2));
                            gifts.Add(new Gift(1032, 2));
                            gifts.Add(new Gift(1033, 2));
                            gifts.Add(new Gift(1034, 2));
                            gifts.Add(new Gift(2205, 1));
                            break;

                        case 2:
                            gifts.Add(new Gift(2205, 1));
                            gifts.Add(new Gift(2189, 1));
                            gifts.Add(new Gift(2034, 2));
                            break;

                        case 3:
                            gifts.Add(new Gift(2205, 1));
                            gifts.Add(new Gift(2189, 1));
                            gifts.Add(new Gift(2034, 2));
                            gifts.Add(new Gift(2105, 1));
                            break;

                        case 4:
                            gifts.Add(new Gift(2205, 1));
                            gifts.Add(new Gift(2189, 1));
                            gifts.Add(new Gift(2034, 2));
                            gifts.Add(new Gift(2193, 1));
                            break;
                    }
                    break;

                case 3118:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2099, 3));
                            gifts.Add(new Gift(2100, 3));
                            gifts.Add(new Gift(2102, 3));
                            break;

                        case 1:
                            gifts.Add(new Gift(2206, 1));
                            gifts.Add(new Gift(2032, 3));
                            break;

                        case 2:
                            gifts.Add(new Gift(2206, 1));
                            gifts.Add(new Gift(2106, 1));
                            gifts.Add(new Gift(2038, 2));
                            break;

                        case 3:
                            gifts.Add(new Gift(2206, 1));
                            gifts.Add(new Gift(2190, 1));
                            gifts.Add(new Gift(2039, 2));
                            gifts.Add(new Gift(2109, 1));
                            break;

                        case 4:
                            gifts.Add(new Gift(2206, 1));
                            gifts.Add(new Gift(2190, 1));
                            gifts.Add(new Gift(2040, 2));
                            gifts.Add(new Gift(2194, 1));
                            break;
                    }
                    break;

                case 3119:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2099, 3));
                            gifts.Add(new Gift(2100, 3));
                            gifts.Add(new Gift(2102, 3));
                            break;

                        case 1:
                            gifts.Add(new Gift(2027, 15));
                            gifts.Add(new Gift(2207, 1));
                            break;

                        case 2:
                            gifts.Add(new Gift(2207, 1));
                            gifts.Add(new Gift(2046, 2));
                            gifts.Add(new Gift(2191, 1));
                            break;

                        case 3:
                            gifts.Add(new Gift(2207, 1));
                            gifts.Add(new Gift(2047, 2));
                            gifts.Add(new Gift(2191, 1));
                            gifts.Add(new Gift(2117, 1));
                            break;

                        case 4:
                            gifts.Add(new Gift(2207, 1));
                            gifts.Add(new Gift(2048, 2));
                            gifts.Add(new Gift(2191, 1));
                            gifts.Add(new Gift(2195, 1));
                            break;
                    }
                    break;

                case 3120:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2099, 3));
                            gifts.Add(new Gift(2100, 3));
                            gifts.Add(new Gift(2102, 3));
                            break;

                        case 1:
                            gifts.Add(new Gift(2208, 1));
                            gifts.Add(new Gift(2017, 10));
                            break;

                        case 2:
                            gifts.Add(new Gift(2208, 1));
                            gifts.Add(new Gift(2192, 1));
                            gifts.Add(new Gift(2042, 2));
                            break;

                        case 3:
                            gifts.Add(new Gift(2208, 1));
                            gifts.Add(new Gift(2192, 1));
                            gifts.Add(new Gift(2043, 2));
                            gifts.Add(new Gift(2118, 1));
                            break;

                        case 4:
                            gifts.Add(new Gift(2208, 1));
                            gifts.Add(new Gift(2192, 1));
                            gifts.Add(new Gift(2044, 2));
                            gifts.Add(new Gift(2196, 1));
                            break;
                    }
                    break;

                case 3121:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2099, 4));
                            gifts.Add(new Gift(2100, 4));
                            gifts.Add(new Gift(2102, 4));
                            gifts.Add(new Gift(1031, 3));
                            gifts.Add(new Gift(1032, 3));
                            gifts.Add(new Gift(1033, 3));
                            gifts.Add(new Gift(1034, 3));
                            break;

                        case 1:
                            gifts.Add(new Gift(2034, 3));
                            gifts.Add(new Gift(2205, 1));
                            gifts.Add(new Gift(2189, 1));
                            break;

                        case 2:
                            gifts.Add(new Gift(2035, 3));
                            gifts.Add(new Gift(2193, 1));
                            gifts.Add(new Gift(2275, 1));
                            break;

                        case 3:
                            gifts.Add(new Gift(2036, 3));
                            gifts.Add(new Gift(2193, 1));
                            gifts.Add(new Gift(1028, 1));
                            break;

                        case 4:
                            gifts.Add(new Gift(2037, 3));
                            gifts.Add(new Gift(2193, 1));
                            gifts.Add(new Gift(1028, 1));
                            gifts.Add(new Gift(1029, 1));
                            gifts.Add(new Gift(2197, 1));
                            break;
                    }
                    break;

                case 3122:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2099, 4));
                            gifts.Add(new Gift(2100, 4));
                            gifts.Add(new Gift(2102, 4));
                            gifts.Add(new Gift(2032, 4));
                            break;

                        case 1:
                            gifts.Add(new Gift(2038, 3));
                            gifts.Add(new Gift(2206, 1));
                            gifts.Add(new Gift(2190, 1));
                            break;

                        case 2:
                            gifts.Add(new Gift(2039, 3));
                            gifts.Add(new Gift(2194, 1));
                            gifts.Add(new Gift(2105, 1));
                            break;

                        case 3:
                            gifts.Add(new Gift(2040, 3));
                            gifts.Add(new Gift(2194, 1));
                            gifts.Add(new Gift(1028, 1));
                            break;

                        case 4:
                            gifts.Add(new Gift(2041, 3));
                            gifts.Add(new Gift(2194, 1));
                            gifts.Add(new Gift(1028, 1));
                            gifts.Add(new Gift(1029, 1));
                            gifts.Add(new Gift(2198, 1));
                            break;
                    }
                    break;

                case 3123:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2099, 4));
                            gifts.Add(new Gift(2100, 4));
                            gifts.Add(new Gift(2102, 4));
                            gifts.Add(new Gift(2047, 15));
                            break;

                        case 1:
                            gifts.Add(new Gift(2046, 3));
                            gifts.Add(new Gift(2205, 1));
                            gifts.Add(new Gift(2189, 1));
                            break;

                        case 2:
                            gifts.Add(new Gift(2047, 3));
                            gifts.Add(new Gift(2195, 1));
                            gifts.Add(new Gift(2117, 1));
                            break;

                        case 3:
                            gifts.Add(new Gift(2048, 3));
                            gifts.Add(new Gift(2195, 1));
                            gifts.Add(new Gift(1028, 1));
                            break;

                        case 4:
                            gifts.Add(new Gift(2049, 3));
                            gifts.Add(new Gift(2195, 1));
                            gifts.Add(new Gift(1028, 1));
                            gifts.Add(new Gift(1029, 1));
                            gifts.Add(new Gift(2199, 1));
                            break;
                    }
                    break;

                case 3124:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2099, 4));
                            gifts.Add(new Gift(2100, 4));
                            gifts.Add(new Gift(2102, 4));
                            gifts.Add(new Gift(2017, 10));
                            break;

                        case 1:
                            gifts.Add(new Gift(2042, 3));
                            gifts.Add(new Gift(2192, 1));
                            gifts.Add(new Gift(2189, 1));
                            break;

                        case 2:
                            gifts.Add(new Gift(2043, 3));
                            gifts.Add(new Gift(2196, 1));
                            gifts.Add(new Gift(2118, 1));
                            break;

                        case 3:
                            gifts.Add(new Gift(2044, 3));
                            gifts.Add(new Gift(2196, 1));
                            gifts.Add(new Gift(1028, 1));
                            break;

                        case 4:
                            gifts.Add(new Gift(2045, 3));
                            gifts.Add(new Gift(2196, 1));
                            gifts.Add(new Gift(1028, 1));
                            gifts.Add(new Gift(1029, 1));
                            gifts.Add(new Gift(2200, 1));
                            break;
                    }
                    break;

                case 3125:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2034, 4));
                            gifts.Add(new Gift(2189, 2));
                            gifts.Add(new Gift(2205, 2));
                            break;

                        case 1:
                            gifts.Add(new Gift(2035, 4));
                            gifts.Add(new Gift(2105, 2));
                            break;

                        case 2:
                            gifts.Add(new Gift(2036, 4));
                            gifts.Add(new Gift(2193, 2));
                            break;

                        case 3:
                            gifts.Add(new Gift(2037, 4));
                            gifts.Add(new Gift(2193, 2));
                            gifts.Add(new Gift(2201, 2));
                            gifts.Add(new Gift(2226, 2));
                            gifts.Add(new Gift(1028, 2));
                            gifts.Add(new Gift(1029, 2));
                            break;

                        case 4:
                            gifts.Add(new Gift(2213, 1));
                            gifts.Add(new Gift(2193, 2));
                            gifts.Add(new Gift(2034, 2));
                            gifts.Add(new Gift(2226, 2));
                            gifts.Add(new Gift(1030, 2));
                            break;
                    }
                    break;

                case 3126:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2038, 4));
                            gifts.Add(new Gift(2106, 2));
                            gifts.Add(new Gift(2206, 2));
                            break;

                        case 1:
                            gifts.Add(new Gift(2039, 4));
                            gifts.Add(new Gift(2109, 2));
                            break;

                        case 2:
                            gifts.Add(new Gift(2040, 4));
                            gifts.Add(new Gift(2194, 2));
                            break;

                        case 3:
                            gifts.Add(new Gift(2040, 4));
                            gifts.Add(new Gift(2194, 2));
                            gifts.Add(new Gift(2201, 2));
                            gifts.Add(new Gift(2231, 2));
                            gifts.Add(new Gift(1028, 2));
                            gifts.Add(new Gift(1029, 2));
                            break;

                        case 4:
                            gifts.Add(new Gift(2214, 1));
                            gifts.Add(new Gift(2194, 1));
                            gifts.Add(new Gift(2231, 2));
                            gifts.Add(new Gift(2202, 1));
                            gifts.Add(new Gift(1030, 2));
                            break;
                    }
                    break;

                case 3127:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2046, 4));
                            gifts.Add(new Gift(2207, 2));
                            break;

                        case 1:
                            gifts.Add(new Gift(2047, 4));
                            gifts.Add(new Gift(2117, 2));
                            break;

                        case 2:
                            gifts.Add(new Gift(2048, 4));
                            gifts.Add(new Gift(2195, 2));
                            break;

                        case 3:
                            gifts.Add(new Gift(2049, 4));
                            gifts.Add(new Gift(2195, 2));
                            gifts.Add(new Gift(2199, 2));
                            gifts.Add(new Gift(1028, 2));
                            gifts.Add(new Gift(1029, 2));
                            break;

                        case 4:
                            gifts.Add(new Gift(2216, 1));
                            gifts.Add(new Gift(2195, 2));
                            gifts.Add(new Gift(2203, 1));
                            gifts.Add(new Gift(1030, 2));
                            break;
                    }
                    break;

                case 3128:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2042, 4));
                            gifts.Add(new Gift(2192, 2));
                            gifts.Add(new Gift(2208, 2));
                            break;

                        case 1:
                            gifts.Add(new Gift(2043, 4));
                            gifts.Add(new Gift(2118, 2));
                            break;

                        case 2:
                            gifts.Add(new Gift(2044, 4));
                            gifts.Add(new Gift(2196, 2));
                            break;

                        case 3:
                            gifts.Add(new Gift(2045, 4));
                            gifts.Add(new Gift(2196, 2));
                            gifts.Add(new Gift(2200, 2));
                            gifts.Add(new Gift(1028, 2));
                            gifts.Add(new Gift(1029, 2));
                            break;

                        case 4:
                            gifts.Add(new Gift(2215, 1));
                            gifts.Add(new Gift(2196, 2));
                            gifts.Add(new Gift(2204, 1));
                            gifts.Add(new Gift(1030, 2));
                            break;
                    }
                    break;

                // GM mini-game
                case 3130:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2042, 4));
                            gifts.Add(new Gift(2192, 2));
                            gifts.Add(new Gift(2208, 2));
                            break;

                        case 1:
                            gifts.Add(new Gift(2043, 4));
                            gifts.Add(new Gift(2118, 2));
                            break;

                        case 2:
                            gifts.Add(new Gift(2044, 4));
                            gifts.Add(new Gift(2196, 2));
                            break;

                        case 3:
                            gifts.Add(new Gift(2045, 4));
                            gifts.Add(new Gift(2196, 2));
                            gifts.Add(new Gift(2200, 2));
                            gifts.Add(new Gift(1028, 2));
                            gifts.Add(new Gift(1029, 2));
                            break;

                        case 4:
                            gifts.Add(new Gift(2215, 1));
                            gifts.Add(new Gift(2196, 2));
                            gifts.Add(new Gift(2204, 1));
                            gifts.Add(new Gift(1030, 2));
                            break;
                    }
                    break;

                case 3131:
                    switch (point)
                    {
                        case 0:
                            gifts.Add(new Gift(2042, 4));
                            gifts.Add(new Gift(2192, 2));
                            gifts.Add(new Gift(2208, 2));
                            break;

                        case 1:
                            gifts.Add(new Gift(2043, 4));
                            gifts.Add(new Gift(2118, 2));
                            break;

                        case 2:
                            gifts.Add(new Gift(2044, 4));
                            gifts.Add(new Gift(2196, 2));
                            break;

                        case 3:
                            gifts.Add(new Gift(2045, 4));
                            gifts.Add(new Gift(2196, 2));
                            gifts.Add(new Gift(2200, 2));
                            gifts.Add(new Gift(1028, 2));
                            gifts.Add(new Gift(1029, 2));
                            break;

                        case 4:
                            gifts.Add(new Gift(2215, 1));
                            gifts.Add(new Gift(2196, 2));
                            gifts.Add(new Gift(2204, 1));
                            gifts.Add(new Gift(1030, 2));
                            break;
                    }
                    break;
            }
            return gifts.OrderBy(s => rand.Next()).FirstOrDefault();
        }

        private static int[] GetMinilandMaxPoint(byte game)
        {
            int[] points;
            switch (game)
            {
                case 0:
                    points = new[] { 999, 4999, 7999, 11999, 15999, 1000000 };
                    break;

                case 1:
                    points = new[] { 999, 4999, 9999, 13999, 17999, 1000000 };
                    break;

                case 2:
                    points = new[] { 999, 3999, 7999, 14999, 24999, 1000000 };
                    break;

                case 3:
                    points = new[] { 999, 3999, 7999, 11999, 19999, 1000000 };
                    break;

                case 4:
                    points = new[] { 999, 4999, 7999, 11999, 15999, 1000000 };
                    break;

                case 5:
                    points = new[] { 999, 4999, 7999, 11999, 15999, 1000000 };
                    break;

                default:
                    points = new[] { 999, 4999, 7999, 14999, 24999, 1000000 };
                    break;
            }
            return points;
        }

        #endregion
    }
}