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

using OpenNos.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace OpenNos.GameObject
{
    public class EventWave
    {
        #region Methods

        #endregion
        public byte Delay { get; private set; }
        public byte Offset { get; set; }
        public DateTime LastStart { get; set; }
        public ConcurrentBag<EventContainer> Events { get; private set; }

        public EventWave(byte delay, ConcurrentBag<EventContainer> events, byte offset = 0)
        {
            Delay = delay;
            Offset = offset;
            Events = events;
        }
    }
}