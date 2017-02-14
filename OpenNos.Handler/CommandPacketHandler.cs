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
using OpenNos.GameObject.Event;
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
        #region Instantiation

        public CommandPacketHandler(ClientSession session)
        {
            Session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        /// <summary>
        /// $AddMonster Command
        /// </summary>
        /// <param name="addMonsterPacket"></param>
        public void AddMonster(AddMonsterPacket addMonsterPacket)
        {
            Logger.Debug("Add Monster Command", Session.Character.GenerateIdentity());
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
                if (!DAOFactory.MapMonsterDAO.DoesMonsterExist(monst.MapMonsterId))
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
            Logger.Debug("Add Skill Command", Session.Character.GenerateIdentity());
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

        /// <summary>
        /// $ArenaWinner Command
        /// </summary>
        /// <param name="arenaWinner"></param>
        public void ArenaWinner(ArenaWinner arenaWinner)
        {
            Logger.Debug("Arena Winner Command", Session.Character.GenerateIdentity());
            Session.Character.ArenaWinner = Session.Character.ArenaWinner == 0 ? 1 : 0;
            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        /// $Ban Command
        /// </summary>
        /// <param name="banPacket"></param>
        public void Ban(BanPacket banPacket)
        {
            if (banPacket != null)
            {
                Logger.Debug(banPacket.ToString(), Session.Character.GenerateIdentity());

                banPacket.Reason = banPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(banPacket.CharacterName);
                if (character != null)
                {
                    ServerManager.Instance.Kick(banPacket.CharacterName);
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = banPacket.Reason,
                        Penalty = PenaltyType.Banned,
                        DateStart = DateTime.Now,
                        DateEnd = banPacket.Duration == 0 ? DateTime.Now.AddYears(15) : DateTime.Now.AddDays(banPacket.Duration),
                        AdminName = Session.Character.Name
                    };
                    Session.Character.InsertOrUpdatePenalty(log);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }

            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Ban CHARACTERNAME DURATION(DAYS) REASON", 10));
            }
        }

        /// <summary>
        /// $BlockExp Command
        /// </summary>
        /// <param name="blockExpPacket"></param>
        public void BlockExp(BlockExpPacket blockExpPacket)
        {
            if (blockExpPacket != null)
            {
                Logger.Debug(blockExpPacket.ToString(), Session.Character.GenerateIdentity());
                if (blockExpPacket.Duration == 0)
                {
                    blockExpPacket.Duration = 60;
                }

                blockExpPacket.Reason = blockExpPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(blockExpPacket.CharacterName);
                if (character != null)
                {
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == blockExpPacket.CharacterName);
                    session?.SendPacket(blockExpPacket.Duration == 1 ? Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), blockExpPacket.Reason)) : Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), blockExpPacket.Reason, blockExpPacket.Duration)));
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = blockExpPacket.Reason,
                        Penalty = PenaltyType.BlockExp,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddMinutes(blockExpPacket.Duration),
                        AdminName = Session.Character.Name
                    };
                    Session.Character.InsertOrUpdatePenalty(log);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$BlockExp CHARACTERNAME DURATION(MINUTES) REASON", 10));
            }
        }

        /// <summary>
        /// $BlockFExp Command
        /// </summary>
        /// <param name="blockFExpPacket"></param>
        public void BlockFExp(BlockFExpPacket blockFExpPacket)
        {
            if (blockFExpPacket != null)
            {
                Logger.Debug(blockFExpPacket.ToString(), Session.Character.GenerateIdentity());
                if (blockFExpPacket.Duration == 0)
                {
                    blockFExpPacket.Duration = 60;
                }

                blockFExpPacket.Reason = blockFExpPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(blockFExpPacket.CharacterName);
                if (character != null)
                {
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == blockFExpPacket.CharacterName);
                    session?.SendPacket(blockFExpPacket.Duration == 1 ? Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), blockFExpPacket.Reason)) : Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), blockFExpPacket.Reason, blockFExpPacket.Duration)));
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = blockFExpPacket.Reason,
                        Penalty = PenaltyType.BlockFExp,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddMinutes(blockFExpPacket.Duration),
                        AdminName = Session.Character.Name
                    };
                    Session.Character.InsertOrUpdatePenalty(log);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$BlockFExp CHARACTERNAME DURATION(MINUTES) REASON", 10));
            }
        }

        /// <summary>
        /// $BlockPM Command
        /// </summary>
        /// <param name="blockPMPacket"></param>
        public void BlockPM(BlockPMPacket blockPMPacket)
        {
            Logger.Debug("BlockPM Command", Session.Character.GenerateIdentity());
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
        /// $BlockRep Command
        /// </summary>
        /// <param name="blockRepPacket"></param>
        public void BlockRep(BlockRepPacket blockRepPacket)
        {
            if (blockRepPacket != null)
            {
                Logger.Debug(blockRepPacket.ToString(), Session.Character.GenerateIdentity());
                if (blockRepPacket.Duration == 0)
                {
                    blockRepPacket.Duration = 60;
                }

                blockRepPacket.Reason = blockRepPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(blockRepPacket.CharacterName);
                if (character != null)
                {
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == blockRepPacket.CharacterName);
                    session?.SendPacket(blockRepPacket.Duration == 1 ? Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), blockRepPacket.Reason)) : Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), blockRepPacket.Reason, blockRepPacket.Duration)));
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = blockRepPacket.Reason,
                        Penalty = PenaltyType.BlockRep,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddMinutes(blockRepPacket.Duration),
                        AdminName = Session.Character.Name
                    };
                    Session.Character.InsertOrUpdatePenalty(log);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$BlockRep CHARACTERNAME DURATION(MINUTES) REASON", 10));
            }
        }

        /// <summary>
        /// $ChangeClass Command
        /// </summary>
        /// <param name="changeClassPacket"></param>
        public void ChangeClass(ChangeClassPacket changeClassPacket)
        {
            Logger.Debug("Change Class Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Change Dignity Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Change FairyLevel Command", Session.Character.GenerateIdentity());
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

        /// <summary>
        /// $ChangeSex Command
        /// </summary>
        /// <param name="changeSexPacket"></param>
        public void ChangeGender(ChangeSexPacket changeSexPacket)
        {
            Logger.Debug("ChangeSex Command", Session.Character.GenerateIdentity());
            Session.Character.ChangeSex();
        }

        /// <summary>
        /// $HeroLvl Command
        /// </summary>
        /// <param name="changeHeroLevelPacket"></param>
        public void ChangeHeroLevel(ChangeHeroLevelPacket changeHeroLevelPacket)
        {
            Logger.Debug("Change HeroLevel Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Change JobLevel Command", Session.Character.GenerateIdentity());
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
                    if (!Session.Character.UseSp)
                    {
                        Session.SendPacket(Session.Character.GenerateSki());
                    }
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
            Logger.Debug("Change Level Packet", Session.Character.GenerateIdentity());
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
                    if (Session.Character.Family != null)
                    {
                        ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                        int? sentChannelId2 = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", ServerManager.ServerGroup, string.Empty, Session.Character.Family.FamilyId.ToString(), "fhis_stc", ServerManager.Instance.ChannelId, MessageType.Family).Result;
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
            Logger.Debug("Change Reputation Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Change SpecialistLevel Command", Session.Character.GenerateIdentity());
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

        /// <summary>
        /// $ChannelInfo Command
        /// </summary>
        /// <param name="channelInfoPacket"></param>
        public void ChannelInfo(ChannelInfoPacket channelInfoPacket)
        {
            Logger.Debug("ChannelInfo Command", Session.Character.GenerateIdentity());
            Session.SendPacket(Session.Character.GenerateSay("---------CHANNEL INFO---------", 11));
            foreach (ClientSession session in ServerManager.Instance.Sessions)
            {
                Session.SendPacket(Session.Character.GenerateSay($"CharacterName: {session.Character.Name} SessionId:{session.SessionId}", 12));
            }
            Session.SendPacket(Session.Character.GenerateSay("---------------------------------------", 11));
        }

        /// <summary>
        /// $CharStat Command
        /// </summary>
        /// <param name="characterStatsPacket"></param>
        public void CharStat(CharacterStatsPacket characterStatsPacket)
        {
            Logger.Debug("CharStat Command", Session.Character.GenerateIdentity());
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
        /// $Clr Command
        /// </summary>
        /// <param name="clearInventoryPacket"></param>
        public void ClearInventory(ClearInventoryPacket clearInventoryPacket)
        {
            Logger.Debug("ClearInventory Command", Session.Character.GenerateIdentity());
            if (clearInventoryPacket != null && clearInventoryPacket.InventoryType != InventoryType.Wear)
            {
                foreach (ItemInstance inv in Session.Character.Inventory.GetAllItems().Where(s => s.Type == clearInventoryPacket.InventoryType))
                {
                    Session.Character.Inventory.DeleteById(inv.Id);
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0, 0));
                }
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Clr INVENTORYTYPE", 10));
            }
        }

        /// <summary>
        /// $Help Command
        /// </summary>
        /// <param name="helpPacket"></param>
        public void Command(HelpPacket helpPacket)
        {
            Logger.Debug("Help Command", Session.Character.GenerateIdentity());

            // TODO: Command displaying detailed informations about commands
            Session.SendPacket(Session.Character.GenerateSay("-------------Commands Info-------------", 11));
            Session.SendPacket(Session.Character.GenerateSay("$AddMonster VNUM MOVE", 12));
            Session.SendPacket(Session.Character.GenerateSay("$AddSkill SKILLID", 12));
            Session.SendPacket(Session.Character.GenerateSay("$ArenaWinner", 12));
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
            Session.SendPacket(Session.Character.GenerateSay("$Event EVENT", 12));
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
            Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME DURATION(MINUTES) REASON", 12));
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
            if (Session.Account.Authority >= AuthorityType.GameMaster)
            {
                Logger.Debug(packet, Session.Character.GenerateIdentity());
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
                        List<ItemInstance> inv = Session.Character.Inventory.AddNewToInventory(vnum, amount);
                        if (inv.Any())
                        {
                            inv.First().Rare = rare;
                            inv.First().Upgrade = upgrade;
                            inv.First().Design = design;

                            WearableInstance wearable = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(inv.First().Slot, inv.First().Type);

                            if (wearable != null && (wearable.Item.EquipmentSlot == EquipmentType.Armor || wearable.Item.EquipmentSlot == EquipmentType.MainWeapon || wearable.Item.EquipmentSlot == EquipmentType.SecondaryWeapon))
                            {
                                wearable.SetRarityPoint();
                            }
                            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {amount}", 12));
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
        }

        /// <summary>
        /// $PortalTo Command
        /// </summary>
        /// <param name="portalToPacket"></param>
        public void CreatePortal(PortalToPacket portalToPacket)
        {
            Logger.Debug("PortalTo Command", Session.Character.GenerateIdentity());
            if (portalToPacket != null)
            {
                if (!Session.HasCurrentMapInstance)
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
            Logger.Debug("Demote Command", Session.Character.GenerateIdentity());
            if (demotePacket != null)
            {
                string name = demotePacket.CharacterName;
                AccountDTO account = DAOFactory.AccountDAO.LoadById(DAOFactory.CharacterDAO.LoadByName(name).AccountId);
                if (account != null)
                {
                    account.Authority = AuthorityType.User;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);
                    if (session != null)
                    {
                        session.Account.Authority = AuthorityType.User;
                        session.Character.Authority = AuthorityType.User;
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                        DAOFactory.AccountDAO.WriteGeneralLog(session.Account.AccountId, session.IpAddress, session.Character.CharacterId, "Demotion", $"by: {Session.Character.Name}");
                    }
                    else
                    {
                        DAOFactory.AccountDAO.WriteGeneralLog(account.AccountId, "127.0.0.1", null, "Demotion", $"by: {Session.Character.Name}");
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
            Logger.Debug("DropRate Changed", Session.Character.GenerateIdentity());
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
            Logger.Debug("Effect Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Fairy Xp Rate Changed", Session.Character.GenerateIdentity());
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

        /// <summary>
        /// $Gift Command
        /// </summary>
        /// <param name="giftPacket"></param>
        public void Gift(GiftPacket giftPacket)
        {
            Logger.Debug("Gift Command", Session.Character.GenerateIdentity());
            if (giftPacket != null)
            {
                if (giftPacket.Name == "*")
                {
                    if (Session.HasCurrentMapInstance)
                    {
                        foreach (ClientSession session in Session.CurrentMapInstance.Sessions)
                        {
                            Session.Character.SendGift(session.Character.CharacterId, giftPacket.VNum, giftPacket.Amount, giftPacket.Rare, giftPacket.Upgrade, false);
                        }
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENT"), 10));
                    }
                }
                else
                {
                    CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(giftPacket.Name);
                    if (chara != null)
                    {
                        Session.Character.SendGift(chara.CharacterId, giftPacket.VNum, giftPacket.Amount, giftPacket.Rare, giftPacket.Upgrade, false);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENT"), 10));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Gift NAME VNUM AMOUNT RARE UPGRADE", 10));
            }
        }

        /// <summary>
        /// $GodMode Command
        /// </summary>
        /// <param name="godModePacket"></param>
        public void GodMode(GodModePacket godModePacket)
        {
            Logger.Debug("GodMode Command", Session.Character.GenerateIdentity());
            Session.Character.HasGodMode = !Session.Character.HasGodMode;
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        /// $Gold Command
        /// </summary>
        /// <param name="goldPacket"></param>
        public void Gold(GoldPacket goldPacket)
        {
            Logger.Debug("Gold Command", Session.Character.GenerateIdentity());
            if (goldPacket != null)
            {
                long gold = goldPacket.Amount;
                long maxGold = ServerManager.MaxGold;
                gold = gold > maxGold ? maxGold : gold;
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
            Logger.Debug("GoldDropRate Changed", Session.Character.GenerateIdentity());
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
        /// $GoldRate Command
        /// </summary>
        /// <param name="goldRatePacket"></param>
        public void GoldRate(GoldRatePacket goldRatePacket)
        {
            Logger.Debug("Gold Rate Changed", Session.Character.GenerateIdentity());
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
            Logger.Debug("Guri Command", Session.Character.GenerateIdentity());
            Session.SendPacket(guriCommandPacket != null ? Session.Character.GenerateGuri(guriCommandPacket.Type, guriCommandPacket.Argument, guriCommandPacket.Value) : Session.Character.GenerateSay("$Guri TYPE ARGUMENT VALUE", 10));
        }

        /// <summary>
        /// $HairColor Command
        /// </summary>
        /// <param name="hairColorPacket"></param>
        public void Haircolor(HairColorPacket hairColorPacket)
        {
            Logger.Debug("Hair Color Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Hair Style Command", Session.Character.GenerateIdentity());
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

        /// <summary>
        /// $Invisible Command
        /// </summary>
        /// <param name="invisiblePacket"></param>
        public void Invisible(InvisiblePacket invisiblePacket)
        {
            Logger.Debug("Invisible Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Kick Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("KickSession Command", Session.Character.GenerateIdentity());
            if (kickPacket != null)
            {
                if (kickPacket.SessionId.HasValue) //if you set the sessionId, remove account verification
                {
                    kickPacket.AccountName = string.Empty;
                }
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
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
            Logger.Debug("Kill Command", Session.Character.GenerateIdentity());
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
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
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

        /// <summary>
        /// $MapDance Command
        /// </summary>
        /// <param name="mapDancePacket"></param>
        public void MapDance(MapDancePacket mapDancePacket)
        {
            Logger.Debug("MapDance Command", Session.Character.GenerateIdentity());
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
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
        }

        /// <summary>
        /// $MapPVP Command
        /// </summary>
        /// <param name="mapPVPPacket"></param>
        public void MapPVP(MapPVPPacket mapPVPPacket)
        {
            Logger.Debug("MapPVP Command", Session.Character.GenerateIdentity());
            Session.CurrentMapInstance.IsPVP = !Session.CurrentMapInstance.IsPVP;
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        /// $Morph Command
        /// </summary>
        /// <param name="morphPacket"></param>
        public void Morph(MorphPacket morphPacket)
        {
            Logger.Debug("Morph Command", Session.Character.GenerateIdentity());
            if (morphPacket != null)
            {
                if (morphPacket.MorphId < 30 && morphPacket.MorphId > 0)
                {
                    Session.Character.UseSp = true;
                    Session.Character.Morph = morphPacket.MorphId;
                    Session.Character.MorphUpgrade = morphPacket.Upgrade;
                    Session.Character.MorphUpgrade2 = morphPacket.MorphDesign;
                    Session.Character.ArenaWinner = morphPacket.ArenaWinner;
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                }
                else if (morphPacket.MorphId > 30)
                {
                    Session.Character.IsVehicled = true;
                    Session.Character.Morph = morphPacket.MorphId;
                    Session.Character.ArenaWinner = morphPacket.ArenaWinner;
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
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Morph MORPHID UPGRADE WINGS ARENA", 10));
            }
        }

        /// <summary>
        /// $Music Command
        /// </summary>
        /// <param name="musicPacket"></param>
        public void Music(MusicPacket musicPacket)
        {
            Logger.Debug("Music Command", Session.Character.GenerateIdentity());
            if (musicPacket != null)
            {
                if (musicPacket.Music > -1)
                {
                    Session.CurrentMapInstance?.Broadcast($"bgm {musicPacket.Music}");
                }
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Music BGM", 10));
            }
        }

        /// <summary>
        /// $Mute Command
        /// </summary>
        /// <param name="mutePacket"></param>
        public void Mute(MutePacket mutePacket)
        {
            if (mutePacket != null)
            {
                Logger.Debug(mutePacket.ToString(), Session.Character.GenerateIdentity());
                if (mutePacket.Duration == 0)
                {
                    mutePacket.Duration = 60;
                }

                mutePacket.Reason = mutePacket.Reason?.Trim();
                CharacterDTO characterToMute = DAOFactory.CharacterDAO.LoadByName(mutePacket.CharacterName);
                if (characterToMute != null)
                {
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == mutePacket.CharacterName);
                    session?.SendPacket(Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), mutePacket.Reason, mutePacket.Duration)));
                    if (session != null && !session.Character.IsMuted())
                    {
                        PenaltyLogDTO log = new PenaltyLogDTO
                        {
                            AccountId = characterToMute.AccountId,
                            Reason = mutePacket.Reason,
                            Penalty = PenaltyType.Muted,
                            DateStart = DateTime.Now,
                            DateEnd = DateTime.Now.AddMinutes(mutePacket.Duration),
                            AdminName = Session.Character.Name
                        };

                        session.Character.InsertOrUpdatePenalty(log);
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
                Session.SendPacket(Session.Character.GenerateSay("$Mute CHARACTERNAME DURATION(MINUTES) REASON", 10));
            }
        }

        /// <summary>
        /// $Packet Command
        /// </summary>
        /// <param name="packetCallbackPacket"></param>
        public void PacketCallBack(PacketCallbackPacket packetCallbackPacket)
        {
            Logger.Debug("PacketCallback Command", Session.Character.GenerateIdentity());
            if (packetCallbackPacket != null)
            {
                Session.SendPacket(packetCallbackPacket.Packet);
                Session.SendPacket(Session.Character.GenerateSay(packetCallbackPacket.Packet, 10));
            }
        }

        /// <summary>
        /// $Position Command
        /// </summary>
        /// <param name="positionPacket"></param>
        public void Position(PositionPacket positionPacket)
        {
            Logger.Debug("Position Command", Session.Character.GenerateIdentity());
            Session.SendPacket(Session.Character.GenerateSay($"Map:{Session.Character.MapInstance.Map.MapId} - X:{Session.Character.PositionX} - Y:{Session.Character.PositionY} - Dir:{Session.Character.Direction}", 12));
        }

        /// <summary>
        /// $Promote Command
        /// </summary>
        /// <param name="promotePacket"></param>
        public void Promote(PromotePacket promotePacket)
        {
            Logger.Debug("Promote Command", Session.Character.GenerateIdentity());
            if (promotePacket != null)
            {
                string name = promotePacket.CharacterName;
                AccountDTO account = DAOFactory.AccountDAO.LoadById(DAOFactory.CharacterDAO.LoadByName(name).AccountId);
                if (account != null)
                {
                    account.Authority = AuthorityType.GameMaster;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);
                    if (session != null)
                    {
                        session.Account.Authority = AuthorityType.GameMaster;
                        session.Character.Authority = AuthorityType.GameMaster;
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                        DAOFactory.AccountDAO.WriteGeneralLog(session.Account.AccountId, session.IpAddress, session.Character.CharacterId, "Promotion", $"by: {Session.Character.Name}");
                    }
                    else
                    {
                        DAOFactory.AccountDAO.WriteGeneralLog(account.AccountId, "127.0.0.1", null, "Promotion", $"by: {Session.Character.Name}");
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
            Logger.Debug("Rarify Command", Session.Character.GenerateIdentity());
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

        /// <summary>
        /// $RemoveMob Packet
        /// </summary>
        /// <param name="removeMobPacket"></param>
        public void RemoveMob(RemoveMobPacket removeMobPacket)
        {
            Logger.Debug("RemoveMob Command", Session.Character.GenerateIdentity());
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

        /// <summary>
        /// $RemovePortal Command
        /// </summary>
        /// <param name="removePortalPacket"></param>
        public void RemovePortal(RemovePortalPacket removePortalPacket)
        {
            Logger.Debug("RemovePortal Command", Session.Character.GenerateIdentity());
            if (Session.HasCurrentMapInstance)
            {
                Portal pt = Session.CurrentMapInstance.Portals.FirstOrDefault(s => s.SourceMapInstanceId == Session.Character.MapInstanceId && Map.GetDistance(new MapCell { X = s.SourceX, Y = s.SourceY }, new MapCell { X = Session.Character.PositionX, Y = Session.Character.PositionY }) < 10);
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
            Logger.Debug("Resize Packet", Session.Character.GenerateIdentity());
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

        /// <summary>
        /// $SearchItem Command
        /// </summary>
        /// <param name="searchItemPacket"></param>
        public void SearchItem(SearchItemPacket searchItemPacket)
        {
            Logger.Debug("SearchItem Command", Session.Character.GenerateIdentity());
            if (searchItemPacket != null)
            {
                IEnumerable<ItemDTO> itemlist = DAOFactory.ItemDAO.FindByName(searchItemPacket.Name).OrderBy(s => s.VNum).ToList();
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

        /// <summary>
        /// $SearchMonster Command
        /// </summary>
        /// <param name="searchMonsterPacket"></param>
        public void SearchMonster(SearchMonsterPacket searchMonsterPacket)
        {
            Logger.Debug("SearchMonster Command", Session.Character.GenerateIdentity());
            if (searchMonsterPacket != null)
            {
                IEnumerable<NpcMonsterDTO> monsterlist = DAOFactory.NpcMonsterDAO.FindByName(searchMonsterPacket.Name).OrderBy(s => s.NpcMonsterVNum).ToList();
                if (monsterlist.Any())
                {
                    foreach (NpcMonsterDTO npcMonster in monsterlist)
                    {
                        Session.SendPacket(Session.Character.GenerateSay($"Monster: {npcMonster.Name} VNum: {npcMonster.NpcMonsterVNum}", 12));
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

        /// <summary>
        /// $Shout Command
        /// </summary>
        /// <param name="shoutPacket"></param>
        public void Shout(ShoutPacket shoutPacket)
        {
            Logger.Debug("Shout Command", Session.Character.GenerateIdentity());
            if (shoutPacket != null)
            {
                int? sentChannelId = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", ServerManager.ServerGroup, Session.Character.Name, string.Empty, shoutPacket.Message, ServerManager.Instance.ChannelId, MessageType.Shout).Result;
            }
        }

        /// <summary>
        /// $ShoutHere Command
        /// </summary>
        /// <param name="shoutHerePacket"></param>
        public void ShoutHere(ShoutHerePacket shoutHerePacket)
        {
            Logger.Debug("ShoutHere Command", Session.Character.GenerateIdentity());
            if (shoutHerePacket != null)
            {
                ServerManager.Instance.Shout(shoutHerePacket.Message);
            }
        }

        /// <summary>
        /// $Shutdown Command
        /// </summary>
        /// <param name="shutdownPacket"></param>
        public void Shutdown(ShutdownPacket shutdownPacket)
        {
            Logger.Debug("Shutdown Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Speed Command", Session.Character.GenerateIdentity());
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

        /// <summary>
        /// $SPRefill Command
        /// </summary>
        /// <param name="sprefillPacket"></param>
        public void SPRefill(SPRefillPacket sprefillPacket)
        {
            Logger.Debug("SPRefill Command", Session.Character.GenerateIdentity());
            Session.Character.SpPoint = 10000;
            Session.Character.SpAdditionPoint = 1000000;
            Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("SP_REFILL"), 0));
            Session.SendPacket(Session.Character.GenerateSpPoint());
        }

        /// <summary>
        /// $Event Command
        /// </summary>
        /// <param name="eventPacket"></param>
        public void StartEvent(EventPacket eventPacket)
        {
            Logger.Debug("Event Command", Session.Character.GenerateIdentity());
            if (eventPacket != null)
            {
                EventHelper.GenerateEvent(eventPacket.EventType);
            }
        }

        /// <summary>
        /// $Stat Command
        /// </summary>
        /// <param name="statCommandPacket"></param>
        public void Stat(StatCommandPacket statCommandPacket)
        {
            Logger.Debug("Stat Command", Session.Character.GenerateIdentity());
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
        /// $Summon Command
        /// </summary>
        /// <param name="summonPacket"></param>
        public void Summon(SummonPacket summonPacket)
        {
            Logger.Debug("Summon Command", Session.Character.GenerateIdentity());
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
                        List<MapCell> possibilities = new List<MapCell>();
                        for (short x = -4; x < 5; x++)
                        {
                            for (short y = -4; y < 5; y++)
                            {
                                possibilities.Add(new MapCell { X = x, Y = y });
                            }
                        }
                        foreach (MapCell possibilitie in possibilities.OrderBy(s => random.Next()))
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

        /// <summary>
        /// $SummonNPC Command
        /// </summary>
        /// <param name="summonPacket"></param>
        public void SummonNPC(SummonNPCPacket summonPacket)
        {
            Logger.Debug("SummonNPC Command", Session.GenerateIdentity());
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
                        List<MapCell> possibilities = new List<MapCell>();
                        for (short x = -4; x < 5; x++)
                        {
                            for (short y = -4; y < 5; y++)
                            {
                                possibilities.Add(new MapCell { X = x, Y = y });
                            }
                        }
                        foreach (MapCell possibilitie in possibilities.OrderBy(s => random.Next()))
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
                            MapNpc monster = new MapNpc { NpcVNum = vnum, MapY = Session.Character.PositionY, MapX = Session.Character.PositionX, MapId = Session.Character.MapInstance.Map.MapId, Position = (byte)Session.Character.Direction, IsMoving = isMoving, MapNpcId = Session.CurrentMapInstance.GetNextMonsterId() };
                            monster.Initialize();
                            Session.CurrentMapInstance.AddNPC(monster);
                            Session.CurrentMapInstance.Broadcast(monster.GenerateIn2());
                        }
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$SummonNPC VNUM AMOUNT MOVE", 10));
            }
        }

        /// <summary>
        /// $Teleport Command
        /// </summary>
        /// <param name="teleportPacket"></param>
        public void Teleport(TeleportPacket teleportPacket)
        {
            Logger.Debug("Teleport Command", Session.Character.GenerateIdentity());
            if (teleportPacket != null)
            {
                if (Session.Character.HasShopOpened || Session.Character.InExchangeOrTrade)
                {
                    Session.Character.Dispose();
                }
                if (Session.Character.IsChangingMapInstance)
                {
                    return;
                }
                short mapId;
                if (short.TryParse(teleportPacket.Data, out mapId))
                {
                    ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                    ServerManager.Instance.ChangeMap(Session.Character.CharacterId, mapId, teleportPacket.X, teleportPacket.Y);
                }
                else
                {
                    ClientSession session = ServerManager.Instance.GetSessionByCharacterName(teleportPacket.Data);
                    if (session != null)
                    {
                        ServerManager.Instance.LeaveMap(Session.Character.CharacterId);
                        short mapX = session.Character.PositionX;
                        short mapY = session.Character.PositionY;
                        if (session.Character.Miniland == session.Character.MapInstance)
                        {
                            ServerManager.Instance.JoinMiniland(Session, session);
                        }
                        else
                        {
                            ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, session.Character.MapInstanceId, mapX, mapY);
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("$Teleport MAP X Y", 10));
                Session.SendPacket(Session.Character.GenerateSay("$Teleport CHARACTERNAME", 10));
            }
        }

        /// <summary>
        /// $TeleportToMe Command
        /// </summary>
        /// <param name="teleportToMePacket"></param>
        public void TeleportToMe(TeleportToMePacket teleportToMePacket)
        {
            Logger.Debug("TeleportToMe Command", Session.Character.GenerateIdentity());
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
                            if (Session.Character.Miniland == Session.Character.MapInstance)
                            {
                                ServerManager.Instance.JoinMiniland(session, Session);
                            }
                            else
                            {
                                ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, Session.Character.MapInstanceId, mapXPossibility, mapYPossibility);
                            }
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
            Logger.Debug("Unban Command", Session.Character.GenerateIdentity());
            if (unbanPacket != null)
            {
                string name = unbanPacket.CharacterName;
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);
                if (chara != null)
                {
                    PenaltyLogDTO log = ServerManager.Instance.PenaltyLogs.FirstOrDefault(s => s.AccountId == chara.AccountId && s.Penalty == PenaltyType.Banned && s.DateEnd > DateTime.Now);
                    if (log != null)
                    {
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        Session.Character.InsertOrUpdatePenalty(log);
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
        /// <param name="undercoverPacket"></param>
        public void Undercover(UndercoverPacket undercoverPacket)
        {
            Logger.Debug("Undercover Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Unmute Command", Session.Character.GenerateIdentity());
            if (unmutePacket != null)
            {
                string name = unmutePacket.CharacterName;
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);
                if (chara != null)
                {
                    if (ServerManager.Instance.PenaltyLogs.Any(s => s.AccountId == chara.AccountId && s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now))
                    {
                        PenaltyLogDTO log = ServerManager.Instance.PenaltyLogs.FirstOrDefault(s => s.AccountId == chara.AccountId && s.Penalty == (byte)PenaltyType.Muted && s.DateEnd > DateTime.Now);
                        if (log != null)
                        {
                            log.DateEnd = DateTime.Now.AddSeconds(-1);
                            Session.Character.InsertOrUpdatePenalty(log);
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
            Logger.Debug("Upgrade Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Wig Color Command", Session.Character.GenerateIdentity());
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
            Logger.Debug("Xp Rate Changed", Session.Character.GenerateIdentity());
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
            Logger.Debug("Zoom Command", Session.Character.GenerateIdentity());
            Session.SendPacket(zoomPacket != null
                ? Session.Character.GenerateGuri(15, zoomPacket.Value)
                : Session.Character.GenerateSay("$Zoom VALUE", 10));
        }

        /// <summary>
        /// Helper method used for sending stats of desired character
        /// </summary>
        /// <param name="character"></param>
        private void SendStats(CharacterDTO character)
        {
            Session.SendPacket(Session.Character.GenerateSay("----- CHARACTER -----", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Name: {character.Name}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Id: {character.CharacterId}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"State: {character.State}", 13));
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
            Session.SendPacket(Session.Character.GenerateSay($"Rage: {character.RagePoint}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Compliment: {character.Compliment}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Fraction: {(character.Faction == 2 ? Language.Instance.GetMessageFromKey("DEMON") : Language.Instance.GetMessageFromKey("ANGEL"))}", 13));
            Session.SendPacket(Session.Character.GenerateSay("----- --------- -----", 13));
            AccountDTO acc = DAOFactory.AccountDAO.LoadById(character.AccountId);
            if (acc != null)
            {
                Session.SendPacket(Session.Character.GenerateSay("----- ACCOUNT -----", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Id: {acc.AccountId}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Name: {acc.Name}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Authority: {acc.Authority}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"RegistrationIP: {acc.RegistrationIP}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Email: {acc.Email}", 13));
                Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
                IEnumerable<PenaltyLogDTO> penaltyLogs = ServerManager.Instance.PenaltyLogs.Where(s => s.AccountId == acc.AccountId).ToList();
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