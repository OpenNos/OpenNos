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
    public enum FamilyLogType : byte
    {
        DailyMessage = 1,
        RaidWon = 2,
        RainbowBattle = 3,
        FamilyXP = 4,
        FamilyLevelUp = 5,
        LevelUp = 6,
        ItemUpgraded = 7,
        RightChanged = 8,
        AuthorityChanged = 9,
        FamilyManaged = 10,
        UserManaged = 11,
        WareHouseAdded = 12,
        WareHouseRemoved = 13
    }
}