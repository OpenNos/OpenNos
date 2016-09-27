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

namespace OpenNos.GameObject
{
    public class Inventory : InventoryDTO
    {
        #region Instantiation

        public Inventory()
        {
        }

        public Inventory(InventoryDTO inventory)
        {
            this.CharacterId = inventory.CharacterId;
            this.Slot = inventory.Slot;
            this.Type = inventory.Type;
            this.ItemInstance = inventory.ItemInstance;
            this.Id = inventory.Id;
        }

        #endregion

        #region Methods

        internal Inventory DeepCopy()
        {
            return (Inventory)this.MemberwiseClone();
        }

        #endregion
    }
}