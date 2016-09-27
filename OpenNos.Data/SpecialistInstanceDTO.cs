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

namespace OpenNos.Data
{
    public class SpecialistInstanceDTO : WearableInstanceDTO, ISpecialistInstance
    {
        #region Properties

        public short SlDamage { get; set; }

        public short SlDefence { get; set; }

        public short SlElement { get; set; }

        public short SlHP { get; set; }

        public byte SpDamage { get; set; }

        public byte SpDark { get; set; }

        public byte SpDefence { get; set; }

        public byte SpElement { get; set; }

        public byte SpFire { get; set; }

        public byte SpHP { get; set; }

        public byte SpLevel { get; set; }

        public byte SpLight { get; set; }

        public byte SpStoneUpgrade { get; set; }

        public byte SpWater { get; set; }

        #endregion
    }
}