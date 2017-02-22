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

using OpenNos.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        #region Properties

        public virtual ICollection<FamilyCharacter> FamilyCharacters { get; set; }

        public int FamilyExperience { get; set; }

        public GenderType FamilyHeadGender { get; set; }

        public long FamilyId { get; set; }

        public byte FamilyLevel { get; set; }

        public virtual ICollection<FamilyLog> FamilyLogs { get; set; }

        [MaxLength(255)]
        public string FamilyMessage { get; set; }

        public FamilyAuthorityType ManagerAuthorityType { get; set; }

        public bool ManagerCanGetHistory { get; set; }

        public bool ManagerCanInvite { get; set; }

        public bool ManagerCanNotice { get; set; }

        public bool ManagerCanShout { get; set; }

        public byte MaxSize { get; set; }

        public FamilyAuthorityType MemberAuthorityType { get; set; }

        public bool MemberCanGetHistory { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public byte WarehouseSize { get; set; }

        #endregion
    }
}