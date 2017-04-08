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

namespace OpenNos.Data
{
    public class MateDTO : MappingBaseDTO
    {
        #region Properties

        public byte Attack { get; set; }

        public bool CanPickUp { get; set; }

        public long CharacterId { get; set; }

        public byte Defence { get; set; }

        public byte Direction { get; set; }

        public long Experience { get; set; }

        public int Hp { get; set; }

        public bool IsSummonable { get; set; }

        public bool IsTeamMember { get; set; }

        public byte Level { get; set; }

        public short Loyalty { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public long MateId { get; set; }

        public MateType MateType { get; set; }

        public int Mp { get; set; }

        public string Name { get; set; }

        public short NpcMonsterVNum { get; set; }

        public short Skin { get; set; }

        #endregion
    }
}