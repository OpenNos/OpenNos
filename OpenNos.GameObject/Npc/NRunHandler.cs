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
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenNos.GameObject
{
    public static class NRunHandler
    {
        #region Methods

        public static void NRun(ClientSession Session, NRunPacket packet)
        {
            if (!Session.HasCurrentMapInstanceNode)
            {
                return;
            }
            MapNpc npc = Session.CurrentMapInstanceNode.Data.Npcs.FirstOrDefault(s => s.MapNpcId == packet.NpcId);
            switch (packet.Runner)
            {
                case 1:
                    if (Session.Character.Class != (byte)ClassType.Adventurer)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ADVENTURER"), 0));
                        return;
                    }
                    if (Session.Character.Level < 15 || Session.Character.JobLevel < 20)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                        return;
                    }
                    if (packet.Type == (byte)Session.Character.Class)
                    {
                        return;
                    }
                    if (Session.Character.Inventory.GetAllItems().All(i => i.Type != InventoryType.Wear))
                    {
                        Session.Character.Inventory.AddNewToInventory((short)(4 + packet.Type * 14), type: InventoryType.Wear);
                        Session.Character.Inventory.AddNewToInventory((short)(81 + packet.Type * 13), type: InventoryType.Wear);
                        switch (packet.Type)
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
                        Session.CurrentMapInstanceNode?.Data.Broadcast(Session.Character.GenerateEq());
                        Session.SendPacket(Session.Character.GenerateEquipment());
                        Session.Character.ChangeClass((ClassType)packet.Type);
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                case 2:
                    Session.SendPacket("wopen 1 0");
                    break;
                case 4:
                    Mate mate = Session.Character.Mates.FirstOrDefault(s => s.MateTransportId == packet.NpcId);
                    switch (packet.Type)
                    {
                        case 2:
                            if (mate != null)
                            {

                                if (Session.Character.Level >= mate.Level)
                                {
                                    Mate teammate = Session.Character.Mates.Where(s => s.IsTeamMember).FirstOrDefault(s => s.MateType == mate.MateType);
                                    if (teammate != null)
                                    {
                                        teammate.IsTeamMember = false;
                                        teammate.MapX = teammate.PositionX;
                                        teammate.MapY = teammate.PositionY;
                                    }
                                    mate.IsTeamMember = true;

                                }
                                else
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                                }
                            }
                            break;
                        case 3:
                            if (mate != null && Session.Character.Miniland == Session.Character.MapInstanceNode)
                            {
                                mate.IsTeamMember = false;
                                mate.MapX = mate.PositionX;
                                mate.MapY = mate.PositionY;
                            }
                            break;
                        case 4:
                            if (mate != null)
                            {
                                if (Session.Character.Miniland == Session.Character.MapInstanceNode)
                                {
                                    mate.IsTeamMember = false;
                                    mate.MapX = mate.PositionX;
                                    mate.MapY = mate.PositionY;
                                }
                                else
                                {
                                    Session.SendPacket($"qna #n_run^4^5^3^{mate.MateTransportId} {Language.Instance.GetMessageFromKey("ASK_KICK_PET")}");
                                }
                                break;

                            }
                            break;
                        case 5:
                            if (mate != null)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(3000, 10, $"#n_run^4^6^3^{mate.MateTransportId}"));
                            }
                            break;
                        case 6:
                            if (mate != null)
                            {
                                if (Session.Character.Miniland != Session.Character.MapInstanceNode)
                                {
                                    mate.IsTeamMember = false;
                                    Session.CurrentMapInstanceNode.Data.Broadcast(mate.GenerateOut());
                                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PET_KICKED"), mate.Name), 11));
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PET_KICKED"), mate.Name), 0));
                                }
                            }
                            break;
                        case 7:
                            if (mate != null)
                            {
                                if (Session.Character.Mates.Any(s => s.MateType == mate.MateType && s.IsTeamMember))
                                {
                                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 11));
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 0));
                                }
                                else
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(3000, 10, $"#n_run^4^9^3^{mate.MateTransportId}"));
                                }
                            }
                            break;
                        case 9:
                            if (mate != null)
                            {
                                if (Session.Character.Level >= mate.Level)
                                    mate.PositionX = (short)(Session.Character.PositionX + 1);
                                mate.PositionY = (short)(Session.Character.PositionY + 1);
                                mate.IsTeamMember = true;
                                Session.CurrentMapInstanceNode.Data.Broadcast(mate.GenerateIn());

                            }
                            else
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                            }

                            break;
                    }
                    Session.SendPacket(Session.Character.GeneratePinit());
                    Session.Character.SendPst();
                    break;
                case 10:
                    Session.SendPacket("wopen 3 0");
                    break;

                case 12:
                    Session.SendPacket($"wopen {packet.Type} 0");
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
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                        if (tp != null)
                        {
                            if (Session.Character.Gold >= 1000 * packet.Type)
                            {
                                ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                                Session.Character.Gold -= 1000 * packet.Type;
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

                case 17:
                    double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                    double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
                    if (!(timeSpanSinceLastPortal >= 4) || !Session.HasCurrentMapInstanceNode)
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                        return;
                    }
                    if (Session.Character.Raid != null && Session.Character.Raid.Launched)
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay("Vous n'avez pas le droit d'aller à l'arène une fois en raid",
                                10));
                        break;
                    }
                    if (Session.Character.Gold >= 500 * (1 + packet.Type))
                    {
                        ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                        Session.Character.LastPortal = currentRunningSeconds;
                        Session.Character.Gold -= 500 * (1 + packet.Type);
                        Session.SendPacket(Session.Character.GenerateGold());
                        ServerManager.Instance.ChangeMapInstanceNode(Session.Character.CharacterId, packet.Type == 0 ? ServerManager.ArenaInstance.Data.MapInstanceNodeId : ServerManager.FamilyArenaInstance.Data.MapInstanceNodeId, packet.Type == 0 ? ServerManager.ArenaInstance.Data.Map.GetRandomPosition().X : ServerManager.FamilyArenaInstance.Data.Map.GetRandomPosition().X, packet.Type == 0 ? ServerManager.ArenaInstance.Data.Map.GetRandomPosition().Y : ServerManager.FamilyArenaInstance.Data.Map.GetRandomPosition().Y);
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                    }
                    break;

                case 18:
                    Session.SendPacket($"npc_req 1 {Session.Character.CharacterId} 17");
                    break;
                case 26:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                        if (tp != null)
                        {
                            if (Session.Character.Gold >= 5000 * packet.Type)
                            {
                                ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                                Session.Character.Gold -= 5000 * packet.Type;
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
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
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
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
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
                            if (Session.Character.Family.LandOfDeath != null && !Session.Character.Family.LandOfDeath.Data.Lock)
                            {
                                if (Session.Character.Level >= 55)
                                {
                                    ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                                    ServerManager.Instance.ChangeMapInstanceNode(Session.Character.CharacterId, Session.Character.Family.LandOfDeath.Data.MapInstanceNodeId, 153, 145);
                                }
                                else
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("LOD_REQUIERE_LVL"), 0));
                                }
                            }
                            else
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("LOD_CLOSED"), 0));
                            }
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NEED_FAMILY"), 0));
                        }
                    }
                    break;

                case 301:
                    if (npc != null)
                    {
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                        if (tp != null)
                        {
                            ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;

                case 1600:
                    Session.SendPacket(Session.Character.OpenFamilyWarehouse());
                    break;

                case 1601:
                    Session.SendPackets(Session.Character.OpenFamilyWarehouseHist());
                    break;

                case 1602:
                    if (Session.Character.Family != null && Session.Character.Family.FamilyLevel >= 3 && Session.Character.Family.WarehouseSize < 21)
                    {
                        if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (500000 >= Session.Character.Gold)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            Session.Character.Family.WarehouseSize = 21;
                            Session.Character.Gold -= 500000;
                            Session.SendPacket(Session.Character.GenerateGold());
                            FamilyDTO fam = Session.Character.Family;
                            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 1603:
                    if (Session.Character.Family != null && Session.Character.Family.FamilyLevel >= 7 && Session.Character.Family.WarehouseSize < 49)
                    {
                        if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (2000000 >= Session.Character.Gold)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            Session.Character.Family.WarehouseSize = 49;
                            Session.Character.Gold -= 2000000;
                            Session.SendPacket(Session.Character.GenerateGold());
                            FamilyDTO fam = Session.Character.Family;
                            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 1604:
                    if (Session.Character.Family != null && Session.Character.Family.FamilyLevel >= 5 && Session.Character.Family.MaxSize < 70)
                    {
                        if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (5000000 >= Session.Character.Gold)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            Session.Character.Family.MaxSize = 70;
                            Session.Character.Gold -= 5000000;
                            Session.SendPacket(Session.Character.GenerateGold());
                            FamilyDTO fam = Session.Character.Family;
                            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 1605:
                    if (Session.Character.Family != null && Session.Character.Family.FamilyLevel >= 9 && Session.Character.Family.MaxSize < 100)
                    {
                        if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (10000000 >= Session.Character.Gold)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            Session.Character.Family.MaxSize = 100;
                            Session.Character.Gold -= 10000000;
                            Session.SendPacket(Session.Character.GenerateGold());
                            FamilyDTO fam = Session.Character.Family;
                            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 23:
                    if (packet.Type == 0)
                    {
                        if (Session.Character.Group != null && Session.Character.Group.CharacterCount == 3)
                        {
                            foreach (ClientSession s in Session.Character.Group.Characters)
                            {
                                if (s.Character.Family != null)
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_MEMBER_ALREADY_IN_FAMILY")));
                                    return;
                                }
                            }
                        }
                        if (Session.Character.Group == null || Session.Character.Group.CharacterCount != 3)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_GROUP_NOT_FULL")));
                            return;
                        }
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInbox($"#glmk^ {14} 1 {Language.Instance.GetMessageFromKey("CREATE_FAMILY").Replace(' ', '^')}"));
                    }
                    else
                    {
                        if (Session.Character.Family == null)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_IN_FAMILY")));
                            return;
                        }
                        if (Session.Character.Family != null && Session.Character.FamilyCharacter != null && Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                            return;
                        }
                        Session.SendPacket($"qna #glrm^1 {Language.Instance.GetMessageFromKey("DISMISS_FAMILY")}");
                    }

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
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
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
                        TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                        if (tp != null)
                        {
                            ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_NRUN_HANDLER"), packet.Runner));
                    break;
            }
        }

        #endregion
    }
}