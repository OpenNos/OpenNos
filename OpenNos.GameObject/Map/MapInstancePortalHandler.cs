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

using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class MapInstancePortalHandler
    {
        public static int SourceX { get; private set; }
        #region Methods
        public static List<MapInstanceTree> GenerateMapInstanceTree(short entryMap)
        {
            List<MapInstanceTree> list = new List<MapInstanceTree>();

            switch (entryMap)
            {
                case 1:
                    list.Add(new MapInstanceTree
                    {
                        MapInstanceTreeType = MapInstanceTreeType.TimeSpace,
                        Portal = new PortalDTO()
                        {
                            SourceX = 134,
                            SourceY = 36,
                            SourceMapId = 1
                        },
                        LevelMaximum = 99,
                        LevelMinimum = 1
                        
                    });
                    break;
            }
            return list;
        }

        public static List<Portal> GenerateMinilandEntryPortals(short entryMap, Guid exitMapinstanceId)
        {
            List<Portal> list = new List<Portal>();

            switch (entryMap)
            {
                case 1:
                    list.Add(new Portal
                    {
                        SourceX = 48,
                        SourceY = 132,
                        DestinationX = 5,
                        DestinationY = 8,
                        Type = (short)PortalType.Miniland,
                        SourceMapId = 1,
                        DestinationMapInstanceId = exitMapinstanceId
                    });
                    break;

                case 145:
                    list.Add(new Portal
                    {
                        SourceX = 9,
                        SourceY = 171,
                        DestinationX = 5,
                        DestinationY = 8,
                        Type = (short)PortalType.Miniland,
                        SourceMapId = 145,
                        DestinationMapInstanceId = exitMapinstanceId
                    });
                    break;
            }

            return list;
        }

        #endregion
    }
}