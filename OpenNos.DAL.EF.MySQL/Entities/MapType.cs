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

namespace OpenNos.DAL.EF.MySQL
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MapType
    {
        #region Instantiation

        public MapType()
        {
            Maps = new HashSet<Map>();
            MapTypeMaps = new HashSet<MapTypeMap>();
        }

        #endregion

        #region Properties

        public virtual ICollection<Drop> Drops { get; set; }
        public virtual Map Map { get; set; }

        [ForeignKey(nameof(Map))]
        public short MapId { get; set; }

        public virtual ICollection<Map> Maps { get; set; }
        public short MapTypeId { get; set; }
        public virtual ICollection<MapTypeMap> MapTypeMaps { get; set; }

        #endregion
    }
}