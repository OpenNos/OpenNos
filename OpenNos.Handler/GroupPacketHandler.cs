using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler
{
    class GroupPacketHandler : IPacketHandler
    {
        #region Instantiation

        public GroupPacketHandler(ClientSession session)
        {
            Session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        /// <summary>
        /// rdPacket packet
        /// </summary>
        /// <param name="rdPacket"></param>
        public void RaidManage(RdPacket rdPacket)
        {
            Group grp;
            switch (rdPacket.Type)
            {
                case 1: //Join
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }
                    ClientSession target = ServerManager.Instance.GetSessionByCharacterId(rdPacket.CharacterId);
                    if (rdPacket.Parameter == null && target?.Character?.Group == null && Session?.Character?.Group?.IsLeader(Session) == true)
                    {
                        GroupJoin(new PJoinPacket { RequestType = GroupRequestType.Invited, CharacterId = rdPacket.CharacterId });
                    }
                    else if (Session?.Character?.Group == null)
                    {
                        GroupJoin(new PJoinPacket { RequestType = GroupRequestType.Accepted, CharacterId = rdPacket.CharacterId });
                    }
                    break;

                case 2: //leave
                    ClientSession sender = ServerManager.Instance.GetSessionByCharacterId(rdPacket.CharacterId);
                    if (sender?.Character?.Group == null)
                    {
                        return;
                    }

                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("LEFT_RAID")), 0));
                    if (Session?.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId, Session.Character.MapX, Session.Character.MapY);
                    }
                    grp = sender.Character?.Group;
                    Session.SendPacket(Session.Character.GenerateRaid(1, true));
                    Session.SendPacket(Session.Character.GenerateRaid(2, true));

                    grp?.Characters?.ToList().ForEach(s =>
                    {
                        s.SendPacket(grp.GenerateRdlst());
                        s.SendPacket(grp.GeneraterRaidmbf());
                        s.SendPacket(s.Character.GenerateRaid(0, false));
                        if (!grp.IsLeader(s))
                        {
                            s.SendPacket(s.Character.GenerateRaid(2, false));
                        }
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
                        if (chartokick?.Character?.Group == null)
                        {
                            return;
                        }

                        chartokick.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("KICK_RAID")), 0));
                        grp = chartokick.Character.Group;
                        chartokick.SendPacket(chartokick.Character.GenerateRaid(1, true));
                        chartokick.SendPacket(chartokick.Character.GenerateRaid(2, true));
                        grp.LeaveGroup(chartokick);
                        grp.Characters.ToList().ForEach(s =>
                        {
                            s.SendPacket(grp.GenerateRdlst());
                            s.SendPacket(s.Character.GenerateRaid(0, false));
                            if (!grp.IsLeader(s))
                            {
                                s.SendPacket(s.Character.GenerateRaid(2, false));
                            }
                        });
                    }

                    break;
                case 4: //disolve
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }
                    if (Session.Character.Group != null && Session.Character.Group.IsLeader(Session))
                    {
                        grp = Session.Character.Group;

                        ClientSession[] grpmembers = new ClientSession[40];
                        grp.Characters.ToList().CopyTo(grpmembers);
                        foreach (ClientSession targetSession in grpmembers)
                        {
                            if (targetSession == null)
                            {
                                continue;
                            }
                            targetSession.SendPacket(targetSession.Character.GenerateRaid(1, true));
                            targetSession.SendPacket(targetSession.Character.GenerateRaid(2, true));
                            targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("RAID_DISOLVED"), 0));
                            grp.LeaveGroup(targetSession);
                        }
                        ServerManager.Instance.GroupList.RemoveAll(s => s.GroupId == grp.GroupId);
                        ServerManager.Instance.GroupsThreadSafe.TryRemove(grp.GroupId, out Group _);
                    }

                    break;
            }
        }

        /// <summary>
        /// rlPacket packet
        /// </summary>
        /// <param name="rlPacket"></param>
        public void RaidListRegister(RlPacket rlPacket)
        {
            switch (rlPacket.Type)
            {
                case 0:
                    if (Session.Character.Group?.IsLeader(Session) == true && Session.Character.Group.GroupType != GroupType.Group && ServerManager.Instance.GroupList.Any(s => s.GroupId == Session.Character.Group.GroupId))
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
                    if (Session.Character.Group != null && Session.Character.Group.IsLeader(Session) && Session.Character.Group.GroupType != GroupType.Group && ServerManager.Instance.GroupList.All(s => s.GroupId != Session.Character.Group.GroupId))
                    {
                        if (Session.Character.Group.Raid?.FirstMap?.InstanceBag.Lock == true)
                        {
                            return;
                        }
                        ServerManager.Instance.GroupList.Add(Session.Character.Group);
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateRl(1));
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo("RAID_REGISTERED"));
                        ServerManager.Instance.Broadcast(Session, $"qnaml 100 #rl {string.Format(Language.Instance.GetMessageFromKey("SEARCH_TEAM_MEMBERS"), Session.Character.Name, Session.Character.Group.Raid?.Label)}", ReceiverType.AllExceptGroup);
                    }
                    break;
                case 2:
                    if (Session.Character.Group != null && Session.Character.Group.IsLeader(Session) && Session.Character.Group.GroupType != GroupType.Group && ServerManager.Instance.GroupList.Any(s => s.GroupId == Session.Character.Group.GroupId))
                    {
                        ServerManager.Instance.GroupList.Remove(Session.Character.Group);
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateRl(2));
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo("RAID_UNREGISTERED"));
                    }
                    break;
                case 3:
                    ClientSession cl = ServerManager.Instance.GetSessionByCharacterName(rlPacket.CharacterName);
                    if (cl != null)
                    {
                        cl.Character.GroupSentRequestCharacterIds.Add(Session.Character.CharacterId);
                        GroupJoin(new PJoinPacket { RequestType = GroupRequestType.Accepted, CharacterId = cl.Character.CharacterId });
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
            bool createNewGroup = true;
            ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(pjoinPacket.CharacterId);

            if (targetSession == null && !pjoinPacket.RequestType.Equals(GroupRequestType.Sharing))
            {
                return;
            }

            switch (pjoinPacket.RequestType)
            {
                case GroupRequestType.Requested:
                case GroupRequestType.Invited:
                    if (pjoinPacket.CharacterId == 0)
                    {
                        return;
                    }
                    if (ServerManager.Instance.IsCharactersGroupFull(pjoinPacket.CharacterId))
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_FULL")));
                        return;
                    }

                    if (ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId) && ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId))
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_GROUP")));
                        return;
                    }

                    if (Session.Character.CharacterId == pjoinPacket.CharacterId)
                    {
                        return;
                    }
                    if (targetSession == null)
                    {
                        return;
                    }
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
                            if (targetSession.Character?.Group == null || targetSession.Character?.Group.GroupType == GroupType.Group)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("GROUP_REQUEST"), targetSession.Character.Name)));
                                targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#pjoin^3^{ Session.Character.CharacterId} #pjoin^4^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU"), Session.Character.Name)}"));
                            }
                            else
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("GROUP_CANT_INVITE"), targetSession.Character.Name)));
                            }
                        }
                        else
                        {
                            targetSession.SendPacket($"qna #rd^1^{Session.Character.CharacterId}^1 {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU_RAID"), Session.Character.Name)}");
                        }

                    }
                    break;
                case GroupRequestType.Sharing:
                    if (Session.Character.Group == null)
                    {
                        return;
                    }
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_SHARE_INFO")));
                    Session.Character.Group.Characters.Replace(s => s.Character.CharacterId != Session.Character.CharacterId).ToList().ForEach(s =>
                    {
                        s.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#pjoin^6^{ Session.Character.CharacterId} #pjoin^7^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INVITED_YOU_SHARE"), Session.Character.Name)}"));
                        Session.Character.GroupSentRequestCharacterIds.Add(s.Character.CharacterId);
                    });
                    break;
                case GroupRequestType.Accepted:
                    if (targetSession != null && !targetSession.Character.GroupSentRequestCharacterIds.Contains(Session.Character.CharacterId))
                    {
                        return;
                    }
                    if (targetSession != null)
                    {
                        targetSession.Character.GroupSentRequestCharacterIds.Remove(Session.Character.CharacterId);

                        if (ServerManager.Instance.IsCharacterMemberOfGroup(Session.Character.CharacterId) && ServerManager.Instance.IsCharacterMemberOfGroup(pjoinPacket.CharacterId))
                        {
                            // everyone is in group, return
                            return;
                        }

                        if (ServerManager.Instance.IsCharactersGroupFull(pjoinPacket.CharacterId) || ServerManager.Instance.IsCharactersGroupFull(Session.Character.CharacterId))
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
                                targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("JOINED_GROUP")));
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
                                    currentGroup.Characters.ToList().ForEach(s =>
                                    {
                                        s.SendPacket(currentGroup.GenerateRdlst());
                                        s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("JOIN_TEAM"), Session.Character.Name), 10));
                                        s.SendPacket(s.Character.GenerateRaid(0, false));
                                        if (!currentGroup.IsLeader(s))
                                        {
                                            s.SendPacket(s.Character.GenerateRaid(2, false));
                                        }
                                    });
                                }
                            }
                        }


                        if (createNewGroup)
                        {
                            Group group = new Group(GroupType.Group);
                            group.JoinGroup(pjoinPacket.CharacterId);
                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("GROUP_JOIN"), targetSession.Character.Name)));
                            group.JoinGroup(Session.Character.CharacterId);
                            ServerManager.Instance.AddGroup(group);
                            targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_ADMIN")));

                            // set back reference to group
                            Session.Character.Group = group;
                            targetSession.Character.Group = group;
                        }
                    }
                    if (Session?.Character?.Group.GroupType != GroupType.Group)
                    {
                        return;
                    }
                    // player join group
                    ServerManager.Instance.UpdateGroup(pjoinPacket.CharacterId);
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GeneratePidx());
                    break;
                default:
                    switch (pjoinPacket.RequestType)
                    {
                        case GroupRequestType.Declined:
                            if (targetSession != null && !targetSession.Character.GroupSentRequestCharacterIds.Contains(Session.Character.CharacterId))
                            {
                                return;
                            }
                            if (targetSession != null)
                            {
                                targetSession.Character.GroupSentRequestCharacterIds.Remove(Session.Character.CharacterId);

                                targetSession.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("REFUSED_GROUP_REQUEST"), Session.Character.Name), 10));
                            }
                            break;
                        case GroupRequestType.AcceptedShare:
                            if (targetSession != null && !targetSession.Character.GroupSentRequestCharacterIds.Contains(Session.Character.CharacterId))
                            {
                                return;
                            }
                            if (targetSession != null)
                            {
                                targetSession.Character.GroupSentRequestCharacterIds.Remove(Session.Character.CharacterId);

                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("ACCEPTED_SHARE"), targetSession.Character.Name), 0));
                                if (Session.Character.Group.IsMemberOfGroup(pjoinPacket.CharacterId))
                                {
                                    Session.Character.SetReturnPoint(targetSession.Character.Return.DefaultMapId, targetSession.Character.Return.DefaultX, targetSession.Character.Return.DefaultY);
                                    targetSession.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("CHANGED_SHARE"), targetSession.Character.Name), 0));
                                }
                            }
                            break;
                        case GroupRequestType.DeclinedShare:
                            if (targetSession != null && !targetSession.Character.GroupSentRequestCharacterIds.Contains(Session.Character.CharacterId))
                            {
                                return;
                            }
                            targetSession?.Character.GroupSentRequestCharacterIds.Remove(Session.Character.CharacterId);

                            Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("REFUSED_SHARE"), 0));
                            break;
                        case GroupRequestType.Requested:
                            break;
                        case GroupRequestType.Invited:
                            break;
                        case GroupRequestType.Accepted:
                            break;
                        case GroupRequestType.Sharing:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
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
            if (string.IsNullOrEmpty(groupSayPacket.Message))
            {
                return;
            }
            ServerManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(groupSayPacket.Message, 3), ReceiverType.Group);
            LogHelper.Instance.InsertChatLog(ChatType.Friend, Session.Character.CharacterId, groupSayPacket.Message, Session.IpAddress);
        }
    }
    #endregion
}
