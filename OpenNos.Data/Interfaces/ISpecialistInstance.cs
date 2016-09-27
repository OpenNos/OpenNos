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
    public interface ISpecialistInstance : IWearableInstance
    {
        #region Properties

        short SlDamage { get; set; }

        short SlDefence { get; set; }

        short SlElement { get; set; }

        short SlHP { get; set; }

        byte SpDamage { get; set; }

        byte SpDark { get; set; }

        byte SpDefence { get; set; }

        byte SpElement { get; set; }

        byte SpFire { get; set; }

        byte SpHP { get; set; }

        byte SpLevel { get; set; }

        byte SpLight { get; set; }

        byte SpStoneUpgrade { get; set; }

        byte SpWater { get; set; }

        #endregion
    }
}