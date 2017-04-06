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

using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class Clock
    {
        #region Instantiation

        public Clock(byte type)
        {
            StopEvents = new List<EventContainer>();
            TimeoutEvents = new List<EventContainer>();
            Type = type;
            DeciSecondRemaining = 1;
            Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(
           x =>
           {
               tick();
           });
        }

        #endregion

        #region Properties

        public int BasesSecondRemaining { get; set; }

        public int DeciSecondRemaining { get; set; }

        public bool Enabled { get; private set; }

        public List<EventContainer> StopEvents { get; set; }

        public List<EventContainer> TimeoutEvents { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public string GetClock()
        {
            return $"evnt {Type} {(Enabled ? 0 : (Type != 3) ? -1 : 1)} {DeciSecondRemaining} {BasesSecondRemaining}";
        }

        public void StartClock()
        {
            Enabled = true;
        }

        public void StopClock()
        {
            Enabled = false;
            StopEvents.ForEach(e =>
            {
                EventHelper.Instance.RunEvent(e);
            });
            StopEvents.RemoveAll(s => s != null);
        }

        private void tick()
        {
            if (Enabled)
            {
                if (DeciSecondRemaining > 0)
                {
                    DeciSecondRemaining -= 10;
                }
                else
                {
                    TimeoutEvents.ForEach(ev =>
                    {
                        EventHelper.Instance.RunEvent(ev);
                    });
                    TimeoutEvents.RemoveAll(s => s != null);
                }
            }
        }

        #endregion
    }
}