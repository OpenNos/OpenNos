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

namespace OpenNos.Pathfinding
{
    public class Node : GridPos, IComparable<Node>
    {
        #region Properties

        public Double F { get; internal set; }

        public Double N { get; internal set; }

        public bool Opened { get; internal set; }

        public Node Parent { get; internal set; }

        #endregion

        #region Methods

        public int CompareTo(Node other)
        {
            return F > other.F ? 1 : F < other.F ? -1 : 0;
        }

        #endregion
    }
}