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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class NRunHandler
    {
        #region Methods

        public static void NRun(ClientSession session, byte type, short runner, short data3, short npcid)
        {
            if (!session.HasCurrentMap)
            {
                return;
            }
            MapNpc npc = session.CurrentMap.Npcs.FirstOrDefault(s => s.MapNpcId == npcid);
            switch (runner)
            {
                case 1:
                    if (session.Character.Class != (byte)ClassType.Adventurer)
                    {
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ADVENTURER"), 0));
                        return;
                    }
                    if (session.Character.Level < 15 || session.Character.JobLevel < 20)
                    {
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                        return;
                    }
                    if (type == (byte)session.Character.Class)
                    {
                        return;
                    }
                    if (session.Character.Inventory.GetAllItems().All(i => i.Type != InventoryType.Wear))
                    {
                        session.Character.Inventory.AddNewToInventory((short)(4 + type * 14), type: InventoryType.Wear);
                        session.Character.Inventory.AddNewToInventory((short)(81 + type * 13), type: InventoryType.Wear);
                        switch (type)
                        {
                            case 1:
                                session.Character.Inventory.AddNewToInventory(68, type: InventoryType.Wear);
                                session.Character.Inventory.AddNewToInventory(2082, 10);
                                break;

                            case 2:
                                session.Character.Inventory.AddNewToInventory(78, type: InventoryType.Wear);
                                session.Character.Inventory.AddNewToInventory(2083, 10);
                                break;

                            case 3:
                                session.Character.Inventory.AddNewToInventory(86, type: InventoryType.Wear);
                                break;
                        }
                        session.CurrentMap?.Broadcast(session.Character.GenerateEq());
                        session.SendPacket(session.Character.GenerateEquipment());
                        session.Character.ChangeClass((ClassType)type);
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                case 2:
                    session.SendPacket("wopen 1 0");
                    break;

                case 10:
                    session.SendPacket("wopen 3 0");
                    break;

                case 12:
                    session.SendPacket($"wopen {type} 0");
                    break;

                case 14:
                    session.SendPacket("wopen 27 0");
                    string recipelist = "m_list 2";

                    if (npc != null)
                    {
                        List<Recipe> tp = npc.Recipes;

                        recipelist = tp.Where(s => s.Amount > 0).Aggregate(recipelist, (current, rec) => current + $" {rec.ItemVNum}");
                        recipelist += " -100";
                        session.SendPacket(recipelist);
                    }
                    break;

                case 16:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            if (session.Character.Gold >= 1000 * type)
                            {
                                ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                                session.Character.Gold -= 1000 * type;
                                session.SendPacket(session.Character.GenerateGold());
                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                            }
                            else
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
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
                            if (session.Character.Gold >= 5000 * type)
                            {
                                ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                                session.Character.Gold -= 5000 * type;
                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                            }
                            else
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
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
                            if (session.Character.Gold >= 500)
                            {
                                ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                                session.Character.Gold -= 500;
                                session.SendPacket(session.Character.GenerateGold());
                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                            }
                            else
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
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
                            ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;

                case 301:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;

                case 5002:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            session.SendPacket("it 3");
                            ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;

                case 5012:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            ServerManager.Instance.LeaveMap(session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
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