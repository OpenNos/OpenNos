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
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ItemInstance : SynchronizableBaseEntity
    {
        #region Properties

        public int Amount { get; set; }

        [ForeignKey(nameof(BoundCharacterId))]
        public Character BoundCharacter { get; set; }

        public long? BoundCharacterId { get; set; }

        public short Design { get; set; }

        public virtual Inventory Inventory { get; set; }

        public virtual Item Item { get; set; }

        public DateTime? ItemDeleteTime { get; set; }

        public short ItemVNum { get; set; }

        public int DurabilityPoint { get; set; }

        public sbyte Rare { get; set; }

        public byte Upgrade { get; set; }

        #endregion
    }
}