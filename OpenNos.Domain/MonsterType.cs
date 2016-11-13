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

namespace OpenNos.Domain
{
    public enum MonsterType : byte
    {
        Unknown = 0,
        Partner = 1,
        NPC = 2,
        Well = 3,
        Portal = 4,
        Boss = 5,
        Elite = 6,
        Peapod = 7,
        Special = 8,
        GemSpaceTime = 9
    }
}