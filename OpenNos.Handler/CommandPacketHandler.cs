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
using OpenNos.WebApi.Reference;
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

        private ClientSession Session => _session;

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
                if (!Session.HasCurrentMapInstance)
                {
                    return;
                }
                NpcMonster npcmonster = ServerManager.GetNpc(addMonsterPacket.MonsterVNum);
                if (npcmonster == null)
                {
                    return;
                }
                MapMonsterDTO monst = new MapMonsterDTO
                {
                    MonsterVNum = addMonsterPacket.MonsterVNum,
                    MapY = Session.Character.PositionY,
                    MapX = Session.Character.PositionX,
                    MapId = Session.Character.MapInstance.Map.MapId,
                    Position = (byte)Session.Character.Direction,
                    IsMoving = addMonsterPacket.IsMoving,
                    MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId()
                };

                // TODO Speed up with DoesMonsterExist
                if (DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) == null)
                {
                    DAOFactory.MapMonsterDAO.Insert(monst);
                    MapMonster monster = DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) as MapMonster;
                    if (monster != null)
                    {
                        monster.Initialize(Session.CurrentMapInstance);
                        monster.StartLife();
                        Session.CurrentMapInstance.AddMonster(monster);
                        Session.CurrentMapInstance?.Broadcast(monster.GenerateIn3());
                    }
                }
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 10));
            }
        }

        /// <summary>
        /// $AddSkill Command
        /// </summary>
        /// <param name="addSkillPacket"></param>
        public void AddSkill(AddSkillPacket addSkillPacket)
        {
            Logger.Debug("Add Skill Command", Session.SessionId);
            if (addSkillPacket != null)
            {
                short skillVNum = addSkillPacket.SkillVnum;
                Skill skillinfo = ServerManager.GetSkill(skillVNum);
                if (skillinfo == null)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_DOES_NOT_EXIST"), 11));
                    return;
                }

                if (skillinfo.SkillVNum < 200)
                {
                    foreach (var skill in Session.Character.Skills.GetAllItems())
                    {
                        if (skillinfo.CastId == skill.Skill.CastId && skill.Skill.SkillVNum < 200)
                        {
                            Session.Character.Skills.Remove(skill.SkillVNum);
                        }
                    }
                }
                else
                {
                    if (Session.Character.Skills.ContainsKey(skillVNum))
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

                Session.Character.Skills[skillVNum] = new CharacterSkill { SkillVNum = skillVNum, CharacterId = Session.Character.CharacterId };

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

        [Packet("$ArenaWinner")]
        public void ArenaWinner(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.ArenaWinner = Session.Character.ArenaWinner == 0 ? 1 : 0;
            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        [Packet("$Backpack")]
        public void Backpack(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.Backpack = Session.Character.HaveBackpack() ? 1 : 0;
            Session.SendPacket(Session.Character.GenerateExts());
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        [Packet("$Ban")]
        public void Ban(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length >= 4)
            {
                string reason = string.Empty;
                int duration;
                bool isduration = int.TryParse(packetsplit[3], out duration);

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
                    DAOFactory.PenaltyLogDAO.Insert(new PenaltyLogDTO
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

        [Packet("$MapPVP")]
        public void MapPVP()
        {
            Session.CurrentMapInstance.IsPVP = !Session.CurrentMapInstance.IsPVP;
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        // TODO: Unify This!

        [Packet("$BlockExp")]
        public void BlockExp(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                string name = packetsplit[2];
                string reason = string.Empty;
                int duration;
                bool isduration = int.TryParse(packetsplit[3], out duration);

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
                    session?.SendPacket(duration == 1 ? Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), reason)) : Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), reason, duration)));
                    if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                    {
                        DAOFactory.PenaltyLogDAO.Insert(new PenaltyLogDTO
                        {
                            AccountId = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId,
                            Reason = reason,
                            Penalty = PenaltyType.BlockExp,
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
                Session.SendPacket(Session.Character.GenerateSay("$BlockExp CHARACTERNAME TIME REASON ", 10));
                Session.SendPacket(Session.Character.GenerateSay("$BlockExp CHARACTERNAME REASON", 10));
            }
        }

        [Packet("$BlockFExp")]
        public void BlockFExp(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                string name = packetsplit[2];
                string reason = string.Empty;
                int duration;
                bool isduration = int.TryParse(packetsplit[3], out duration);

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
                    session?.SendPacket(duration == 1 ? Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), reason)) : Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), reason, duration)));
                    if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                    {
                        DAOFactory.PenaltyLogDAO.Insert(new PenaltyLogDTO
                        {
                            AccountId = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId,
                            Reason = reason,
                            Penalty = PenaltyType.BlockFExp,
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
                Session.SendPacket(Session.Character.GenerateSay("$BlockFExp CHARACTERNAME TIME REASON ", 10));
                Session.SendPacket(Session.Character.GenerateSay("$BlockFExp CHARACTERNAME REASON", 10));
            }
        }

        [Packet("$BlockRep")]
        public void BlockRep(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                string name = packetsplit[2];
                string reason = string.Empty;
                int duration;
                bool isduration = int.TryParse(packetsplit[3], out duration);

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
                    session?.SendPacket(duration == 1 ? Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), reason)) : Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), reason, duration)));
                    if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                    {
                        DAOFactory.PenaltyLogDAO.Insert(new PenaltyLogDTO
                        {
                            AccountId = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).AccountId,
                            Reason = reason,
                            Penalty = PenaltyType.BlockRep,
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
                Session.SendPacket(Session.Character.GenerateSay("$BlockRep CHARACTERNAME TIME REASON ", 10));
                Session.SendPacket(Session.Character.GenerateSay("$BlockRep CHARACTERNAME REASON", 10));
            }
        }

        /////////////////////////////////////////////////////////////////////////////////

        [Packet("$BlockPM")]
        public void BlockPM()
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
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
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
                    Session.SendPacket(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVEL_CHANGED"), fairy.Item.Name), 10));
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
                if (changeHeroLevelPacket.HeroLevel <= 255)
                {
                    Session.Character.HeroLevel = changeHeroLevelPacket.HeroLevel;
                    Session.Character.HeroXp = 0;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("HEROLEVEL_CHANGED"), 0));
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPacket(Session.Character.GenerateStatInfo());
                    Session.SendPacket(Session.Character.GenerateStatChar());
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(6), Session.Character.PositionX, Session.Character.PositionY);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(198), Session.Character.PositionX, Session.Character.PositionY);
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
                if ((Session.Character.Class == 0 && changeJobLevelPacket.JobLevel <= 20 || Session.Character.Class != 0 && changeJobLevelPacket.JobLevel <= 255) && changeJobLevelPacket.JobLevel > 0)
                {
                    Session.Character.JobLevel = changeJobLevelPacket.JobLevel;
                    Session.Character.JobLevelXp = 0;
                    Session.Character.Skills.ClearAll();
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("JOBLEVEL_CHANGED"), 0));
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(8), Session.Character.PositionX, Session.Character.PositionY);

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
                if (changeLevelPacket.Level > 0)
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
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(6), Session.Character.PositionX, Session.Character.PositionY);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(198), Session.Character.PositionX, Session.Character.PositionY);
                    ServerManager.Instance.UpdateGroup(Session.Character.CharacterId);
                    if(Session.Character.Family != null)
                    { 
                      ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                    }
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
        /// $ChangeRep Command
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
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
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
                    if (changeSpecialistLevelPacket.SpecialistLevel <= 255 && changeSpecialistLevelPacket.SpecialistLevel > 0)
                    {
                        sp.SpLevel = changeSpecialistLevelPacket.SpecialistLevel;
                        sp.XP = 0;
                        Session.SendPacket(Session.Character.GenerateLev());
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SPLEVEL_CHANGED"), 0));
                        Session.SendPacket(Session.Character.GenerateSki());
                        Session.Character.LearnSPSkill();
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(8), Session.Character.PositionX, Session.Character.PositionY);
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

            // TODO: Command displaying detailed informations about commands
            Session.SendPacket(Session.Character.GenerateSay("-------------Commands Info-------------", 11));
            Session.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$AddSkill SKILLID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ArenaWinner", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Backpack", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME REASON", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME TIME REASON ", 12));
            Session.SendPacket(Session.Character.GenerateSay("$BlockExp CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$BlockFExp CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$BlockPM", 12));
            Session.SendPacket(Session.Character.GenerateSay("$BlockRep CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeClass CLASS", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeDignity AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeRep AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ChangeSex", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CharStat CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID COLOR", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE UPGRADE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID RARE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem ITEMID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$CreateItem SPID UPGRADE WINGS", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Demote CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$DropRate VALUE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Effect EFFECTID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$FairyXpRate VALUE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$FLvl FAIRYLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Gift USERNAME(*) VNUM AMOUNT RARE UPGRADE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Gift VNUM AMOUNT RARE UPGRADE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$GodMode", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Gold AMOUNT", 12));
            Session.SendPacket(Session.Character.GenerateSay("$GoldDropRate Value", 12));
            Session.SendPacket(Session.Character.GenerateSay("$GoldRate Value", 12));
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
            Session.SendPacket(Session.Character.GenerateSay("$Music BGM", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME REASON", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME TIME REASON ", 12));
            Session.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY PORTALTYPE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$PortalTo MAPID DESTX DESTY", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Position", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Promote CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RemoveMob", 12));
            Session.SendPacket(Session.Character.GenerateSay("$RemovePortal", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Resize SIZE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SearchItem NAME(%)", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SearchMonster NAME(%)", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Shout MESSAGE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Shutdown", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SPLvl SPLEVEL", 12));
            Session.SendPacket(Session.Character.GenerateSay("$SPRefill", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Stat", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Teleport Map X Y", 12));
            Session.SendPacket(Session.Character.GenerateSay("$TeleportToMe CHARACTERNAME(*)", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Unban CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Undercover", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Unmute CHARACTERNAME", 12));
            Session.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 12));
            Session.SendPacket(Session.Character.GenerateSay("$WigColor COLORID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$XpRate VALUE", 12));
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

                Item iteminfo = ServerManager.GetItem(vnum);
                if (iteminfo != null)
                {
                    if (iteminfo.IsColored)
                    {
                        if (packetsplit.Length > 3)
                        {
                            byte.TryParse(packetsplit[3], out design);
                        }
                    }
                    else if (iteminfo.Type == 0)
                    {
                        switch (packetsplit.Length)
                        {
                            case 4:
                                sbyte.TryParse(packetsplit[3], out rare);
                                break;

                            case 5:
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
                                break;
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
        /// <param name="portalToPacket"></param>
        public void CreatePortal(PortalToPacket portalToPacket)
        {
            Logger.Debug("PortalTo Command", Session.SessionId);
            if (portalToPacket != null)
            {
                if ( !Session.HasCurrentMapInstance)
                {
                    return;
                }
                short mapId = Session.Character.MapId;
                short mapX = Session.Character.PositionX;
                short mapY = Session.Character.PositionY;
                Portal portal = new Portal()
                {
                    SourceMapId = mapId,
                    SourceX = mapX,
                    SourceY = mapY,
                    DestinationMapId = portalToPacket.DestinationMapId,
                    DestinationX = portalToPacket.DestinationX,
                    DestinationY = portalToPacket.DestinationY,
                    Type = portalToPacket.PortalType == null ? (short)-1 : (short)portalToPacket.PortalType
                };
                Session.CurrentMapInstance.Portals.Add(portal);
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGp(portal));
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
                string name = demotePacket.CharacterName;
                AccountDTO account = DAOFactory.AccountDAO.LoadById(DAOFactory.CharacterDAO.LoadByName(name).AccountId);
                if (account != null)
                {
                    // TODO: Write GeneralLog entry on Demotion or Promotion
                    account.Authority = AuthorityType.User;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);
                    if (session != null)
                    {
                        session.Account.Authority = AuthorityType.User;
                        session.Character.Authority = AuthorityType.User;
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                    }
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Demote CHARACTERNAME", 10));
            }
        }

        /// <summary>
        /// $DropRate Command
        /// </summary>
        /// <param name="dropRatePacket"></param>
        public void DropRate(DropRatePacket dropRatePacket)
        {
            Logger.Debug("DropRate Changed", Session.SessionId);
            if (dropRatePacket != null)
            {
                if (dropRatePacket.Value <= 1000)
                {
                    ServerManager.DropRate = dropRatePacket.Value;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("DROP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$DropRate VALUE", 10));
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
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(effectCommandpacket.EffectId), Session.Character.PositionX, Session.Character.PositionY);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Effect EFFECT", 10));
            }
        }

        /// <summary>
        /// $FairyXPRate Command
        /// </summary>
        /// <param name="fairyXpRatePacket"></param>
        public void FairyXpRate(FairyXpRatePacket fairyXpRatePacket)
        {
            Logger.Debug("Fairy Xp Rate Changed", Session.SessionId);
            if (fairyXpRatePacket != null)
            {
                if (fairyXpRatePacket.Value <= 1000)
                {
                    ServerManager.FairyXpRate = fairyXpRatePacket.Value;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("FAIRYXP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$FairyXpRate VALUE", 10));
            }
        }

        [Packet("$Gift")]
        public void Gift(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            byte upgrade;
            sbyte rare;
            short vnum;
            byte amount;
            switch (packetsplit.Length)
            {
                case 6:
                    if (!(byte.TryParse(packetsplit[3], out amount) && short.TryParse(packetsplit[2], out vnum) && sbyte.TryParse(packetsplit[4], out rare) && byte.TryParse(packetsplit[5], out upgrade)))
                    {
                        return;
                    }
                    Session.Character.SendGift(Session.Character.CharacterId, vnum, amount, rare, upgrade, false);
                    break;

                case 7:
                    string name = packetsplit[2];
                    if (!(byte.TryParse(packetsplit[4], out amount) && short.TryParse(packetsplit[3], out vnum) && sbyte.TryParse(packetsplit[5], out rare) && byte.TryParse(packetsplit[6], out upgrade)))
                    {
                        return;
                    }
                    if (name == "*")
                    {
                        if (Session.HasCurrentMapInstance)
                        {
                            foreach (ClientSession session in Session.CurrentMapInstance.Sessions)
                            {
                                Session.Character.SendGift(session.Character.CharacterId, vnum, amount, rare, upgrade, false);
                            }
                        }
                    }
                    else
                    {
                        CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);

                        if (chara != null)
                        {
                            Session.Character.SendGift(chara.CharacterId, vnum, amount, rare, upgrade, false);
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                            return;
                        }
                    }
                    break;

                default:
                    Session.SendPacket(Session.Character.GenerateSay("$Gift USERNAME VNUM AMOUNT RARE UPGRADE", 10));

                    break;
            }
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENDED"), 10));
        }

        [Packet("$GodMode")]
        public void GodMode(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.HasGodMode = !Session.Character.HasGodMode;
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
        /// $GoldDropRate Command
        /// </summary>
        /// <param name="goldDropRatePacket"></param>
        public void GoldDropRate(GoldDropRatePacket goldDropRatePacket)
        {
            Logger.Debug("GoldDropRate Changed", Session.SessionId);
            if (goldDropRatePacket != null)
            {
                if (goldDropRatePacket.Value <= 1000)
                {
                    ServerManager.GoldDropRate = goldDropRatePacket.Value;
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_DROP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$GoldDropRate VALUE", 10));
            }
        }

        /// <summary>
        /// $GoldRate
        /// </summary>
        /// <param name="goldRatePacket"></param>
        public void GoldRate(GoldRatePacket goldRatePacket)
        {
            Logger.Debug("Gold Rate Changed", Session.SessionId);
            if (goldRatePacket != null)
            {
                if (goldRatePacket.Value <= 1000)
                {
                    ServerManager.GoldRate = goldRatePacket.Value;

                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$GoldRate RATE", 10));
            }
        }

        /// <summary>
        /// $Guri Command
        /// </summary>
        /// <param name="guriCommandPacket"></param>
        public void Guri(GuriCommandPacket guriCommandPacket)
        {
            Logger.Debug("Guri Command", Session.SessionId);
            Session.SendPacket(guriCommandPacket != null
                ? Session.Character.GenerateGuri(guriCommandPacket.Type, guriCommandPacket.Argument,
                    guriCommandPacket.Value)
                : Session.Character.GenerateSay("$Guri TYPE ARGUMENT VALUE", 10));
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
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateIn());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
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
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateIn());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
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
            Session.Character.Invisible = !Session.Character.Invisible;
            Session.Character.InvisibleGm = !Session.Character.InvisibleGm;
            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateInvisible());

            Session.SendPacket(Session.Character.GenerateEq());
            if (Session.Character.InvisibleGm)
            {
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllExceptMe);
            }
            else
            {
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
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
                if (kickPacket.CharacterName == "*")
                {
                    foreach (ClientSession cs in ServerManager.Instance.Sessions)
                    {
                        cs.Disconnect();
                    }
                }
                ServerManager.Instance.Kick(kickPacket.CharacterName);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Kick CHARACTERNAME", 10));
            }
        }

        /// <summary>
        /// $KickSession Command
        /// </summary>
        public void KickSession(KickSessionPacket kickPacket)
        {
            Logger.Debug("KickSession Command", Session.SessionId);
            if (kickPacket != null)
            {
                if (kickPacket.SessionId.HasValue) //if you set the sessionId, remove account verification
                {
                    kickPacket.AccountName = string.Empty;
                }

                ServerCommunicationClient.Instance.HubProxy.Invoke("KickSession", kickPacket.SessionId, kickPacket.AccountName);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$KickSession ACCOUNTNAME <SESSIONID>", 10));
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
                    Session.CurrentMapInstance?.Broadcast($"su 1 {Session.Character.CharacterId} 1 {id} 1114 4 11 4260 0 0 0 0 60000 3 0");
                    Session.CurrentMapInstance?.Broadcast(null, ServerManager.Instance.GetUserMethod<string>((long)id, nameof(Character.GenerateStat)), ReceiverType.OnlySomeone, string.Empty, (long)id);
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
            if (Session.HasCurrentMapInstance)
            {
                Session.CurrentMapInstance.IsDancing = !Session.CurrentMapInstance.IsDancing;
                if (Session.CurrentMapInstance.IsDancing)
                {
                    Session.Character.Dance();
                    Session.CurrentMapInstance?.Broadcast("dance 2");
                }
                else
                {
                    Session.Character.Dance();
                    Session.CurrentMapInstance?.Broadcast("dance");
                }
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
                verify = short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && short.TryParse(packetsplit[5], out arg[3]);
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
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                        }
                        else if (arg[0] > 30)
                        {
                            Session.Character.IsVehicled = true;
                            Session.Character.Morph = arg[0];
                            Session.Character.ArenaWinner = arg[3];
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                        }
                        else
                        {
                            Session.Character.IsVehicled = false;
                            Session.Character.UseSp = false;
                            Session.Character.ArenaWinner = 0;
                            Session.SendPacket(Session.Character.GenerateCond());
                            Session.SendPacket(Session.Character.GenerateLev());
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                        }
                    }
                    break;

                default:
                    Session.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 10));
                    break;
            }
        }

        /// <summary>
        /// $Music Command
        /// </summary>
        /// <param name="musicPacket"></param>
        public void Music(MusicPacket musicPacket)
        {
            Logger.Debug("Music Command", Session.SessionId);
            if (musicPacket != null)
            {
                if (musicPacket.Music > -1)
                {
                    Session.CurrentMapInstance?.Broadcast($"bgm {musicPacket.Music}");
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Music BGM", 10));
            }
        }

        [Packet("$Mute")]
        public void Mute(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length > 3)
            {
                string name = packetsplit[2];
                string reason = string.Empty;
                int duration;
                bool isduration = int.TryParse(packetsplit[3], out duration);

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
                    session?.SendPacket(duration == 1 ? Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), reason)) : Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), reason, duration)));
                    if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                    {
                        DAOFactory.PenaltyLogDAO.Insert(new PenaltyLogDTO
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

        [Packet("$Packet")]
        public void PacketCallBack(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            string str = string.Empty;
            if (packetsplit.Length > 2)
            {
                for (int i = 2; i < packetsplit.Length; i++)
                {
                    str += packetsplit[i] + " ";
                }
            }
            str = str.Trim();
            if (string.IsNullOrWhiteSpace(str))
            {
                return;
            }
            Session.SendPacket(str);
            Session.SendPacket(Session.Character.GenerateSay(str, 10));
        }

        [Packet("$Position")]
        public void Position(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.SendPacket(Session.Character.GenerateSay($"Map:{Session.Character.MapInstance.Map.MapId} - X:{Session.Character.PositionX} - Y:{Session.Character.PositionY} - Dir:{Session.Character.Direction}", 12));
        }

        /// <summary>
        /// $Promote Command
        /// </summary>
        /// <param name="promotePacket"></param>
        public void Promote(PromotePacket promotePacket)
        {
            Logger.Debug("Promote Command", Session.SessionId);
            if (promotePacket != null)
            {
                string name = promotePacket.CharacterName;
                AccountDTO account = DAOFactory.AccountDAO.LoadById(DAOFactory.CharacterDAO.LoadByName(name).AccountId);
                if (account != null)
                {
                    // TODO: Write GeneralLog entry on Promotion and Demotion
                    account.Authority = AuthorityType.Admin;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);
                    if (session != null)
                    {
                        session.Account.Authority = AuthorityType.Admin;
                        session.Character.Authority = AuthorityType.Admin;
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                    }
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Promote CHARACTERNAME", 10));
            }
        }

        /// <summary>
        /// $Rarify Command
        /// </summary>
        /// <param name="rarifyPacket"></param>
        public void Rarify(RarifyPacket rarifyPacket)
        {
            Logger.Debug("Rarify Command", Session.SessionId);
            if (rarifyPacket != null)
            {
                if (rarifyPacket.Slot > -1)
                {
                    WearableInstance wearableInstance = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(rarifyPacket.Slot, 0);
                    wearableInstance?.RarifyItem(Session, rarifyPacket.Mode, rarifyPacket.Protection);
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Rarify SLOT MODE PROTECTION", 10));
            }
        }

        [Packet("$RemoveMob")]
        public void RemoveMob(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            if (Session.HasCurrentMapInstance)
            {
                MapMonster monst = Session.CurrentMapInstance.GetMonster(Session.Character.LastMonsterId);
                if (monst != null)
                {
                    if (monst.IsAlive)
                    {
                        Session.CurrentMapInstance.Broadcast($"su 1 {Session.Character.CharacterId} 3 {monst.MapMonsterId} 1114 4 11 4260 0 0 0 0 {6000} 3 0");
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MONSTER_REMOVED"), monst.MapMonsterId, monst.Monster.Name, monst.MapId, monst.MapX, monst.MapY), 12));
                        Session.CurrentMapInstance.RemoveMonster(monst);
                        if (DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) != null)
                        {
                            DAOFactory.MapMonsterDAO.DeleteById(monst.MapMonsterId);
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MONSTER_NOT_ALIVE")), 11));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MONSTER_NOT_FOUND"), 11));
                }
            }
        }

        [Packet("$RemovePortal")]
        public void RemoveNearestPortal(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            if (Session.HasCurrentMapInstance)
            {
                Portal pt = Session.CurrentMapInstance.Portals.FirstOrDefault(s => s.SourceMapInstanceId == Session.Character.MapInstanceId && Map.GetDistance(new MapCell { X = s.SourceX, Y = s.SourceY }, new MapCell {  X = Session.Character.PositionX, Y = Session.Character.PositionY }) < 10);
                if (pt != null)
                {
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NEAREST_PORTAL"), pt.SourceMapId, pt.SourceX, pt.SourceY), 12));
                    Session.CurrentMapInstance.Portals.Remove(pt);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGp(pt));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_PORTAL_FOUND"), 11));
                }
            }
        }

        /// <summary>
        /// $Resize Command
        /// </summary>
        /// <param name="resizePacket"></param>
        public void Resize(ResizePacket resizePacket)
        {
            Logger.Debug("Resize Packet", Session.SessionId);
            if (resizePacket != null)
            {
                if (resizePacket.Value > -1)
                {
                    Session.Character.Size = resizePacket.Value;
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateScal());
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
                string name = string.Empty;
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
            string message = string.Empty;
            if (packetsplit.Length > 2)
            {
                for (int i = 2; i < packetsplit.Length; i++)
                {
                    message += packetsplit[i] + " ";
                }
            }
            message = message.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            //session is not on current server, check api if the target character is on another server
            int? sentChannelId = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", message, ServerManager.Instance.ChannelId, MessageType.Shout, string.Empty, null).Result;
        }

        [Packet("$ShoutHere")]
        public void ShoutHere(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            string[] packetsplit = packet.Split(' ');
            string message = string.Empty;
            if (packetsplit.Length > 2)
            {
                for (int i = 2; i < packetsplit.Length; i++)
                {
                    message += packetsplit[i] + " ";
                }
            }
            message = message.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

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

        /// <summary>
        /// $Speed Command
        /// </summary>
        /// <param name="speedPacket"></param>
        public void Speed(SpeedPacket speedPacket)
        {
            Logger.Debug("Speed Command", Session.SessionId);
            if (speedPacket != null)
            {
                if (speedPacket.Value < 60)
                {
                    Session.Character.Speed = speedPacket.Value;
                    Session.Character.IsCustomSpeed = true;
                    Session.SendPacket(Session.Character.GenerateCond());
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Speed SPEED", 10));
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
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("XP_RATE_NOW")}: {ServerManager.XPRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("DROP_RATE_NOW")}: {ServerManager.DropRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("GOLD_RATE_NOW")}: {ServerManager.GoldRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("GOLD_DROPRATE_NOW")}: {ServerManager.GoldDropRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("FAIRYXP_RATE_NOW")}: {ServerManager.FairyXpRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("SERVER_WORKING_TIME")}: {(Process.GetCurrentProcess().StartTime - DateTime.Now).ToString(@"d\ hh\:mm\:ss")} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("MEMORY")}: {GC.GetTotalMemory(true) / (1024 * 1024)}MB ", 13));

            foreach (string message in ServerCommunicationClient.Instance.HubProxy.Invoke<IEnumerable<string>>("RetrieveServerStatistics").Result)
            {
                Session.SendPacket(Session.Character.GenerateSay(message, 13));
            }
        }

        /// <summary>
        /// $CharStat Command
        /// </summary>
        /// <param name="characterStatsPacket"></param>
        public void CharStat(CharacterStatsPacket characterStatsPacket)
        {
            Logger.Debug("CharStat Command", Session.SessionId);
            // TODO: Optimize this!
            if (characterStatsPacket != null)
            {
                string name = characterStatsPacket.CharacterName;
                if (!string.IsNullOrEmpty(name))
                {
                    if (ServerManager.Instance.GetSessionByCharacterName(name) != null)
                    {
                        Character character = ServerManager.Instance.GetSessionByCharacterName(name).Character;
                        SendStats(character);
                    }
                    else if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                    {
                        CharacterDTO characterDTO = DAOFactory.CharacterDAO.LoadByName(name);
                        SendStats(characterDTO);
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay("$CharStat CHARACTERNAME", 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$CharStat CHARACTERNAME", 10));
            }
        }

        /// <summary>
        /// Helper method used for sending stats of desired character
        /// </summary>
        /// <param name="character"></param>
        private void SendStats(CharacterDTO character)
        {
            // TODO: Optimize THIS!
            Session.SendPacket(Session.Character.GenerateSay("---- CHARACTER ----", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Name: {character.Name}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Id: {character.CharacterId}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Gender: {character.Gender}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Class: {character.Class}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Level: {character.Level}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"JobLevel: {character.JobLevel}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"HeroLevel: {character.HeroLevel}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Gold: {character.Gold}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Bio: {character.Biography}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"MapId: {Session.CurrentMapInstance.Map.MapId}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"MapX: {Session.Character.PositionX}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"MapY: {Session.Character.PositionY}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Reputation: {character.Reput}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Dignity: {character.Dignity}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Compliment: {character.Compliment}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Fraction: {(character.Faction == 2 ? Language.Instance.GetMessageFromKey("DEMON") : Language.Instance.GetMessageFromKey("ANGEL"))}", 13));
            Session.SendPacket(Session.Character.GenerateSay("---- --------- ----", 13));
            AccountDTO acc = DAOFactory.AccountDAO.LoadById(character.AccountId);
            if (acc != null)
            {
                Session.SendPacket(Session.Character.GenerateSay("----- ACCOUNT -----", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Id: {acc.AccountId}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Name: {acc.Name}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Authority: {acc.Authority}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"RegistrationIP: {acc.RegistrationIP}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Email: {acc.Email}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"LastSession: {acc.LastSession}", 13));
                Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));

            }
            IEnumerable<PenaltyLogDTO> penalties = DAOFactory.PenaltyLogDAO.LoadByAccount(character.AccountId).Any() ? DAOFactory.PenaltyLogDAO.LoadByAccount(character.AccountId) : Session.Account.PenaltyLogs;
            IEnumerable<PenaltyLogDTO> penaltyLogs = penalties as PenaltyLogDTO[] ?? penalties.ToArray();
            PenaltyLogDTO penalty = penaltyLogs.LastOrDefault(s => s.DateEnd > DateTime.Now);
            if (penalty != null)
            {
                Session.SendPacket(Session.Character.GenerateSay("----- PENALTY -----", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Type: {penalty.Penalty}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"AdminName: {penalty.AdminName}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Reason: {penalty.Reason}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"DateStart: {penalty.DateStart}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"DateEnd: {penalty.DateEnd}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Bans: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Banned)}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Mutes: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Muted)}", 13));
                Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
            }
        }

        /// <summary>
        /// $Summon Command
        /// </summary>
        /// <param name="summonPacket"></param>
        public void Summon(SummonPacket summonPacket)
        {
            Logger.Debug("Summon Command", Session.SessionId);
            if (summonPacket != null)
            {
                if (Session.IsOnMap && Session.HasCurrentMapInstance)
                {
                    Random random = new Random();

                    short vnum = summonPacket.NpcMonsterVNum;
                    byte amount = summonPacket.Amount;
                    bool isMoving = summonPacket.IsMoving;

                    NpcMonster npcmonster = ServerManager.GetNpc(vnum);
                    if (npcmonster == null)
                    {
                        return;
                    }
                    for (int i = 0; i < amount; i++)
                    {
                        List<MapCell> Possibilities = new List<MapCell>();
                        for (short x = -4; x < 5; x++)
                        {
                            for (short y = -4; y < 5; y++)
                            {
                                Possibilities.Add(new MapCell { X = x, Y = y });
                            }
                        }
                        foreach (MapCell possibilitie in Possibilities.OrderBy(s => random.Next()))
                        {
                            short mapx = (short)(Session.Character.PositionX + possibilitie.X);
                            short mapy = (short)(Session.Character.PositionY + possibilitie.Y);
                            if (!Session.CurrentMapInstance?.Map.IsBlockedZone(mapx, mapy) ?? false)
                            {
                                break;
                            }
                        }

                        if (Session.HasCurrentMapInstance)
                        {
                            // ReSharper disable once PossibleNullReferenceException HasCurrentMapInstance NullCheck
                            MapMonster monster = new MapMonster { MonsterVNum = vnum, MapY = Session.Character.PositionY, MapX = Session.Character.PositionX, MapId = Session.Character.MapInstance.Map.MapId, Position = (byte)Session.Character.Direction, IsMoving = isMoving, MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId(), ShouldRespawn = false };
                            monster.Initialize(Session.CurrentMapInstance);
                            monster.StartLife();
                            Session.CurrentMapInstance.AddMonster(monster);
                            Session.CurrentMapInstance.Broadcast(monster.GenerateIn3());
                        }
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Summon VNUM AMOUNT MOVE", 10));
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

            if (Session.Character.IsChangingMapInstance)
            {
                return;
            }

            if (packetsplit.Length > 4)
            {
                verify = short.TryParse(packetsplit[2], out arg[0]) && short.TryParse(packetsplit[3], out arg[1]) && short.TryParse(packetsplit[4], out arg[2]) && DAOFactory.MapDAO.LoadById(arg[0]) != null;
            }
            switch (packetsplit.Length)
            {
                case 3:
                    ClientSession session = ServerManager.Instance.GetSessionByCharacterName(packetsplit[2]);
                    if (session != null)
                    {
                        ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                        short mapX = session.Character.PositionX;
                        short mapY = session.Character.PositionY;
                        ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, session.Character.MapInstanceId, mapX, mapY);
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                    break;

                case 5:
                    if (verify)
                    {
                        ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, arg[0], arg[1], arg[2]);
                    }
                    break;

                default:
                    Session.SendPacket(Session.Character.GenerateSay("$Teleport MAP X Y", 10));
                    Session.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 10));
                    break;
            }
        }

        /// <summary>
        /// $TeleportToMe Command
        /// </summary>
        /// <param name="teleportToMePacket"></param>
        public void TeleportToMe(TeleportToMePacket teleportToMePacket)
        {
            Logger.Debug("TeleportToMe Command", Session.SessionId);
            Random random = new Random();
            if (teleportToMePacket != null)
            {
                string name = teleportToMePacket.CharacterName;

                if (name == "*")
                {
                    foreach (ClientSession session in ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.CharacterId != Session.Character.CharacterId))
                    {
                        // clear any shop or trade on target character
                        session.Character.Dispose();

                        if (!session.Character.IsChangingMapInstance && Session.HasCurrentMapInstance)
                        {
                            ServerManager.Instance.LeaveMap(session.Character.CharacterId);

                            List<MapCell> possibilities = new List<MapCell>();
                            for (short x = -6; x < 6; x++)
                            {
                                for (short y = -6; y < 6; y++)
                                {
                                    possibilities.Add(new MapCell { X = x, Y = y });
                                }
                            }

                            short mapXPossibility = Session.Character.PositionX;
                            short mapYPossibility = Session.Character.PositionY;
                            foreach (MapCell possibility in possibilities.OrderBy(s => random.Next()))
                            {
                                mapXPossibility = (short)(Session.Character.PositionX + possibility.X);
                                mapYPossibility = (short)(Session.Character.PositionY + possibility.Y);
                                if (!Session.CurrentMapInstance.Map.IsBlockedZone(mapXPossibility, mapYPossibility))
                                {
                                    break;
                                }
                            }
                            ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, Session.Character.MapInstanceId, mapXPossibility, mapYPossibility);
                        }
                    }
                }
                else
                {
                    ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterName(name);

                    if (targetSession != null && !targetSession.Character.IsChangingMapInstance)
                    {
                        // clear any shop or trade on target character
                        targetSession.Character.Dispose();

                        ServerManager.Instance.LeaveMap(targetSession.Character.CharacterId);
                        targetSession.Character.IsSitting = false;
                        ServerManager.Instance.ChangeMapInstance(targetSession.Character.CharacterId, Session.Character.MapInstanceId, (short)(Session.Character.PositionX + 1), (short)(Session.Character.PositionY + 1));
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

        /// <summary>
        /// $Unban Command
        /// </summary>
        /// <param name="unbanPacket"></param>
        public void Unban(UnbanPacket unbanPacket)
        {
            Logger.Debug("Unban Command", Session.SessionId);
            if (unbanPacket != null)
            {
                string name = unbanPacket.CharacterName;
                if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                {
                    if (DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(name).AccountId).Any(s => s.Penalty == PenaltyType.Banned && s.DateEnd > DateTime.Now))
                    {
                        PenaltyLogDTO log = DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(name).AccountId).FirstOrDefault(s => s.Penalty == PenaltyType.Banned && s.DateEnd > DateTime.Now);
                        if (log != null)
                        {
                            log.DateEnd = DateTime.Now.AddSeconds(-1);
                            DAOFactory.PenaltyLogDAO.Update(log);
                        }
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

        /// <summary>
        /// $Undercover Command
        /// </summary>
        /// <param name="packet"></param>
        [Packet("$Undercover")]
        public void Undercover(string packet)
        {
            Logger.Debug(packet, Session.SessionId);
            Session.Character.Undercover = !Session.Character.Undercover;
            Session.SendPacket(Session.Character.GenerateEq());
            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
        }

        /// <summary>
        /// $Unmute Command
        /// </summary>
        /// <param name="unmutePacket"></param>
        public void Unmute(UnmutePacket unmutePacket)
        {
            Logger.Debug("Unmute Command", Session.SessionId);
            if (unmutePacket != null)
            {
                string name = unmutePacket.CharacterName;
                ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);

                if (session != null)
                {
                    if (session.Account.PenaltyLogs.Any(s => s.AccountId == session.Account.AccountId && s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now))
                    {
                        PenaltyLogDTO log = session.Account.PenaltyLogs.FirstOrDefault(s => s.AccountId == session.Account.AccountId && s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now);
                        if (log != null)
                        {
                            log.DateEnd = DateTime.Now.AddSeconds(-1);
                        }
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_MUTED"), 10));
                    }
                }
                else if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                {
                    if (DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(name).AccountId).Any(s => s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now))
                    {
                        PenaltyLogDTO log = DAOFactory.PenaltyLogDAO.LoadByAccount(DAOFactory.CharacterDAO.LoadByName(name).AccountId).FirstOrDefault(s => s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now);
                        if (log != null)
                        {
                            log.DateEnd = DateTime.Now.AddSeconds(-1);
                            DAOFactory.PenaltyLogDAO.Update(log);
                        }
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

        /// <summary>
        /// $Upgrade Command
        /// </summary>
        /// <param name="upgradePacket"></param>
        public void Upgrade(UpgradePacket upgradePacket)
        {
            Logger.Debug("Upgrade Command", Session.SessionId);
            if (upgradePacket != null)
            {
                if (upgradePacket.Slot > -1)
                {
                    WearableInstance wearableInstance = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(upgradePacket.Slot, 0);
                    wearableInstance?.UpgradeItem(Session, upgradePacket.Mode, upgradePacket.Protection, true);
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Upgrade SLOT MODE PROTECTION", 10));
            }
        }

        /// <summary>
        /// $WigColor Command
        /// </summary>
        /// <param name="wigColorPacket"></param>
        public void WigColor(WigColorPacket wigColorPacket)
        {
            Logger.Debug("Wig Color Command", Session.SessionId);
            if (wigColorPacket != null)
            {
                WearableInstance wig = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);
                if (wig != null)
                {
                    wig.Design = wigColorPacket.Color;
                    Session.SendPacket(Session.Character.GenerateEq());
                    Session.SendPacket(Session.Character.GenerateEquipment());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateIn());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$WigColor COLORID", 10));
            }
        }

        /// <summary>
        /// $XpRate Command
        /// </summary>
        /// <param name="xpRatePacket"></param>
        public void XpRate(XpRatePacket xpRatePacket)
        {
            Logger.Debug("Xp Rate Changed", Session.SessionId);
            if (xpRatePacket != null)
            {
                if (xpRatePacket.Value <= 1000)
                {
                    ServerManager.XPRate = xpRatePacket.Value;

                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("XP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$XpRate RATE", 10));
            }
        }

        /// <summary>
        /// $Zoom Command
        /// </summary>
        /// <param name="zoomPacket"></param>
        public void Zoom(ZoomPacket zoomPacket)
        {
            Logger.Debug("Zoom Command", Session.SessionId);
            Session.SendPacket(zoomPacket != null
                ? Session.Character.GenerateGuri(15, zoomPacket.Value)
                : Session.Character.GenerateSay("$Zoom VALUE", 10));
        }

        private async void ShutdownTask()
        {
            string message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 5);
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
            message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_MIN"), 1);
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
            message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 30);
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
            message = string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 10);
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