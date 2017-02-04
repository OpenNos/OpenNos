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

namespace OpenNos.GameObject.Buff
{
    public class BCardEntry
    {
        #region Members

        public readonly bool AffectingOpposite;

        public readonly bool PVPOnly;

        public readonly SubType SubType;

        public readonly Type Type;

        public readonly int Value1;

        public readonly int Value2;

        #endregion

        #region Instantiation

        public BCardEntry(Type type, SubType subType, int value1, int value2, bool pvpOnly, bool affectingOpposite = false)
        {
            Type = type;
            SubType = subType;
            Value1 = value1;
            Value2 = value2;
            PVPOnly = pvpOnly;
            AffectingOpposite = affectingOpposite;
        }

        #endregion
    }
}