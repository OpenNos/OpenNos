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
    public interface IWearableInstance : IItemInstance
    {
        #region Properties

        byte Ammo { get; set; }

        byte Cellon { get; set; }

        short CloseDefence { get; set; }

        short Concentrate { get; set; }

        short CriticalDodge { get; set; }

        byte CriticalLuckRate { get; set; }

        short CriticalRate { get; set; }

        short DamageMaximum { get; set; }

        short DamageMinimum { get; set; }

        byte DarkElement { get; set; }

        short DarkResistance { get; set; }

        short DefenceDodge { get; set; }

        short DistanceDefence { get; set; }

        short DistanceDefenceDodge { get; set; }

        short ElementRate { get; set; }

        byte FireElement { get; set; }

        short FireResistance { get; set; }

        short HitRate { get; set; }

        short HP { get; set; }

        bool IsEmpty { get; set; }

        bool IsFixed { get; set; }

        byte LightElement { get; set; }

        short LightResistance { get; set; }

        short MagicDefence { get; set; }

        short MP { get; set; }

        byte WaterElement { get; set; }

        short WaterResistance { get; set; }

        long XP { get; set; }

        #endregion
    }
}