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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.CommandPackets;
using OpenNos.GameObject.Helpers;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

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

        public void Act4Connect(Act4ConnectPacket act4ConnectPacket)
        {
            ClientSession player = ServerManager.Instance.GetSessionByCharacterName(act4ConnectPacket.Name ?? Session.Character.Name);
            if (player == null)
            {
                return;
            }
            switch (player.Character.Faction)
            {
                case 0:
                    ServerManager.Instance.ChangeMap(player.Character.CharacterId, 145, 51, 41);
                    player.SendPacket(UserInterfaceHelper.Instance.GenerateInfo("ACT4_NEED_FACTION"));
                    return;
                case FactionType.Angel:
                    player.Character.MapId = 130;
                    player.Character.MapX = 12;
                    player.Character.MapY = 40;
                    break;
                case FactionType.Demon:
                    player.Character.MapId = 131;
                    player.Character.MapX = 12;
                    player.Character.MapY = 40;
                    break;
            }
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, act4ConnectPacket, Session.IpAddress);
            player.Character.ConnectAct4();
        }

        /// <summary>
        ///     $AddMonster Command
        /// </summary>
        /// <param name="addMonsterPacket"></param>
        public void AddMonster(AddMonsterPacket addMonsterPacket)
        {
            if (addMonsterPacket != null)
            {
                if (!Session.HasCurrentMapInstance)
                {
                    return;
                }
                NpcMonster npcmonster = ServerManager.Instance.GetNpc(addMonsterPacket.MonsterVNum);
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
                    Position = (byte) Session.Character.Direction,
                    IsMoving = addMonsterPacket.IsMoving,
                    MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId()
                };
                if (!DAOFactory.MapMonsterDAO.DoesMonsterExist(monst.MapMonsterId))
                {
                    DAOFactory.MapMonsterDAO.Insert(monst);
                    if (DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) is MapMonster monster)
                    {
                        monster.Initialize(Session.CurrentMapInstance);
                        Session.CurrentMapInstance.AddMonster(monster);
                        Session.CurrentMapInstance?.Broadcast(monster.GenerateIn());
                    }
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, addMonsterPacket, Session.IpAddress);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddMonsterPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $AddPartner Command
        /// </summary>
        /// <param name="addPartnerPacket"></param>
        public void AddPartner(AddPartnerPacket addPartnerPacket)
        {
            if (addPartnerPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, addPartnerPacket, Session.IpAddress);
                AddMate(addPartnerPacket.MonsterVNum, addPartnerPacket.Level, MateType.Partner);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddPartnerPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $AddPet Command
        /// </summary>
        /// <param name="addPetPacket"></param>
        public void AddPet(AddPetPacket addPetPacket)
        {
            if (addPetPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, addPetPacket, Session.IpAddress);
                AddMate(addPetPacket.MonsterVNum, addPetPacket.Level, MateType.Pet);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddPartnerPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $AddSkill Command
        /// </summary>
        /// <param name="addSkillPacket"></param>
        public void AddSkill(AddSkillPacket addSkillPacket)
        {
            if (addSkillPacket != null)
            {
                short skillVNum = addSkillPacket.SkillVnum;
                Skill skillinfo = ServerManager.Instance.GetSkill(skillVNum);
                if (skillinfo == null)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_DOES_NOT_EXIST"), 11));
                    return;
                }
                if (skillinfo.SkillVNum < 200)
                {
                    foreach (CharacterSkill skill in Session.Character.Skills.Select(s => s.Value))
                    {
                        if (skillinfo.CastId == skill.Skill.CastId && skill.Skill.SkillVNum < 200)
                        {
                            Session.Character.Skills.TryRemove(skill.SkillVNum, out CharacterSkill value);
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
                        CharacterSkill oldupgrade = Session.Character.Skills.Select(s => s.Value).FirstOrDefault(s =>
                            s.Skill.UpgradeSkill == skillinfo.UpgradeSkill && s.Skill.UpgradeType == skillinfo.UpgradeType && s.Skill.UpgradeSkill != 0);
                        if (oldupgrade != null)
                        {
                            Session.Character.Skills.TryRemove(oldupgrade.SkillVNum, out CharacterSkill value);
                        }
                    }
                }
                Session.Character.Skills[skillVNum] = new CharacterSkill {SkillVNum = skillVNum, CharacterId = Session.Character.CharacterId};
                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                Session.SendPacket(Session.Character.GenerateLev());
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, addSkillPacket, Session.IpAddress);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddSkillPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $ArenaWinner Command
        /// </summary>
        /// <param name="arenaWinnerPacket"></param>
        public void ArenaWinner(ArenaWinner arenaWinnerPacket)
        {
            Session.Character.ArenaWinner = Session.Character.ArenaWinner == 0 ? 1 : 0;
            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, arenaWinnerPacket, Session.IpAddress);
        }

        /// <summary>
        ///     $Ban Command
        /// </summary>
        /// <param name="banPacket"></param>
        public void Ban(BanPacket banPacket)
        {
            if (banPacket != null)
            {
                banPacket.Reason = banPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(banPacket.CharacterName);
                if (character != null)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, banPacket, Session.IpAddress);
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
                Session.SendPacket(Session.Character.GenerateSay(BanPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $BlockExp Command
        /// </summary>
        /// <param name="blockExpPacket"></param>
        public void BlockExp(BlockExpPacket blockExpPacket)
        {
            if (blockExpPacket != null)
            {
                if (blockExpPacket.Duration == 0)
                {
                    blockExpPacket.Duration = 60;
                }

                blockExpPacket.Reason = blockExpPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(blockExpPacket.CharacterName);
                if (character != null)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, blockExpPacket, Session.IpAddress);
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == blockExpPacket.CharacterName);
                    session?.SendPacket(blockExpPacket.Duration == 1
                        ? UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), blockExpPacket.Reason))
                        : UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), blockExpPacket.Reason, blockExpPacket.Duration)));
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
                Session.SendPacket(Session.Character.GenerateSay(BlockExpPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $BlockFExp Command
        /// </summary>
        /// <param name="blockFExpPacket"></param>
        public void BlockFExp(BlockFExpPacket blockFExpPacket)
        {
            if (blockFExpPacket != null)
            {
                if (blockFExpPacket.Duration == 0)
                {
                    blockFExpPacket.Duration = 60;
                }

                blockFExpPacket.Reason = blockFExpPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(blockFExpPacket.CharacterName);
                if (character != null)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, blockFExpPacket, Session.IpAddress);
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == blockFExpPacket.CharacterName);
                    session?.SendPacket(blockFExpPacket.Duration == 1
                        ? UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), blockFExpPacket.Reason))
                        : UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), blockFExpPacket.Reason, blockFExpPacket.Duration)));
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
                Session.SendPacket(Session.Character.GenerateSay(BlockFExpPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $BlockPM Command
        /// </summary>
        /// <param name="blockPmPacket"></param>
        public void BlockPm(BlockPMPacket blockPmPacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, blockPmPacket, Session.IpAddress);
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
        ///     $BlockRep Command
        /// </summary>
        /// <param name="blockRepPacket"></param>
        public void BlockRep(BlockRepPacket blockRepPacket)
        {
            if (blockRepPacket != null)
            {
                if (blockRepPacket.Duration == 0)
                {
                    blockRepPacket.Duration = 60;
                }

                blockRepPacket.Reason = blockRepPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(blockRepPacket.CharacterName);
                if (character != null)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, blockRepPacket, Session.IpAddress);
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == blockRepPacket.CharacterName);
                    session?.SendPacket(blockRepPacket.Duration == 1
                        ? UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), blockRepPacket.Reason))
                        : UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), blockRepPacket.Reason, blockRepPacket.Duration)));
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
                Session.SendPacket(Session.Character.GenerateSay(BlockRepPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $ChangeClass Command
        /// </summary>
        /// <param name="changeClassPacket"></param>
        public void ChangeClass(ChangeClassPacket changeClassPacket)
        {
            if (changeClassPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, changeClassPacket, Session.IpAddress);
                Session.Character.ChangeClass(changeClassPacket.ClassType);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeClassPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $ChangeDignity Command
        /// </summary>
        /// <param name="changeDignityPacket"></param>
        public void ChangeDignity(ChangeDignityPacket changeDignityPacket)
        {
            if (changeDignityPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, changeDignityPacket, Session.IpAddress);
                if (changeDignityPacket.Dignity >= -1000 && changeDignityPacket.Dignity <= 100)
                {
                    Session.Character.Dignity = changeDignityPacket.Dignity;
                    Session.SendPacket(Session.Character.GenerateFd());
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("DIGNITY_CHANGED"), 12));
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_DIGNITY"), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeDignityPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $FLvl Command
        /// </summary>
        /// <param name="changeFairyLevelPacket"></param>
        public void ChangeFairyLevel(ChangeFairyLevelPacket changeFairyLevelPacket)
        {
            WearableInstance fairy = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte) EquipmentType.Fairy, InventoryType.Wear);
            if (changeFairyLevelPacket != null)
            {
                if (fairy != null)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, changeFairyLevelPacket, Session.IpAddress);
                    short fairylevel = changeFairyLevelPacket.FairyLevel;
                    fairylevel -= fairy.Item.ElementRate;
                    fairy.ElementRate = fairylevel;
                    fairy.XP = 0;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVEL_CHANGED"), fairy.Item.Name), 10));
                    Session.SendPacket(Session.Character.GeneratePairy());
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_FAIRY"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeFairyLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $ChangeSex Command
        /// </summary>
        /// <param name="changeSexPacket"></param>
        public void ChangeGender(ChangeSexPacket changeSexPacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, changeSexPacket, Session.IpAddress);
            Session.Character.ChangeSex();
        }

        /// <summary>
        ///     $HeroLvl Command
        /// </summary>
        /// <param name="changeHeroLevelPacket"></param>
        public void ChangeHeroLevel(ChangeHeroLevelPacket changeHeroLevelPacket)
        {
            if (changeHeroLevelPacket != null)
            {
                if (changeHeroLevelPacket.HeroLevel <= 255)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, changeHeroLevelPacket, Session.IpAddress);
                    Session.Character.HeroLevel = changeHeroLevelPacket.HeroLevel;
                    Session.Character.HeroXp = 0;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("HEROLEVEL_CHANGED"), 0));
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
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeHeroLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $JLvl Command
        /// </summary>
        /// <param name="changeJobLevelPacket"></param>
        public void ChangeJobLevel(ChangeJobLevelPacket changeJobLevelPacket)
        {
            if (changeJobLevelPacket != null)
            {
                if ((Session.Character.Class == 0 && changeJobLevelPacket.JobLevel <= 20 || Session.Character.Class != 0 && changeJobLevelPacket.JobLevel <= 255) && changeJobLevelPacket.JobLevel > 0)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, changeJobLevelPacket, Session.IpAddress);
                    Session.Character.JobLevel = changeJobLevelPacket.JobLevel;
                    Session.Character.JobLevelXp = 0;
                    Session.Character.Skills.Clear();
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("JOBLEVEL_CHANGED"), 0));
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(8), Session.Character.PositionX, Session.Character.PositionY);
                    Session.Character.Skills[(short) (200 + 20 * (byte) Session.Character.Class)] = new CharacterSkill
                    {
                        SkillVNum = (short) (200 + 20 * (byte) Session.Character.Class),
                        CharacterId = Session.Character.CharacterId
                    };
                    Session.Character.Skills[(short) (201 + 20 * (byte) Session.Character.Class)] = new CharacterSkill
                    {
                        SkillVNum = (short) (201 + 20 * (byte) Session.Character.Class),
                        CharacterId = Session.Character.CharacterId
                    };
                    Session.Character.Skills[236] = new CharacterSkill
                    {
                        SkillVNum = 236,
                        CharacterId = Session.Character.CharacterId
                    };
                    if (!Session.Character.UseSp)
                    {
                        Session.SendPacket(Session.Character.GenerateSki());
                    }
                    Session.Character.LearnAdventurerSkill();
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeJobLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Lvl Command
        /// </summary>
        /// <param name="changeLevelPacket"></param>
        public void ChangeLevel(ChangeLevelPacket changeLevelPacket)
        {
            if (changeLevelPacket != null)
            {
                if (changeLevelPacket.Level > 0)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, changeLevelPacket, Session.IpAddress);
                    Session.Character.Level = changeLevelPacket.Level;
                    Session.Character.LevelXp = 0;
                    Session.Character.Hp = (int) Session.Character.HPLoad();
                    Session.Character.Mp = (int) Session.Character.MPLoad();
                    Session.SendPacket(Session.Character.GenerateStat());
                    Session.SendPacket(Session.Character.GenerateStatInfo());
                    Session.SendPacket(Session.Character.GenerateStatChar());
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("LEVEL_CHANGED"), 0));
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(6), Session.Character.PositionX, Session.Character.PositionY);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(198), Session.Character.PositionX, Session.Character.PositionY);
                    ServerManager.Instance.UpdateGroup(Session.Character.CharacterId);
                    if (Session.Character.Family != null)
                    {
                        ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                        CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                        {
                            DestinationCharacterId = Session.Character.Family.FamilyId,
                            SourceCharacterId = Session.Character.CharacterId,
                            SourceWorldId = ServerManager.Instance.WorldId,
                            Message = "fhis_stc",
                            Type = MessageType.Family
                        });
                    }
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $ChangeRep Command
        /// </summary>
        /// <param name="changeReputationPacket"></param>
        public void ChangeReputation(ChangeReputationPacket changeReputationPacket)
        {
            if (changeReputationPacket != null)
            {
                if (changeReputationPacket.Reputation > 0)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, changeReputationPacket, Session.IpAddress);
                    Session.Character.Reput = changeReputationPacket.Reputation;
                    Session.SendPacket(Session.Character.GenerateFd());
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("REP_CHANGED"), 0));
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeReputationPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $SPLvl Command
        /// </summary>
        /// <param name="changeSpecialistLevelPacket"></param>
        public void ChangeSpecialistLevel(ChangeSpecialistLevelPacket changeSpecialistLevelPacket)
        {
            if (changeSpecialistLevelPacket != null)
            {
                SpecialistInstance sp = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte) EquipmentType.Sp, InventoryType.Wear);
                if (sp != null && Session.Character.UseSp)
                {
                    if (changeSpecialistLevelPacket.SpecialistLevel <= 255 && changeSpecialistLevelPacket.SpecialistLevel > 0)
                    {
                        LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, changeSpecialistLevelPacket, Session.IpAddress);
                        sp.SpLevel = changeSpecialistLevelPacket.SpecialistLevel;
                        sp.XP = 0;
                        Session.SendPacket(Session.Character.GenerateLev());
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SPLEVEL_CHANGED"), 0));
                        Session.Character.LearnSPSkill();
                        Session.SendPacket(Session.Character.GenerateSki());
                        Session.SendPackets(Session.Character.GenerateQuicklist());
                        Session.Character.Skills.ToList().ForEach(s => s.Value.LastUse = DateTime.Now.AddDays(-1));
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(8), Session.Character.PositionX, Session.Character.PositionY);
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                    }
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeSpecialistLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $ChannelInfo Command
        /// </summary>
        /// <param name="channelInfoPacket"></param>
        public void ChannelInfo(ChannelInfoPacket channelInfoPacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, channelInfoPacket, Session.IpAddress);
            Session.SendPacket(Session.Character.GenerateSay("---------CHANNEL INFO---------", 11));
            foreach (ClientSession session in ServerManager.Instance.Sessions)
            {
                Session.SendPacket(Session.Character.GenerateSay($"CharacterName: {session.Character.Name} SessionId: {session.SessionId}", 12));
            }
            Session.SendPacket(Session.Character.GenerateSay("---------------------------------------", 11));
        }

        /// <summary>
        ///     $CharEdit Command
        /// </summary>
        /// <param name="characterEditPacket"></param>
        public void CharacterEdit(CharacterEditPacket characterEditPacket)
        {
            if (characterEditPacket != null)
            {
                if (characterEditPacket.Property == null || string.IsNullOrEmpty(characterEditPacket.Data))
                {
                    return;
                }
                PropertyInfo propertyInfo = Session.Character.GetType().GetProperty(characterEditPacket.Property);
                if (propertyInfo == null)
                {
                    return;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, characterEditPacket, Session.IpAddress);
                propertyInfo.SetValue(Session.Character, Convert.ChangeType(characterEditPacket.Data, propertyInfo.PropertyType));
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                Session.Character.Save();
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(CharacterEditPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $CharStat Command
        /// </summary>
        /// <param name="characterStatsPacket"></param>
        public void CharStat(CharacterStatsPacket characterStatsPacket)
        {
            string returnHelp = CharacterStatsPacket.ReturnHelp();
            if (characterStatsPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, characterStatsPacket, Session.IpAddress);
                string name = characterStatsPacket.CharacterName;
                if (int.TryParse(characterStatsPacket.CharacterName, out int sessionId))
                {
                    if (ServerManager.Instance.GetSessionBySessionId(sessionId) != null)
                    {
                        Character character = ServerManager.Instance.GetSessionBySessionId(sessionId).Character;
                        SendStats(character);
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    if (ServerManager.Instance.GetSessionByCharacterName(name) != null)
                    {
                        Character character = ServerManager.Instance.GetSessionByCharacterName(name).Character;
                        SendStats(character);
                    }
                    else if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                    {
                        CharacterDTO characterDto = DAOFactory.CharacterDAO.LoadByName(name);
                        SendStats(characterDto);
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(returnHelp, 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(returnHelp, 10));
            }
        }

        /// <summary>
        ///     $Clear Command
        /// </summary>
        /// <param name="clearInventoryPacket"></param>
        public void ClearInventory(ClearInventoryPacket clearInventoryPacket)
        {
            if (clearInventoryPacket != null && clearInventoryPacket.InventoryType != InventoryType.Wear)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, clearInventoryPacket, Session.IpAddress);
                Parallel.ForEach(Session.Character.Inventory.Select(s => s.Value).Where(s => s.Type == clearInventoryPacket.InventoryType), inv =>
                {
                    Session.Character.Inventory.DeleteById(inv.Id);
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inv.Type, inv.Slot));
                });
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ClearInventoryPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Help Command
        /// </summary>
        /// <param name="helpPacket"></param>
        public void Command(HelpPacket helpPacket)
        {
            // TODO: Command displaying detailed informations about commands
            Session.SendPacket(Session.Character.GenerateSay("-------------Commands Info-------------", 11));

            // TODO: OPTIMIZE!
            List<Type> classes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).Where(t => t.IsClass && t.Namespace == "OpenNos.GameObject.CommandPackets").OrderBy(x => x.Name)
                .ToList();
            foreach (Type type in classes)
            {
                object classInstance = Activator.CreateInstance(type);
                Type classType = classInstance.GetType();
                MethodInfo method = classType.GetMethod("ReturnHelp");
                if (method == null)
                {
                    continue;
                }
                string message = method.Invoke(classInstance, null).ToString();
                if (!string.IsNullOrEmpty(message))
                {
                    Session.SendPacket(Session.Character.GenerateSay(message, 12));
                }
            }

            Session.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 11));
        }

        /// <summary>
        ///     $CreateItem Packet
        /// </summary>
        /// <param name="createItemPacket"></param>
        public void CreateItem(CreateItemPacket createItemPacket)
        {
            if (createItemPacket != null)
            {
                short vnum = createItemPacket.VNum;
                sbyte rare = 0;
                byte upgrade = 0, amount = 1, design = 0;
                if (vnum == 1046)
                {
                    return; // cannot create gold as item, use $Gold instead
                }
                Item iteminfo = ServerManager.Instance.GetItem(vnum);
                if (iteminfo != null)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, createItemPacket, Session.IpAddress);
                    if (iteminfo.IsColored || iteminfo.VNum == 302)
                    {
                        if (createItemPacket.Design.HasValue)
                        {
                            design = createItemPacket.Design.Value;
                        }
                        rare = createItemPacket.Upgrade.HasValue && iteminfo.VNum == 302 ? (sbyte) createItemPacket.Upgrade.Value : rare;
                    }
                    else if (iteminfo.Type == 0)
                    {
                        if (createItemPacket.Upgrade.HasValue)
                        {
                            if (iteminfo.EquipmentSlot != EquipmentType.Sp)
                            {
                                upgrade = createItemPacket.Upgrade.Value;
                            }
                            else
                            {
                                design = createItemPacket.Upgrade.Value;
                            }
                            if (iteminfo.EquipmentSlot != EquipmentType.Sp && upgrade == 0 && iteminfo.BasicUpgrade != 0)
                            {
                                upgrade = iteminfo.BasicUpgrade;
                            }
                        }
                        if (createItemPacket.Design.HasValue)
                        {
                            if (iteminfo.EquipmentSlot == EquipmentType.Sp)
                            {
                                upgrade = createItemPacket.Design.Value;
                            }
                            else
                            {
                                rare = (sbyte) createItemPacket.Design.Value;
                            }
                        }
                    }
                    if (createItemPacket.Design.HasValue && !createItemPacket.Upgrade.HasValue)
                    {
                        amount = createItemPacket.Design.Value > 99 ? (byte) 99 : createItemPacket.Design.Value;
                    }
                    ItemInstance inv = Session.Character.Inventory.AddNewToInventory(vnum, amount, Rare: rare, Upgrade: upgrade, Design: design).FirstOrDefault();
                    if (inv != null)
                    {
                        WearableInstance wearable = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(inv.Slot, inv.Type);
                        if (wearable != null)
                        {
                            switch (wearable.Item.EquipmentSlot)
                            {
                                case EquipmentType.Armor:
                                case EquipmentType.MainWeapon:
                                case EquipmentType.SecondaryWeapon:
                                    wearable.SetRarityPoint();
                                    break;

                                case EquipmentType.Boots:
                                case EquipmentType.Gloves:
                                    wearable.FireResistance = (short) (wearable.Item.FireResistance * upgrade);
                                    wearable.DarkResistance = (short) (wearable.Item.DarkResistance * upgrade);
                                    wearable.LightResistance = (short) (wearable.Item.LightResistance * upgrade);
                                    wearable.WaterResistance = (short) (wearable.Item.WaterResistance * upgrade);
                                    break;
                            }
                        }
                        Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {amount}", 12));
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                    }
                }
                else
                {
                    UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_ITEM"), 0);
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(CreateItemPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $PortalTo Command
        /// </summary>
        /// <param name="portalToPacket"></param>
        public void CreatePortal(PortalToPacket portalToPacket)
        {
            if (portalToPacket != null)
            {
                if (!Session.HasCurrentMapInstance)
                {
                    return;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, portalToPacket, Session.IpAddress);
                Portal portal = new Portal
                {
                    SourceMapId = Session.Character.MapId,
                    SourceX = Session.Character.PositionX,
                    SourceY = Session.Character.PositionY,
                    DestinationMapId = portalToPacket.DestinationMapId,
                    DestinationX = portalToPacket.DestinationX,
                    DestinationY = portalToPacket.DestinationY,
                    Type = portalToPacket.PortalType == null ? (short) -1 : (short) portalToPacket.PortalType
                };
                Session.CurrentMapInstance.Portals.Add(portal);
                Session.CurrentMapInstance?.Broadcast(portal.GenerateGp());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(PortalToPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Demote Command
        /// </summary>
        /// <param name="demotePacket"></param>
        public void Demote(DemotePacket demotePacket)
        {
            if (demotePacket != null)
            {
                string name = demotePacket.CharacterName;
                AccountDTO account = DAOFactory.AccountDAO.LoadById(DAOFactory.CharacterDAO.LoadByName(name).AccountId);
                if (account != null && account.Authority > AuthorityType.User)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, demotePacket, Session.IpAddress);
                    account.Authority -= 1;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);
                    if (session != null)
                    {
                        session.Account.Authority -= 1;
                        session.Character.Authority -= 1;
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                        DAOFactory.AccountDAO.WriteGeneralLog(session.Account.AccountId, session.IpAddress, session.Character.CharacterId, GeneralLogType.Demotion, $"by: {Session.Character.Name}");
                    }
                    else
                    {
                        DAOFactory.AccountDAO.WriteGeneralLog(account.AccountId, "127.0.0.1", null, GeneralLogType.Demotion, $"by: {Session.Character.Name}");
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
                Session.SendPacket(Session.Character.GenerateSay(DemotePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $DropRate Command
        /// </summary>
        /// <param name="dropRatePacket"></param>
        public void DropRate(DropRatePacket dropRatePacket)
        {
            if (dropRatePacket != null)
            {
                if (dropRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.DropRate = dropRatePacket.Value;
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, dropRatePacket, Session.IpAddress);
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("DROP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(DropRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Effect Command
        /// </summary>
        /// <param name="effectCommandpacket"></param>
        public void Effect(EffectCommandPacket effectCommandpacket)
        {
            if (effectCommandpacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, effectCommandpacket, Session.IpAddress);
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEff(effectCommandpacket.EffectId), Session.Character.PositionX, Session.Character.PositionY);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(EffectCommandPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $FairyXPRate Command
        /// </summary>
        /// <param name="fairyXpRatePacket"></param>
        public void FairyXpRate(FairyXpRatePacket fairyXpRatePacket)
        {
            if (fairyXpRatePacket != null)
            {
                if (fairyXpRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.FairyXpRate = fairyXpRatePacket.Value;
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, fairyXpRatePacket, Session.IpAddress);
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("FAIRYXP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(FairyXpRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Gift Command
        /// </summary>
        /// <param name="giftPacket"></param>
        public void Gift(GiftPacket giftPacket)
        {
            if (giftPacket != null)
            {
                if (giftPacket.CharacterName == "*")
                {
                    if (!Session.HasCurrentMapInstance)
                    {
                        return;
                    }
                    GiftPacket giftLog = giftPacket;
                    Parallel.ForEach(Session.CurrentMapInstance.Sessions, session =>
                    {
                        giftLog.CharacterName = session.Character.Name;
                        LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, giftLog, Session.IpAddress);
                        Session.Character.SendGift(session.Character.CharacterId, giftPacket.VNum, giftPacket.Amount, giftPacket.Rare, giftPacket.Upgrade, false);
                    });
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENT"), 10));
                }
                else
                {
                    CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(giftPacket.CharacterName);
                    if (chara != null)
                    {
                        LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, giftPacket, Session.IpAddress);
                        Session.Character.SendGift(chara.CharacterId, giftPacket.VNum, giftPacket.Amount, giftPacket.Rare, giftPacket.Upgrade, false);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENT"), 10));
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GiftPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $GodMode Command
        /// </summary>
        /// <param name="godModePacket"></param>
        public void GodMode(GodModePacket godModePacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, godModePacket, Session.IpAddress);
            Session.Character.HasGodMode = !Session.Character.HasGodMode;
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        ///     $Gold Command
        /// </summary>
        /// <param name="goldPacket"></param>
        public void Gold(GoldPacket goldPacket)
        {
            if (goldPacket != null)
            {
                long gold = goldPacket.Amount;
                long maxGold = ServerManager.Instance.MaxGold;
                gold = gold > maxGold ? maxGold : gold;
                if (gold >= 0)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, goldPacket, Session.IpAddress);
                    Session.Character.Gold = gold;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_SET"), 0));
                    Session.SendPacket(Session.Character.GenerateGold());
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $GoldDropRate Command
        /// </summary>
        /// <param name="goldDropRatePacket"></param>
        public void GoldDropRate(GoldDropRatePacket goldDropRatePacket)
        {
            if (goldDropRatePacket != null)
            {
                if (goldDropRatePacket.Value <= 1000)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, goldDropRatePacket, Session.IpAddress);
                    ServerManager.Instance.GoldDropRate = goldDropRatePacket.Value;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_DROP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldDropRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $GoldRate Command
        /// </summary>
        /// <param name="goldRatePacket"></param>
        public void GoldRate(GoldRatePacket goldRatePacket)
        {
            if (goldRatePacket != null)
            {
                if (goldRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.GoldRate = goldRatePacket.Value;
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, goldRatePacket, Session.IpAddress);
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Guri Command
        /// </summary>
        /// <param name="guriCommandPacket"></param>
        public void Guri(GuriCommandPacket guriCommandPacket)
        {
            if (guriCommandPacket == null)
            {
                Session.Character.GenerateSay(GuriCommandPacket.ReturnHelp(), 10);
                return;
            }
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, guriCommandPacket, Session.IpAddress);
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(guriCommandPacket.Type, guriCommandPacket.Argument, Session.Character.CharacterId, guriCommandPacket.Value));
        }

        /// <summary>
        ///     $HairColor Command
        /// </summary>
        /// <param name="hairColorPacket"></param>
        public void Haircolor(HairColorPacket hairColorPacket)
        {
            if (hairColorPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, hairColorPacket, Session.IpAddress);
                Session.Character.HairColor = hairColorPacket.HairColor;
                Session.SendPacket(Session.Character.GenerateEq());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateIn());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(HairColorPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $HairStyle Command
        /// </summary>
        /// <param name="hairStylePacket"></param>
        public void Hairstyle(HairStylePacket hairStylePacket)
        {
            if (hairStylePacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, hairStylePacket, Session.IpAddress);
                Session.Character.HairStyle = hairStylePacket.HairStyle;
                Session.SendPacket(Session.Character.GenerateEq());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateIn());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(HairStylePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $FairyXPRate Command
        /// </summary>
        /// <param name="heroXpRatePacket"></param>
        public void HeroXpRate(HeroXpRatePacket heroXpRatePacket)
        {
            if (heroXpRatePacket != null)
            {
                if (heroXpRatePacket.Value <= 1000)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, heroXpRatePacket, Session.IpAddress);
                    ServerManager.Instance.HeroXpRate = heroXpRatePacket.Value;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("HEROXP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(HeroXpRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Invisible Command
        /// </summary>
        /// <param name="invisiblePacket"></param>
        public void Invisible(InvisiblePacket invisiblePacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, invisiblePacket, Session.IpAddress);
            Session.Character.Invisible = !Session.Character.Invisible;
            Session.Character.InvisibleGm = !Session.Character.InvisibleGm;
            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateInvisible());
            Session.SendPacket(Session.Character.GenerateEq());
            if (Session.Character.InvisibleGm)
            {
                Session.Character.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s => Session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateOut(), ReceiverType.AllExceptMe);
            }
            else
            {
                Session.Character.Mates.Where(m => m.IsTeamMember).ToList().ForEach(m => Session.CurrentMapInstance?.Broadcast(m.GenerateIn()));
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
            }
        }

        /// <summary>
        ///     $Kick Command
        /// </summary>
        /// <param name="kickPacket"></param>
        public void Kick(KickPacket kickPacket)
        {
            if (kickPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, kickPacket, Session.IpAddress);
                if (kickPacket.CharacterName == "*")
                {
                    Parallel.ForEach(ServerManager.Instance.Sessions, session => { session.Disconnect(); });
                }
                ServerManager.Instance.Kick(kickPacket.CharacterName);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(KickPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $KickSession Command
        /// </summary>
        public void KickSession(KickSessionPacket kickSessionPacket)
        {
            if (kickSessionPacket != null)
            {
                if (kickSessionPacket.SessionId.HasValue) //if you set the sessionId, remove account verification
                {
                    kickSessionPacket.AccountName = string.Empty;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, kickSessionPacket, Session.IpAddress);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                AccountDTO account = DAOFactory.AccountDAO.LoadByName(kickSessionPacket.AccountName);
                CommunicationServiceClient.Instance.KickSession(account?.AccountId, kickSessionPacket.SessionId);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(KickSessionPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Kill Command
        /// </summary>
        /// <param name="killPacket"></param>
        public void Kill(KillPacket killPacket)
        {
            if (killPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, killPacket, Session.IpAddress);
                string name = killPacket.CharacterName;
                long? id = ServerManager.Instance.GetProperty<long?>(name, nameof(Character.CharacterId));

                if (id != null)
                {
                    bool hasGodMode = ServerManager.Instance.GetProperty<bool>(name, nameof(Character.HasGodMode));
                    if (hasGodMode)
                    {
                        return;
                    }
                    int? hp = ServerManager.Instance.GetProperty<int?>((long) id, nameof(Character.Hp));
                    if (hp == 0)
                    {
                        return;
                    }
                    ServerManager.Instance.SetProperty((long) id, nameof(Character.Hp), 0);
                    ServerManager.Instance.SetProperty((long) id, nameof(Character.LastDefence), DateTime.Now);
                    Session.CurrentMapInstance?.Broadcast($"su 1 {Session.Character.CharacterId} 1 {id} 1114 4 11 4260 0 0 0 0 60000 3 0");
                    Session.CurrentMapInstance?.Broadcast(null, ServerManager.Instance.GetUserMethod<string>((long) id, nameof(Character.GenerateStat)), ReceiverType.OnlySomeone, string.Empty,
                        (long) id);
                    ServerManager.Instance.AskRevive((long) id);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(KillPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $MapDance Command
        /// </summary>
        /// <param name="mapDancePacket"></param>
        public void MapDance(MapDancePacket mapDancePacket)
        {
            if (!Session.HasCurrentMapInstance)
            {
                return;
            }
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, mapDancePacket, Session.IpAddress);
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

        /// <summary>
        ///     $MapPVP Command
        /// </summary>
        /// <param name="mapPvpPacket"></param>
        public void MapPVP(MapPVPPacket mapPvpPacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, mapPvpPacket, Session.IpAddress);
            Session.CurrentMapInstance.IsPVP = !Session.CurrentMapInstance.IsPVP;
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        ///     $Morph Command
        /// </summary>
        /// <param name="morphPacket"></param>
        public void Morph(MorphPacket morphPacket)
        {
            if (morphPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, morphPacket, Session.IpAddress);
                if (morphPacket.MorphId < 30 && morphPacket.MorphId > 0)
                {
                    Session.Character.UseSp = true;
                    Session.Character.SpInstance = Session.Character.Inventory?.LoadBySlotAndType<SpecialistInstance>((byte) EquipmentType.Sp, InventoryType.Wear);
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
                    Session.Character.SpInstance = null;
                    Session.Character.ArenaWinner = 0;
                    Session.SendPacket(Session.Character.GenerateCond());
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(MorphPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Music Command
        /// </summary>
        /// <param name="musicPacket"></param>
        public void Music(MusicPacket musicPacket)
        {
            if (musicPacket != null)
            {
                if (musicPacket.Music >= 0)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, musicPacket, Session.IpAddress);
                    Session.CurrentMapInstance?.Broadcast($"bgm {musicPacket.Music}");
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(MusicPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Mute Command
        /// </summary>
        /// <param name="mutePacket"></param>
        public void Mute(MutePacket mutePacket)
        {
            if (mutePacket != null)
            {
                if (mutePacket.Duration == 0)
                {
                    mutePacket.Duration = 60;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, mutePacket, Session.IpAddress);
                mutePacket.Reason = mutePacket.Reason?.Trim();
                MuteMethod(mutePacket.CharacterName, mutePacket.Reason, mutePacket.Duration);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(MutePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Packet Command
        /// </summary>
        /// <param name="packetCallbackPacket"></param>
        public void PacketCallBack(PacketCallbackPacket packetCallbackPacket)
        {
            if (packetCallbackPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, packetCallbackPacket, Session.IpAddress);
                Session.SendPacket(packetCallbackPacket.Packet);
                Session.SendPacket(Session.Character.GenerateSay(packetCallbackPacket.Packet, 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(PacketCallbackPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Position Command
        /// </summary>
        /// <param name="positionPacket"></param>
        public void Position(PositionPacket positionPacket)
        {
            if (Session.CurrentMapInstance == null)
            {
                return;
            }
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, positionPacket, Session.IpAddress);
            Session.SendPacket(Session.Character.GenerateSay(
                $"Map:{Session.CurrentMapInstance.Map.MapId} - X:{Session.Character.PositionX} - Y:{Session.Character.PositionY} - Dir:{Session.Character.Direction} - Grid:{Session.CurrentMapInstance.Map.Grid[Session.Character.PositionX, Session.Character.PositionY].Value}",
                12));
        }

        /// <summary>
        ///     $Promote Command
        /// </summary>
        /// <param name="promotePacket"></param>
        public void Promote(PromotePacket promotePacket)
        {
            if (promotePacket != null)
            {
                string name = promotePacket.CharacterName;
                AccountDTO account = DAOFactory.AccountDAO.LoadById(DAOFactory.CharacterDAO.LoadByName(name).AccountId);
                if (account != null && account.Authority >= AuthorityType.User && account.Authority < AuthorityType.GameMaster)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, promotePacket, Session.IpAddress);
                    account.Authority += 1;
                    DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                    ClientSession session = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);
                    if (session != null)
                    {
                        session.Account.Authority += 1;
                        session.Character.Authority += 1;
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                        DAOFactory.AccountDAO.WriteGeneralLog(session.Account.AccountId, session.IpAddress, session.Character.CharacterId, GeneralLogType.Promotion, $"by: {Session.Character.Name}");
                    }
                    else
                    {
                        DAOFactory.AccountDAO.WriteGeneralLog(account.AccountId, "127.0.0.1", null, GeneralLogType.Promotion, $"by: {Session.Character.Name}");
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
                Session.SendPacket(Session.Character.GenerateSay(PromotePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Rarify Command
        /// </summary>
        /// <param name="rarifyPacket"></param>
        public void Rarify(RarifyPacket rarifyPacket)
        {
            if (rarifyPacket != null)
            {
                if (rarifyPacket.Slot < 0)
                {
                    return;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, rarifyPacket, Session.IpAddress);
                WearableInstance wearableInstance = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(rarifyPacket.Slot, 0);
                wearableInstance?.RarifyItem(Session, rarifyPacket.Mode, rarifyPacket.Protection);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(RarifyPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $RemoveMob Packet
        /// </summary>
        /// <param name="removeMobPacket"></param>
        public void RemoveMob(RemoveMobPacket removeMobPacket)
        {
            if (!Session.HasCurrentMapInstance)
            {
                return;
            }
            MapMonster monst = Session.CurrentMapInstance.GetMonster(Session.Character.LastMonsterId);
            if (monst != null)
            {
                if (monst.IsAlive)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, removeMobPacket, Session.IpAddress);
                    Session.CurrentMapInstance.Broadcast($"su 1 {Session.Character.CharacterId} 3 {monst.MapMonsterId} 1114 4 11 4260 0 0 0 0 {6000} 3 0");
                    Session.SendPacket(Session.Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("MONSTER_REMOVED"), monst.MapMonsterId, monst.Monster.Name, monst.MapId, monst.MapX, monst.MapY), 12));
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

        /// <summary>
        ///     $RemovePortal Command
        /// </summary>
        /// <param name="removePortalPacket"></param>
        public void RemovePortal(RemovePortalPacket removePortalPacket)
        {
            if (!Session.HasCurrentMapInstance)
            {
                return;
            }
            Portal portal = Session.CurrentMapInstance.Portals.FirstOrDefault(s =>
                s.SourceMapInstanceId == Session.Character.MapInstanceId &&
                Map.GetDistance(new MapCell {X = s.SourceX, Y = s.SourceY}, new MapCell {X = Session.Character.PositionX, Y = Session.Character.PositionY}) < 10);
            if (portal != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, removePortalPacket, Session.IpAddress);
                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NEAREST_PORTAL"), portal.SourceMapId, portal.SourceX, portal.SourceY), 12));
                Session.CurrentMapInstance.Portals.Remove(portal);
                Session.CurrentMapInstance?.Broadcast(portal.GenerateGp());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_PORTAL_FOUND"), 11));
            }
        }

        /// <summary>
        ///     $Resize Command
        /// </summary>
        /// <param name="resizePacket"></param>
        public void Resize(ResizePacket resizePacket)
        {
            if (resizePacket != null)
            {
                if (resizePacket.Value < 0)
                {
                    return;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, resizePacket, Session.IpAddress);
                Session.Character.Size = resizePacket.Value;
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateScal());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ResizePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $SearchItem Command
        /// </summary>
        /// <param name="searchItemPacket"></param>
        public void SearchItem(SearchItemPacket searchItemPacket)
        {
            if (searchItemPacket != null)
            {
                // TODO REVIEW COMMAND LOGGING
                IEnumerable<ItemDTO> itemlist = DAOFactory.ItemDAO.FindByName(string.IsNullOrEmpty(searchItemPacket.Data) ? string.Empty : searchItemPacket.Data).OrderBy(s => s.VNum).Skip(0 * 200)
                    .Take(200).ToList();
                if (itemlist.Any())
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, searchItemPacket, Session.IpAddress);
                    foreach (ItemDTO item in itemlist)
                    {
                        Session.SendPacket(Session.Character.GenerateSay($"Item: {(string.IsNullOrEmpty(item.Name) ? "none" : item.Name)} VNum: {item.VNum}", 12));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_NOT_FOUND"), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SearchItemPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $SearchMonster Command
        /// </summary>
        /// <param name="searchMonsterPacket"></param>
        public void SearchMonster(SearchMonsterPacket searchMonsterPacket)
        {
            if (searchMonsterPacket != null)
            {
                IEnumerable<NpcMonsterDTO> monsterlist = DAOFactory.NpcMonsterDAO.FindByName(string.IsNullOrEmpty(searchMonsterPacket.Name) ? string.Empty : searchMonsterPacket.Name)
                    .OrderBy(s => s.NpcMonsterVNum).Skip(searchMonsterPacket.Page * 200).Take(200).ToList();
                if (monsterlist.Any())
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, searchMonsterPacket, Session.IpAddress);
                    foreach (NpcMonsterDTO npcMonster in monsterlist)
                    {
                        Session.SendPacket(Session.Character.GenerateSay($"Monster: {(string.IsNullOrEmpty(npcMonster.Name) ? "none" : npcMonster.Name)} VNum: {npcMonster.NpcMonsterVNum}", 12));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MONSTER_NOT_FOUND"), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SearchMonsterPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Shout Command
        /// </summary>
        /// <param name="shoutPacket"></param>
        public void Shout(ShoutPacket shoutPacket)
        {
            if (shoutPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, shoutPacket, Session.IpAddress);
                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                {
                    DestinationCharacterId = null,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = shoutPacket.Message,
                    Type = MessageType.Shout
                });
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ShoutPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $ShoutHere Command
        /// </summary>
        /// <param name="shoutHerePacket"></param>
        public void ShoutHere(ShoutHerePacket shoutHerePacket)
        {
            if (shoutHerePacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, shoutHerePacket, Session.IpAddress);
                ServerManager.Instance.Shout(shoutHerePacket.Message);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ShoutHerePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Shutdown Command
        /// </summary>
        /// <param name="shutdownPacket"></param>
        public void Shutdown(ShutdownPacket shutdownPacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, shutdownPacket, Session.IpAddress);
            if (ServerManager.Instance.TaskShutdown != null)
            {
                ServerManager.Instance.ShutdownStop = true;
                ServerManager.Instance.TaskShutdown = null;
            }
            else
            {
                ServerManager.Instance.TaskShutdown = new Task(ServerManager.Instance.ShutdownTask);
                ServerManager.Instance.TaskShutdown.Start();
            }
        }

        /// <summary>
        ///     $ShutdownAll Command
        /// </summary>
        /// <param name="shutdownAllPacket"></param>
        public void ShutdownAll(ShutdownAllPacket shutdownAllPacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, shutdownAllPacket, Session.IpAddress);
            CommunicationServiceClient.Instance.Shutdown(!string.IsNullOrEmpty(shutdownAllPacket.WorldGroup) ? shutdownAllPacket.WorldGroup : ServerManager.Instance.ServerGroup);
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        ///     $Speed Command
        /// </summary>
        /// <param name="speedPacket"></param>
        public void Speed(SpeedPacket speedPacket)
        {
            if (speedPacket != null)
            {
                if (speedPacket.Value >= 60)
                {
                    return;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, speedPacket, Session.IpAddress);
                Session.Character.Speed = speedPacket.Value;
                Session.Character.IsCustomSpeed = true;
                Session.SendPacket(Session.Character.GenerateCond());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SpeedPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $SPRefill Command
        /// </summary>
        /// <param name="sprefillPacket"></param>
        public void SpRefill(SPRefillPacket sprefillPacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, sprefillPacket, Session.IpAddress);
            Session.Character.SpPoint = 10000;
            Session.Character.SpAdditionPoint = 1000000;
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SP_REFILL"), 0));
            Session.SendPacket(Session.Character.GenerateSpPoint());
        }

        /// <summary>
        ///     $Event Command
        /// </summary>
        /// <param name="eventPacket"></param>
        public void StartEvent(EventPacket eventPacket)
        {
            if (eventPacket != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, eventPacket, Session.IpAddress);
                EventHelper.Instance.GenerateEvent(eventPacket.EventType);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(EventPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Stat Command
        /// </summary>
        /// <param name="statCommandPacket"></param>
        public void Stat(StatCommandPacket statCommandPacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, statCommandPacket, Session.IpAddress);
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("XP_RATE_NOW")}: {ServerManager.Instance.XPRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("DROP_RATE_NOW")}: {ServerManager.Instance.DropRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("GOLD_RATE_NOW")}: {ServerManager.Instance.GoldRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("GOLD_DROPRATE_NOW")}: {ServerManager.Instance.GoldDropRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("HERO_XPRATE_NOW")}: {ServerManager.Instance.HeroXpRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("FAIRYXP_RATE_NOW")}: {ServerManager.Instance.FairyXpRate} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("SERVER_WORKING_TIME")}: {Process.GetCurrentProcess().StartTime - DateTime.Now:d\\ hh\\:mm\\:ss} ",
                13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("MEMORY")}: {GC.GetTotalMemory(true) / (1024 * 1024)}MB ", 13));

            foreach (string message in CommunicationServiceClient.Instance.RetrieveServerStatistics())
            {
                Session.SendPacket(Session.Character.GenerateSay(message, 13));
            }
        }

        /// <summary>
        ///     $Summon Command
        /// </summary>
        /// <param name="summonPacket"></param>
        public void Summon(SummonPacket summonPacket)
        {
            if (summonPacket != null)
            {
                if (!Session.IsOnMap || !Session.HasCurrentMapInstance)
                {
                    return;
                }
                NpcMonster npcmonster = ServerManager.Instance.GetNpc(summonPacket.NpcMonsterVNum);
                if (npcmonster == null)
                {
                    return;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, summonPacket, Session.IpAddress);
                Random random = new Random();
                for (int i = 0; i < summonPacket.Amount; i++)
                {
                    List<MapCell> possibilities = new List<MapCell>();
                    for (short x = -4; x < 5; x++)
                    {
                        for (short y = -4; y < 5; y++)
                        {
                            possibilities.Add(new MapCell {X = x, Y = y});
                        }
                    }
                    // TODO: Find a fancy way to parallelize as we dont care about order it needs to be randomized
                    foreach (MapCell possibilitie in possibilities.OrderBy(s => random.Next()))
                    {
                        short mapx = (short) (Session.Character.PositionX + possibilitie.X);
                        short mapy = (short) (Session.Character.PositionY + possibilitie.Y);
                        if (!Session.CurrentMapInstance?.Map.IsBlockedZone(mapx, mapy) ?? false)
                        {
                            break;
                        }
                    }

                    if (Session.CurrentMapInstance == null)
                    {
                        continue;
                    }
                    MapMonster monster = new MapMonster
                    {
                        MonsterVNum = summonPacket.NpcMonsterVNum,
                        MapY = Session.Character.PositionY,
                        MapX = Session.Character.PositionX,
                        MapId = Session.Character.MapInstance.Map.MapId,
                        Position = (byte) Session.Character.Direction,
                        IsMoving = summonPacket.IsMoving,
                        MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId(),
                        ShouldRespawn = false
                    };
                    monster.Initialize(Session.CurrentMapInstance);
                    Session.CurrentMapInstance.AddMonster(monster);
                    Session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SummonPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $SummonNPC Command
        /// </summary>
        /// <param name="summonNpcPacket"></param>
        public void SummonNPC(SummonNPCPacket summonNpcPacket)
        {
            // TODO: Fix it, doesn't work!
            if (summonNpcPacket != null)
            {
                if (!Session.IsOnMap || !Session.HasCurrentMapInstance)
                {
                    return;
                }
                NpcMonster npcmonster = ServerManager.Instance.GetNpc(summonNpcPacket.NpcMonsterVNum);
                if (npcmonster == null)
                {
                    return;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, summonNpcPacket, Session.IpAddress);
                Random random = new Random();
                for (int i = 0; i < summonNpcPacket.Amount; i++)
                {
                    List<MapCell> possibilities = new List<MapCell>();
                    for (short x = -4; x < 5; x++)
                    {
                        for (short y = -4; y < 5; y++)
                        {
                            possibilities.Add(new MapCell {X = x, Y = y});
                        }
                    }
                    // TODO: Find a fancy way to parallelize as we dont care about order it needs to be randomized
                    foreach (MapCell possibilitie in possibilities.OrderBy(s => random.Next()))
                    {
                        short mapx = (short) (Session.Character.PositionX + possibilitie.X);
                        short mapy = (short) (Session.Character.PositionY + possibilitie.Y);
                        if (!Session.CurrentMapInstance?.Map.IsBlockedZone(mapx, mapy) ?? false)
                        {
                            break;
                        }
                    }
                    if (Session.CurrentMapInstance == null)
                    {
                        continue;
                    }
                    MapNpc monster = new MapNpc
                    {
                        NpcVNum = summonNpcPacket.NpcMonsterVNum,
                        MapY = Session.Character.PositionY,
                        MapX = Session.Character.PositionX,
                        MapId = Session.Character.MapInstance.Map.MapId,
                        Position = (byte) Session.Character.Direction,
                        IsMoving = summonNpcPacket.IsMoving,
                        MapNpcId = Session.CurrentMapInstance.GetNextMonsterId()
                    };
                    monster.Initialize(Session.CurrentMapInstance);
                    Session.CurrentMapInstance.AddNPC(monster);
                    Session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SummonNPCPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Teleport Command
        /// </summary>
        /// <param name="teleportPacket"></param>
        public void Teleport(TeleportPacket teleportPacket)
        {
            if (teleportPacket != null)
            {
                if (Session.Character.HasShopOpened || Session.Character.InExchangeOrTrade)
                {
                    Session.Character.DisposeShopAndExchange();
                }
                if (Session.Character.IsChangingMapInstance)
                {
                    return;
                }
                if (short.TryParse(teleportPacket.Data, out short mapId))
                {
                    // TODO CHECK IF MAP EXIST
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, teleportPacket, Session.IpAddress);
                    ServerManager.Instance.ChangeMap(Session.Character.CharacterId, mapId, teleportPacket.X, teleportPacket.Y);
                }
                else
                {
                    ClientSession session = ServerManager.Instance.GetSessionByCharacterName(teleportPacket.Data);
                    if (session != null)
                    {
                        LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, teleportPacket, Session.IpAddress);
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
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(TeleportPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $TeleportToMe Command
        /// </summary>
        /// <param name="teleportToMePacket"></param>
        public void TeleportToMe(TeleportToMePacket teleportToMePacket)
        {
            Random random = new Random();
            if (teleportToMePacket != null)
            {
                if (teleportToMePacket.CharacterName == "*")
                {
                    Parallel.ForEach(ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.CharacterId != Session.Character.CharacterId), session =>
                    {
                        // clear any shop or trade on target character
                        session.Character.DisposeShopAndExchange();
                        if (session.Character.IsChangingMapInstance || !Session.HasCurrentMapInstance)
                        {
                            return;
                        }
                        LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, teleportToMePacket, Session.IpAddress);
                        List<MapCell> possibilities = new List<MapCell>();
                        for (short x = -6; x < 6; x++)
                        {
                            for (short y = -6; y < 6; y++)
                            {
                                possibilities.Add(new MapCell {X = x, Y = y});
                            }
                        }

                        short mapXPossibility = Session.Character.PositionX;
                        short mapYPossibility = Session.Character.PositionY;
                        foreach (MapCell possibility in possibilities.OrderBy(s => random.Next()))
                        {
                            mapXPossibility = (short) (Session.Character.PositionX + possibility.X);
                            mapYPossibility = (short) (Session.Character.PositionY + possibility.Y);
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
                    });
                }
                else
                {
                    ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterName(teleportToMePacket.CharacterName);

                    if (targetSession != null && !targetSession.Character.IsChangingMapInstance)
                    {
                        // clear any shop or trade on target character
                        targetSession.Character.DisposeShopAndExchange();
                        targetSession.Character.IsSitting = false;
                        ServerManager.Instance.ChangeMapInstance(targetSession.Character.CharacterId, Session.Character.MapInstanceId, (short) (Session.Character.PositionX + 1),
                            (short) (Session.Character.PositionY + 1));
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(TeleportToMePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Unban Command
        /// </summary>
        /// <param name="unbanPacket"></param>
        public void Unban(UnbanPacket unbanPacket)
        {
            if (unbanPacket != null)
            {
                string name = unbanPacket.CharacterName;
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);
                if (chara != null)
                {
                    PenaltyLogDTO log = ServerManager.Instance.PenaltyLogs.FirstOrDefault(s => s.AccountId == chara.AccountId && s.Penalty == PenaltyType.Banned && s.DateEnd > DateTime.Now);
                    if (log != null)
                    {
                        LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, unbanPacket, Session.IpAddress);
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
                Session.SendPacket(Session.Character.GenerateSay(UnbanPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Undercover Command
        /// </summary>
        /// <param name="undercoverPacket"></param>
        public void Undercover(UndercoverPacket undercoverPacket)
        {
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, undercoverPacket, Session.IpAddress);
            Session.Character.Undercover = !Session.Character.Undercover;
            Session.SendPacket(Session.Character.GenerateEq());
            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
        }

        /// <summary>
        ///     $Unmute Command
        /// </summary>
        /// <param name="unmutePacket"></param>
        public void Unmute(UnmutePacket unmutePacket)
        {
            if (unmutePacket != null)
            {
                string name = unmutePacket.CharacterName;
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);
                if (chara != null)
                {
                    if (ServerManager.Instance.PenaltyLogs.Any(s => s.AccountId == chara.AccountId && s.Penalty == (byte) PenaltyType.Muted && s.DateEnd > DateTime.Now))
                    {
                        LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, unmutePacket, Session.IpAddress);
                        PenaltyLogDTO log = ServerManager.Instance.PenaltyLogs.FirstOrDefault(s => s.AccountId == chara.AccountId && s.Penalty == (byte) PenaltyType.Muted && s.DateEnd > DateTime.Now);
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
                Session.SendPacket(Session.Character.GenerateSay(UnmutePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Upgrade Command
        /// </summary>
        /// <param name="upgradePacket"></param>
        public void Upgrade(UpgradeCommandPacket upgradePacket)
        {
            if (upgradePacket != null)
            {
                if (upgradePacket.Slot < 0)
                {
                    return;
                }
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, upgradePacket, Session.IpAddress);
                WearableInstance wearableInstance = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(upgradePacket.Slot, 0);
                wearableInstance?.UpgradeItem(Session, upgradePacket.Mode, upgradePacket.Protection, true);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(UpgradeCommandPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Warn Command
        /// </summary>
        /// <param name="warningPacket"></param>
        public void Warn(WarningPacket warningPacket)
        {
            string characterName = warningPacket.CharacterName;
            CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(characterName);
            if (character != null)
            {
                LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, warningPacket, Session.IpAddress);
                ClientSession session = ServerManager.Instance.GetSessionByCharacterName(characterName);
                session?.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("WARNING"), warningPacket.Reason)));
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = character.AccountId,
                    Reason = warningPacket.Reason,
                    Penalty = PenaltyType.Warning,
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now,
                    AdminName = Session.Character.Name
                };
                Session.Character.InsertOrUpdatePenalty(log);
                int penaltyCount = DAOFactory.PenaltyLogDAO.LoadByAccount(character.AccountId).Count(p => p.Penalty == PenaltyType.Warning);
                switch (penaltyCount)
                {
                    case 2:
                        MuteMethod(characterName, "Auto-Warning mute: 2 strikes", 30);
                        break;

                    case 3:
                        MuteMethod(characterName, "Auto-Warning mute: 3 strikes", 60);
                        break;

                    case 4:
                        MuteMethod(characterName, "Auto-Warning mute: 4 strikes", 720);
                        break;

                    case 5:
                        MuteMethod(characterName, "Auto-Warning mute: 5 strikes", 1440);
                        break;

                    case 6:
                        MuteMethod(characterName, "You've been THUNDERSTRUCK", 6969); // imagined number as for I = √(-1), complex z = a + bi
                        break;
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
            }
        }

        /// <summary>
        ///     $WigColor Command
        /// </summary>
        /// <param name="wigColorPacket"></param>
        public void WigColor(WigColorPacket wigColorPacket)
        {
            if (wigColorPacket != null)
            {
                WearableInstance wig = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte) EquipmentType.Hat, InventoryType.Wear);
                if (wig != null)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, wigColorPacket, Session.IpAddress);
                    wig.Design = wigColorPacket.Color;
                    Session.SendPacket(Session.Character.GenerateEq());
                    Session.SendPacket(Session.Character.GenerateEquipment());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateIn());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(WigColorPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $XpRate Command
        /// </summary>
        /// <param name="xpRatePacket"></param>
        public void XpRate(XpRatePacket xpRatePacket)
        {
            if (xpRatePacket != null)
            {
                if (xpRatePacket.Value <= 1000)
                {
                    LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, xpRatePacket, Session.IpAddress);
                    ServerManager.Instance.XPRate = xpRatePacket.Value;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("XP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(XpRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        ///     $Zoom Command
        /// </summary>
        /// <param name="zoomPacket"></param>
        public void Zoom(ZoomPacket zoomPacket)
        {
            if (zoomPacket == null)
            {
                Session.Character.GenerateSay(ZoomPacket.ReturnHelp(), 10);
                return;
            }
            LogHelper.Instance.InsertCommandLog(Session.Character.CharacterId, zoomPacket, Session.IpAddress);
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(15, zoomPacket.Value, Session.Character.CharacterId));
        }

        // TODO MOVE THAT SHIT
        /// <summary>
        ///     private AddMate method
        /// </summary>
        /// <param name="vnum"></param>
        /// <param name="level"></param>
        /// <param name="mateType"></param>
        private void AddMate(short vnum, byte level, MateType mateType)
        {
            NpcMonster mateNpc = ServerManager.Instance.GetNpc(vnum);
            if (Session.CurrentMapInstance == Session.Character.Miniland && mateNpc != null)
            {
                if (level == 0)
                {
                    level = 1;
                }
                Mate mate = new Mate(Session.Character, mateNpc, level, mateType);
                Session.Character.AddPet(mate);
            }
            else
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_IN_MINILAND"), 0));
            }
        }

        // TODO MOVE THAT SHIT
        /// <summary>
        ///     private mute method
        /// </summary>
        /// <param name="characterName"></param>
        /// <param name="reason"></param>
        /// <param name="duration"></param>
        private void MuteMethod(string characterName, string reason, int duration)
        {
            CharacterDTO characterToMute = DAOFactory.CharacterDAO.LoadByName(characterName);
            if (characterToMute != null)
            {
                ClientSession session = ServerManager.Instance.GetSessionByCharacterName(characterName);
                if (session != null && !session.Character.IsMuted())
                {
                    session?.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), reason, duration)));
                }
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = characterToMute.AccountId,
                    Reason = reason,
                    Penalty = PenaltyType.Muted,
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddMinutes(duration),
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

        /// <summary>
        ///     Helper method used for sending stats of desired character
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
            Session.SendPacket(Session.Character.GenerateSay(
                $"Faction: {(character.Faction == FactionType.Demon ? Language.Instance.GetMessageFromKey("DEMON") : Language.Instance.GetMessageFromKey("ANGEL"))}", 13));
            Session.SendPacket(Session.Character.GenerateSay("----- --------- -----", 13));
            AccountDTO account = DAOFactory.AccountDAO.LoadById(character.AccountId);
            if (account != null)
            {
                Session.SendPacket(Session.Character.GenerateSay("----- ACCOUNT -----", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Id: {account.AccountId}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Name: {account.Name}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Authority: {account.Authority}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"RegistrationIP: {account.RegistrationIP}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Email: {account.Email}", 13));
                Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
                IEnumerable<PenaltyLogDTO> penaltyLogs = ServerManager.Instance.PenaltyLogs.Where(s => s.AccountId == account.AccountId).ToList();
                PenaltyLogDTO penalty = penaltyLogs.LastOrDefault(s => s.DateEnd > DateTime.Now);
                Session.SendPacket(Session.Character.GenerateSay("----- PENALTY -----", 13));
                if (penalty != null)
                {
                    Session.SendPacket(Session.Character.GenerateSay($"Type: {penalty.Penalty}", 13));
                    Session.SendPacket(Session.Character.GenerateSay($"AdminName: {penalty.AdminName}", 13));
                    Session.SendPacket(Session.Character.GenerateSay($"Reason: {penalty.Reason}", 13));
                    Session.SendPacket(Session.Character.GenerateSay($"DateStart: {penalty.DateStart}", 13));
                    Session.SendPacket(Session.Character.GenerateSay($"DateEnd: {penalty.DateEnd}", 13));
                }
                Session.SendPacket(Session.Character.GenerateSay($"Bans: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Banned)}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Mutes: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Muted)}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Warnings: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Warning)}", 13));
                Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
            }
            ClientSession session = ServerManager.Instance.GetSessionByCharacterName(character.Name);
            if (session == null)
            {
                return;
            }
            Session.SendPacket(Session.Character.GenerateSay("----- SESSION -----", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Current IP: {session.IpAddress}", 13));
            Session.SendPacket(Session.Character.GenerateSay("----- ------------ -----", 13));
        }

        #endregion
    }
}