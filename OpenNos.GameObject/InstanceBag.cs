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

using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class InstanceBag
    {
        #region Instantiation

        public InstanceBag()
        {
            Clock = new Clock(1);
            DeadList = new List<long>();
            UnlockEvents = new List<EventContainer>();
            ButtonLocker = new Locker();
            MonsterLocker = new Locker();
        }

        #endregion

        #region Properties

        public Clock Clock { get; set; }

        public int Combo { get; set; }

        public long Creator { get; set; }

        public List<long> DeadList { get; set; }

        public byte EndState { get; set; }

        public short Lives { get; set; }

        public bool Lock { get; set; }

        public int MonstersKilled { get; set; }

        public int NpcsKilled { get; set; }

        public int Point { get; set; }

        public int RoomsVisited { get; set; }
        public Locker MonsterLocker { get;  set; }
        public Locker ButtonLocker { get;  set; }
        public List<EventContainer> UnlockEvents { get;  set; }

        #endregion

        #region Methods

        public string GenerateScore()
        {
            return $"rnsc {Point}";
        }

        #endregion
    }
}