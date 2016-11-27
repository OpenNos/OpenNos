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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF
{
    public class RespawnMapType
    {
        #region Instantiation

        public RespawnMapType()
        {
            Respawn = new HashSet<Respawn>();
            MapTypes = new HashSet<MapType>();
            MapTypes1 = new HashSet<MapType>();
        }

        #endregion

        #region Properties

        public short DefaultMapId { get; set; }

        public short DefaultX { get; set; }

        public short DefaultY { get; set; }

        public virtual Map Map { get; set; }

        public ICollection<MapType> MapTypes { get; set; }

        public ICollection<MapType> MapTypes1 { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public virtual ICollection<Respawn> Respawn { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RespawnMapTypeId { get; set; }

        #endregion
    }
}