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

namespace OpenNos.DAL.EF.MySQL
{
    using System;
    using Domain;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PenaltyLog
    {
        #region Properties

        [Key]
        public int PenaltyLogId { get; set; }
        [ForeignKey(nameof(Account))]
        public long AccountId{get;set;}
        public DateTime DateEnd { get; set; }
        public DateTime DateStart { get; set; }
        [MaxLength(255)]
        public string Reason { get; set; }
        public PenaltyType Penatly { get; set; }
        public virtual Account Account { get; set; }

        #endregion
    }
}