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
    public static class NRunHandler
    {
        #region Methods

        public static void NRun(ClientSession Session, byte type, short runner, short npcid)
        {
            if (!Session.HasCurrentMapInstance)
            {
                return;
            }
            MapNpc npc = Session.CurrentMapInstance.Npcs.FirstOrDefault(s => s.MapNpcId == npcid);
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
                    if (type == (byte)Session.Character.Class)
                    {
                        return;
                    }
                    if (Session.Character.Inventory.GetAllItems().All(i => i.Type != InventoryType.Wear))
                    {
                        Session.Character.Inventory.AddNewToInventory((short)(4 + type * 14), type: InventoryType.Wear);
                        Session.Character.Inventory.AddNewToInventory((short)(81 + type * 13), type: InventoryType.Wear);
                        switch (type)
                        {
                            case 1:
                                Session.Character.Inventory.AddNewToInventory(68, type: InventoryType.Wear);
                                Session.Character.Inventory.AddNewToInventory(2082, 10);
                                break;

                            case 2:
                                Session.Character.Inventory.AddNewToInventory(78, type: InventoryType.Wear);
                                Session.Character.Inventory.AddNewToInventory(2083, 10);
                                break;

                            case 3:
                                Session.Character.Inventory.AddNewToInventory(86, type: InventoryType.Wear);
                                break;
                        }
                        Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEq());
                        Session.SendPacket(Session.Character.GenerateEquipment());
                        Session.Character.ChangeClass((ClassType)type);
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                case 2:
                    Session.SendPacket("wopen 1 0");
                    break;

                case 10:
                    Session.SendPacket("wopen 3 0");
                    break;

                case 12:
                    Session.SendPacket($"wopen {type} 0");
                    break;

                case 14:
                    Session.SendPacket("wopen 27 0");
                    string recipelist = "m_list 2";

                    if (npc != null)
                    {
                        List<Recipe> tp = npc.Recipes;
                        foreach (Recipe s in tp)
                        {
                            if (s.Amount > 0) recipelist = recipelist + $" {s.ItemVNum}";
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
                                ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                                Session.Character.Gold -= 1000 * type;
                                Session.SendPacket(Session.Character.GenerateGold());
                                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
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
                                ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                                Session.Character.Gold -= 5000 * type;
                                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
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
                                ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                                Session.Character.Gold -= 500;
                                Session.SendPacket(Session.Character.GenerateGold());
                                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
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
                            ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;
                case 150:
                    if (npc != null)
                    {
                        if (Session.Character.Family != null)
                        {
                            if (Session.Character.Family.LandOfDeath != null && !Session.Character.Family.LandOfDeath.Lock)
                            {
                                ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, Session.Character.Family.LandOfDeath.MapInstanceId, 153, 145);
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LOD_CLOSED"), 0));
                            }
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NEED_FAMILY"), 0));
                        }
                    }
                    break;
                case 301:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;

                case 23:
                    Session.SendPacket(Session.Character.GenerateInbox(type, 14));
                    //Session.SendPacket(Session.Character.GenerateFamilyMember());
                    break;

                case 60:
                    StaticBonusDTO medal = Session.Character.StaticBonusList.FirstOrDefault(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
                    byte Medal = 0;
                    int Time = 0;
                    if (medal != null)
                    {
                        Medal = medal.StaticBonusType == StaticBonusType.BazaarMedalGold ? (byte)MedalType.Gold : (byte)MedalType.Silver;
                        Time = (int)(medal.DateEnd - DateTime.Now).TotalHours;
                    }
                    Session.SendPacket($"wopen 32 {Medal} {Time}");
                    break;

                case 5002:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            Session.SendPacket("it 3");
                            ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;
                case 5001:
                    if (npc != null)
                    {
                        ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, 130, 12, 40);
                    }
                    break;
                case 5011:
                    if (npc != null)
                    {
                        ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, 170, 127, 46);
                    }
                    break;
                case 5012:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == type);
                        if (tp != null)
                        {
                            ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_NRUN_HANDLER"), runner));
                    break;
            }
        }

        #endregion
    }
}