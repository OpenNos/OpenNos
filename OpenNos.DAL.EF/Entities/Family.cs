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
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF
{
    public class Family
    {
        #region Properties

        //Not complety done
        public string Name { get; set; }

        public long FamilyId { get; set; }

        public string FamilyOwner { get; set; }

        public byte FamilyLevel { get; set; }

        public long FamilyExpAmount { get; set; }

        public byte FamilyAuthority { get; set; }

        public byte MemberAmount { get; set; }

        public byte MemberMaxAmount { get; set; }

        public DateTime FamilyEnterDate { get; set; }

        [MaxLength(255)]
        public string FamilyIntro { get; set; }

        public virtual byte OwnerSex { get; set; }

        #endregion
    }
}