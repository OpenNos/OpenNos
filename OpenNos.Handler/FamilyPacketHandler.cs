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
using System;

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

        //#region Methods
        //[Packet("#glmk")]
        //public void CreateFamily(string packet)
        //{
        //    if (Session.Character.Group != null && Session.Character.Group.CharacterCount == 3)
        //    {
        //        string name = packet.Split('^')[1];
        //        FamilyDTO family = new FamilyDTO()
        //        {
        //            Name = name,
        //            FamilyExperience = 0,
        //            FamilyLevel = 1,
        //            FamilyMessage = "",
        //            MaxSize = 50,
        //            Size = 3
        //        };

        //        DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
        //        Session.CurrentMap.Broadcast(Session.Character.GenerateSay($"The family {name} was founded!", 13));
        //        foreach (ClientSession c in Session.Character.Group.Characters)
        //        {
        //            FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO()
        //            {
        //                CharacterId = c.Character.CharacterId,
        //                DailyMessage = "",
        //                Experience = 0,
        //                Authority = Domain.FamilyAuthority.Assistant,
        //                FamilyId = family.FamilyId,
        //                JoinDate = DateTime.Now,
        //                Rank = Domain.FamilyMemberRank.Member,
        //            };
        //            if(Session.Character.CharacterId == c.Character.CharacterId)
        //            {
        //                familyCharacter.Authority = Domain.FamilyAuthority.Head;
        //            }
        //            DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);
        //            Session.CurrentMap.Broadcast($"gidx 1 {c.Character.CharacterId} {family.FamilyId} {family.Name}({familyCharacter.Authority.ToString()}) ");
        //        }
        //    }
        //}

        //[Packet(":")]
        //public void FamilyChat(string packet)
        //{
        //    string msg = String.Empty;
        //    int i = 0;
        //    foreach (string s in packet.Split(' '))
        //    {
        //        if (i != 0)
        //        {
        //            msg += s + " ";
        //        }
        //        i++;
        //    }
        //    msg = msg.Substring(1);
            
        //    Session.CurrentMap.Broadcast(Session.Character.GenerateSay(msg, 6));
        //    Session.CurrentMap.Broadcast(Session.Character.GenerateSpk(msg, 1));
        //}
        //#endregion
    }
}