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
using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF.Entities
{
    public class Nosmate
    {
        #region Properties

        public virtual Character Character { get; set; }

        public byte Attack { get; set; }

        public bool CanPickUp { get; set; }

        public long CharacterId { get; set; }

        public virtual NpcMonster NpcMonster { get; set; }

        public short NpcMonsterVNum { get; set; }

        public byte Defence { get; set; }

        public long Experience { get; set; }

        public bool HasSkin { get; set; }

        public bool IsSummonable { get; set; }

        public byte Level { get; set; }

        public short Loyalty { get; set; }

        public MateType MateType { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [Key]
        public long NosmateId { get; set; }

        #endregion
    }
}