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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF
{
    public class ItemCard
    {
        #region Properties

        public virtual Card Card { get; set; }

        public short CardChance { get; set; }

        [Key, Column(Order = 1)]
        public short CardId { get; set; }

        public virtual Item Item { get; set; }

        [Key, Column(Order = 0)]
        public short ItemVNum { get; set; }

        #endregion
    }
}