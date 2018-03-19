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

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class NpcToSummon
    {
        #region Instantiation

        public NpcToSummon(short vnum, MapCell spawnCell, long target, ConcurrentBag<EventContainer> deathEvents, bool isProtected = false, bool isMate = false)
        {
            VNum = vnum;
            SpawnCell = spawnCell;
            Target = target;
            DeathEvents = deathEvents;
            IsProtected = isProtected;
            IsMate = isMate;
        }

        #endregion

        #region Properties

        public ConcurrentBag<EventContainer> DeathEvents { get; }

        public bool IsMate { get; }

        public bool IsProtected { get; }

        public MapCell SpawnCell { get; }

        public long Target { get; }

        public short VNum { get; }

        #endregion
    }
}