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
using System.Linq;

namespace OpenNos.Handler
{
    public class FamilyPacketHandler : IPacketHandler
    {
        #region Members

        private readonly ClientSession _session;

        #endregion

        #region Instantiation

        public FamilyPacketHandler(ClientSession session)
        {
            _session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session => _session;

        #endregion

        #region Methods
        [Packet("#glmk")]
        public void CreateFamily(string packet)
        {
            if (Session.Character.Group != null && Session.Character.Group.CharacterCount == 3)
            {
                foreach (ClientSession s in Session.Character.Group.Characters)
                {
                    if (s.Character.Family != null || s.Character.FamilyCharacter != null)
                    {
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_MEMBER_ALREADY_IN_FAMILY")));
                        return;
                    }
                }
                if (Session.Character.Gold < 200000)
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                    return;
                }
                string name = packet.Split('^')[1];
                if (DAOFactory.FamilyDAO.LoadByName(name) != null)
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_NAME_ALREADY_USED")));
                    return;
                }
                Session.Character.Gold -= 200000;
                Session.SendPacket(Session.Character.GenerateGold());
                FamilyDTO family = new FamilyDTO
                {
                    Name = name,
                    FamilyExperience = 0,
                    FamilyLevel = 1,
                    FamilyMessage = string.Empty,
                    MaxSize = 50
                };
                DAOFactory.FamilyDAO.InsertOrUpdate(ref family);

                ServerManager.Instance.Broadcast(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAMILY_FOUNDED"), name), 0));
                foreach (ClientSession c in Session.Character.Group.Characters)
                {
                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO
                    {
                        CharacterId = c.Character.CharacterId,
                        DailyMessage = string.Empty,
                        Experience = 0,
                        Authority = Session.Character.CharacterId == c.Character.CharacterId ? FamilyAuthority.Head : FamilyAuthority.Assistant,
                        FamilyId = family.FamilyId,
                        JoinDate = DateTime.Now,
                        Rank = FamilyMemberRank.Member,
                    };
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);
                }
                ServerManager.Instance.FamilyRefresh();
                Session.Character.Group.Characters.ForEach(s => s.CurrentMap.Broadcast(s.Character.GenerateGidx()));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_GROUP_NOT_FULL")));
            }
        }

