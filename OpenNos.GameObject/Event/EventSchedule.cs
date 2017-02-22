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
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace OpenNos.GameObject
{
    public class EventSchedule : IConfigurationSectionHandler
    {
        #region Methods

        public object Create(object parent, object configContext, XmlNode section)
        {
            List<Schedule> list = new List<Schedule>();
            foreach (XmlNode aSchedule in section.ChildNodes)
            {
                list.Add(GetSchedule(aSchedule));
            }
            return list;
        }

        private static Schedule GetSchedule(XmlNode str)
        {
            if (str.Attributes != null)
            {
                Schedule result = new Schedule
                {
                    Event = (EventType)Enum.Parse(typeof(EventType), str.Attributes["event"].Value),
                    Time = TimeSpan.Parse(str.Attributes["time"].Value)
                };
                return result;
            }
            return null;
        }

        #endregion
    }
}