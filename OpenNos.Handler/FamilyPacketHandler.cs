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
using OpenNos.GameObject;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
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
                foreach(ClientSession s in Session.Character.Group.Characters)
                {
                    if(s.Character.Family != null || s.Character.FamilyCharacter != null)
                    {
                        Session.SendPacket(Session.Character.GenerateInfo("One character in Group is already in a family!"));
                        return;
                    }
                }
                if(Session.Character.Gold < 200000)
                {
                    Session.SendPacket(Session.Character.GenerateInfo("You don't have enough Gold!"));
                    return;
                }
                string name = packet.Split('^')[1];
                if(DAOFactory.FamilyDAO.LoadByName(name) != null)
                {
                    Session.SendPacket(Session.Character.GenerateInfo("There is already a family with this name!"));
                    return;
                }
                Session.Character.Gold -= 200000;
                Session.SendPacket(Session.Character.GenerateGold());
                FamilyDTO family = new FamilyDTO()
                {
                    Name = name,
                    FamilyExperience = 0,
                    FamilyLevel = 1,
                    FamilyMessage = "",
                    MaxSize = 50,
                    Size = 3
                };

                DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
                ServerManager.Instance.Broadcast(Session.Character.GenerateMsg($"The family {name} was founded!", 0));
                foreach (ClientSession c in Session.Character.Group.Characters)
                {
                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO()
                    {
                        CharacterId = c.Character.CharacterId,
                        DailyMessage = "",
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
                //TODO: NO FULL GROUP WARNING
            }
        }

        [Packet(":")]
        public void FamilyChat(string packet)
        {
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                string msg = String.Empty;
                int i = 0;
                foreach (string s in packet.Split(' '))
                {
                    if (i != 0)
                    {
                        msg += s + " ";
                    }
                    i++;
                }
                msg = msg.Substring(1);

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
                                if (s.Account.Authority == Domain.AuthorityType.Admin)
                                {
                                    prefix = $"[GM {Session.Character.Name}]:";
                                }
                                s.SendPacket(Session.Character.GenerateSay(prefix + msg, 6));
                            }
                            s.SendPacket(Session.Character.GenerateSpk(msg, 1));

                            //if (s.HasCurrentMap && Session.HasCurrentMap && s.CurrentMap == Session.CurrentMap)
                            //{
                            //    s.SendPacket(Session.Character.GenerateSpk(msg, 1));
                            //}
                        }
                    }
                }
            }
        }

        [Packet("%Familyinvite")]
        public void InviteFamily(string packet)
        {
            string[] packetsplit = packet.Split(' ');

            if(packetsplit.Length != 3)
            {
                return;
            }

            if(Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member)
            {
                Session.SendPacket(Session.Character.GenerateInfo("You are not permitted to invite players!"));
                return;
            }

            ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterName(packetsplit[2]);
            if(otherSession== null)
            {
                Session.SendPacket(Session.Character.GenerateInfo("This user is not online!"));
                return;
            }

            Session.SendPacket(Session.Character.GenerateInfo($"{otherSession.Character.Name} has been invited!"));
            otherSession.SendPacket($"dlg #gjoin^1^{Session.Character.CharacterId} #gjoin^2^{Session.Character.CharacterId} The Family {Session.Character.Family.Name} wants you to join. Accept?");
            Session.Character.FamilyInviteCharacters.Add(otherSession.Character.CharacterId);
        }

        [Packet("#gjoin")]
        public void JoinFamily(string packet)
        {
            string[] packetsplit = packet.Split('^');
            long characterId = 0;
            if (packetsplit.Length == 3 && long.TryParse(packetsplit[2], out characterId) && packetsplit[1] == "1")
            {
                ClientSession inviteSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                if (inviteSession != null && inviteSession.Character.FamilyInviteCharacters.Contains(Session.Character.CharacterId))
                {
                    inviteSession.Character.Family.Size += 1;
                    Session.Character.Family = inviteSession.Character.Family;

                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO()
                    {
                        CharacterId = Session.Character.CharacterId,
                        DailyMessage = "",
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
                    Session.CurrentMap?.Broadcast(Session.Character.GenerateMsg($"{Session.Character.Name} joined the family {Session.Character.Family.Name}", 0));
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
                        var temp = DAOFactory.FamilyCharacterDAO.LoadByFamilyId(Session.Character.Family.FamilyId).FirstOrDefault(s => s.Authority == FamilyAuthority.Head);
                        string familyHead = DAOFactory.CharacterDAO.LoadById(temp.CharacterId).Name;
                        Session.SendPacket($"ginfo {Session.Character.Family.Name} {familyHead} 0 {Session.Character.Family.FamilyLevel} {Session.Character.Family.FamilyExperience} 200000 {Session.Character.Family.Size} {Session.Character.Family.MaxSize} 1 1 1 1 1 1 1 1");
                        Session.SendPacket(Session.Character.GenerateFamilyMember());
                        Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                        Session.SendPacket(Session.Character.GenerateFamilyMemberExp());
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
                if(Session.Character.Family == null || Session.Character.FamilyCharacter == null)
                {
                    return;
                }
                if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member)
                {
                    Session.SendPacket(Session.Character.GenerateInfo("You are not permitted to kick players!"));
                    return;
                }
                ClientSession kickSession = ServerManager.Instance.GetSessionByCharacterName(packetsplit[2]);
                if (kickSession != null && kickSession.Character.Family?.FamilyId == Session.Character.Family.FamilyId)
                {
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
                                s.SendPacket(s.Character.GenerateMsg($"{packetsplit[2]} has been kicked out of the family!", 0));
                            }
                        }
                    }
                }
                else
                {
                    if (DAOFactory.FamilyCharacterDAO.LoadByCharacterId(DAOFactory.CharacterDAO.LoadByName(packetsplit[2]).CharacterId).FamilyId == Session.Character.Family.FamilyId)
                    {
                        DAOFactory.FamilyCharacterDAO.Delete(packetsplit[2]);
                        FamilyDTO family = DAOFactory.FamilyDAO.LoadById(Session.Character.Family.FamilyId);
                        family.Size -= 1;
                        DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
                    }
                }
            }
        }
        #endregion
    }
}