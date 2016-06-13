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

using System.ComponentModel.DataAnnotations.Schema;

namespace OpenNos.DAL.EF.MySQL
{
    public class Inventory
    {
        #region Public Properties

        public virtual Character Character { get; set; }

        [Index("IX_SlotAndType", 1, IsUnique = true, Order = 0)]
        public long CharacterId { get; set; }

        public long InventoryId { get; set; }

        public virtual ItemInstance ItemInstance { get; set; }

        [Index("IX_SlotAndType", 2, IsUnique = true, Order = 1)]
        public short Slot { get; set; }

        [Index("IX_SlotAndType", 3, IsUnique = true, Order = 2)]
        public byte Type { get; set; }

        #endregion
    }
}