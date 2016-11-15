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
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class CommandPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public CommandPacketHandler(ClientSession session)
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

        /// <summary>
        /// $AddMonster Command
        /// </summary>
        /// <param name="addMonsterPacket"></param>
        public void AddMonster(AddMonsterPacket addMonsterPacket)
        {
            Logger.Debug("Add Monster Command", Session.SessionId);
            if (addMonsterPacket != null)
            {
                NpcMonster npcmonster = ServerManager.GetNpc(addMonsterPacket.MonsterVNum);
                if (npcmonster == null)
                {
                    return;
                }
                MapMonsterDTO monst = new MapMonsterDTO()
                {
                    MonsterVNum = addMonsterPacket.MonsterVNum,
                    MapY = Session.Character.MapY,
                    MapX = Session.Character.MapX,
                    MapId = Session.Character.MapId,
                    Position = (byte)Session.Character.Direction,
                    IsMoving = addMonsterPacket.IsMoving,
                    MapMonsterId = Session.CurrentMap.GetNextMonsterId()
                };
                MapMonster monster = null;

                // TODO Speed up with DoesMonsterExist
                if (DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) == null)
                {
                    DAOFactory.MapMonsterDAO.Insert(monst);
                    monster = DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) as MapMonster;
                    monster.Initialize(Session.CurrentMap);
                    Session.CurrentMap.AddMonster(monster);
                    Session.CurrentMap?.Broadcast(monster.GenerateIn3());
                }
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 10));
            }
        }

        [Packet("$ArenaWinner")]
        public void ArenaWinner(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.ArenaWinner = Session.Character.ArenaWinner == 0 ? 1 : 0;
            Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        [Packet("$Backpack")]
        public void BackPack(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.BackPack = Session.Character.BackPack == 0 ? 1 : 0;
            Session.SendPacket(Session.Character.GenerateExts());
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        [Packet("$Ban")]
        public void Ban(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int duration = 0;
            if (packetsplit.Length >= 4)
            {
                string name = packetsplit[2];
                string reason = String.Empty;
                bool isduration = Int32.TryParse(packetsplit[3], out duration);

                // check if duration can be parsed first
                duration = isduration ? duration : 0;

                // get data from 3rd or 4th packetsplit depending on if duration was parsed or not
                for (int i = isduration ? 4 : 3; i < packetsplit.Length; i++)
                {
                    reason += packetsplit[i] + " ";
                }
                reason = reason.Trim();

                ServerManager.Instance.Kick(packetsplit[2]);
                if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                {
                    DAOFactory.PenaltyLogDAO.Insert(new PenaltyLogDTO()
                    {
                        AccountId = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId,
                        Reason = reason,
                        Penalty = PenaltyType.Banned,
                        DateStart = DateTime.Now,
                        DateEnd = duration == 0 ? DateTime.Now.AddYears(15) : DateTime.Now.AddDays(duration),
                        AdminName = Session.Character.Name
                    });
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME TIME REASON ", 10));
                Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME REASON", 10));
            }
        }

        [Packet("$BlockPM")]
        public void BlockPM(string packet)
        {
            if (!Session.Character.GmPvtBlock)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_BLOCK_ENABLE"), 10));
                Session.Character.GmPvtBlock = true;
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_BLOCK_DISABLE"), 10));
                Session.Character.GmPvtBlock = false;
            }
        }

        /// <summary>
        /// $ChangeClass Command
        /// </summary>
        /// <param name="changeClassPacket"></param>
        public void ChangeClass(ChangeClassPacket changeClassPacket)
        {
            Logger.Debug("Change Class Command", Session.SessionId);
            if (changeClassPacket != null)
            {
                Session.Character.ChangeClass(changeClassPacket.ClassType);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 10));
            }
        }

        /// <summary>
        /// $ChangeDignity Command
        /// </summary>
        /// <param name="changeDignityPacket"></param>
        public void ChangeDignity(ChangeDignityPacket changeDignityPacket)
        {
            Logger.Debug("Change Dignity Command", Session.SessionId);
            if (changeDignityPacket != null)
            {
                if (changeDignityPacket.Dignity >= -1000 && changeDignityPacket.Dignity <= 100)
                {
                    Session.Character.Dignity = changeDignityPacket.Dignity;
                    Session.SendPacket(Session.Character.GenerateFd());
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("DIGNITY_CHANGED"), 12));
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_DIGNITY"), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$ChangeDignity DIGNITY", 10));
            }
        }

        /// <summary>
        /// $FLvl Command
        /// </summary>
        /// <param name="changeFairyLevelPacket"></param>
        public void ChangeFairyLevel(ChangeFairyLevelPacket changeFairyLevelPacket)
        {
            Logger.Debug("Change FairyLevel Command", Session.SessionId);
            WearableInstance fairy = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (changeFairyLevelPacket != null)
            {
                if (fairy != null)
                {
                    short fairylevel = changeFairyLevelPacket.FairyLevel;
                    fairylevel -= fairy.Item.ElementRate;
                    fairy.ElementRate = fairylevel;
                    fairy.XP = 0;
                    Session.SendPacket(Session.Character.GenerateMsg(String.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVEL_CHANGED"), fairy.Item.Name), 10));
                    Session.SendPacket(Session.Character.GeneratePairy());
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_FAIRY"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$FLvl FAIRYLEVEL", 10));
            }
        }

        [Packet("$ChangeSex")]
        public void ChangeGender(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.ChangeSex();
        }

        /// <summary>
        /// $HeroLvl Command
        /// </summary>
        /// <param name="changeHeroLevelPacket"></param>
        public void ChangeHeroLevel(ChangeHeroLevelPacket changeHeroLevelPacket)
        {
            Logger.Debug("Change HeroLevel Command", Session.SessionId);
            if (changeHeroLevelPacket != null)
            {
                if (changeHeroLevelPacket.HeroLevel < 51 && changeHeroLevelPacket.HeroLevel >= 0)
                {
                    Session.Character.HeroLevel = changeHeroLevelPacket.HeroLevel;
                    Session.Character.HeroXp = 0;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HEROLEVEL_CHANGED"), 0));
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPacket(Session.Character.GenerateStatInfo());
                    Session.SendPacket(Session.Character.GenerateStatChar());
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(6), Session.Character.MapX, Session.Character.MapY);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(198), Session.Character.MapX, Session.Character.MapY);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$HeroLvl HEROLEVEL", 10));
            }
        }

        /// <summary>
        /// $JLvl Command
        /// </summary>
        /// <param name="changeJobLevelPacket"></param>
        public void ChangeJobLevel(ChangeJobLevelPacket changeJobLevelPacket)
        {
            Logger.Debug("Change JobLevel Command", Session.SessionId);
            if (changeJobLevelPacket != null)
            {
                if (((Session.Character.Class == 0 && changeJobLevelPacket.JobLevel <= 20) || (Session.Character.Class != 0 && changeJobLevelPacket.JobLevel <= 80)) && changeJobLevelPacket.JobLevel > 0)
                {
                    Session.Character.JobLevel = changeJobLevelPacket.JobLevel;
                    Session.Character.JobLevelXp = 0;
                    Session.Character.Skills.ClearAll();
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOBLEVEL_CHANGED"), 0));
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(8), Session.Character.MapX, Session.Character.MapY);

                    Session.Character.Skills[(short)(200 + 20 * (byte)Session.Character.Class)] = new CharacterSkill { SkillVNum = (short)(200 + 20 * (byte)Session.Character.Class), CharacterId = Session.Character.CharacterId };
                    Session.Character.Skills[(short)(201 + 20 * (byte)Session.Character.Class)] = new CharacterSkill { SkillVNum = (short)(201 + 20 * (byte)Session.Character.Class), CharacterId = Session.Character.CharacterId };
                    Session.Character.Skills[236] = new CharacterSkill { SkillVNum = 236, CharacterId = Session.Character.CharacterId };

                    Session.SendPacket(Session.Character.GenerateSki());
                    Session.Character.LearnAdventurerSkill();
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$JLvl JOBLEVEL", 10));
            }
        }

        /// <summary>
        /// $Lvl Command
        /// </summary>
        /// <param name="changeLevelPacket"></param>
        public void ChangeLevel(ChangeLevelPacket changeLevelPacket)
        {
            Logger.Debug("Change Level Packet", Session.SessionId);
            if (changeLevelPacket != null)
            {
                if (changeLevelPacket.Level < 100 && changeLevelPacket.Level > 0)
                {
                    Session.Character.Level = changeLevelPacket.Level;
                    Session.Character.LevelXp = 0;
                    Session.Character.Hp = (int)Session.Character.HPLoad();
                    Session.Character.Mp = (int)Session.Character.MPLoad();
                    Session.SendPacket(Session.Character.GenerateStat());
                    Session.SendPacket(Session.Character.GenerateStatInfo());
                    Session.SendPacket(Session.Character.GenerateStatChar());
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("LEVEL_CHANGED"), 0));
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(6), Session.Character.MapX, Session.Character.MapY);
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(198), Session.Character.MapX, Session.Character.MapY);
                    ServerManager.Instance.UpdateGroup(Session.Character.CharacterId);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 10));
            }
        }

        /// <summary>
        /// $ChangeReput Command
        /// </summary>
        /// <param name="changeReputationPacket"></param>
        public void ChangeReputation(ChangeReputationPacket changeReputationPacket)
        {
            Logger.Debug("Change Reputation Command", Session.SessionId);
            if (changeReputationPacket != null)
            {
                if (changeReputationPacket.Reputation > 0)
                {
                    Session.Character.Reput = changeReputationPacket.Reputation;
                    Session.SendPacket(Session.Character.GenerateFd());
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("REP_CHANGED"), 0));
                    Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$ChangeReput AMOUNT", 10));
            }
        }

        /// <summary>
        /// $SPLvl Command
        /// </summary>
        /// <param name="changeSpecialistLevelPacket"></param>
        public void ChangeSpecialistLevel(ChangeSpecialistLevelPacket changeSpecialistLevelPacket)
        {
            Logger.Debug("Change SpecialistLevel Command", Session.SessionId);
            SpecialistInstance sp = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);

            if (changeSpecialistLevelPacket != null)
            {
                if (sp != null && Session.Character.UseSp)
                {
                    if (changeSpecialistLevelPacket.SpecialistLevel <= 99 && changeSpecialistLevelPacket.SpecialistLevel > 0)
                    {
                        sp.SpLevel = changeSpecialistLevelPacket.SpecialistLevel;
                        sp.XP = 0;
                        Session.SendPacket(Session.Character.GenerateLev());
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SPLEVEL_CHANGED"), 0));
                        Session.SendPacket(Session.Character.GenerateSki());
                        Session.Character.LearnSPSkill();
                        Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(8), Session.Character.MapX, Session.Character.MapY);
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 10));
            }
        }

        [Packet("$Help")]
        public void Command(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.SendPacket(Session.Character.GenerateSay("-------------Commands Info-------------", 11));
            Session.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ArenaWinner", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Backpack", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME REASON", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME TIME REASON ", 12));
            Session.SendPacket(Session.Character.GenerateSay("$BlockPM", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeDignity AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeReput AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeSex", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Effect EFFECTID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$FLvl FAIRYLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Gift USERNAME(*) VNUM AMOUNT RARE UPGRADE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Gift VNUM AMOUNT RARE UPGRADE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$GodMode", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Gold AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Guri TYPE ARGUMENT VALUE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$HairColor COLORID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$HairStyle STYLEID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$HeroLvl HEROLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Invisible", 12));
            Session.SendPacket(Session.Character.GenerateSay("$JLvl JOBLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Kill CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Lvl LEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$MapDance", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME REASON", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME TIME REASON ", 12));
            Session.SendPacket(Session.Character.GenerateSay("$PlayMusic MUSIC", 12));
            Session.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY PORTALTYPE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Position", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RateDrop RATE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RateFairyXp RATE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RateGold RATE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RateXp RATE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RemoveMob", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RemovePortal", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SPRefill", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SearchItem NAME(%)", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SearchMonster NAME(%)", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Shout MESSAGE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Shutdown", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SkillAdd SKILLID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Stat", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Teleport Map X Y", 12));
            Session.SendPacket(Session.Character.GenerateSay("$TeleportToMe CHARACTERNAME(*)", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Unban CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Unmute CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 12));
            Session.SendPacket(Session.Character.GenerateSay("$WigColor COLORID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Zoom VALUE", 12));
            Session.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 11));
        }

        [Packet("$CreateItem")]
        public void CreateItem(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte amount = 1, upgrade = 0, design = 0;
            sbyte rare = 0;
            short vnum;
            Item iteminfo = null;
            if (packetsplit.Length != 5 && packetsplit.Length != 4 && packetsplit.Length != 3)
            {
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 10));
                Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 10));
            }
            else if (short.TryParse(packetsplit[2], out vnum))
            {
                if (vnum == 1046)
                {
                    return; // cannot create gold as item, use $Gold instead
                }

                iteminfo = ServerManager.GetItem(vnum);
                if (iteminfo != null)
                {
                    if (iteminfo.IsColored)
                    {
                        if (packetsplit.Count() > 3)
                        {
                            byte.TryParse(packetsplit[3], out design);
                        }
                    }
                    else if (iteminfo.Type == 0)
                    {
                        if (packetsplit.Length == 4)
                        {
                            sbyte.TryParse(packetsplit[3], out rare);
                        }
                        else if (packetsplit.Length == 5)
                        {
                            if (iteminfo.EquipmentSlot == EquipmentType.Sp)
                            {
                                byte.TryParse(packetsplit[3], out upgrade);
                                upgrade = upgrade > 15 ? (byte)15 : upgrade;
                                byte.TryParse(packetsplit[4], out design);
                            }
                            else
                            {
                                sbyte.TryParse(packetsplit[3], out rare);
                                byte.TryParse(packetsplit[4], out upgrade);
                                upgrade = upgrade > 10 ? (byte)10 : upgrade;
                                if (upgrade == 0)
                                {
                                    if (iteminfo.BasicUpgrade != 0)
                                    {
                                        upgrade = iteminfo.BasicUpgrade;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (packetsplit.Length > 3 && !byte.TryParse(packetsplit[3], out amount))
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                            return;
                        }
                    }
                    amount = amount > 99 ? (byte)99 : amount;
                    ItemInstance inv = Session.Character.Inventory.AddNewToInventory(vnum, amount);
                    if (inv != null)
                    {
                        inv.Rare = rare;
                        inv.Upgrade = upgrade;
                        inv.Design = design;

                        WearableInstance wearable = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(inv.Slot, inv.Type);

                        if (wearable != null && (wearable.Item.EquipmentSlot == EquipmentType.Armor || wearable.Item.EquipmentSlot == EquipmentType.MainWeapon || wearable.Item.EquipmentSlot == EquipmentType.SecondaryWeapon))
                        {
                            wearable.SetRarityPoint();
                        }

                        short Slot = inv.Slot;
                        if (Slot != -1)
                        {
                            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {amount}", 12));
                            Session.SendPacket(Session.Character.GenerateInventoryAdd(vnum, inv.Amount, iteminfo.Type, Slot, rare, design, upgrade, 0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                    }
                }
                else
                {
                    Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_ITEM"), 0);
                }
            }
        }

        /// <summary>
        /// $PortalTo Command
        /// </summary>
        /// <param name="packet"></param>
        public void CreatePortal(PortalToPacket portalToPacket)
        {
            Logger.Debug("PortalTo Command", Session.SessionId);
            if (portalToPacket != null)
            {
                if (ServerManager.GetMap(portalToPacket.DestinationMapId) == null)
                {
                    return;
                }
                short mapId = Session.Character.MapId;
                short mapX = Session.Character.MapX;
                short mapY = Session.Character.MapY;
                PortalDTO portal = new PortalDTO()
                {
                    SourceMapId = mapId,
                    SourceX = mapX,
                    SourceY = mapY,
                    DestinationMapId = portalToPacket.DestinationMapId,
                    DestinationX = portalToPacket.DestinationX,
                    DestinationY = portalToPacket.DestinationY,
                    Type = (short)portalToPacket.PortalType
                };
                Session.CurrentMap.Portals.Add(portal);
                Session.CurrentMap?.Broadcast(Session.Character.GenerateGp(portal));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY PORTALTYPE", 10));
                Session.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY", 10));
            }
        }

        /// <summary>
        /// $Demote Command
        /// </summary>
        /// <param name="demotePacket"></param>
        public void Demote(DemotePacket demotePacket)
        {
            Logger.Debug("Demote Command", Session.SessionId);
            if (demotePacket != null)
            {
                long accountId = DAOFactory.CharacterDAO.LoadByName(demotePacket.CharacterName).AccountId;
                AccountDTO account = DAOFactory.AccountDAO.LoadById(accountId);
                ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == demotePacket.CharacterName);
                if (account != null)
                {
                    account.Authority = AuthorityType.User;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
                if (session != null)
                {
                    session.Account.Authority = AuthorityType.User;
                    session.Character.Authority = AuthorityType.User;
                    ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Demote CHARACTERNAME", 10));
            }
        }

        /// <summary>
        /// $Effect Command
        /// </summary>
        /// <param name="effectCommandpacket"></param>
        public void Effect(EffectCommandPacket effectCommandpacket)
        {
            Logger.Debug("Effect Command", Session.SessionId);
            if (effectCommandpacket != null)
            {
                Session.CurrentMap?.Broadcast(Session.Character.GenerateEff(effectCommandpacket.EffectId), Session.Character.MapX, Session.Character.MapY);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Effect EFFECT", 10));
            }
        }

        [Packet("$Gift")]
        public void Gift(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte amount;
            short vnum = -1;
            sbyte rare = 0;
            byte upgrade = 0;
            if (packetsplit.Length > 5)
            {
                if (packetsplit.Length == 6)
                {
                    if (!(byte.TryParse(packetsplit[3], out amount) && short.TryParse(packetsplit[2], out vnum) && sbyte.TryParse(packetsplit[4], out rare) && byte.TryParse(packetsplit[5], out upgrade)))
                    {
                        return;
                    }
                    Session.Character.SendGift(Session.Character.CharacterId, vnum, amount, rare, upgrade, false);
                }
                else if (packetsplit.Length == 7)
                {
                    string name = packetsplit[2];
                    if (!(byte.TryParse(packetsplit[4], out amount) && short.TryParse(packetsplit[3], out vnum) && sbyte.TryParse(packetsplit[5], out rare) && byte.TryParse(packetsplit[6], out upgrade)))
                    {
                        return;
                    }
                    if (name == "*")
                    {
                        foreach (ClientSession session in Session.CurrentMap.Sessions)
                        {
                            Session.Character.SendGift((session.Character.CharacterId), vnum, amount, rare, upgrade, false);
                        }
                    }
                    else
                    {
                        CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);

                        if (chara != null)
                        {
                            Session.Character.SendGift((chara.CharacterId), vnum, amount, rare, upgrade, false);
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                            return;
                        }
                    }
                }
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENDED"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Gift USERNAME VNUM AMOUNT RARE UPGRADE", 10));
            }
        }

        [Packet("$GodMode")]
        public void GodMode(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.HasGodMode = Session.Character.HasGodMode ? false : true;
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        /// $Gold Command
        /// </summary>
        /// <param name="goldPacket"></param>
        public void Gold(GoldPacket goldPacket)
        {
            Logger.Debug("Gold Command", Session.SessionId);
            if (goldPacket != null)
            {
                long gold = goldPacket.Amount;
                gold = gold > 1000000000 ? 1000000000 : gold;
                if (gold >= 0)
                {
                    Session.Character.Gold = gold;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_SET"), 0));
                    Session.SendPacket(Session.Character.GenerateGold());
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Gold AMOUNT", 10));
            }
        }

        /// <summary>
        /// $HairColor Command
        /// </summary>
        /// <param name="hairColorPacket"></param>
        public void Haircolor(HairColorPacket hairColorPacket)
        {
            Logger.Debug("Hair Color Command", Session.SessionId);
            if (hairColorPacket != null)
            {
                Session.Character.HairColor = hairColorPacket.HairColor;
                Session.SendPacket(Session.Character.GenerateEq());
                Session.CurrentMap?.Broadcast(Session.Character.GenerateIn());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$HairColor COLORID", 10));
            }
        }

        /// <summary>
        /// $HairStyle Command
        /// </summary>
        /// <param name="hairStylePacket"></param>
        public void Hairstyle(HairStylePacket hairStylePacket)
        {
            Logger.Debug("Hair Style Command", Session.SessionId);
            if (hairStylePacket != null)
            {
                Session.Character.HairStyle = hairStylePacket.HairStyle;
                Session.SendPacket(Session.Character.GenerateEq());
                Session.CurrentMap?.Broadcast(Session.Character.GenerateIn());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$HairStyle STYLEID", 10));
            }
        }

        [Packet("$Invisible")]
        public void Invisible(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.Invisible = Session.Character.Invisible ? false : true;
            Session.CurrentMap?.Broadcast(Session.Character.GenerateInvisible());
            Session.Character.InvisibleGm = Session.Character.InvisibleGm ? false : true;
            Session.SendPacket(Session.Character.GenerateEq());
            if (Session.Character.InvisibleGm)
            {
                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllExceptMe);
            }
            else
            {
                Session.CurrentMap?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
            }
        }

        /// <summary>
        /// $Kick Command
        /// </summary>
        /// <param name="kickPacket"></param>
        public void Kick(KickPacket kickPacket)
        {
            Logger.Debug("Kick Command", Session.SessionId);
            if (kickPacket != null)
            {
                ServerManager.Instance.Kick(kickPacket.CharacterName);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Kick CHARACTERNAME", 10));
            }
        }

        /// <summary>
        /// $Kill Command
        /// </summary>
        /// <param name="killPacket"></param>
        public void Kill(KillPacket killPacket)
        {
            Logger.Debug("Kill Command", Session.SessionId);
            if (killPacket != null)
            {
                string name = killPacket.CharacterName;

                long? id = ServerManager.Instance.GetProperty<long?>(name, nameof(Character.CharacterId));

                if (id != null)
                {
                    bool hasGodMode = ServerManager.Instance.GetProperty<bool>(name, nameof(Character.HasGodMode));
                    if (hasGodMode)
                    {
                        return;
                    }
                    int? Hp = ServerManager.Instance.GetProperty<int?>((long)id, nameof(Character.Hp));
                    if (Hp == 0)
                    {
                        return;
                    }
                    ServerManager.Instance.SetProperty((long)id, nameof(Character.Hp), 0);
                    ServerManager.Instance.SetProperty((long)id, nameof(Character.LastDefence), DateTime.Now);
                    Session.CurrentMap?.Broadcast($"su 1 {Session.Character.CharacterId} 1 {id} 1114 4 11 4260 0 0 0 0 60000 3 0");
                    Session.CurrentMap?.Broadcast(null, ServerManager.Instance.GetUserMethod<string>((long)id, nameof(Character.GenerateStat)), ReceiverType.OnlySomeone, String.Empty, (long)id);
                    ServerManager.Instance.AskRevive((long)id);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Kill CHARACTERNAME", 10));
            }
        }

        [Packet("$MapDance")]
        public void MapDance(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.CurrentMap.IsDancing = Session.CurrentMap.IsDancing ? false : true;
            if (Session.CurrentMap.IsDancing)
            {
                Session.Character.Dance();
                Session.CurrentMap?.Broadcast("dance 2");
            }
            else
            {
                Session.Character.Dance();
                Session.CurrentMap?.Broadcast("dance");
            }
        }

        [Packet("$Morph")]
        public void Morph(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
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
                        if (arg[0] < 30 && arg[0] > 0)
                        {
                            Session.Character.UseSp = true;
                            Session.Character.Morph = arg[0];
                            Session.Character.MorphUpgrade = arg[1];
                            Session.Character.MorphUpgrade2 = arg[2];
                            Session.Character.ArenaWinner = arg[3];
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                        }
                        else if (arg[0] > 30)
                        {
                            Session.Character.IsVehicled = true;
                            Session.Character.Morph = arg[0];
                            Session.Character.ArenaWinner = arg[3];
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                        }
                        else
                        {
                            Session.Character.IsVehicled = false;
                            Session.Character.UseSp = false;
                            Session.Character.ArenaWinner = 0;
                            Session.SendPacket(Session.Character.GenerateCond());
                            Session.SendPacket(Session.Character.GenerateLev());
                            Session.CurrentMap?.Broadcast(Session.Character.GenerateCMode());
                        }
                    }
                    break;

                default:
                    Session.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 10));
                    break;
            }
        }

        [Packet("$Mute")]
        public void Mute(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int duration = 1;
            if (packetsplit.Length > 3)
            {
                string name = packetsplit[2];
                string reason = String.Empty;
                bool isduration = Int32.TryParse(packetsplit[3], out duration);

                duration = isduration ? duration : 1;

                // get data from 3rd or 4th packetsplit depending on if duration was parsed or not
                for (int i = isduration ? 4 : 3; i < packetsplit.Length; i++)
                {
                    reason += packetsplit[i] + " ";
                }
                reason = reason.Trim();

                ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);
                if (duration != 0)
                {
                    if (session != null)
                    {
                        session.Account.PenaltyLogs.Add(new PenaltyLogDTO()
                        {
                            AccountId = session.Account.AccountId,
                            Reason = reason,
                            Penalty = PenaltyType.Muted,
                            DateStart = DateTime.Now,
                            DateEnd = DateTime.Now.AddHours(duration),
                            AdminName = Session.Character.Name
                        });
                        if (duration == 1)
                        {
                            session.SendPacket(Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), reason)));
                        }
                        else
                        {
                            session.SendPacket(Session.Character.GenerateInfo(String.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), reason, duration)));
                        }
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                    }
                    else if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                    {
                        DAOFactory.PenaltyLogDAO.Insert(new PenaltyLogDTO()
                        {
                            AccountId = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId,
                            Reason = reason,
                            Penalty = PenaltyType.Muted,
                            DateStart = DateTime.Now,
                            DateEnd = DateTime.Now.AddHours(duration),
                            AdminName = Session.Character.Name
                        });
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME TIME REASON ", 10));
                Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME REASON", 10));
            }
        }

        [Packet("$PlayMusic")]
        public void PlayMusic(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                if (packetsplit.Length <= 1)
                {
                    return;
                }
                short arg;
                short.TryParse(packetsplit[2], out arg);
                if (arg > -1)
                {
                    Session.CurrentMap?.Broadcast($"bgm {arg}");
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$PlayMusic BGMUSIC", 10));
            }
        }

        [Packet("$Position")]
        public void Position(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.SendPacket(Session.Character.GenerateSay($"Map:{Session.Character.MapId} - X:{Session.Character.MapX} - Y:{Session.Character.MapY} - Dir:{Session.Character.Direction}", 12));
        }

        [Packet("$Promote")]
        public void Promote(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                string name = packetsplit[2];
                long accountId = DAOFactory.CharacterDAO.LoadByName(name).AccountId;
                AccountDTO account = DAOFactory.AccountDAO.LoadById(accountId);
                ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);
                if (account != null)
                {
                    account.Authority = AuthorityType.Admin;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
                if (session != null)
                {
                    session.Account.Authority = AuthorityType.Admin;
                    session.Character.Authority = AuthorityType.Admin;
                    ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Promote CHARACTERNAME", 10));
            }
        }

        [Packet("$Rarify")]
        public void Rarify(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 10));
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
                    WearableInstance wearableInstance = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(itemslot, 0);
                    if (wearableInstance != null)
                    {
                        wearableInstance.RarifyItem(Session, (RarifyMode)mode, (RarifyProtection)protection);
                    }
                }
            }
        }

        [Packet("$RateDrop")]
        public void RateDrop(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int rate;
            if (packetsplit.Length > 2)
            {
                if (int.TryParse(packetsplit[2], out rate) && rate <= 1000)
                {
                    ServerManager.DropRate = rate;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("DROP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$RateDrop RATE", 10));
            }
        }

        [Packet("$RateFairyXp")]
        public void RateFairyXp(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int rate;
            if (packetsplit.Length > 2)
            {
                if (int.TryParse(packetsplit[2], out rate) && rate <= 1000)
                {
                    ServerManager.FairyXpRate = rate;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("FAIRYXP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$RateFairyXp RATE", 10));
            }
        }

        [Packet("$RateGold")]
        public void RateGold(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int rate;
            if (packetsplit.Length > 2)
            {
                if (int.TryParse(packetsplit[2], out rate) && rate <= 1000)
                {
                    ServerManager.GoldRate = rate;

                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$RateGold RATE", 10));
            }
        }

        [Packet("$RateXp")]
        public void RateXp(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            int rate;
            if (packetsplit.Length > 2)
            {
                if (int.TryParse(packetsplit[2], out rate) && rate <= 1000)
                {
                    ServerManager.XPRate = rate;

                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("XP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$RateXp RATE", 10));
            }
        }

        [Packet("$RemoveMob")]
        public void RemoveMob(string packet)
        {
            MapMonster monst = Session.CurrentMap.GetMonster(Session.Character.LastMonsterId);
            if (monst != null)
            {
                if (monst.Alive)
                {
                    Session.CurrentMap.Broadcast($"su 1 {Session.Character.CharacterId} 3 {monst.MapMonsterId} 1114 4 11 4260 0 0 0 0 {6000} 3 0");
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MONSTER_REMOVED"), monst.MapMonsterId, monst.Monster.Name, monst.MapId, monst.MapX, monst.MapY), 12));
                    Session.CurrentMap.RemoveMonster(monst);
                    if (DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) != null)
                    {
                        DAOFactory.MapMonsterDAO.DeleteById(monst.MapMonsterId);
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("MONSTER_NOT_ALIVE")), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MONSTER_NOT_FOUND"), 11));
            }
        }

        [Packet("$RemovePortal")]
        public void RemoveNearestPortal(string packet)
        {
            PortalDTO pt = Session.CurrentMap.Portals.FirstOrDefault(s => s.SourceMapId == Session.Character.MapId && Map.GetDistance(new MapCell { MapId = s.SourceMapId, X = s.SourceX, Y = s.SourceY }, new MapCell { MapId = Session.Character.MapId, X = Session.Character.MapX, Y = Session.Character.MapY }) < 10);
            if (pt != null)
            {
                Session.SendPacket(Session.Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("NEAREST_PORTAL"), pt.SourceMapId, pt.SourceX, pt.SourceY), 12));
                Session.CurrentMap.Portals.Remove(pt);
                Session.CurrentMap?.Broadcast(Session.Character.GenerateGp(pt));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_PORTAL_FOUND"), 11));
            }
        }

        [Packet("$Resize")]
        public void Resize(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short arg = -1;

            if (packetsplit.Length > 2)
            {
                short.TryParse(packetsplit[2], out arg);

                if (arg > -1)
                {
                    Session.Character.Size = arg;
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateScal());
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 10));
            }
        }

        [Packet("$SearchItem")]
        public void SearchItem(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                string name = String.Empty;
                for (int i = 2; i < packetsplit.Length; i++)
                {
                    name += packetsplit[i] + " ";
                }
                name = name.Trim();
                IEnumerable<ItemDTO> itemlist = DAOFactory.ItemDAO.FindByName(name).OrderBy(s => s.VNum).ToList();
                if (itemlist.Any())
                {
                    foreach (ItemDTO item in itemlist)
                    {
                        Session.SendPacket(Session.Character.GenerateSay($"Item: {item.Name} VNum: {item.VNum}", 12));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_NOT_FOUND"), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$SearchItem NAME", 10));
            }
        }

        [Packet("$SearchMonster")]
        public void SearchMonster(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 3)
            {
                IEnumerable<NpcMonsterDTO> monsterlist = DAOFactory.NpcMonsterDAO.FindByName(packetsplit[2]).OrderBy(s => s.NpcMonsterVNum).ToList();
                if (monsterlist.Any())
                {
                    foreach (NpcMonsterDTO NpcMonster in monsterlist)
                    {
                        Session.SendPacket(Session.Character.GenerateSay($"Monster: {NpcMonster.Name} VNum: {NpcMonster.NpcMonsterVNum}", 12));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MONSTER_NOT_FOUND"), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$SearchMonster NAME", 10));
            }
        }

        [Packet("$Shout")]
        public void Shout(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            string message = String.Empty;
            if (packetsplit.Length > 2)
            {
                for (int i = 2; i < packetsplit.Length; i++)
                {
                    message += packetsplit[i] + " ";
                }
            }
            message.Trim();
            ServerManager.Instance.Shout(message);
        }

        [Packet("$Shutdown")]
        public void Shutdown(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            if (ServerManager.Instance.TaskShutdown != null)
            {
                ServerManager.Instance.ShutdownStop = true;
                ServerManager.Instance.TaskShutdown = null;
            }
            else
            {
                ServerManager.Instance.TaskShutdown = new Task(ShutdownTask);
                ServerManager.Instance.TaskShutdown.Start();
            }
        }

        [Packet("$SkillAdd")]
        public void SkillAdd(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short vnum = 0;
            if (packetsplit.Length > 2 && short.TryParse(packetsplit[2], out vnum))
            {
                Skill skillinfo = ServerManager.GetSkill(vnum);
                if (skillinfo == null)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_DOES_NOT_EXIST"), 11));
                    return;
                }

                if (skillinfo.SkillVNum < 200)
                {
                    foreach (var skill in Session.Character.Skills.GetAllItems())
                    {
                        if ((skillinfo.CastId == skill.Skill.CastId) && (skill.Skill.SkillVNum < 200))
                        {
                            Session.Character.Skills.Remove(skill.SkillVNum);
                        }
                    }
                }
                else
                {
                    if (Session.Character.Skills.ContainsKey(vnum))
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_ALREADY_EXIST"), 11));
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

                Session.Character.Skills[vnum] = new CharacterSkill() { SkillVNum = vnum, CharacterId = Session.Character.CharacterId };

                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                Session.SendPacket(Session.Character.GenerateLev());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
            }
        }

        [Packet("$Speed")]
        public void Speed(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
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
                        Session.Character.IsCustomSpeed = true;
                        Session.SendPacket(Session.Character.GenerateCond());
                    }
                    break;

                default:
                    Session.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 10));
                    break;
            }
        }

        [Packet("$SPRefill")]
        public void SPRefill(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.SpPoint = 10000;
            Session.Character.SpAdditionPoint = 1000000;
            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SP_REFILL"), 0));
            Session.SendPacket(Session.Character.GenerateSpPoint());
        }

        [Packet("$Stat")]
        public void Stat(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("TOTAL_SESSION")}: {ServerManager.Instance.SessionCount} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("XP_RATE_NOW")}: {ServerManager.XPRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("DROP_RATE_NOW")}: {ServerManager.DropRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("GOLD_RATE_NOW")}: {ServerManager.GoldRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("FAIRYXP_RATE_NOW")}: {ServerManager.FairyXpRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("SERVER_WORKING_TIME")}: {(Process.GetCurrentProcess().StartTime - DateTime.Now).ToString("d\\ hh\\:mm\\:ss")} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("MEMORY")}: {(GC.GetTotalMemory(true) / (1024 * 1024))}MB ", 13));
        }

        [Packet("$Summon")]
        public void Summon(string packet)
        {
            if (Session.IsOnMap)
            {
                Map currentMap = Session.CurrentMap;
                Random random = new Random();
                Logger.Debug(packet, Session.SessionId);
                string[] packetsplit = packet.Split(' ');
                short vnum = 0;
                byte qty = 1, move = 0;
                if (packetsplit.Length == 5 && short.TryParse(packetsplit[2], out vnum) && byte.TryParse(packetsplit[3], out qty) && byte.TryParse(packetsplit[4], out move))
                {
                    NpcMonster npcmonster = ServerManager.GetNpc(vnum);
                    if (npcmonster == null)
                    {
                        return;
                    }
                    Map map = Session.CurrentMap;
                    for (int i = 0; i < qty; i++)
                    {
                        short mapx;
                        short mapy;
                        List<MapCell> Possibilities = new List<MapCell>();
                        for (short x = -4; x < 5; x++)
                        {
                            for (short y = -4; y < 5; y++)
                            {
                                Possibilities.Add(new MapCell() { X = x, Y = y });
                            }
                        }
                        foreach (MapCell possibilitie in Possibilities.OrderBy(s => random.Next()))
                        {
                            mapx = (short)(Session.Character.MapX + possibilitie.X);
                            mapy = (short)(Session.Character.MapY + possibilitie.Y);
                            if (!Session.CurrentMap?.IsBlockedZone(mapx, mapy) ?? false)
                            {
                                break;
                            }
                        }

                        MapMonster monster = new MapMonster() { MonsterVNum = vnum, MapY = Session.Character.MapY, MapX = Session.Character.MapX, MapId = Session.Character.MapId, Position = (byte)Session.Character.Direction, IsMoving = move == 1 ? true : false, MapMonsterId = Session.CurrentMap.GetNextMonsterId(), Respawn = false };
                        monster.Initialize(currentMap);
                        currentMap?.AddMonster(monster);
                        currentMap?.Broadcast(monster.GenerateIn3());
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 10));
                }
            }
        }

        [Packet("$Teleport")]
        public void Teleport(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            short[] arg = new short[3];
            bool verify = false;

            if (Session.Character.HasShopOpened || Session.Character.InExchangeOrTrade)
            {
                Session.Character.Dispose();
            }

            if (packetsplit.Length > 4)
            {
                verify = (short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && DAOFactory.MapDAO.LoadById(arg[0]) != null);
            }
            switch (packetsplit.Length)
            {
                case 3:
                    string name = packetsplit[2];
                    short? mapId = ServerManager.Instance.GetProperty<short?>(name, nameof(Character.MapId));
                    short? mapx = ServerManager.Instance.GetProperty<short?>(name, nameof(Character.MapX));
                    short? mapy = ServerManager.Instance.GetProperty<short?>(name, nameof(Character.MapY));
                    if (mapy != null && mapx != null && mapId != null)
                    {
                        ServerManager.Instance.MapOut(Session.Character.CharacterId);
                        Session.Character.MapId = (short)mapId;
                        Session.Character.MapX = (short)((short)(mapx) + 1);
                        Session.Character.MapY = (short)((short)(mapy) + 1);
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, (short)mapId, (short)((short)(mapx) + 1), (short)((short)(mapy) + 1));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                    break;

                case 5:
                    if (verify)
                    {
                        ServerManager.Instance.MapOut(Session.Character.CharacterId);
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, arg[0], arg[1], arg[2]);
                    }
                    break;

                default:
                    Session.SendPacket(Session.Character.GenerateSay("$Teleport MAP X Y", 10));
                    Session.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 10));
                    break;
            }
        }

        [Packet("$TeleportToMe")]
        public void TeleportToMe(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            Random random = new Random();
            if (packetsplit.Length == 3)
            {
                string name = packetsplit[2];

                if (name == "*")
                {
                    foreach (ClientSession session in ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.CharacterId != Session.Character.CharacterId))
                    {
                        // clear any shop or trade on target character
                        session.Character.Dispose();

                        ServerManager.Instance.MapOut(session.Character.CharacterId);

                        List<MapCell> possibilities = new List<MapCell>();
                        for (short x = -6; x < 6; x++)
                        {
                            for (short y = -6; y < 6; y++)
                            {
                                possibilities.Add(new MapCell() { X = x, Y = y });
                            }
                        }

                        short mapXPossibility = Session.Character.MapX;
                        short mapYPossibility = Session.Character.MapY;
                        foreach (MapCell possibility in possibilities.OrderBy(s => random.Next()))
                        {
                            mapXPossibility = (short)(Session.Character.MapX + possibility.X);
                            mapYPossibility = (short)(Session.Character.MapY + possibility.Y);
                            if (!Session.CurrentMap.IsBlockedZone(mapXPossibility, mapYPossibility))
                            {
                                break;
                            }
                        }
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, Session.Character.MapId, mapXPossibility, mapYPossibility);
                    }
                }
                else
                {
                    ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterName(name);

                    if (targetSession != null)
                    {
                        // clear any shop or trade on target character
                        targetSession.Character.Dispose();

                        ServerManager.Instance.MapOut(targetSession.Character.CharacterId);
                        targetSession.Character.IsSitting = false;
                        ServerManager.Instance.ChangeMap(targetSession.Character.CharacterId, Session.Character.MapId, (short)((Session.Character.MapX) + (short)1), (short)((Session.Character.MapY) + (short)1));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$TeleportToMe CHARACTERNAME", 10));
            }
        }

        [Packet("$Guri")]
        public void TestGuri(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte type = 0, argument = 0;
            short value = 0;
            if (packetsplit.Length > 3)
            {
                if (byte.TryParse(packetsplit[2], out type) && byte.TryParse(packetsplit[3], out argument) && short.TryParse(packetsplit[4], out value))
                {
                    Session.SendPacket(Session.Character.GenerateGuri(type, argument, value));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Guri TYPE ARGUMENT VALUE", 10));
            }
        }

        [Packet("$Unban")]
        public void Unban(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                string name = packetsplit[2];
                if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                {
                    if (DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId).Any(s => s.Penalty == PenaltyType.Banned && s.DateEnd > DateTime.Now))
                    {
                        PenaltyLogDTO log = DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId).FirstOrDefault(s => s.Penalty == PenaltyType.Banned && s.DateEnd > DateTime.Now);
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        DAOFactory.PenaltyLogDAO.Update(log);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_BANNED"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Unban CHARACTERNAME", 10));
            }
        }

        [Packet("$Unmute")]
        public void Unmute(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 2)
            {
                string name = packetsplit[2];
                ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);

                if (session != null)
                {
                    if (session.Account.PenaltyLogs.Where(s => s.AccountId == session.Account.AccountId && s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now).Any())
                    {
                        PenaltyLogDTO log = session.Account.PenaltyLogs.Where(s => s.AccountId == session.Account.AccountId && s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now).FirstOrDefault();
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_MUTED"), 10));
                    }
                }
                else if (DAOFactory.CharacterDAO.LoadByName(packetsplit[2]) != null)
                {
                    if (DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId).Any(s => s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now))
                    {
                        PenaltyLogDTO log = DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId).FirstOrDefault(s => s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now);
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        DAOFactory.PenaltyLogDAO.Update(log);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_MUTED"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Unmute CHARACTERNAME", 10));
            }
        }

        [Packet("$Upgrade")]
        public void Upgrade(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 5)
            {
                Session.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 10));
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
                    WearableInstance wearableInstance = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(itemslot, 0);
                    if (wearableInstance != null)
                    {
                        wearableInstance.UpgradeItem(Session, (UpgradeMode)mode, (UpgradeProtection)protection, true);
                    }
                }
            }
        }

        [Packet("$WigColor")]
        public void WigColor(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte wigcolor = 0;
            if (packetsplit.Length > 2)
            {
                if (Byte.TryParse(packetsplit[2], out wigcolor))
                {
                    WearableInstance wig = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);
                    if (wig != null)
                    {
                        wig.Design = wigcolor;
                        Session.SendPacket(Session.Character.GenerateEq());
                        Session.SendPacket(Session.Character.GenerateEquipment());
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateIn());
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                        return;
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$WigColor COLORID", 10));
            }
        }

        [Packet("$Zoom")]
        public void Zoom(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte arg = 0;
            if (packetsplit.Length > 2 && byte.TryParse(packetsplit[2], out arg))
            {
                Session.SendPacket(Session.Character.GenerateGuri(15, arg));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Zoom VALUE", 10));
            }
        }

        private async void ShutdownTask()
        {
            string message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 5);
            ServerManager.Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            ServerManager.Instance.Broadcast(Session.Character.GenerateMsg(message, 2));
            for (int i = 0; i < 60 * 4; i++)
            {
                await Task.Delay(1000);
                if (ServerManager.Instance.ShutdownStop)
                {
                    ServerManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 1);
            ServerManager.Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            ServerManager.Instance.Broadcast(Session.Character.GenerateMsg(message, 2));
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (ServerManager.Instance.ShutdownStop)
                {
                    ServerManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 30);
            ServerManager.Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            ServerManager.Instance.Broadcast(Session.Character.GenerateMsg(message, 2));
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (ServerManager.Instance.ShutdownStop)
                {
                    ServerManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            message = String.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 10);
            ServerManager.Instance.Broadcast($"say 1 0 10 ({Language.Instance.GetMessageFromKey("ADMINISTRATOR")}){message}");
            ServerManager.Instance.Broadcast(Session.Character.GenerateMsg(message, 2));
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                if (ServerManager.Instance.ShutdownStop)
                {
                    ServerManager.Instance.ShutdownStop = false;
                    return;
                }
            }
            ServerManager.Instance.SaveAll();
            Environment.Exit(0);
        }

        #endregion
    }
}