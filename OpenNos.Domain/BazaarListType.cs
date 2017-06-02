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
    public enum BazaarListType : byte
    {
        Default = 0,
        Weapon = 1,
        Armor = 2,
        Equipment = 3,
        Jewelery = 4,
        Specialist = 5,
        Pet = 6,
        Npc = 7,
        Shell = 8,
        Main = 9,
        Usable = 10,
        Other = 11,
        Vehicle = 12
    }
}