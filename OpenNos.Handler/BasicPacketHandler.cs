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
using OpenNos.Core.Handling;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Packets.ClientPackets;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using OpenNos.PathFinder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class BasicPacketHandler : IPacketHandler
    {
        #region Instantiation

        public BasicPacketHandler(ClientSession session)
        {
            Session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        /// <summary>
        /// gop packet
        /// </summary>
        /// <param name="characterOptionPacket"></param>
        public void CharacterOptionChange(CharacterOptionPacket characterOptionPacket)
        {
            switch (characterOptionPacket.Option)
            {
                case CharacterOption.BuffBlocked:
                    Session.Character.BuffBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.BuffBlocked ? "BUFF_BLOCKED" : "BUFF_UNLOCKED"), 0));
                    break;

                case CharacterOption.EmoticonsBlocked:
                    Session.Character.EmoticonsBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.EmoticonsBlocked ? "EMO_BLOCKED" : "EMO_UNLOCKED"), 0));
                    break;

                case CharacterOption.ExchangeBlocked:
                    Session.Character.ExchangeBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.ExchangeBlocked ? "EXCHANGE_BLOCKED" : "EXCHANGE_UNLOCKED"), 0));
                    break;

                case CharacterOption.FriendRequestBlocked:
                    Session.Character.FriendRequestBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.FriendRequestBlocked ? "FRIEND_REQ_BLOCKED" : "FRIEND_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupRequestBlocked:
                    Session.Character.GroupRequestBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.GroupRequestBlocked ? "GROUP_REQ_BLOCKED" : "GROUP_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.HeroChatBlocked:
                    Session.Character.HeroChatBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.HeroChatBlocked ? "HERO_CHAT_BLOCKED" : "HERO_CHAT_UNLOCKED"), 0));
                    break;

                case CharacterOption.HpBlocked:
                    Session.Character.HpBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.HpBlocked ? "HP_BLOCKED" : "HP_UNLOCKED"), 0));
                    break;

                case CharacterOption.MinilandInviteBlocked:
                    Session.Character.MinilandInviteBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.MinilandInviteBlocked ? "MINI_INV_BLOCKED" : "MINI_INV_UNLOCKED"), 0));
                    break;

                case CharacterOption.MouseAimLock:
                    Session.Character.MouseAimLock = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.MouseAimLock ? "MOUSE_LOCKED" : "MOUSE_UNLOCKED"), 0));
                    break;

                case CharacterOption.QuickGetUp:
                    Session.Character.QuickGetUp = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.QuickGetUp ? "QUICK_GET_UP_ENABLED" : "QUICK_GET_UP_DISABLED"), 0));
                    break;

                case CharacterOption.WhisperBlocked:
                    Session.Character.WhisperBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.WhisperBlocked ? "WHISPER_BLOCKED" : "WHISPER_UNLOCKED"), 0));
                    break;

                case CharacterOption.FamilyRequestBlocked:
                    Session.Character.FamilyRequestBlocked = characterOptionPacket.IsActive == false;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey(Session.Character.FamilyRequestBlocked ? "FAMILY_REQ_LOCKED" : "FAMILY_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupSharing:
                    Group grp = ServerManager.Instance.Groups.FirstOrDefault(g => g.IsMemberOfGroup(Session.Character.CharacterId));
                    if (grp == null)
                    {
                        return;
                    }
                    if (grp.IsLeader(Session))
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_MASTER"), 0));
                        return;
                    }
                    if (characterOptionPacket.IsActive == false)
                    {
                        Group group = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(Session.Character.CharacterId));
                        if (group != null)
                        {
                            group.SharingMode = 1;
                        }
                        Session.CurrentMapInstance?.Broadcast(Session, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING"), 0), ReceiverType.Group);
                    }
                    else
                    {
                        Group group = ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(Session.Character.CharacterId));
                        if (group != null)
                        {
                            group.SharingMode = 0;
                        }
                        Session.CurrentMapInstance?.Broadcast(Session, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING_BY_ORDER"), 0), ReceiverType.Group);
                    }
                    break;
            }
            Session.SendPacket(Session.Character.GenerateStat());
        }

        /// <summary>
        /// compl packet
        /// </summary>
        /// <param name="complimentPacket"></param>
        public void Compliment(ComplimentPacket complimentPacket)
        {
            if (complimentPacket != null)
            {
                Logger.Debug(Session.Character.GenerateIdentity(), "Compliment Packet");
                long complimentedCharacterId = complimentPacket.CharacterId;
                if (Session.Character.Level >= 30)
                {
                    GeneralLogDTO dto = Session.Character.GeneralLogs.LastOrDefault(s => s.LogData == "World" && s.LogType == "Connection");
                    GeneralLogDTO lastcompliment = Session.Character.GeneralLogs.LastOrDefault(s => s.LogData == "World" && s.LogType == "Compliment");
                    if (dto != null && dto.Timestamp.AddMinutes(60) <= DateTime.Now)
                    {
                        if (lastcompliment == null || lastcompliment.Timestamp.AddDays(1) <= DateTime.Now.Date)
                        {
                            short? compliment = ServerManager.Instance.GetProperty<short?>(complimentedCharacterId, nameof(Character.Compliment));
                            compliment++;
                            ServerManager.Instance.SetProperty(complimentedCharacterId, nameof(Character.Compliment), compliment);
                            Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_GIVEN"), ServerManager.Instance.GetProperty<string>(complimentedCharacterId, nameof(Character.Name))), 12));
                            Session.Character.GeneralLogs.Add(new GeneralLogDTO
                            {
                                AccountId = Session.Account.AccountId,
                                CharacterId = Session.Character.CharacterId,
                                IpAddress = Session.IpAddress,
                                LogData = "World",
                                LogType = "Compliment",
                                Timestamp = DateTime.Now
                            });

                            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_RECEIVED"), Session.Character.Name), 12), ReceiverType.OnlySomeone, characterId: complimentedCharacterId);
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_COOLDOWN"), 11));
                        }
                    }
                    else
                    {
                        if (dto != null)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("COMPLIMENT_LOGIN_COOLDOWN"), (dto.Timestamp.AddMinutes(60) - DateTime.Now).Minutes), 11));
                        }
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("COMPLIMENT_NOT_MINLVL"), 11));
                }
            }
        }

        /// <summary>
        /// dir packet
        /// </summary>
        /// <param name="directionPacket"></param>
        public void Dir(DirectionPacket directionPacket)
        {
            if (directionPacket.CharacterId == Session.Character.CharacterId)
            {
                Session.Character.Direction = directionPacket.Direction;
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateDir());
            }
        }

        /// <summary>
        /// pcl packet
        /// </summary>
        /// <param name="getGiftPacket"></param>
        public void GetGift(GetGiftPacket getGiftPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), "GetGift packet");
            int giftId = getGiftPacket.GiftId;
            if (Session.Character.MailList.ContainsKey(giftId))
            {
                MailDTO mail = Session.Character.MailList[giftId];
                if (getGiftPacket.Type == 4 && mail.AttachmentVNum != null)
                {
                    if (Session.Character.Inventory.CanAddItem((short)mail.AttachmentVNum))
                    {
                        ItemInstance newInv = Session.Character.Inventory.AddNewToInventory((short)mail.AttachmentVNum, mail.AttachmentAmount, Upgrade: mail.AttachmentUpgrade, Rare: (sbyte)mail.AttachmentRarity).FirstOrDefault();
                        if (newInv != null)
                        {
                            if (newInv.Rare != 0)
                            {
                                WearableInstance wearable = newInv as WearableInstance;
                                wearable?.SetRarityPoint();
                            }
                            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_GIFTED")}: {newInv.Item.Name} x {mail.AttachmentAmount}", 12));

                            if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                            {
                                DAOFactory.MailDAO.DeleteById(mail.MailId);
                            }
                            Session.SendPacket($"parcel 2 1 {giftId}");
                            if (Session.Character.MailList.ContainsKey(giftId))
                            {
                                Session.Character.MailList.Remove(giftId);
                            }
                        }
                    }
                    else
                    {
                        Session.SendPacket("parcel 5 1 0");
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                    }
                }
                else if (getGiftPacket.Type == 5)
                {
                    Session.SendPacket($"parcel 7 1 {giftId}");

                    if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                    {
                        DAOFactory.MailDAO.DeleteById(mail.MailId);
                    }
                    if (Session.Character.MailList.ContainsKey(giftId))
                    {
                        Session.Character.MailList.Remove(giftId);
                    }
                }
            }
        }

        /// <summary>
        /// ncif packet
        /// </summary>
        /// <param name="ncifPacket"></param>
        public void GetNamedCharacterInformation(NcifPacket ncifPacket)
        {
            switch (ncifPacket.Type)
            {
                // characters
                case 1:
                    Session.SendPacket(ServerManager.Instance.GetSessionByCharacterId(ncifPacket.TargetId)?.Character?.GenerateStatInfo());
                    break;

                // npcs/mates
                case 2:
                    if (Session.HasCurrentMapInstance)
                    {
                        Session.CurrentMapInstance.Npcs.Where(n => n.MapNpcId == (int)ncifPacket.TargetId).ToList().ForEach(npc =>
                        {
                            NpcMonster npcinfo = ServerManager.Instance.GetNpc(npc.NpcVNum);
                            if (npcinfo == null)
                            {
                                return;
                            }
                            Session.SendPacket($"st 2 {ncifPacket.TargetId} {npcinfo.Level} {npcinfo.HeroLevel} 100 100 50000 50000");
                        });
                        Parallel.ForEach(Session.CurrentMapInstance.Sessions, session =>
                        {
                            Mate mate = session.Character.Mates.FirstOrDefault(s => s.MateTransportId == (int)ncifPacket.TargetId);
                            if (mate != null)
                            {
                                Session.SendPacket(mate.GenerateStatInfo());
                            }
                        });
                    }
                    break;

                // monsters
                case 3:
                    if (Session.HasCurrentMapInstance)
                    {
                        Session.CurrentMapInstance.Monsters.Where(m => m.MapMonsterId == (int)ncifPacket.TargetId).ToList().ForEach(monster =>
                        {
                            NpcMonster monsterinfo = ServerManager.Instance.GetNpc(monster.MonsterVNum);
                            if (monsterinfo == null)
                            {
                                return;
                            }
                            Session.Character.LastMonsterId = monster.MapMonsterId;
                            Session.SendPacket($"st 3 {ncifPacket.TargetId} {monsterinfo.Level} {monsterinfo.HeroLevel} {(int)((float)monster.CurrentHp / (float)monster.Monster.MaxHP * 100)} {(int)((float)monster.CurrentMp / (float)monster.Monster.MaxMP * 100)} {monster.CurrentHp} {monster.CurrentMp}");
                        });
                    }
                    break;
            }
        }

        /// <summary>
        /// npinfo packet
        /// </summary>
        /// <param name="npinfoPacket"></param>
        public void GetStats(NpinfoPacket npinfoPacket)
        {
            Session.SendPacket(Session.Character.GenerateStatChar());
            if (npinfoPacket.Page != Session.Character.ScPage)
            {
                Session.Character.ScPage = npinfoPacket.Page;
                Session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());
                Session.SendPackets(Session.Character.GenerateScP(npinfoPacket.Page));
            }
        }
        /// <summary>
        /// rdPacket packet
        /// </summary>
        /// <param name="rdPacket"></param>
        public void RaidManage(RdPacket rdPacket)
        {
            Group grp;
            switch (rdPacket.Type)
            {
                case 1://Join
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }
                    ClientSession target = ServerManager.Instance.GetSessionByCharacterId(rdPacket.CharacterId);
                    if (rdPacket.Parameter == null && target?.Character?.Group == null && Session.Character.Group.IsLeader(Session))
                    {
                        GroupJoin(new PJoinPacket() { RequestType = GroupRequestType.Invited, CharacterId = rdPacket.CharacterId });
                    }
                    else if (Session.Character.Group == null)
                    {
                        GroupJoin(new PJoinPacket() { RequestType = GroupRequestType.Accepted, CharacterId = rdPacket.CharacterId });
                    }
                    break;

                case 2://leave
                    ClientSession sender = ServerManager.Instance.GetSessionByCharacterId(rdPacket.CharacterId);
                    if (sender.Character?.Group == null)
                        return;

                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("LEFT_RAID")), 0));
                    if (Session?.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId, Session.Character.MapX, Session.Character.MapY);
                    }
                    grp = sender.Character?.Group;
                    Session.SendPacket(Session.Character.GenerateRaid(1, true));
                    Session.SendPacket(Session.Character.GenerateRaid(2, true));

                    grp.Characters.ForEach(s =>
                    {
                        s.SendPacket(grp.GenerateRdlst());
                        s.SendPacket(grp.GeneraterRaidmbf());
                        s.SendPacket(s.Character.GenerateRaid(0, false));
                    });
                    break;
                case 3:
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }
                    if (Session.Character.Group != null && Session.Character.Group.IsLeader(Session))
                    {
                        ClientSession chartokick = ServerManager.Instance.GetSessionByCharacterId(rdPacket.CharacterId);
                        if (chartokick.Character?.Group == null)
                            return;

                        chartokick.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("KICK_RAID")), 0));
                        grp = chartokick.Character?.Group;
                        chartokick.SendPacket(chartokick.Character.GenerateRaid(1, true));
                        chartokick.SendPacket(chartokick.Character.GenerateRaid(2, true));
                        grp.LeaveGroup(chartokick);
                        grp.Characters.ForEach(s =>
                        {
                            s.SendPacket(grp.GenerateRdlst());
                            s.SendPacket(s.Character.GenerateRaid(0, false));
                        });
                    }

                    break;
                case 4://disolve
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }
                    if (Session.Character.Group != null && Session.Character.Group.IsLeader(Session))
                    {
                        grp = Session.Character.Group;

                        ClientSession[] grpmembers = new ClientSession[40];
                        grp.Characters.CopyTo(grpmembers);
                        foreach (ClientSession targetSession in grpmembers)
                        {
                            if (targetSession != null)
                            {
                                targetSession.SendPacket(targetSession.Character.GenerateRaid(1, true));
                                targetSession.SendPacket(targetSession.Character.GenerateRaid(2, true));
                                targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("RAID_DISOLVED"), 0));
                                grp.LeaveGroup(targetSession);
                            }
                        }
                        ServerManager.Instance.GroupList.RemoveAll(s => s.GroupId == grp.GroupId);
                        ServerManager.Instance.GroupsThreadSafe.Remove(grp.GroupId);
                    }

                    break;
            }
        }

        /// <summary>
        /// rlPacket packet
        /// </summary>
        /// <param name="rdPacket"></param>
        public void RaidListRegister(RlPacket rlPacket)
        {
            switch (rlPacket.Type)
            {
                case 0:
                    if (Session.Character.Group != null && Session.Character.Group.IsLeader(Session) && Session.Character.Group.GroupType != GroupType.Group && ServerManager.Instance.GroupList.Any(s => s.GroupId == Session.Character.Group.GroupId))
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateRl(1));
                    }
                    else if (Session.Character.Group != null && Session.Character.Group.GroupType != GroupType.Group && Session.Character.Group.IsLeader(Session))
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateRl(2));
                    }
                    else if (Session.Character.Group != null)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateRl(3));
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateRl(0));
                    }
                    break;
                case 1:
                    if (Session.Character.Group != null && Session.Character.Group.GroupType != GroupType.Group && !ServerManager.Instance.GroupList.Any(s => s.GroupId == Session.Character.Group.GroupId))
                    {
                        ServerManager.Instance.GroupList.Add(Session.Character.Group);
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format("RAID_REGISTERED")));
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateRl(1));
                        ServerManager.Instance.Broadcast(Session, $"qnaml 100 #rl {(string.Format(Language.Instance.GetMessageFromKey("SEARCH_TEAM_MEMBERS"), Session.Character.Name))}", ReceiverType.AllExceptGroup);
                    }
                    break;
                case 2:
                    if (Session.Character.Group != null && Session.Character.Group.GroupType != GroupType.Group && ServerManager.Instance.GroupList.Any(s => s.GroupId == Session.Character.Group.GroupId))
                    {
                        ServerManager.Instance.GroupList.Remove(Session.Character.Group);
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format("RAID_UNREGISTERED")));
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateRl(2));
                    }
                    break;
                case 3:
                    ClientSession cl = ServerManager.Instance.GetSessionByCharacterName(rlPacket.CharacterName);
                    if (cl != null)
                    {
                        cl.Character.GroupSentRequestCharacterIds.Add(Session.Character.CharacterId);
                        GroupJoin(new PJoinPacket() { RequestType = GroupRequestType.Accepted, CharacterId = cl.Character.CharacterId });
                    }
                    break;
            }
        }

        /// <summary>
        /// pjoin packet
        /// </summary>
        /// <param name="pjoinPacket"></param>
        public void GroupJoin(PJoinPacket pjoinPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), "Joining group");
            bool createNewGroup = true;
            ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(pjoinPacket.CharacterId);

            if (targetSession == null && !pjoinPacket.RequestType.Equals(GroupRequestType.Sharing))
            {
                return;
            }

            if (pjoinPacket.RequestType.Equals(GroupRequestType.Requested) || pjoinPacket.RequestType.Equals(GroupRequestType.Invited))
            {
                if (pjoinPacket.CharacterId == 0)
                {
                    return;
                }
                if (ServerManager.Instance.IsCharactersGroupFull(pjoinPacket.CharacterId))
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                    return;
                }

                if (ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId) &&
                    ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId))
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_GROUP")));
                    return;
                }

                if (Session.Character.CharacterId != pjoinPacket.CharacterId)
                {
                    if (targetSession != null)
                    {
                        if (Session.Character.IsBlockedByCharacter(pjoinPacket.CharacterId))
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                            return;
                        }

                        if (targetSession.Character.GroupRequestBlocked)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("GROUP_BLOCKED"), 0));
                        }
                        else
                        {
                            // save sent group request to current character
                            Session.Character.GroupSentRequestCharacterIds.Add(targetSession.Character.CharacterId);
                            if (Session.Character.Group == null || Session.Character.Group.GroupType == GroupType.Group)
                            {
                                if (targetSession?.Character?.Group == null || targetSession?.Character?.Group.GroupType == GroupType.Group)
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("GROUP_REQUEST"), targetSession.Character.Name)));
                                    targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#pjoin^3^{ Session.Character.CharacterId} #pjoin^4^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU"), Session.Character.Name)}"));
                                }
                                else
                                {
                                    //can't invite raid member
                                }
                            }
                            else
                            {
                                targetSession.SendPacket($"qna #rd^1^{Session.Character.CharacterId}^1 {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU_RAID"), Session.Character.Name)}");
                            }

                        }
                    }
                }
            }
            else if (pjoinPacket.RequestType.Equals(GroupRequestType.Sharing))
            {
                if (Session.Character.Group != null)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_SHARE_INFO")));
                    Session.Character.Group.Characters.Where(s => s.Character.CharacterId != Session.Character.CharacterId).ToList().ForEach(s =>
                    {
                        s.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#pjoin^6^{ Session.Character.CharacterId} #pjoin^7^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU_SHARE"), Session.Character.Name)}"));
                        Session.Character.GroupSentRequestCharacterIds.Add(s.Character.CharacterId);
                    });
                }
            }
            else if (pjoinPacket.RequestType.Equals(GroupRequestType.Accepted))
            {
                if (!targetSession.Character.GroupSentRequestCharacterIds.Contains(Session.Character.CharacterId))
                {
                    return;
                }
                targetSession.Character.GroupSentRequestCharacterIds.Remove(Session.Character.CharacterId);

                if (ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId) &&
                    ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId))
                {
                    // everyone is in group, return
                    return;
                }

                if (ServerManager.Instance.IsCharactersGroupFull(pjoinPacket.CharacterId)
                    || ServerManager.Instance.IsCharactersGroupFull(Session.Character.CharacterId))
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                    targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                    return;
                }


                // get group and add to group
                if (ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId))
                {
                    // target joins source
                    Group currentGroup = ServerManager.Instance.GetGroupByCharacterId(Session.Character.CharacterId);

                    if (currentGroup != null)
                    {
                        currentGroup.JoinGroup(targetSession);
                        targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("JOINED_GROUP"), 10));
                        createNewGroup = false;
                    }
                }
                else if (ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId))
                {
                    // source joins target
                    Group currentGroup = ServerManager.Instance.GetGroupByCharacterId(pjoinPacket.CharacterId);

                    if (currentGroup != null)
                    {
                        createNewGroup = false;
                        if (currentGroup.GroupType == GroupType.Group)
                        {
                            currentGroup.JoinGroup(Session);
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("RAID_JOIN"), Session.Character.Name), 10));
                            if (Session.Character.Level > currentGroup.Raid?.LevelMaximum || Session.Character.Level < currentGroup.Raid?.LevelMinimum)
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RAID_LEVEL_INCORRECT"), 10));
                                if (Session.Character.Level >= currentGroup.Raid?.LevelMaximum + 10 /* && AlreadySuccededToday*/)
                                {
                                    //modal 1 ALREADY_SUCCEDED_AS_ASSISTANT
                                }
                            }

                            currentGroup.JoinGroup(Session);
                            Session.SendPacket(Session.Character.GenerateRaid(1, false));
                            currentGroup.Characters.ForEach(s =>
                            {
                                s.SendPacket(currentGroup.GenerateRdlst());
                                s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("JOIN_TEAM"), Session.Character.Name), 10));
                                s.SendPacket(s.Character.GenerateRaid(0, false));
                            });
                        }
                    }
                }


                if (createNewGroup)
                {
                    Group group = new Group(GroupType.Group);
                    group.JoinGroup(pjoinPacket.CharacterId);
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("GROUP_JOIN"), targetSession.Character.Name), 10));
                    group.JoinGroup(Session.Character.CharacterId);
                    ServerManager.Instance.AddGroup(group);
                    targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_ADMIN")));

                    // set back reference to group
                    Session.Character.Group = group;
                    targetSession.Character.Group = group;
                }
                if (Session.Character.Group.GroupType == GroupType.Group)
                {
                    // player join group
                    ServerManager.Instance.UpdateGroup(pjoinPacket.CharacterId);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GeneratePidx());
                }
            }
            else if (pjoinPacket.RequestType == GroupRequestType.Declined)
            {
                if (!targetSession.Character.GroupSentRequestCharacterIds.Contains(Session.Character.CharacterId))
                {
                    return;
                }
                targetSession.Character.GroupSentRequestCharacterIds.Remove(Session.Character.CharacterId);

                targetSession.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("REFUSED_GROUP_REQUEST"), Session.Character.Name), 10));
            }
            else if (pjoinPacket.RequestType == GroupRequestType.AcceptedShare)
            {
                if (!targetSession.Character.GroupSentRequestCharacterIds.Contains(Session.Character.CharacterId))
                {
                    return;
                }
                targetSession.Character.GroupSentRequestCharacterIds.Remove(Session.Character.CharacterId);

                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("ACCEPTED_SHARE"), targetSession.Character.Name), 0));
                if (Session.Character.Group.IsMemberOfGroup(pjoinPacket.CharacterId))
                {
                    Session.Character.SetReturnPoint(targetSession.Character.Return.DefaultMapId, targetSession.Character.Return.DefaultX, targetSession.Character.Return.DefaultY);
                    targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("CHANGED_SHARE"), targetSession.Character.Name), 0));
                }
            }
            else if (pjoinPacket.RequestType == GroupRequestType.DeclinedShare)
            {
                if (!targetSession.Character.GroupSentRequestCharacterIds.Contains(Session.Character.CharacterId))
                {
                    return;
                }
                targetSession.Character.GroupSentRequestCharacterIds.Remove(Session.Character.CharacterId);

                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("REFUSED_SHARE"), 0));
            }
        }

        /// <summary>
        /// pleave packet
        /// </summary>
        /// <param name="pleavePacket"></param>
        public void GroupLeave(PLeavePacket pleavePacket)
        {
            ServerManager.Instance.GroupLeave(Session);
        }

        /// <summary>
        /// ; packet
        /// </summary>
        /// <param name="groupSayPacket"></param>
        public void GroupTalk(GroupSayPacket groupSayPacket)
        {
            if (!string.IsNullOrEmpty(groupSayPacket.Message))
            {
                ServerManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(groupSayPacket.Message, 3), ReceiverType.Group);
            }
        }

        /// <summary>
        /// btk packet
        /// </summary>
        /// <param name="btkPacket"></param>
        public void FriendTalk(BtkPacket btkPacket)
        {
            if (string.IsNullOrEmpty(btkPacket.Message))
            {
                return;
            }
            string message = btkPacket.Message;
            if (message.Length > 60)
            {
                message = message.Substring(0, 60);
            }
            message = message.Trim();

            CharacterDTO character = DAOFactory.CharacterDAO.LoadById(btkPacket.CharacterId);
            if (character != null)
            {
                //session is not on current server, check api if the target character is on another server
                int? sentChannelId = CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                {
                    DestinationCharacterId = character.CharacterId,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = PacketFactory.Serialize(Session.Character.GenerateTalk(message)),
                    Type = MessageType.PrivateChat
                });
                if (!sentChannelId.HasValue) //character is even offline on different world
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_OFFLINE")));
                }
            }
        }

        /// <summary>
        /// fdel packet
        /// </summary>
        /// <param name="fDelPacket"></param>
        public void FriendDelete(FDelPacket fDelPacket)
        {
            Session.Character.DeleteRelation(fDelPacket.CharacterId);
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("FRIEND_DELETED")));
        }

        /// <summary>
        /// fins packet
        /// </summary>
        /// <param name="fInsPacket"></param>
        public void FriendAdd(FInsPacket fInsPacket)
        {
            if (!Session.Character.IsFriendlistFull())
            {
                long characterId = fInsPacket.CharacterId;
                if (!Session.Character.IsFriendOfCharacter(characterId))
                {
                    if (!Session.Character.IsBlockedByCharacter(characterId))
                    {
                        if (!Session.Character.IsBlockingCharacter(characterId))
                        {
                            ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                            if (otherSession != null)
                            {
                                if (otherSession.Character.FriendRequestCharacters.Contains(Session.Character.CharacterId))
                                {
                                    switch (fInsPacket.Type)
                                    {
                                        case 1:
                                            Session.Character.AddRelation(characterId, CharacterRelationType.Friend);
                                            Session.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_ADDED")}");
                                            otherSession.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_ADDED")}");
                                            break;

                                        case 2:
                                            otherSession.SendPacket(Language.Instance.GetMessageFromKey("FRIEND_REJECTED"));
                                            break;

                                        default:
                                            if (Session.Character.IsFriendlistFull())
                                            {
                                                Session.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_FULL")}");
                                                otherSession.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_FULL")}");
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    otherSession.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#fins^1^{Session.Character.CharacterId} #fins^2^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("FRIEND_ADD"), Session.Character.Name)}"));
                                    Session.Character.FriendRequestCharacters.Add(characterId);
                                }
                            }
                        }
                        else
                        {
                            Session.SendPacket($"info {Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKING")}");
                        }
                    }
                    else
                    {
                        Session.SendPacket($"info {Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")}");
                    }
                }
                else
                {
                    Session.SendPacket($"info {Language.Instance.GetMessageFromKey("ALREADY_FRIEND")}");
                }
            }
            else
            {
                Session.SendPacket($"info {Language.Instance.GetMessageFromKey("FRIEND_FULL")}");
            }
        }

        /// <summary>
        /// bldel packet
        /// </summary>
        /// <param name="blDelPacket"></param>
        public void BlacklistDelete(BlDelPacket blDelPacket)
        {
            Session.Character.DeleteBlackList(blDelPacket.CharacterId);
            Session.SendPacket(Session.Character.GenerateBlinit());
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_DELETED")));
        }

        /// <summary>
        /// blins packet
        /// </summary>
        /// <param name="blInsPacket"></param>
        public void BlacklistAdd(BlInsPacket blInsPacket)
        {
            Session.Character.AddRelation(blInsPacket.CharacterId, CharacterRelationType.Blocked);
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_ADDED")));
            Session.SendPacket(Session.Character.GenerateBlinit());
        }

        /// <summary>
        ///  guri packet
        /// </summary>
        /// <param name="guriPacket"></param>
        public void Guri(GuriPacket guriPacket)
        {
            if (guriPacket != null)
            {
                if (guriPacket.Type == 10 && guriPacket.Data >= 973 && guriPacket.Data <= 999 && !Session.Character.EmoticonsBlocked)
                {
                    if (Convert.ToInt64(guriPacket.User.Value) == Session.Character.CharacterId)
                    {
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateEff(guriPacket.Data + 4099), ReceiverType.AllNoEmoBlocked);
                    }
                    else
                    {
                        Mate mate = Session.Character.Mates.FirstOrDefault(s => s.MateTransportId == Convert.ToInt32(guriPacket.User.Value));
                        if (mate != null)
                        {
                            Session.CurrentMapInstance?.Broadcast(Session, mate.GenerateEff(guriPacket.Data + 4099), ReceiverType.AllNoEmoBlocked);
                        }
                    }
                }
                else if (guriPacket.Type == 300)
                {
                    if (guriPacket.Argument == 8023)
                    {
                        if (Int16.TryParse(guriPacket.Data.ToString(), out short slot))
                        {
                            ItemInstance box = Session.Character.Inventory.LoadBySlotAndType<BoxInstance>(slot, InventoryType.Equipment);
                            if (box != null)
                            {
                                if (guriPacket.Value.Length == 1)
                                {
                                    box.Item.Use(Session, ref box, 1, new string[] { guriPacket.Value });
                                }
                                else
                                {
                                    box.Item.Use(Session, ref box, 1);
                                }
                            }
                        }
                    }
                }
                else if (guriPacket.Type == 506)
                {
                    if (ServerManager.Instance.EventInWaiting)
                    {
                        Session.Character.IsWaitingForEvent = true;
                    }
                }
                else if (guriPacket.Type == 199 && guriPacket.Argument == 2)
                {
                    short[] listWingOfFriendship = { 2160, 2312, 10048 };
                    short vnumToUse = -1;
                    foreach (short vnum in listWingOfFriendship)
                    {
                        if (Session.Character.Inventory.CountItem(vnum) > 0)
                        {
                            vnumToUse = vnum;
                        }
                    }
                    if (vnumToUse != -1)
                    {
                        if (!Int64.TryParse(guriPacket.User.Value.ToString(), out long charId))
                        {
                            return;
                        }
                        ClientSession session = ServerManager.Instance.GetSessionByCharacterId(charId);
                        if (session != null)
                        {
                            if (Session.Character.IsFriendOfCharacter(charId))
                            {
                                if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                                {
                                    if (Session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
                                    {
                                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                                        return;
                                    }
                                    short mapy = session.Character.PositionY;
                                    short mapx = session.Character.PositionX;
                                    short mapId = session.Character.MapInstance.Map.MapId;

                                    ServerManager.Instance.ChangeMap(Session.Character.CharacterId, mapId, mapx, mapy);
                                    Session.Character.Inventory.RemoveItemAmount(vnumToUse);
                                }
                                else
                                {
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("USER_ON_INSTANCEMAP"), 0));
                                }
                            }
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_WINGS"), 10));
                    }
                }
                else if (guriPacket.Type == 400)
                {
                    if (guriPacket.User.HasValue)
                    {
                        if (!Int16.TryParse(guriPacket.User.Value.ToString(), out short MapNpcId))
                        {
                            return;
                        }
                        if (!Session.HasCurrentMapInstance)
                        {
                            return;
                        }
                        MapNpc npc = Session.CurrentMapInstance.Npcs.FirstOrDefault(n => n.MapNpcId.Equals(MapNpcId));
                        if (npc != null)
                        {
                            NpcMonster mapobject = ServerManager.Instance.GetNpc(npc.NpcVNum);

                            int RateDrop = ServerManager.Instance.DropRate;
                            int delay = (int)Math.Round((3 + mapobject.RespawnTime / 1000d) * Session.Character.TimesUsed);
                            delay = delay > 11 ? 8 : delay;
                            if (Session.Character.LastMapObject.AddSeconds(delay) < DateTime.Now)
                            {
                                if (mapobject.Drops.Any(s => s.MonsterVNum != null))
                                {
                                    if (mapobject.VNumRequired > 10 && Session.Character.Inventory.CountItem(mapobject.VNumRequired) < mapobject.AmountRequired)
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEM"), 0));
                                        return;
                                    }
                                }
                                Random random = new Random();
                                double randomAmount = ServerManager.Instance.RandomNumber() * random.NextDouble();
                                DropDTO drop = mapobject.Drops.FirstOrDefault(s => s.MonsterVNum == npc.NpcVNum);
                                if (drop != null)
                                {
                                    int dropChance = drop.DropChance;
                                    if (randomAmount <= (double)dropChance * RateDrop / 5000.000)
                                    {
                                        short vnum = drop.ItemVNum;
                                        ItemInstance newInv = Session.Character.Inventory.AddNewToInventory(vnum).FirstOrDefault();
                                        Session.Character.LastMapObject = DateTime.Now;
                                        Session.Character.TimesUsed++;
                                        if (Session.Character.TimesUsed >= 4)
                                        {
                                            Session.Character.TimesUsed = 0;
                                        }
                                        if (newInv != null)
                                        {
                                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), newInv.Item.Name), 0));
                                            Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), newInv.Item.Name), 11));
                                        }
                                        else
                                        {
                                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                        }
                                    }
                                    else
                                    {
                                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("TRY_FAILED"), 0));
                                    }
                                }
                            }
                            else
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("TRY_FAILED_WAIT"), (int)(Session.Character.LastMapObject.AddSeconds(delay) - DateTime.Now).TotalSeconds), 0));
                            }
                        }
                    }
                }
                else if (guriPacket.Type == 710)
                {
                    if (guriPacket.Value != null)
                    {
                        // MapNpc npc = Session.CurrentMapInstance.Npcs.FirstOrDefault(n =>
                        // n.MapNpcId.Equals(Convert.ToInt16(guriPacket.Data)); NpcMonster mapObject
                        // = ServerManager.Instance.GetNpc(npc.NpcVNum); teleport free
                    }
                }
                else if (guriPacket.Type == 750)
                {
                    if (!guriPacket.User.HasValue)
                    {
                        const short baseVnum = 1623;
                        if (Int16.TryParse(guriPacket.Argument.ToString(), out short faction))
                        {
                            if (Session.Character.Inventory.CountItem(baseVnum + faction) > 0)
                            {
                                Session.Character.Faction = faction;
                                Session.Character.Inventory.RemoveItemAmount(baseVnum + faction);
                                Session.SendPacket("scr 0 0 0 0 0 0 0");
                                Session.SendPacket(Session.Character.GenerateFaction());
                                Session.SendPacket(Session.Character.GenerateEff(4799 + faction));
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{faction}"), 0));
                            }
                        }
                    }
                }
                else if (guriPacket.Type == 2)
                {
                    Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(2, 1, Session.Character.CharacterId), Session.Character.PositionX, Session.Character.PositionY);
                }
                else if (guriPacket.Type == 4)
                {
                    const int speakerVNum = 2173;
                    const int petnameVNum = 2157;
                    if (guriPacket.Argument == 1)
                    {
                        Mate mate = Session.Character.Mates.FirstOrDefault(s => s.MateTransportId == guriPacket.Data);
                        if (mate != null)
                        {
                            mate.Name = guriPacket.Value;
                            Session.CurrentMapInstance.Broadcast(mate.GenerateOut());
                            Session.CurrentMapInstance.Broadcast(mate.GenerateIn());
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NEW_NAME_PET")));
                            Session.SendPacket(Session.Character.GeneratePinit());
                            Session.SendPackets(Session.Character.GeneratePst());
                            Session.SendPackets(Session.Character.GenerateScP());
                            Session.Character.Inventory.RemoveItemAmount(petnameVNum);
                        }
                    }

                    // presentation message
                    if (guriPacket.Argument == 2)
                    {
                        int presentationVNum = Session.Character.Inventory.CountItem(1117) > 0 ? 1117 : (Session.Character.Inventory.CountItem(9013) > 0 ? 9013 : -1);
                        if (presentationVNum != -1)
                        {
                            string message = string.Empty;

                            // message = $" ";
                            string[] valuesplit = guriPacket.Value.Split(' ');
                            for (int i = 0; i < valuesplit.Length; i++)
                            {
                                message += valuesplit[i] + "^";
                            }
                            message = message.Substring(0, message.Length - 1); // Remove the last ^
                            message = message.Trim();
                            if (message.Length > 60)
                            {
                                message = message.Substring(0, 60);
                            }

                            Session.Character.Biography = message;
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("INTRODUCTION_SET"), 10));
                            Session.Character.Inventory.RemoveItemAmount(presentationVNum);
                        }
                    }

                    // Speaker
                    if (guriPacket.Argument == 3)
                    {
                        if (Session.Character.Inventory.CountItem(speakerVNum) > 0)
                        {
                            string message = $"<{Language.Instance.GetMessageFromKey("SPEAKER")}> [{Session.Character.Name}]:";
                            string[] valuesplit = guriPacket.Value.Split(' ');
                            for (int i = 0; i < valuesplit.Length; i++)
                            {
                                message += valuesplit[i] + " ";
                            }
                            if (message.Length > 120)
                            {
                                message = message.Substring(0, 120);
                            }

                            message = message.Trim();

                            if (Session.Character.IsMuted())
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SPEAKER_CANT_BE_USED"), 10));
                                return;
                            }
                            Session.Character.Inventory.RemoveItemAmount(speakerVNum);
                            ServerManager.Instance.Broadcast(Session.Character.GenerateSay(message, 13));
                        }
                    }
                }
                else if (guriPacket.Type == 199 && guriPacket.Argument == 1)
                {
                    if (Int64.TryParse(guriPacket.User.Value.ToString(), out long charId))
                    {
                        if (!Session.Character.IsFriendOfCharacter(charId))
                        {
                            Session.SendPacket(Language.Instance.GetMessageFromKey("CHARACTER_NOT_IN_FRIENDLIST"));
                            return;
                        }
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(3000, 4, $"#guri^199^2^{guriPacket.User.Value}"));
                    }
                }
                else if (guriPacket.Type == 201)
                {
                    if (Session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBasket))
                    {
                        Session.SendPacket(Session.Character.GenerateStashAll());
                    }
                }
                else if (guriPacket.Type == 202)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PARTNER_BACKPACK"), 10));
                    Session.SendPacket(Session.Character.GeneratePStashAll());
                }
                else if (guriPacket.Type == 208 && guriPacket.Argument == 0)
                {
                    if (short.TryParse(guriPacket.User.Value.ToString(), out short pearlSlot) && short.TryParse(guriPacket.Value, out short mountSlot))
                    {
                        ItemInstance mount = Session.Character.Inventory.LoadBySlotAndType<ItemInstance>(mountSlot, InventoryType.Main);
                        BoxInstance pearl = Session.Character.Inventory.LoadBySlotAndType<BoxInstance>(pearlSlot, InventoryType.Equipment);
                        if (mount != null && pearl != null)
                        {
                            pearl.HoldingVNum = mount.ItemVNum;
                            Session.Character.Inventory.RemoveItemAmountFromInventory(1, mount.Id);
                        }
                    }
                }
                else if (guriPacket.Type == 209 && guriPacket.Argument == 0)
                {
                    if (short.TryParse(guriPacket.User.Value.ToString(), out short pearlSlot) && short.TryParse(guriPacket.Value, out short mountSlot))
                    {
                        WearableInstance fairy = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>(mountSlot, InventoryType.Equipment);
                        BoxInstance pearl = Session.Character.Inventory.LoadBySlotAndType<BoxInstance>(pearlSlot, InventoryType.Equipment);
                        if (fairy != null && pearl != null)
                        {
                            pearl.HoldingVNum = fairy.ItemVNum;
                            pearl.ElementRate = fairy.ElementRate;
                            Session.Character.Inventory.RemoveItemAmountFromInventory(1, fairy.Id);
                        }
                    }
                }
                else if (guriPacket.Type == 203 && guriPacket.Argument == 0)
                {
                    // SP points initialization
                    int[] listPotionResetVNums = { 1366, 1427, 5115, 9040 };
                    int vnumToUse = -1;
                    foreach (int vnum in listPotionResetVNums)
                    {
                        if (Session.Character.Inventory.CountItem(vnum) > 0)
                        {
                            vnumToUse = vnum;
                        }
                    }
                    if (vnumToUse != -1)
                    {
                        if (Session.Character.UseSp)
                        {
                            SpecialistInstance specialistInstance = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>((byte)EquipmentType.Sp, InventoryType.Wear);
                            if (specialistInstance != null)
                            {
                                specialistInstance.SlDamage = 0;
                                specialistInstance.SlDefence = 0;
                                specialistInstance.SlElement = 0;
                                specialistInstance.SlHP = 0;

                                specialistInstance.DamageMinimum = 0;
                                specialistInstance.DamageMaximum = 0;
                                specialistInstance.HitRate = 0;
                                specialistInstance.CriticalLuckRate = 0;
                                specialistInstance.CriticalRate = 0;
                                specialistInstance.DefenceDodge = 0;
                                specialistInstance.DistanceDefenceDodge = 0;
                                specialistInstance.ElementRate = 0;
                                specialistInstance.DarkResistance = 0;
                                specialistInstance.LightResistance = 0;
                                specialistInstance.FireResistance = 0;
                                specialistInstance.WaterResistance = 0;
                                specialistInstance.CriticalDodge = 0;
                                specialistInstance.CloseDefence = 0;
                                specialistInstance.DistanceDefence = 0;
                                specialistInstance.MagicDefence = 0;
                                specialistInstance.HP = 0;
                                specialistInstance.MP = 0;

                                Session.Character.Inventory.RemoveItemAmount(vnumToUse);
                                Session.Character.Inventory.DeleteFromSlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                                Session.Character.Inventory.AddToInventoryWithSlotAndType(specialistInstance, InventoryType.Wear, (byte)EquipmentType.Sp);
                                Session.SendPacket(Session.Character.GenerateCond());
                                Session.SendPacket(specialistInstance.GenerateSlInfo());
                                Session.SendPacket(Session.Character.GenerateLev());
                                Session.SendPacket(Session.Character.GenerateStatChar());
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_RESET"), 0));
                            }
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORMATION_NEEDED"), 10));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_POINTS"), 10));
                    }
                }
            }
        }

        /// <summary>
        /// hero packet
        /// </summary>
        /// <param name="heroPacket"></param>
        public void Hero(HeroPacket heroPacket)
        {
            if (string.IsNullOrEmpty(heroPacket.Message))
            {
                return;
            }
            if (Session.Character.IsReputHero() >= 3)
            {
                heroPacket.Message = heroPacket.Message.Trim();
                ServerManager.Instance.Broadcast(Session, $"msg 5 [{Session.Character.Name}]:{heroPacket.Message}", ReceiverType.AllNoHeroBlocked);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_HERO"), 11));
            }
        }

        /// <summary>
        /// RstartPacket packet
        /// </summary>
        /// <param name="rStartPacket"></param>
        public void GetRStart(RStartPacket rStartPacket)
        {
            if (rStartPacket.Type == 1)
            {
                Session.CurrentMapInstance.InstanceBag.Lock = true;
                Preq(new PreqPacket());
            }
        }

        /// <summary>
        /// PreqPacket packet
        /// </summary>
        /// <param name="packet"></param>
        public void Preq(PreqPacket packet)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), packet.ToString());
            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
            double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
            if (!(timeSpanSinceLastPortal >= 4) || !Session.HasCurrentMapInstance)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                return;
            }
            Parallel.ForEach(Session.CurrentMapInstance.Portals.Concat(Session.Character.GetExtraPortal()), portal =>
            {
                if (Session.Character.PositionY >= portal.SourceY - 1 && Session.Character.PositionY <= portal.SourceY + 1
                    && Session.Character.PositionX >= portal.SourceX - 1 && Session.Character.PositionX <= portal.SourceX + 1)
                {
                    switch (portal.Type)
                    {
                        case (sbyte)PortalType.MapPortal:
                        case (sbyte)PortalType.TSNormal:
                        case (sbyte)PortalType.Open:
                        case (sbyte)PortalType.Miniland:
                        case (sbyte)PortalType.TSEnd:
                        case (sbyte)PortalType.Exit:
                        case (sbyte)PortalType.Effect:
                        case (sbyte)PortalType.ShopTeleport:
                            break;
                        case (sbyte)PortalType.Raid:
                            if (Session.Character.Group?.Raid != null)
                            {
                                if (Session.Character.Group.IsLeader(Session))
                                {
                                    Session.SendPacket($"qna #mkraid^0^275 {Language.Instance.GetMessageFromKey("DO_YOU_WANT_RAID")}");
                                }
                                else
                                {
                                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_TEAM_LEADER_CAN_START"), 10));
                                }
                            }
                            else
                            {
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NEED_TEAM"), 10));
                            }
                            return;
                        default:
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"), 10));
                            return;
                    }

                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance && !Session.CurrentMapInstance.InstanceBag.Lock)
                    {
                        if (Session.Character.CharacterId == Session.CurrentMapInstance.InstanceBag.Creator)
                        {
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#rstart^1 rstart {Language.Instance.GetMessageFromKey("ASK_ENTRY_IN_FIRST_ROOM")}"));
                        }
                        return;
                    }
                    portal.OnTraversalEvents.ForEach(e =>
                    {
                        EventHelper.Instance.RunEvent(e);
                    });
                    if (portal.DestinationMapInstanceId == default(Guid))
                    {
                        return;
                    }
                    Session.SendPacket(Session.CurrentMapInstance.GenerateRsfn());

                    Session.Character.LastPortal = currentRunningSeconds;

                    if (ServerManager.Instance.GetMapInstance(portal.SourceMapInstanceId).MapInstanceType != MapInstanceType.BaseMapInstance && ServerManager.Instance.GetMapInstance(portal.DestinationMapInstanceId).MapInstanceType == MapInstanceType.BaseMapInstance)
                    {
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId, Session.Character.MapX, Session.Character.MapY);
                    }
                    else if (portal.DestinationMapInstanceId == Session.Character.Miniland.MapInstanceId)
                    {
                        ServerManager.Instance.JoinMiniland(Session, Session);
                    }
                    else
                    {
                        ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, portal.DestinationMapInstanceId, portal.DestinationX, portal.DestinationY);
                    }
                }
            });
        }

        /// <summary>
        /// pulse packet
        /// </summary>
        /// <param name="pulsepacket"></param>
        public void Pulse(PulsePacket pulsepacket)
        {
            Session.Character.LastPulse += 60;
            if (pulsepacket.Tick != Session.Character.LastPulse)
            {
                Session.Disconnect();
            }
            Session.Character.DeleteTimeout();
            CommunicationServiceClient.Instance.PulseAccount(Session.Account.AccountId);
        }

        /// <summary>
        /// req_info packet
        /// </summary>
        /// <param name="reqInfoPacket"></param>
        public void ReqInfo(ReqInfoPacket reqInfoPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), reqInfoPacket.ToString());
            if (reqInfoPacket.Type == 6)
            {
                if (reqInfoPacket.MateVNum.HasValue)
                {
                    Mate mate = Session.CurrentMapInstance.Sessions.FirstOrDefault(s => s.Character?.Mates != null && s.Character.Mates.Any(o => o.MateTransportId == reqInfoPacket.MateVNum.Value))?.Character.Mates.Find(o => o.MateTransportId == reqInfoPacket.MateVNum.Value);
                    Session.SendPacket(mate?.GenerateEInfo());
                }
            }
            else if (reqInfoPacket.Type == 5)
            {
                NpcMonster npc = ServerManager.Instance.GetNpc((short)reqInfoPacket.TargetVNum);
                if (npc != null)
                {
                    Session.SendPacket(npc.GenerateEInfo());
                }
            }
            else
            {
                Session.SendPacket(ServerManager.Instance.GetSessionByCharacterId(reqInfoPacket.TargetVNum)?.Character?.GenerateReqInfo());
            }
        }

        /// <summary>
        /// rest packet
        /// </summary>
        /// <param name="sitpacket"></param>
        public void Rest(SitPacket sitpacket)
        {
            sitpacket.Users.ForEach(u =>
            {
                if (u.UserType == 1)
                {
                    Session.Character.Rest();
                }
                else
                {
                    Session.CurrentMapInstance.Broadcast(Session.Character.Mates.FirstOrDefault(s => s.MateTransportId == (int)u.UserId)?.GenerateRest());
                }
            });
        }

        /// <summary>
        /// revival packet
        /// </summary>
        /// <param name="revivalPacket"></param>
        public void Revive(RevivalPacket revivalPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), revivalPacket.ToString());
            if (Session.Character.Hp > 0)
            {
                return;
            }
            switch (revivalPacket.Type)
            {
                case 0:
                    switch (Session.CurrentMapInstance.MapInstanceType)
                    {
                        case MapInstanceType.LodInstance:
                            const int saver = 1211;
                            if (Session.Character.Inventory.CountItem(saver) < 1)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_SAVER"), 0));
                                ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            }
                            else
                            {
                                Session.Character.Inventory.RemoveItemAmount(saver);
                                Session.Character.Hp = (int)Session.Character.HPLoad();
                                Session.Character.Mp = (int)Session.Character.MPLoad();
                                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateRevive());
                                Session.SendPacket(Session.Character.GenerateStat());
                            }
                            break;

                        default:
                            const int seed = 1012;
                            if (Session.Character.Inventory.CountItem(seed) < 10 && Session.Character.Level > 20)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_POWER_SEED"), 0));
                                ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_SEED_SAY"), 0));
                            }
                            else
                            {
                                if (Session.Character.Level > 20)
                                {
                                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("SEED_USED"), 10), 10));
                                    Session.Character.Inventory.RemoveItemAmount(seed, 10);
                                    Session.Character.Hp = (int)(Session.Character.HPLoad() / 2);
                                    Session.Character.Mp = (int)(Session.Character.MPLoad() / 2);
                                }
                                else
                                {
                                    Session.Character.Hp = (int)Session.Character.HPLoad();
                                    Session.Character.Mp = (int)Session.Character.MPLoad();
                                }
                                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateTp());
                                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateRevive());
                                Session.SendPacket(Session.Character.GenerateStat());
                            }
                            break;
                    }
                    break;

                case 1:
                    ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                    break;

                case 2:
                    if (Session.Character.Gold >= 100)
                    {
                        Session.Character.Hp = (int)Session.Character.HPLoad();
                        Session.Character.Mp = (int)Session.Character.MPLoad();
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateTp());
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateRevive());
                        Session.SendPacket(Session.Character.GenerateStat());
                        Session.Character.Gold -= 100;
                        Session.SendPacket(Session.Character.GenerateGold());
                        Session.Character.LastPVPRevive = DateTime.Now;
                    }
                    else
                    {
                        ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                    }
                    break;
            }
        }

        /// <summary>
        /// say packet
        /// </summary>
        /// <param name="sayPacket"></param>
        public void Say(SayPacket sayPacket)
        {
            if (string.IsNullOrEmpty(sayPacket.Message))
            {
                return;
            }
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            string message = sayPacket.Message;
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 12));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 12));
                }
            }
            else
            {
                byte type = 0;
                if (Session.Character.Authority == AuthorityType.Moderator)
                {
                    type = 12;
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(message.Trim(), 1), ReceiverType.AllExceptMe);
                    message = $"[{Language.Instance.GetMessageFromKey("SUPPORT")} {Session.Character.Name}]: " + message;
                }
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateSay(message.Trim(), type), ReceiverType.AllExceptMe);
            }
        }

        /// <summary>
        /// pst packet
        /// </summary>
        /// <param name="pstPacket"></param>
        public void SendMail(PstPacket pstpacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), pstpacket.ToString());
            if (pstpacket != null)
            {
                if (pstpacket.Data != null)
                {
                    CharacterDTO Receiver = DAOFactory.CharacterDAO.LoadByName(pstpacket.Receiver);
                    if (Receiver != null)
                    {
                        string[] datasplit = pstpacket.Data.Split(' ');
                        WearableInstance headWearable = Session.Character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Hat, InventoryType.Wear);
                        byte color = headWearable != null && headWearable.Item.IsColored ? (byte)headWearable.Design : (byte)Session.Character.HairColor;
                        MailDTO mailcopy = new MailDTO
                        {
                            AttachmentAmount = 0,
                            IsOpened = false,
                            Date = DateTime.Now,
                            Title = datasplit[0],
                            Message = datasplit[1],
                            ReceiverId = Receiver.CharacterId,
                            SenderId = Session.Character.CharacterId,
                            IsSenderCopy = true,
                            SenderClass = Session.Character.Class,
                            SenderGender = Session.Character.Gender,
                            SenderHairColor = Enum.IsDefined(typeof(HairColorType), color) ? (HairColorType)color : 0,
                            SenderHairStyle = Session.Character.HairStyle,
                            EqPacket = Session.Character.GenerateEqListForPacket(),
                            SenderMorphId = Session.Character.Morph == 0 ? (short)-1 : (short)(Session.Character.Morph > short.MaxValue ? 0 : Session.Character.Morph)
                        };
                        MailDTO mail = new MailDTO
                        {
                            AttachmentAmount = 0,
                            IsOpened = false,
                            Date = DateTime.Now,
                            Title = datasplit[0],
                            Message = datasplit[1],
                            ReceiverId = Receiver.CharacterId,
                            SenderId = Session.Character.CharacterId,
                            IsSenderCopy = false,
                            SenderClass = Session.Character.Class,
                            SenderGender = Session.Character.Gender,
                            SenderHairColor = Enum.IsDefined(typeof(HairColorType), color) ? (HairColorType)color : 0,
                            SenderHairStyle = Session.Character.HairStyle,
                            EqPacket = Session.Character.GenerateEqListForPacket(),
                            SenderMorphId = Session.Character.Morph == 0 ? (short)-1 : (short)(Session.Character.Morph > short.MaxValue ? 0 : Session.Character.Morph)
                        };

                        DAOFactory.MailDAO.InsertOrUpdate(ref mailcopy);
                        DAOFactory.MailDAO.InsertOrUpdate(ref mail);

                        Session.Character.MailList.Add((Session.Character.MailList.Any() ? Session.Character.MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mailcopy);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAILED"), 11));
                        Session.SendPacket(Session.Character.GeneratePost(mailcopy, 2));
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                }
                if(!pstpacket.Unknow1.HasValue)
                {
                    if (int.TryParse(pstpacket.Id.ToString(), out int id) && byte.TryParse(pstpacket.Type.ToString(), out byte type))
                    {
                        if (pstpacket.Argument == 3)
                        {
                            if (Session.Character.MailList.ContainsKey(id))
                            {
                                if (!Session.Character.MailList[id].IsOpened)
                                {
                                    Session.Character.MailList[id].IsOpened = true;
                                    MailDTO mailupdate = Session.Character.MailList[id];
                                    DAOFactory.MailDAO.InsertOrUpdate(ref mailupdate);
                                }
                                Session.SendPacket(Session.Character.GeneratePostMessage(Session.Character.MailList[id], type));
                            }
                        }
                        else if (pstpacket.Argument == 2)
                        {
                            if (Session.Character.MailList.ContainsKey(id))
                            {
                                MailDTO mail = Session.Character.MailList[id];
                                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAIL_DELETED"), 11));
                                Session.SendPacket($"post 2 {type} {id}");
                                if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                                {
                                    DAOFactory.MailDAO.DeleteById(mail.MailId);
                                }
                                if (Session.Character.MailList.ContainsKey(id))
                                {
                                    Session.Character.MailList.Remove(id);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// qset packet
        /// </summary>
        /// <param name="qSetPacket"></param>
        public void SetQuicklist(QSetPacket qSetPacket)
        {
            Logger.Debug(Session.Character.GenerateIdentity(), qSetPacket.ToString());
            short data1 = 0, data2 = 0, type = qSetPacket.Type, q1 = qSetPacket.Q1, q2 = qSetPacket.Q2;
            if (qSetPacket.Data1.HasValue)
            {
                data1 = qSetPacket.Data1.Value;
            }
            if (qSetPacket.Data2.HasValue)
            {
                data2 = qSetPacket.Data2.Value;
            }
            switch (type)
            {
                case 0:
                case 1:

                    // client says qset 0 1 3 2 6 answer -> qset 1 3 0.2.6.0
                    Session.Character.QuicklistEntries.RemoveAll(n => n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                    Session.Character.QuicklistEntries.Add(new QuicklistEntryDTO
                    {
                        CharacterId = Session.Character.CharacterId,
                        Type = type,
                        Q1 = q1,
                        Q2 = q2,
                        Slot = data1,
                        Pos = data2,
                        Morph = Session.Character.UseSp ? (short)Session.Character.Morph : (short)0
                    });

                    Session.SendPacket($"qset {q1} {q2} {type}.{data1}.{data2}.0");
                    break;

                case 2:

                    // DragDrop / Reorder qset type to1 to2 from1 from2 vars -> q1 q2 data1 data2
                    QuicklistEntryDTO qlFrom = Session.Character.QuicklistEntries.SingleOrDefault(n => n.Q1 == data1 && n.Q2 == data2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                    if (qlFrom != null)
                    {
                        QuicklistEntryDTO qlTo = Session.Character.QuicklistEntries.SingleOrDefault(n => n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                        qlFrom.Q1 = q1;
                        qlFrom.Q2 = q2;

                        if (qlTo == null)
                        {
                            // Put 'from' to new position (datax)
                            Session.SendPacket($"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                            // old 'from' is now empty.
                            Session.SendPacket($"qset {data1} {data2} 7.7.-1.0");
                        }
                        else
                        {
                            // Put 'from' to new position (datax)
                            Session.SendPacket($"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                            // 'from' is now 'to' because they exchanged
                            qlTo.Q1 = data1;
                            qlTo.Q2 = data2;
                            Session.SendPacket($"qset {qlTo.Q1} {qlTo.Q2} {qlTo.Type}.{qlTo.Slot}.{qlTo.Pos}.0");
                        }
                    }

                    break;

                case 3:

                    // Remove from Quicklist
                    Session.Character.QuicklistEntries.RemoveAll(n => n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));
                    Session.SendPacket($"qset {q1} {q2} 7.7.-1.0");
                    break;

                default:
                    return;
            }
        }

        /// <summary>
        /// game_start packet
        /// </summary>
        /// <param name="gameStartPacket"></param>
        public void StartGame(GameStartPacket gameStartPacket)
        {
            if (Session.IsOnMap || !Session.HasSelectedCharacter)
            {
                // character should have been selected in SelectCharacter
                return;
            }
            Session.CurrentMapInstance = Session.Character.MapInstance;
            if (ConfigurationManager.AppSettings["SceneOnCreate"].ToLower() == "true" & Session.Character.GeneralLogs.Count(s => s.LogType == "Connection") < 2)
            {
                Session.SendPacket("scene 40");
            }
            if (ConfigurationManager.AppSettings["WorldInformation"].ToLower() == "true")
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                string productVersion = assembly != null && assembly.Location != null ? FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion : "1337";
                Session.SendPacket(Session.Character.GenerateSay("----------[World Information]----------", 10));
                Session.SendPacket(Session.Character.GenerateSay($"OpenNos by OpenNos Team\nVersion : v{productVersion}", 11));
                Session.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 10));
            }
            Session.Character.LoadSpeed();
            Session.Character.LoadSkills();
            Session.SendPacket(Session.Character.GenerateTit());
            Session.SendPacket(Session.Character.GenerateSpPoint());
            Session.SendPacket("rsfi 1 1 0 9 0 9");
            if (Session.Character.Hp <= 0)
            {
                ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
            }
            else
            {
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
            }
            Session.SendPacket(Session.Character.GenerateSki());
            Session.SendPacket($"fd {Session.Character.Reput} 0 {(int)Session.Character.Dignity} {Math.Abs(Session.Character.GetDignityIco())}");
            Session.SendPacket(Session.Character.GenerateFd());
            Session.SendPacket("rage 0 250000");
            Session.SendPacket("rank_cool 0 0 18000");
            SpecialistInstance specialistInstance = Session.Character.Inventory.LoadBySlotAndType<SpecialistInstance>(8, InventoryType.Wear);
            StaticBonusDTO medal = Session.Character.StaticBonusList.FirstOrDefault(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
            if (medal != null)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOGIN_MEDAL"), 12));
            }

            if (Session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBasket))
            {
                Session.SendPacket("ib 1278 1");
            }

            if (Session.Character.MapInstance.Map.MapTypes.Any(m => m.MapTypeId == (short)MapTypeEnum.CleftOfDarkness))
            {
                Session.SendPacket("bc 0 0 0");
            }
            if (specialistInstance != null)
            {
                Session.SendPacket(Session.Character.GenerateSpPoint());
            }
            Session.SendPacket("scr 0 0 0 0 0 0");
            for (int i = 0; i < 10; i++)
            {
                Session.SendPacket($"bn {i} {Language.Instance.GetMessageFromKey($"BN{i}")}");
            }
            Session.SendPacket(Session.Character.GenerateExts());
            Session.SendPacket(Session.Character.GenerateMlinfo());
            Session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());

            Session.SendPacket(Session.Character.GeneratePinit());
            Session.SendPackets(Session.Character.GeneratePst());

            Session.SendPacket("zzim");
            Session.SendPacket($"twk 2 {Session.Character.CharacterId} {Session.Account.Name} {Session.Character.Name} shtmxpdlfeoqkr");

            // qstlist target sqst bf
            Session.SendPacket("act6");
            Session.SendPacket(Session.Character.GenerateFaction());
            Session.SendPackets(Session.Character.GenerateScP());
            Session.SendPackets(Session.Character.GenerateScN());
#pragma warning disable 618
            Session.Character.GenerateStartupInventory();
#pragma warning restore 618

            Session.SendPacket(Session.Character.GenerateGold());
            Session.SendPackets(Session.Character.GenerateQuicklist());

            string clinit = "clinit";
            string flinit = "flinit";
            string kdlinit = "kdlinit";
            foreach (CharacterDTO character in ServerManager.Instance.TopComplimented)
            {
                clinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Compliment}|{character.Name}";
            }
            foreach (CharacterDTO character in ServerManager.Instance.TopReputation)
            {
                flinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Reput}|{character.Name}";
            }
            foreach (CharacterDTO character in ServerManager.Instance.TopPoints)
            {
                kdlinit += $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Act4Points}|{character.Name}";
            }

            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());

            Session.SendPacket(Session.Character.GenerateFinit());
            Session.SendPacket(Session.Character.GenerateBlinit());
            Session.SendPacket(clinit);
            Session.SendPacket(flinit);
            Session.SendPacket(kdlinit);

            Session.Character.LastPVPRevive = DateTime.Now;

            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                Session.SendPacket(Session.Character.GenerateGInfo());
                Session.SendPackets(Session.Character.GetFamilyHistory());
                Session.SendPacket(Session.Character.GenerateFamilyMember());
                Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                Session.SendPacket(Session.Character.GenerateFamilyMemberExp());
                if (!string.IsNullOrWhiteSpace(Session.Character.Family.FamilyMessage))
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo("--- Family Message ---\n" + Session.Character.Family.FamilyMessage));
                }
            }

            IEnumerable<PenaltyLogDTO> warning = DAOFactory.PenaltyLogDAO.LoadByAccount(Session.Character.AccountId).Where(p => p.Penalty == PenaltyType.Warning);
            if (warning.Any())
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("WARNING_INFO"), warning.Count())));
            }

            // finfo - friends info
            Session.Character.RefreshMail();
            Session.Character.LoadSentMail();
            Session.Character.DeleteTimeout();

            foreach (StaticBuffDTO sb in DAOFactory.StaticBuffDAO.LoadByCharacterId(Session.Character.CharacterId))
            {
                Session.Character.AddStaticBuff(sb);
            }
        }

        /// <summary>
        /// walk packet
        /// </summary>
        /// <param name="walkPacket"></param>
        public void Walk(WalkPacket walkPacket)
        {
            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
            double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
            int distance = Map.GetDistance(new MapCell { X = Session.Character.PositionX, Y = Session.Character.PositionY }, new MapCell { X = walkPacket.XCoordinate, Y = walkPacket.YCoordinate });

            if (Session.HasCurrentMapInstance && !Session.CurrentMapInstance.Map.IsBlockedZone(walkPacket.XCoordinate, walkPacket.YCoordinate) && !Session.Character.IsChangingMapInstance && !Session.Character.HasShopOpened)
            {
                if ((Session.Character.Speed >= walkPacket.Speed || Session.Character.LastSpeedChange.AddSeconds(1) > DateTime.Now) && !(distance > 60 && timeSpanSinceLastPortal > 10))
                {
                    if (Session.Character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                    {
                        Session.Character.MapX = walkPacket.XCoordinate;
                        Session.Character.MapY = walkPacket.YCoordinate;
                    }
                    Session.Character.PositionX = walkPacket.XCoordinate;
                    Session.Character.PositionY = walkPacket.YCoordinate;
                    Session.Character.BrushFire = BestFirstSearch.LoadBrushFire(new GridPos()
                    {
                        X = Session.Character.PositionX,
                        Y = Session.Character.PositionY
                    }, Session.CurrentMapInstance.Map.Grid);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateMv());
                    Session.SendPacket(Session.Character.GenerateCond());
                    Session.Character.LastMove = DateTime.Now;

                    Session.CurrentMapInstance?.OnAreaEntryEvents?.Where(s => s.InZone(Session.Character.PositionX, Session.Character.PositionY)).ToList().ForEach(e =>
                    {
                        e.Events.ForEach(evt => EventHelper.Instance.RunEvent(evt));
                    });
                    Session.CurrentMapInstance?.OnAreaEntryEvents?.RemoveAll(s => s.InZone(Session.Character.PositionX, Session.Character.PositionY));

                    Session.CurrentMapInstance?.OnMoveOnMapEvents?.ForEach(e =>
                    {
                        EventHelper.Instance.RunEvent(e);
                    });
                    Session.CurrentMapInstance?.OnMoveOnMapEvents?.RemoveAll(s => s != null);
                }
                else
                {
                    Session.Disconnect();
                }
            }
        }

        /// <summary>
        /// / packet
        /// </summary>
        /// <param name="whisperPacket"></param>
        public void Whisper(WhisperPacket whisperPacket)
        {
            try
            {
                // TODO: Implement WhisperSupport
                if (string.IsNullOrEmpty(whisperPacket.Message))
                {
                    return;
                }
                string characterName = whisperPacket.Message.Split(' ')[whisperPacket.Message.StartsWith("GM ") ? 1 : 0];
                string message = string.Empty;
                string[] packetsplit = whisperPacket.Message.Split(' ');
                for (int i = packetsplit[0] == "GM" ? 2 : 1; i < packetsplit.Length; i++)
                {
                    message += packetsplit[i] + " ";
                }
                if (message.Length > 60)
                {
                    message = message.Substring(0, 60);
                }
                message = message.Trim();

                Session.SendPacket(Session.Character.GenerateSpk(message, 5));
                CharacterDTO receiver = DAOFactory.CharacterDAO.LoadByName(characterName);
                if (receiver.CharacterId == Session.Character.CharacterId)
                {
                    return;
                }
                if (receiver != null)
                {
                    if (Session.Character.IsBlockedByCharacter(receiver.CharacterId))
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                        return;
                    }
                }
                int? sentChannelId = CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                {
                    DestinationCharacterId = receiver.CharacterId,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = Session.Character.GenerateSpk(message, Session.Account.Authority == AuthorityType.GameMaster ? 15 : 5),
                    Type = packetsplit[0] == "GM" ? MessageType.WhisperGM : MessageType.Whisper
                });
                if (sentChannelId == null)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED")));
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Whisper failed.", e);
            }
        }

        #endregion
    }
}