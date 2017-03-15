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

using NetHierarchy;
using OpenNos.Domain;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class MapInstanceNode : Node<MapInstance>
    {
        #region Properties
        public DateTime ClockEnd { get; set; }

        public List<Tuple<EventActionType, object>> EntryEvents { get; set; }

        #endregion

        #region Methods

        public void GenerateMinimap(MapInstanceNode node, ref List<string> liste)
        {
            liste.Add(node.Data.GenerateRsfn(true));
            foreach (MapInstanceNode map in node.Children)
            {
                GenerateMinimap(map, ref liste);
            }
        }
        public string GetClock()
        {
            return $"evnt 1 0 {(int)((ClockEnd - DateTime.Now).TotalSeconds * 10)} 1";
        }
        
        public string RunMapTreeEvent(EventActionType eventaction, object param)
        {
            switch (eventaction)
            {
                case EventActionType.CLOCK:
                    ClockEnd = DateTime.Now.AddSeconds(Convert.ToDouble(param));
                    break;
            }
            return string.Empty;
        }

        public List<string> GetMinimap()
        {
            List<string> lst = new List<string>();
            lst.Add("rsfm 0 0 4 12");
            GenerateMinimap(this, ref lst);
            return lst;
        }
        #endregion
    }
}