        [Packet("fmg")]
        public void FamilyManagement(string packet)
        {
            if (Session.Character.Family == null)
            {
                return;
            }
            if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager)
            {
                return;
            }
            string[] packetsplit = packet.Split(' ');
            long targetId;
            if (long.TryParse(packetsplit[3], out targetId))
            {
                if (DAOFactory.FamilyCharacterDAO.LoadByCharacterId(targetId)?.FamilyId != Session.Character.FamilyCharacter.FamilyId)
                {
                    return;
                }
                ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(targetId);
                switch (packetsplit[2])
                {
                    case "0":
                        if (targetSession == null)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("PLAYER_OFFLINE")));
                            return;
                        }
                        if (Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                            return;
                        }
                        if (targetSession.Character.FamilyCharacter.Authority != FamilyAuthority.Assistant)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("ONLY_PROMOTE_ASSISTANT")));
                            return;
                        }
                        targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Head;
                        Session.Character.FamilyCharacter.Authority = FamilyAuthority.Assistant;
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                        FamilyCharacterDTO chara = targetSession.Character.FamilyCharacter;
                        DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);

                        chara = Session.Character.FamilyCharacter;
                        DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);

                        ServerManager.Instance.FamilyRefresh();
                        Session.CurrentMap?.Broadcast(Session.Character.GenerateGidx());
                        targetSession.CurrentMap?.Broadcast(targetSession.Character.GenerateGidx());

                        break;
                    case "1":
                        if (targetSession == null)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("PLAYER_OFFLINE")));
                            return;
                        }
                        if (Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                            return;
                        }
                        if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                            return;
                        }
                        if (DAOFactory.FamilyCharacterDAO.LoadByFamilyId(Session.Character.Family.FamilyId).Count(s => s.Authority == FamilyAuthority.Assistant) == 2)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_TWO_ASSISTANT")));
                            return;
                        }
                        targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Assistant;
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                        chara = targetSession.Character.FamilyCharacter;
                        DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);

                        ServerManager.Instance.FamilyRefresh();

                        targetSession.CurrentMap?.Broadcast(targetSession.Character.GenerateGidx());

                        break;
                    case "2":
                        if (targetSession == null)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("PLAYER_OFFLINE")));
                            return;
                        }
                        if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                            return;
                        }
                        if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant && Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("ASSISTANT_UNDEMOTABLE")));
                            return;
                        }
                        targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Manager;
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                        chara = targetSession.Character.FamilyCharacter;
                        DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);

                        ServerManager.Instance.FamilyRefresh();
                        targetSession.CurrentMap?.Broadcast(targetSession.Character.GenerateGidx());
                        break;
                    case "3":
                        if (targetSession == null)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("PLAYER_OFFLINE")));
                            return;
                        }
                        if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                            return;
                        }
                        if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant && Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                        {
                            Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("ASSISTANT_UNDEMOTABLE")));
                            return;
                        }
                        targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Member;
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                        chara = targetSession.Character.FamilyCharacter;
                        DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);
                        ServerManager.Instance.FamilyRefresh();
                        targetSession.CurrentMap?.Broadcast(targetSession.Character.GenerateGidx());
                        break;
                }
            }
        }

        [Packet("%Notice")]
        [Packet("%Avertissement")]
        public void FamilyMessage(string packet)
        {
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                {
                    string msg = string.Empty;
                    int i = 0;
                    foreach (string str in packet.Split(' '))
                    {
                        if (i > 1)
                        {
                            msg += str + " ";
                        }
                        i++;
                    }
                    Session.Character.Family.FamilyMessage = msg;
                    FamilyDTO fam = Session.Character.Family;
                    DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                    ServerManager.Instance.FamilyRefresh();
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        int? sentChannelId = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", Session.Character.GenerateInfo("--- Family Message ---\n" + Session.Character.Family.FamilyMessage), ServerManager.Instance.ChannelId, MessageType.Family, Session.Character.Family.FamilyId.ToString(), null).Result;
                    }
                }
            }
        }

        [Packet("%Cridefamille")]
        [Packet("%Familyshout")]
        public void FamilyCall(string packet)
        {
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                {
                    string msg = string.Empty;
                    int i = 0;
                    foreach (string str in packet.Split(' '))
                    {
                        if (i > 1)
                        {
                            msg += str + " ";
                        }
                        i++;
                    }
                    int? sentChannelId = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", Session.Character.GenerateMsg($"<{Language.Instance.GetMessageFromKey("FAMILYCALL")}> {msg}", 0), ServerManager.Instance.ChannelId, MessageType.Family, Session.Character.Family.FamilyId.ToString(), null).Result;
                }
            }
        }

        [Packet(":")]
        public void FamilyChat(string packet)
        {
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                string msg = string.Empty;
                int i = 0;
                foreach (string str in packet.Split(' '))
                {
                    if (i != 0)
                    {
                        msg += str + " ";
                    }
                    i++;
                }
                msg = msg.Substring(1);
                string ccmsg = $"[{Session.Character.Name}]:{msg}";
                if (Session.Account.Authority == AuthorityType.Admin)
                {
                    ccmsg = $"[GM {Session.Character.Name}]:{msg}";
                }

                int? sentChannelId = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", ccmsg, ServerManager.Instance.ChannelId, MessageType.FamilyChat, Session.Character.Family.FamilyId.ToString(), null).Result;
                foreach (ClientSession s in ServerManager.Instance.Sessions)
                {
                    if (s.HasSelectedCharacter && s.Character.Family != null && s.Character.FamilyCharacter != null)
                    {
                        if (s.Character.Family.FamilyId == Session.Character.Family.FamilyId)
                        {
                            if (Session.HasCurrentMap && s.HasCurrentMap && Session.CurrentMap == s.CurrentMap && !Session.Character.InvisibleGm)
                            {
                                s.SendPacket(Session.Character.GenerateSay(msg, 6));
                            }
                            else
                            {
                                string prefix = $"[{Session.Character.Name}]:";
                                if (Session.Account.Authority == AuthorityType.Admin)
                                {
                                    prefix = $"[GM {Session.Character.Name}]:";
                                }
                                s.SendPacket(Session.Character.GenerateSay(prefix + msg, 6));
                            }
                            s.SendPacket(Session.Character.GenerateSpk(msg, 1));
                        }
                    }
                }
            }
        }

        [Packet("%Familyinvite")]
        [Packet("%Invitationdefamille")]
        public void InviteFamily(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if (packetsplit.Length != 3)
            {
                return;
            }
            if (Session.Character.Family == null || Session.Character.FamilyCharacter == null)
            {
                return;
            }

            if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member)
            {
                Session.SendPacket(Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("FAMILY_INVITATION_NOT_ALLOWED"))));
                return;
            }

            ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterName(packetsplit[2]);
            if (otherSession == null)
            {
                Session.SendPacket(Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"))));
                return;
            }

            if (Session.Character.IsBlockedByCharacter(otherSession.Character.CharacterId))
            {
                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                return;
            }

            if (otherSession.Character.Family != null || otherSession.Character.FamilyCharacter != null)
            {
                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_FAMILY")));
                return;
            }

            Session.SendPacket(Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("FAMILY_INVITED"), otherSession.Character.Name)));
            otherSession.SendPacket($"dlg #gjoin^1^{Session.Character.CharacterId} #gjoin^2^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("ASK_FAMILY_INVITED"), Session.Character.Family.Name)}");
            Session.Character.FamilyInviteCharacters.Add(otherSession.Character.CharacterId);
        }

        [Packet("#gjoin")]
        public void JoinFamily(string packet)
        {
            string[] packetsplit = packet.Split('^');
            long characterId;
            if (packetsplit.Length == 3 && long.TryParse(packetsplit[2], out characterId) && packetsplit[1] == "1")
            {
                ClientSession inviteSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                if (inviteSession != null && inviteSession.Character.FamilyInviteCharacters.Contains(Session.Character.CharacterId))
                {

                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO
                    {
                        CharacterId = Session.Character.CharacterId,
                        DailyMessage = string.Empty,
                        Experience = 0,
                        Authority = FamilyAuthority.Member,
                        FamilyId = inviteSession.Character.Family.FamilyId,
                        JoinDate = DateTime.Now,
                        Rank = FamilyMemberRank.Member,
                    };
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);
                    ServerManager.Instance.FamilyRefresh();
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateGidx());
                    int? sentChannelId = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAMILY_JOINED"), Session.Character.Name, Session.Character.Family.Name), 0), ServerManager.Instance.ChannelId, MessageType.Family, Session.Character.Family.FamilyId.ToString(), null).Result;
                    Session.SendPacket(Session.Character.GenerateFamilyMember());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberExp());
                }
            }
        }

        [Packet("glist")]
        public void FamilyList(string packet)
        {
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                string[] packetsplit = packet.Split(' ');
                if (packetsplit.Length == 4)
                {
                    if (packetsplit[3] == "2")
                    {
                        if (Session.Character.FamilyCharacter != null && Session.Character.Family != null)
                        {
                            FamilyCharacter familyCharacter = Session.Character.Family.FamilyCharacters.FirstOrDefault(s => s.Authority == FamilyAuthority.Head);
                            if (familyCharacter != null)
                            {
                                Session.SendPacket($"ginfo {Session.Character.Family.Name} {familyCharacter.Character.Name} 0 {Session.Character.Family.FamilyLevel} {Session.Character.Family.FamilyExperience} {CharacterHelper.LoadFamilyXPData(Session.Character.Family.FamilyLevel)} {Session.Character.Family.FamilyCharacters.Count()} {Session.Character.Family.MaxSize} {(byte)Session.Character.FamilyCharacter.Authority} 1 0 0 0 0 0 0 {Session.Character.Family.FamilyMessage.Replace(' ', '^')}");
                            }
                            Session.SendPacket(Session.Character.GenerateFamilyMember());
                            Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                            Session.SendPacket(Session.Character.GenerateFamilyMemberExp());
                        }
                    }
                }
            }
        }

        [Packet("%Familykick")]
        [Packet("%Rejetdefamille")]
        public void FamilyKick(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 3)
            {
                if (Session.Character.Family == null || Session.Character.FamilyCharacter == null)
                {
                    return;
                }
                if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member)
                {
                    Session.SendPacket(Session.Character.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("NOT_ALLOWED_KICK"))));
                    return;
                }
                ClientSession kickSession = ServerManager.Instance.GetSessionByCharacterName(packetsplit[2]);
                if (kickSession != null && kickSession.Character.Family?.FamilyId == Session.Character.Family.FamilyId)
                {
                    if (kickSession.Character.FamilyCharacter?.Authority == FamilyAuthority.Head)
                    {
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_KICK_HEAD")));
                        return;
                    }
                    if (kickSession.Character.CharacterId == Session.Character.CharacterId)
                    {
                        Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_KICK_YOURSELF")));
                        return;
                    }
                    DAOFactory.FamilyCharacterDAO.Delete(packetsplit[2]);
                    ServerManager.Instance.FamilyRefresh();
                    kickSession.CurrentMap?.Broadcast(kickSession.Character.GenerateGidx());
                }
                else
                {
                    CharacterDTO dbCharacter = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]);
                    if (dbCharacter != null)
                    {
                        FamilyCharacterDTO dbFamilyCharacter = DAOFactory.FamilyCharacterDAO.LoadByCharacterId(dbCharacter.CharacterId);
                        if (dbFamilyCharacter != null && dbFamilyCharacter.FamilyId == Session.Character.Family.FamilyId)
                        {
                            DAOFactory.FamilyCharacterDAO.Delete(packetsplit[2]);
                            ServerManager.Instance.FamilyRefresh();
                        }
                    }
                }

            }
        }

        [Packet("%Familyleave")]
        [Packet("%Congédefamille")]
        public void FamilyLeave(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 2)
            {
                if (Session.Character.Family == null || Session.Character.FamilyCharacter == null)
                {
                    return;
                }
                if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                {
                    Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("CANNOT_LEAVE_FAMILY")));
                    return;
                }

                DAOFactory.FamilyCharacterDAO.Delete(Session.Character.Name);
                ServerManager.Instance.FamilyRefresh();
                Session.CurrentMap?.Broadcast(Session.Character.GenerateGidx());
            }
        }
        #endregion
    }
}