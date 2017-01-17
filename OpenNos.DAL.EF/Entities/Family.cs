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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OpenNos.Domain;
using OpenNos.Data;

namespace OpenNos.DAL.EF
{
    public class Family
    {
        #region Instantiation

        public Family()
        {
            FamilyCharacters = new HashSet<FamilyCharacter>();
            FamilyLogs = new HashSet<FamilyLog>();
        }

        #endregion

        #region Method
        public void InsertFamilyLog(FamilyLogType logtype, string CharacterName, string CharacterName2, string RainBowFamily, string Message, byte Level, int Experience, int ItemVNum, byte Upgrade, int RaidType)
        {
            string value = string.Empty;
            switch (logtype)
            {
                case FamilyLogType.DailyMessage:
                    value = Message;
                    break;
                case FamilyLogType.FamilyXP:
                    value = Experience.ToString();
                    break;
                case FamilyLogType.Level:
                    value = Level.ToString();
                    break;
                case FamilyLogType.Raid:
                    value = RaidType.ToString();
                    break;
                case FamilyLogType.Upgrade:
                    value = $"{ItemVNum}|{Upgrade}";
                    break;
                case FamilyLogType.UserManage:
                    value = CharacterName2;
                    break;
                case FamilyLogType.FamilyLevel:
                    value = Level.ToString();
                    break;
                case FamilyLogType.FamilyManage:
                    value = CharacterName;
                    break;
                case FamilyLogType.RainbowBattle:
                    value = RainBowFamily;
                    break;
            }
            FamilyLogDTO log = new FamilyLogDTO();
            if(log!=null)
            {
               //insert
               //refresh
            }
        }
        #endregion

        #region Properties

        public virtual ICollection<FamilyCharacter> FamilyCharacters { get; set; }

        public int FamilyExperience { get; set; }

        public long FamilyId { get; set; }

        public byte FamilyLevel { get; set; }

        public virtual ICollection<FamilyLog> FamilyLogs { get; set; }

        [MaxLength(255)]
        public string FamilyMessage { get; set; }

        public byte MaxSize { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public bool ManagerCanInvite { get; set; }

        public bool ManagerCanShout { get; set; }

        public bool ManagerCanNotice { get; set; }

        public FamilyAuthorityType ManagerAuthorityType { get; set; }

        public bool ManagerCanGetHistory { get; set; }

        public bool MemberCanGetHistory { get; set; }

        public GenderType FamilyHeadGender { get; set; }

        public FamilyAuthorityType MemberAuthorityType { get; set; }


        #endregion
    }
}