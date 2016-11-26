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

using System.Collections.Generic;

namespace OpenNos.DAL.EF
{
    public class RespawnMapType
    {

        public RespawnMapType()
        {
            Respawn = new HashSet<Respawn>();
            MapType = new HashSet<MapType>();
        }

        #region Properties

        public virtual ICollection<Respawn> Respawn { get; set; }

        public virtual Map Map { get; set; }

        public ICollection<MapType> MapType { get; set; }

        public short DefaultMapId { get; set; }

        public long RespawnMapTypeId { get; set; }

        public short DefaultX { get; set; }

        public short DefaultY { get; set; }

        #endregion
    }
}