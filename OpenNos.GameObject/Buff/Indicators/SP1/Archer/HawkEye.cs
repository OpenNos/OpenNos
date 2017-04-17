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

using OpenNos.GameObject.Buff.BCard;

namespace OpenNos.GameObject.Buff.Indicators.SP1.Archer
{
    public class HawkEye : IndicatorBase
    {
        #region Instantiation

        public HawkEye(int Level)
        {
            Name = "Hawk Eye";
            Duration = 3000;
            Id = 74;
            base.Level = Level;
            DirectBuffs.Add(new BCardEntry(Type.Effect, SubType.EagleEyes, 1, 0, false));
        }

        #endregion
    }
}