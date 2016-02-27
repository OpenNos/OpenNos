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
    public enum ItemType : byte
    {
        // wear
        Weapon = 0,
        Armor = 1,
        Fashion = 2,
        Jewelery = 3,
        Specialist = 4,
        Box = 5,
        Shell = 6,
        //sth = 7,8,9,
        Main = 10,
        Upgrade = 11,
        Production = 12,
        Map = 13,
        Magical1 = 14,
        Potion = 15,
        Event = 16,
        Quest = 17,
        //sth = 18,19,
        Sell = 20,
        Food = 21,
        Snack = 22,
        //sth = 23,
        Magical2 = 24,
        Part = 25,
        Teacher = 26,
        Ammo = 27,
        Special = 28
    }
}