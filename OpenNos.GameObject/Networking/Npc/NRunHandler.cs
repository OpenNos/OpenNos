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
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class NRunHandler
    {
        #region Methods

        public static void NRun(ClientSession Session, byte type, short runner, short data3, short npcid)
        {
            MapNpc npc = Session.CurrentMap.Npcs.FirstOrDefault(s => s.MapNpcId == npcid);
            switch (runner)
            {
                case 1:
                    if (Session.Character.Class != (byte)ClassType.Adventurer)
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ADVENTURER"), 0));
                        return;
                    }
                    if (Session.Character.Level < 15 || Session.Character.JobLevel < 20)
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                        return;
                    }
                    if (type == Session.Character.Class)
                    {
                        return;
                    }
                    if (!Session.Character.EquipmentList.Inventory.Any())
                    {
                        ItemInstance newItem1 = InventoryList.CreateItemInstance((short)(4 + type * 14));
                        Session.Character.EquipmentList.AddToInventoryWithSlotAndType(newItem1, InventoryType.Equipment, newItem1.Item.EquipmentSlot);
                        ItemInstance newItem2 = InventoryList.CreateItemInstance((short)(81 + type * 13));
                        Session.Character.EquipmentList.AddToInventoryWithSlotAndType(newItem2, InventoryType.Equipment, newItem2.Item.EquipmentSlot);
                        switch (type)
                        {
                            case 1:
                                ItemInstance newItem68 = InventoryList.CreateItemInstance(68);
                                Session.Character.EquipmentList.AddToInventoryWithSlotAndType(newItem68, InventoryType.Equipment, newItem68.Item.EquipmentSlot);
                                Session.Character.InventoryList.AddNewItemToInventory(2082, 10);
                                break;

                            case 2:
                                ItemInstance newItem78 = InventoryList.CreateItemInstance(78);
                                Session.Character.EquipmentList.AddToInventoryWithSlotAndType(newItem78, InventoryType.Equipment, newItem78.Item.EquipmentSlot);
                                Session.Character.InventoryList.AddNewItemToInventory(2083, 10);
                                break;

                            case 3:
                                ItemInstance newItem86 = InventoryList.CreateItemInstance(86);
                                Session.Character.EquipmentList.AddToInventoryWithSlotAndType(newItem86, InventoryType.Equipment, newItem86.Item.EquipmentSlot);
                                break;
                        }
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateEq());
                        Session.SendPacket(Session.Character.GenerateEquipment());
                        Session.Character.ChangeClass(Convert.ToByte(type));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                case 2:
                    Session.SendPacket($"wopen 1 0");
                    break;

                case 10:
                    Session.SendPacket($"wopen 3 0");
                    break;

                case 12:
                    Session.SendPacket($"wopen {type} 0");
                    break;

                case 14:
                    Session.SendPacket($"wopen 27 0");
                    string recipelist = "m_list 2";

                    if (npc != null)
                    {
                        List<Recipe> tp = npc.Recipes;

                        foreach (Recipe rec in tp.Where(s => s.Amount > 0))
                        {
                            recipelist += String.Format(" {0}", rec.ItemVNum);
                        }
                        recipelist += " -100";
                        Session.SendPacket(recipelist);
                    }
                    break;

                case 16:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            if (Session.Character.Gold >= 1000 * type)
                            {
                                ServerManager.Instance.MapOut(Session.Character.CharacterId);
                                Session.Character.Gold -= 1000 * type;
                                Session.SendPacket(Session.Character.GenerateGold());
                                Session.Character.MapY = tp.MapY;
                                Session.Character.MapX = tp.MapX;
                                Session.Character.MapId = tp.MapId;
                                ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                            }
                        }
                    }
                    break;

                case 26:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            if (Session.Character.Gold >= 5000 * type)
                            {
                                ServerManager.Instance.MapOut(Session.Character.CharacterId);
                                Session.Character.Gold -= 5000 * type;
                                Session.SendPacket(Session.Character.GenerateGold());
                                Session.Character.MapY = tp.MapY;
                                Session.Character.MapX = tp.MapX;
                                Session.Character.MapId = tp.MapId;
                                ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                            }
                        }
                    }
                    break;

                case 45:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            if (Session.Character.Gold >= 500)
                            {
                                ServerManager.Instance.MapOut(Session.Character.CharacterId);
                                Session.Character.Gold -= 500;
                                Session.SendPacket(Session.Character.GenerateGold());
                                Session.Character.MapY = tp.MapY;
                                Session.Character.MapX = tp.MapX;
                                Session.Character.MapId = tp.MapId;
                                ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                            }
                        }
                    }
                    break;

                case 132:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            ServerManager.Instance.MapOut(Session.Character.CharacterId);
                            Session.Character.MapY = tp.MapY;
                            Session.Character.MapX = tp.MapX;
                            Session.Character.MapId = tp.MapId;
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                        }
                    }
                    break;

                case 301:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            ServerManager.Instance.MapOut(Session.Character.CharacterId);
                            Session.Character.MapY = tp.MapY;
                            Session.Character.MapX = tp.MapX;
                            Session.Character.MapId = tp.MapId;
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                        }
                    }
                    break;

                case 5002:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            Session.SendPacket("it 3");
                            ServerManager.Instance.MapOut(Session.Character.CharacterId);
                            Session.Character.MapY = tp.MapY;
                            Session.Character.MapX = tp.MapX;
                            Session.Character.MapId = tp.MapId;
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                        }
                    }
                    break;

                case 5012:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            ServerManager.Instance.MapOut(Session.Character.CharacterId);
                            Session.Character.MapY = tp.MapY;
                            Session.Character.MapX = tp.MapX;
                            Session.Character.MapId = tp.MapId;
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                        }
                    }
                    break;

                default:
                    Logger.Log.Warn(String.Format(Language.Instance.GetMessageFromKey("NO_NRUN_HANDLER"), runner));
                    break;
            }
        }

        #endregion
    }
}