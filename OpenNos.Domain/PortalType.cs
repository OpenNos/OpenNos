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
    public enum PortalType : sbyte
    {
        MapPortal = -1,
        TSNormal = 0, // same over >127 - sbyte
        Closed = 1,
        Open = 2,
        Miniland = 3,
        TSEnd = 4,
        TSEndClosed = 5,
        Exit = 6,
        ExitClosed = 7,
        Raid = 8,
        Effect = 9, // same as 13 - 19 and 20 - 126
        BlueRaid = 10,
        DarkRaid = 11,
        TimeSpace = 12,
        ShopTeleport = 20
    }
}