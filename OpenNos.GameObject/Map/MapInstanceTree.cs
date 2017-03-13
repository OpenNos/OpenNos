﻿using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class MapInstanceTree
    {
        public short LevelMaximum { get; internal set; }

        public short LevelMinimum { get; internal set; }

        public MapInstanceTreeType MapInstanceTreeType { get; set; }

        public PortalDTO Portal { get; set; }

        public List<Item> Items { get; set; }

        public string Label { get; set; }

    }
}