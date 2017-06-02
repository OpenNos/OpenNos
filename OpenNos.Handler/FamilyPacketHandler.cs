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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenNos.Handler
{
    public class FamilyPacketHandler : IPacketHandler
    {
        #region Instantiation

        public FamilyPacketHandler(ClientSession session)
        {
            Session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        /// <summary>
        /// fauth packet
        /// </summary>
        /// <param name="fAuthPacket"></param>
        public void ChangeAuthority(FAuthPacket fAuthPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            if (Session.Character.Family == null || Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
            {
                return;
            }
            Session.Character.Family.InsertFamilyLog(FamilyLogType.RightChanged, Session.Character.Name, authority: fAuthPacket.MemberType, righttype: fAuthPacket.AuthorityId + 1, rightvalue: fAuthPacket.Value);
            switch (fAuthPacket.MemberType)
            {
                case FamilyAuthority.Manager:
                    switch (fAuthPacket.AuthorityId)
                    {
                        case 0:
                            Session.Character.Family.ManagerCanInvite = fAuthPacket.Value == 1;
                            break;

                        case 1:
                            Session.Character.Family.ManagerCanNotice = fAuthPacket.Value == 1;
                            break;

                        case 2:
                            Session.Character.Family.ManagerCanShout = fAuthPacket.Value == 1;
                            break;

                        case 3:
                            Session.Character.Family.ManagerCanGetHistory = fAuthPacket.Value == 1;
                            break;

                        case 4:
                            Session.Character.Family.ManagerAuthorityType = (FamilyAuthorityType)fAuthPacket.Value;
                            break;
                    }
                    break;

                case FamilyAuthority.Member:
                    switch (fAuthPacket.AuthorityId)
                    {
                        case 0:
                            Session.Character.Family.MemberCanGetHistory = fAuthPacket.Value == 1;
                            break;

                        case 1:
                            Session.Character.Family.MemberAuthorityType = (FamilyAuthorityType)fAuthPacket.Value;
                            break;
                    }
                    break;
            }
            FamilyDTO fam = Session.Character.Family;
            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
            ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
            {
                DestinationCharacterId = fam.FamilyId,
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = "fhis_stc",
                Type = MessageType.Family
            });
            Session.SendPacket(Session.Character.GenerateGInfo());
        }

        /// <summary>
        /// glmk packet
        /// </summary>
        /// <param name="createFamilyPacket"></param>
        public void CreateFamily(CreateFamilyPacket createFamilyPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);

            if (Session.Character.Group != null && Session.Character.Group.GroupType == GroupType.Group && Session.Character.Group.CharacterCount == 3)
            {
                foreach (ClientSession session in Session.Character.Group.Characters)
                {
                    if (session.Character.Family != null || session.Character.FamilyCharacter != null)
                    {
                        return;
                    }
                }
                if (Session.Character.Gold < 200000)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                    return;
                }
                string name = createFamilyPacket.CharacterName;
                if (DAOFactory.FamilyDAO.LoadByName(name) != null)
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_NAME_ALREADY_USED")));
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

                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAMILY_FOUNDED"), name), 0));
                foreach (ClientSession session in Session.Character.Group.Characters)
                {
                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO
                    {
                        CharacterId = session.Character.CharacterId,
                        DailyMessage = string.Empty,
                        Experience = 0,
                        Authority = Session.Character.CharacterId == session.Character.CharacterId ? FamilyAuthority.Head : FamilyAuthority.Assistant,
                        FamilyId = family.FamilyId,
                        Rank = 0
                    };
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);
                }
                ServerManager.Instance.FamilyRefresh(family.FamilyId);
                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                {
                    DestinationCharacterId = family.FamilyId,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = "fhis_stc",
                    Type = MessageType.Family
                }); Session.Character.Group.Characters.ForEach(s => s.CurrentMapInstance.Broadcast(s.Character.GenerateGidx()));
            }
        }

        [Packet("%Familyshout")]
        [Packet("%Cridefamille")]
        public void FamilyCall(string packet)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager && Session.Character.Family.ManagerCanShout || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
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
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                    {
                        DestinationCharacterId = Session.Character.Family.FamilyId,
                        SourceCharacterId = Session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = UserInterfaceHelper.Instance.GenerateMsg($"<{Language.Instance.GetMessageFromKey("FAMILYCALL")}> {msg}", 0),
                        Type = MessageType.Family
                    });
                }
            }
        }

        /// <summary>
        /// today_cts packet
        /// </summary>
        /// <param name="todayPacket"></param>
        public void FamilyChangeMessage(TodayPacket todayPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            Session.SendPacket("today_stc");
        }

        /// <summary>
        /// : packet
        /// </summary>
        /// <param name="familyChatPacket"></param>
        public void FamilyChat(FamilyChatPacket familyChatPacket)
        {
            if (string.IsNullOrEmpty(familyChatPacket.Message))
            {
                return;
            }
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                string msg = familyChatPacket.Message;
                string ccmsg = $"[{Session.Character.Name}]:{msg}";
                if (Session.Account.Authority == AuthorityType.GameMaster)
                {
                    ccmsg = $"[GM {Session.Character.Name}]:{msg}";
                }

                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                {
                    DestinationCharacterId = Session.Character.Family.FamilyId,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = ccmsg,
                    Type = MessageType.FamilyChat
                });
                List<ClientSession> tmp = ServerManager.Instance.Sessions.ToList();
                foreach (ClientSession s in tmp)
                {
                    if (s.HasSelectedCharacter && s.Character.Family != null && Session.Character.Family != null && s.Character.Family?.FamilyId == Session.Character.Family?.FamilyId)
                    {
                        if (Session.HasCurrentMapInstance && s.HasCurrentMapInstance && Session.CurrentMapInstance == s.CurrentMapInstance && !Session.Character.InvisibleGm)
                        {
                            s.SendPacket(Session.Character.GenerateSay(msg, 6));
                        }
                        else
                        {
                            string prefix = $"[{Session.Character.Name}]:";
                            if (Session.Account.Authority == AuthorityType.GameMaster)
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

        /// <summary>
        /// f_deposit packet
        /// </summary>
        /// <param name="fDepositPacket"></param>
        public void FamilyDeposit(FDepositPacket fDepositPacket)
        {
            if (Session.Character.Family == null || !(Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head
              || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant
              || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member && Session.Character.Family.MemberAuthorityType != FamilyAuthorityType.NONE
              || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager && Session.Character.Family.ManagerAuthorityType != FamilyAuthorityType.NONE
              )
             )
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return;
            }

            ItemInstance item = Session.Character.Inventory.LoadBySlotAndType(fDepositPacket.Slot, fDepositPacket.Inventory);
            ItemInstance itemdest = Session.Character.Family.Warehouse.LoadBySlotAndType(fDepositPacket.NewSlot, InventoryType.FamilyWareHouse);

            // check if the destination slot is out of range
            if (fDepositPacket.NewSlot > Session.Character.Family.WarehouseSize)
            {
                return;
            }

            // check if the character is allowed to move the item
            if (Session.Character.InExchangeOrTrade)
            {
                return;
            }

            // actually move the item from source to destination
            Session.Character.Inventory.FDepositItem(fDepositPacket.Inventory, fDepositPacket.Slot, fDepositPacket.Amount, fDepositPacket.NewSlot, ref item, ref itemdest);
        }

        /// <summary>
        /// glrm packet
        /// </summary>
        /// <param name="familyDissmissPacket"></param>
        public void FamilyDismiss(FamilyDismissPacket familyDissmissPacket)
        {
            if (Session.Character.Family == null || Session.Character.FamilyCharacter == null || Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
            {
                return;
            }
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);

            Family fam = Session.Character.Family;
            List<ClientSession> sessions = ServerManager.Instance.Sessions.Where(s => s.Character?.Family != null && s.Character.Family.FamilyId == fam.FamilyId).ToList();

            fam.FamilyCharacters.ForEach(s => { DAOFactory.FamilyCharacterDAO.Delete(s.Character.Name); });
            fam.FamilyLogs.ForEach(s => { DAOFactory.FamilyLogDAO.Delete(s.FamilyLogId); });
            DAOFactory.FamilyDAO.Delete(fam.FamilyId);
            ServerManager.Instance.FamilyRefresh(fam.FamilyId);
            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
            {
                DestinationCharacterId = fam.FamilyId,
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = "fhis_stc",
                Type = MessageType.Family
            });
            sessions.ForEach(s => s.CurrentMapInstance.Broadcast(s.Character.GenerateGidx()));
        }

        [Packet("%Familydismiss")]
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
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("NOT_ALLOWED_KICK"))));
                    return;
                }
                ClientSession kickSession = ServerManager.Instance.GetSessionByCharacterName(packetsplit[2]);
                if (kickSession != null && kickSession.Character.Family?.FamilyId == Session.Character.Family.FamilyId)
                {
                    if (kickSession.Character.FamilyCharacter?.Authority == FamilyAuthority.Head)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_KICK_HEAD")));
                        return;
                    }
                    if (kickSession.Character.CharacterId == Session.Character.CharacterId)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_KICK_YOURSELF")));
                        return;
                    }
                    DAOFactory.FamilyCharacterDAO.Delete(packetsplit[2]);
                    Session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyManaged, kickSession.Character.Name);

                    kickSession.CurrentMapInstance?.Broadcast(kickSession.Character.GenerateGidx());
                }
                else
                {
                    CharacterDTO dbCharacter = DAOFactory.CharacterDAO.LoadByName(packetsplit[2]);
                    if (dbCharacter != null)
                    {
                        FamilyCharacterDTO dbFamilyCharacter = DAOFactory.FamilyCharacterDAO.LoadByCharacterId(dbCharacter.CharacterId);
                        if (dbFamilyCharacter != null && dbFamilyCharacter.FamilyId == Session.Character.Family.FamilyId)
                        {
                            if (dbFamilyCharacter.Authority == FamilyAuthority.Head)
                            {
                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_KICK_HEAD")));
                                return;
                            }
                            DAOFactory.FamilyCharacterDAO.Delete(packetsplit[2]);
                            Session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyManaged, dbCharacter.Name);
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
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("CANNOT_LEAVE_FAMILY")));
                    return;
                }
                Session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyManaged, Session.Character.Name);
                long FamilyId = Session.Character.Family.FamilyId;
                DAOFactory.FamilyCharacterDAO.Delete(Session.Character.Name);

                ServerManager.Instance.FamilyRefresh(FamilyId);
                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                {
                    DestinationCharacterId = FamilyId,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = "fhis_stc",
                    Type = MessageType.Family
                });
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
            }
        }

        /// <summary>
        /// glist packet
        /// </summary>
        /// <param name="gListPacket"></param>
        public void FamilyList(GListPacket gListPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                if (gListPacket.Type == 2)
                {
                    Session.SendPacket(Session.Character.GenerateGInfo());
                    Session.SendPacket(Session.Character.GenerateFamilyMember());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberExp());
                }
            }
        }

        /// <summary>
        /// fmg packet
        /// </summary>
        /// <param name="familyManagementPacket"></param>
        public void FamilyManagement(FamilyManagementPacket familyManagementPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            if (Session.Character.Family == null)
            {
                return;
            }
            if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager)
            {
                return;
            }
            long targetId = familyManagementPacket.TargetId;
            if (DAOFactory.FamilyCharacterDAO.LoadByCharacterId(targetId)?.FamilyId != Session.Character.FamilyCharacter.FamilyId)
            {
                return;
            }
            ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(targetId);
            switch (familyManagementPacket.FamilyAuthorityType)
            {
                case FamilyAuthority.Head:
                    if (targetSession == null)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("PLAYER_OFFLINE")));
                        return;
                    }
                    if (Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                        return;
                    }
                    if (targetSession.Character.FamilyCharacter.Authority != FamilyAuthority.Assistant)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("ONLY_PROMOTE_ASSISTANT")));
                        return;
                    }
                    targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Head;
                    FamilyCharacterDTO chara = targetSession.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);

                    Session.Character.Family.Warehouse.GetAllItems().ForEach(s => { s.CharacterId = targetSession.Character.CharacterId; DAOFactory.IteminstanceDAO.InsertOrUpdate(s); });
                    Session.Character.FamilyCharacter.Authority = FamilyAuthority.Assistant;
                    FamilyCharacterDTO chara2 = Session.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara2);

                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                    break;

                case FamilyAuthority.Assistant:
                    if (targetSession == null)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("PLAYER_OFFLINE")));
                        return;
                    }
                    if (Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                        return;
                    }
                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                        return;
                    }
                    if (DAOFactory.FamilyCharacterDAO.LoadByFamilyId(Session.Character.Family.FamilyId).Count(s => s.Authority == FamilyAuthority.Assistant) == 2)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_TWO_ASSISTANT")));
                        return;
                    }
                    targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Assistant;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                    chara = targetSession.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);

                    break;

                case FamilyAuthority.Manager:
                    if (targetSession == null)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("PLAYER_OFFLINE")));
                        return;
                    }
                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                        return;
                    }
                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant && Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("ASSISTANT_UNDEMOTABLE")));
                        return;
                    }
                    targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Manager;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                    chara = targetSession.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);

                    break;

                case FamilyAuthority.Member:
                    if (targetSession == null)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("PLAYER_OFFLINE")));
                        return;
                    }
                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                        return;
                    }
                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant && Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("ASSISTANT_UNDEMOTABLE")));
                        return;
                    }
                    targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Member;
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                    chara = targetSession.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);
                    break;
            }
            Session.Character.Family.InsertFamilyLog(FamilyLogType.AuthorityChanged, Session.Character.Name, targetSession.Character.Name, authority: familyManagementPacket.FamilyAuthorityType);
            targetSession.CurrentMapInstance?.Broadcast(targetSession.Character.GenerateGidx());
            if (familyManagementPacket.FamilyAuthorityType == FamilyAuthority.Head)
            {
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
            }
        }

        [Packet("%Notice")]
        [Packet("%Avertissement")]
        public void FamilyMessage(string packet)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager && Session.Character.Family.ManagerCanShout || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
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
                    ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                    {
                        DestinationCharacterId = Session.Character.Family.FamilyId,
                        SourceCharacterId = Session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = "fhis_stc",
                        Type = MessageType.Family
                    });
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                        {
                            DestinationCharacterId = Session.Character.Family.FamilyId,
                            SourceCharacterId = Session.Character.CharacterId,
                            SourceWorldId = ServerManager.Instance.WorldId,
                            Message = UserInterfaceHelper.Instance.GenerateInfo("--- Family Message ---\n" + Session.Character.Family.FamilyMessage),
                            Type = MessageType.Family
                        });
                    }
                }
            }
        }

        /// <summary>
        /// frank_cts packet
        /// </summary>
        /// <param name="frankCtsPacket"></param>
        public void FamilyRank(FrankCtsPacket frankCtsPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateFrank(frankCtsPacket.Type));
        }

        /// <summary>
        /// fhis_cts packet
        /// </summary>
        /// <param name="fhistCtsPacket"></param>
        public void FamilyRefreshHist(FhistCtsPacket fhistCtsPacket)
        {
            Session.SendPackets(Session.Character.GetFamilyHistory());
        }

        /// <summary>
        /// f_repos packet
        /// </summary>
        /// <param name="fReposPacket"></param>
        public void FamilyRepos(FReposPacket fReposPacket)
        {
            if (Session.Character.Family == null || !(Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head
             || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant
             || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member && Session.Character.Family.MemberAuthorityType == FamilyAuthorityType.ALL
             || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager && Session.Character.Family.ManagerAuthorityType == FamilyAuthorityType.ALL))
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return;
            }

            // check if the character is allowed to move the item
            if (Session.Character.InExchangeOrTrade || fReposPacket.Amount <= 0)
            {
                return;
            }
            if (fReposPacket.NewSlot > Session.Character.Family.WarehouseSize)
            {
                return;
            }

            ItemInstance sourceInventory = Session.Character.Family.Warehouse.LoadBySlotAndType(fReposPacket.OldSlot, InventoryType.FamilyWareHouse);
            ItemInstance destinationInventory = Session.Character.Family.Warehouse.LoadBySlotAndType(fReposPacket.NewSlot, InventoryType.FamilyWareHouse);

            if (sourceInventory != null && fReposPacket.Amount <= sourceInventory.Amount)
            {
                if (destinationInventory == null)
                {
                    destinationInventory = sourceInventory.DeepCopy();
                    sourceInventory.Amount -= fReposPacket.Amount;
                    destinationInventory.Amount = fReposPacket.Amount;
                    destinationInventory.Slot = fReposPacket.NewSlot;
                    if (sourceInventory.Amount > 0)
                    {
                        destinationInventory.Id = Guid.NewGuid();
                    }
                    else
                    {
                        sourceInventory = null;
                    }
                }
                else
                {
                    if (destinationInventory.ItemVNum == sourceInventory.ItemVNum && (byte)sourceInventory.Item.Type != 0)
                    {
                        if (destinationInventory.Amount + fReposPacket.Amount > 99)
                        {
                            int saveItemCount = destinationInventory.Amount;
                            destinationInventory.Amount = 99;
                            sourceInventory.Amount = (byte)(saveItemCount + sourceInventory.Amount - 99);
                        }
                        else
                        {
                            destinationInventory.Amount += fReposPacket.Amount;
                            sourceInventory.Amount -= fReposPacket.Amount;
                            if (sourceInventory.Amount == 0)
                            {
                                DAOFactory.IteminstanceDAO.Delete(sourceInventory.Id);
                                sourceInventory = null;
                            }
                        }
                    }
                    else
                    {
                        destinationInventory.Slot = fReposPacket.OldSlot;
                        sourceInventory.Slot = fReposPacket.NewSlot;
                    }
                }
            }
            if (sourceInventory != null && sourceInventory.Amount > 0)
            {
                DAOFactory.IteminstanceDAO.InsertOrUpdate(sourceInventory);
            }
            if (destinationInventory != null && destinationInventory.Amount > 0)
            {
                DAOFactory.IteminstanceDAO.InsertOrUpdate(destinationInventory);
            }
            Session.SendPacket((destinationInventory != null) ? destinationInventory.GenerateFStash() : UserInterfaceHelper.Instance.GenerateFStashRemove(fReposPacket.NewSlot));
            Session.SendPacket((sourceInventory != null) ? sourceInventory.GenerateFStash() : UserInterfaceHelper.Instance.GenerateFStashRemove(fReposPacket.OldSlot));
            ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
        }

        /// <summary>
        /// f_withdraw packet
        /// </summary>
        /// <param name="fWithdrawPacket"></param>
        public void FamilyWithdraw(FWithdrawPacket fWithdrawPacket)
        {
            if (Session.Character.Family == null || !(Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head
           || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant
           || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member && Session.Character.Family.MemberAuthorityType == FamilyAuthorityType.ALL
           || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager && Session.Character.Family.ManagerAuthorityType == FamilyAuthorityType.ALL))
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return;
            }
            ItemInstance previousInventory = Session.Character.Family.Warehouse.LoadBySlotAndType(fWithdrawPacket.Slot, InventoryType.FamilyWareHouse);
            if (fWithdrawPacket.Amount <= 0 || previousInventory == null || fWithdrawPacket.Amount > previousInventory.Amount)
            {
                return;
            }
            ItemInstance item2 = previousInventory.DeepCopy();
            item2.Id = Guid.NewGuid();
            item2.Amount = fWithdrawPacket.Amount;
            item2.CharacterId = Session.Character.CharacterId;

            previousInventory.Amount -= fWithdrawPacket.Amount;
            if (previousInventory.Amount <= 0)
            {
                previousInventory = null;
            }
            List<ItemInstance> newInv = Session.Character.Inventory.AddToInventory(item2, item2.Item.Type);
            Session.SendPacket(UserInterfaceHelper.Instance.GenerateFStashRemove(fWithdrawPacket.Slot));
            if (previousInventory != null)
            {
                DAOFactory.IteminstanceDAO.InsertOrUpdate(previousInventory);
            }
            else
            {
                FamilyCharacter fhead = Session.Character.Family.FamilyCharacters.FirstOrDefault(s => s.Authority == FamilyAuthority.Head);
                if (fhead == null)
                    return;
                DAOFactory.IteminstanceDAO.DeleteFromSlotAndType(fhead.CharacterId, fWithdrawPacket.Slot, InventoryType.FamilyWareHouse);
            }
            Session.Character.Family.InsertFamilyLog(FamilyLogType.WareHouseRemoved, Session.Character.Name, message: $"{item2.ItemVNum}|{fWithdrawPacket.Amount}");
        }

        [Packet("%Familyinvite")]
        [Packet("%Invitationdefamille")]
        public void InviteFamily(string packet)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            string[] packetsplit = packet.Split(' ');

            if (packetsplit.Length != 3)
            {
                return;
            }
            if (Session.Character.Family == null || Session.Character.FamilyCharacter == null)
            {
                return;
            }

            if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager && !Session.Character.Family.ManagerCanInvite)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("FAMILY_INVITATION_NOT_ALLOWED"))));
                return;
            }

            ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterName(packetsplit[2]);
            if (otherSession == null)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"))));
                return;
            }

            if (Session.Character.IsBlockedByCharacter(otherSession.Character.CharacterId))
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                return;
            }

            if (Session.Character.Family.FamilyCharacters.Count() + 1 > Session.Character.Family.MaxSize)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_FULL")));
                return;
            }

            if (otherSession.Character.Family != null || otherSession.Character.FamilyCharacter != null)
            {
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_FAMILY")));
                return;
            }

            Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(string.Format(Language.Instance.GetMessageFromKey("FAMILY_INVITED"), otherSession.Character.Name)));
            otherSession.SendPacket(UserInterfaceHelper.Instance.GenerateDialog($"#gjoin^1^{Session.Character.CharacterId} #gjoin^2^{Session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("ASK_FAMILY_INVITED"), Session.Character.Family.Name)}"));
            Session.Character.FamilyInviteCharacters.Add(otherSession.Character.CharacterId);
        }

        /// <summary>
        /// gjoin packet
        /// </summary>
        /// <param name="joinFamilyPacket"></param>
        public void JoinFamily(JoinFamilyPacket joinFamilyPacket)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            long characterId = joinFamilyPacket.CharacterId;
            if (joinFamilyPacket.Type == 1)
            {
                ClientSession inviteSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                if (inviteSession != null && inviteSession.Character.FamilyInviteCharacters.Contains(Session.Character.CharacterId) && inviteSession.Character.Family != null && inviteSession.Character.Family.FamilyCharacters != null)
                {
                    if (inviteSession.Character.Family.FamilyCharacters.Count() + 1 > inviteSession.Character.Family.MaxSize)
                    {
                        return;
                    }
                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO
                    {
                        CharacterId = Session.Character.CharacterId,
                        DailyMessage = string.Empty,
                        Experience = 0,
                        Authority = FamilyAuthority.Member,
                        FamilyId = inviteSession.Character.Family.FamilyId,
                        Rank = 0
                    };
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);
                    inviteSession.Character.Family.InsertFamilyLog(FamilyLogType.UserManaged, inviteSession.Character.Name, Session.Character.Name);
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                    {
                        DestinationCharacterId = inviteSession.Character.Family.FamilyId,
                        SourceCharacterId = Session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAMILY_JOINED"), Session.Character.Name, inviteSession.Character.Family.Name), 0),
                        Type = MessageType.Family
                    });

                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
                    Session.SendPacket(Session.Character.GenerateFamilyMember());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberExp());
                }
            }
        }

        [Packet("%Sex")]
        [Packet("%Sexe")]
        public void ResetSex(string packet)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            string[] packetsplit = packet.Split(' ');
            if (packetsplit.Length != 3)
            {
                return;
            }
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null && Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head && byte.TryParse(packetsplit[2], out byte rank))
            {
                foreach (FamilyCharacter familyCharacter in Session.Character.Family.FamilyCharacters)
                {
                    FamilyCharacterDTO familyCharacterDto = familyCharacter;
                    familyCharacterDto.Rank = 0;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacterDto);
                }
                FamilyDTO fam = Session.Character.Family;
                fam.FamilyHeadGender = (GenderType)rank;
                DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                {
                    DestinationCharacterId = fam.FamilyId,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = "fhis_stc",
                    Type = MessageType.Family
                });
                Session.SendPacket(Session.Character.GenerateFamilyMember());
                Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());

                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                {
                    DestinationCharacterId = fam.FamilyId,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAMILY_HEAD_CHANGE_GENDER")), 0),
                    Type = MessageType.Family
                });
            }
        }

        [Packet("%Title")]
        [Packet("%Titre")]
        public void TitleChange(string packet)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null && Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
            {
                string[] packetsplit = packet.Split(' ');
                if (packetsplit.Length != 4)
                {
                    return;
                }

                FamilyCharacterDTO fchar = Session.Character.Family.FamilyCharacters.FirstOrDefault(s => s.Character.Name == packetsplit[2]);
                if (fchar != null && byte.TryParse(packetsplit[3], out byte rank))
                {
                    fchar.Rank = (FamilyMemberRank)rank;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref fchar);
                    ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                    {
                        DestinationCharacterId = Session.Character.Family.FamilyId,
                        SourceCharacterId = Session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = "fhis_stc",
                        Type = MessageType.Family
                    }); Session.SendPacket(Session.Character.GenerateFamilyMember());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                }
            }
        }

        [Packet("%Today")]
        [Packet("%Aujourd'hui")]
        public void TodayMessage(string packet)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
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
                bool islog = Session.Character.Family.FamilyLogs.Any(s => s.FamilyLogType == FamilyLogType.DailyMessage && s.FamilyLogData.StartsWith(Session.Character.Name) && s.Timestamp.AddDays(1) > DateTime.Now);
                if (!islog)
                {
                    Session.Character.FamilyCharacter.DailyMessage = msg;
                    FamilyCharacterDTO fchar = Session.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref fchar);
                    Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                    Session.Character.Family.InsertFamilyLog(FamilyLogType.DailyMessage, Session.Character.Name, message: msg);
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_CHANGE_MESSAGE")));
                }
            }
        }

        #endregion
    }
}