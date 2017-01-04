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

        public ClientSession Session
        {
            get { return _session; }
        }

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
                    MaxSize = 50,
                    Size = 3
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
                        Authority = FamilyAuthority.Assistant,
                        FamilyId = family.FamilyId,
                        JoinDate = DateTime.Now,
                        Rank = FamilyMemberRank.Member,
                    };
                    if (Session.Character.CharacterId == c.Character.CharacterId)
                    {
                        familyCharacter.Authority = FamilyAuthority.Head;
                    }
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);
                    c.Character.FamilyCharacterId = familyCharacter.FamilyCharacterId;
                    c.Character.Save();
                    c.Character.Family = family;
                    c.Character.FamilyCharacter = familyCharacter;
                    Session.CurrentMap.Broadcast(c.Character.GenerateGidx());
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_GROUP_NOT_FULL")));
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

                int? sentChannelId = ServerCommunicationClient.Instance.HubProxy.Invoke<int?>("SendMessageToCharacter", ccmsg, ServerManager.Instance.ChannelId, MessageType.Family, Session.Character.Family.FamilyId.ToString(), null).Result;
                foreach (ClientSession s in ServerManager.Instance.Sessions)
                {
                    if (s.HasSelectedCharacter && s.Character.Family != null && s.Character.FamilyCharacter != null)
                    {
                        if (s.Character.Family.FamilyId == Session.Character.Family.FamilyId)
                        {
                            if (Session.HasCurrentMap && s.HasCurrentMap && Session.CurrentMap == s.CurrentMap)
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
            if (otherSession.Character.Family != null || otherSession.Character.FamilyCharacter != null || otherSession.Character.FamilyCharacterId != null)
            {
                Session.SendPacket(Session.Character.GenerateInfo("This user is already in another family!"));
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
                    inviteSession.Character.Family.Size += 1;
                    Session.Character.Family = inviteSession.Character.Family;

                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO
                    {
                        CharacterId = Session.Character.CharacterId,
                        DailyMessage = string.Empty,
                        Experience = 0,
                        Authority = FamilyAuthority.Member,
                        FamilyId = Session.Character.Family.FamilyId,
                        JoinDate = DateTime.Now,
                        Rank = FamilyMemberRank.Member,
                    };
                    FamilyDTO family = DAOFactory.FamilyDAO.LoadById(inviteSession.Character.Family.FamilyId);
                    family.Size += 1;
                    DAOFactory.FamilyDAO.InsertOrUpdate(ref family);

                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);

                    Session.Character.FamilyCharacter = familyCharacter;
                    Session.Character.FamilyCharacterId = familyCharacter.FamilyCharacterId;
                    Session.Character.Save();
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateGidx());
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAMILY_JOINED"), Session.Character.Name, Session.Character.Family.Name), 0));
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
                            Session.Character.Family = DAOFactory.FamilyDAO.LoadById(Session.Character.Family.FamilyId);
                            FamilyCharacterDTO familyCharacter = DAOFactory.FamilyCharacterDAO.LoadByFamilyId(Session.Character.Family.FamilyId).FirstOrDefault(s => s.Authority == FamilyAuthority.Head);
                            if (familyCharacter != null)
                            {
                                string familyHead = DAOFactory.CharacterDAO.LoadById(familyCharacter.CharacterId).Name;
                                Session.SendPacket($"ginfo {Session.Character.Family.Name} {familyHead} 0 {Session.Character.Family.FamilyLevel} {Session.Character.Family.FamilyExperience} {CharacterHelper.LoadFamilyXPData(Session.Character.Family.FamilyLevel)} {Session.Character.Family.Size} {Session.Character.Family.MaxSize} 1 1 1 1 1 1 1 1");
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
                        Session.SendPacket(Session.Character.GenerateInfo("You can't kick the family head!"));
                        return;
                    }
                    if (kickSession.Character.CharacterId == Session.Character.CharacterId)
                    {
                        Session.SendPacket(Session.Character.GenerateInfo("You can't kick yourself!"));
                        return;
                    }
                    FamilyDTO family = DAOFactory.FamilyDAO.LoadById(Session.Character.Family.FamilyId);
                    family.Size += 1;
                    DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
                    Session.Character.Family.Size -= 1;

                    kickSession.Character.Family = null;
                    DAOFactory.FamilyCharacterDAO.Delete(packetsplit[2]);
                    kickSession.Character.FamilyCharacter = null;
                    kickSession.Character.FamilyCharacterId = null;
                    kickSession.Character.Save();
                    kickSession.CurrentMap?.Broadcast(kickSession.Character.GenerateGidx());
                    foreach (ClientSession s in ServerManager.Instance.Sessions)
                    {
                        if (s.HasSelectedCharacter && s.Character.Family != null && s.Character.FamilyCharacter != null)
                        {
                            if (s.Character.Family.FamilyId == Session.Character.Family.FamilyId)
                            {
                                string name = packetsplit[2];
                                s.SendPacket(s.Character.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAMILY_KICKED"), name), 0));
                            }
                        }
                    }
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
                            FamilyDTO family = DAOFactory.FamilyDAO.LoadById(Session.Character.Family.FamilyId);
                            family.Size -= 1;
                            DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
                        }
                    }
                }
            }
        }

        [Packet("%Familyleave")]
        public void FamilyLeave(string packet)
        {
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length == 2)
            {
                if (Session.Character.Family == null || Session.Character.FamilyCharacter == null)
                {
                    return;
                }
                if (Session.Character.FamilyCharacter.Authority != FamilyAuthority.Member)
                {
                    Session.SendPacket(Session.Character.GenerateInfo("You can only leave the family as a member!"));
                    return;
                }
                foreach (ClientSession s in ServerManager.Instance.Sessions)
                {
                    if (s.HasSelectedCharacter && s.Character.Family != null && s.Character.FamilyCharacter != null)
                    {
                        if (s.Character.Family.FamilyId == Session.Character.Family.FamilyId)
                        {
                            s.SendPacket(s.Character.GenerateMsg($"{Session.Character.Name} has left the family!", 0));
                        }
                    }
                }
                FamilyDTO family = DAOFactory.FamilyDAO.LoadById(Session.Character.Family.FamilyId);
                family.Size += 1;
                DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
                Session.Character.Family.Size -= 1;

                Session.Character.Family = null;
                DAOFactory.FamilyCharacterDAO.Delete(Session.Character.Name);
                Session.Character.FamilyCharacter = null;
                Session.Character.FamilyCharacterId = null;
                Session.Character.Save();
                Session.CurrentMap?.Broadcast(Session.Character.GenerateGidx());
            }
        }
        #endregion
    }
}