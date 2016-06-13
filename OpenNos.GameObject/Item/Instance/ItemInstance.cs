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
    public class ItemInstance : ItemInstanceDTO
    {
        #region Private Members

        private Item item;

        #endregion

        #region Public Instantiation

        public ItemInstance()
        {
        }

        public ItemInstance(ItemInstanceDTO inventoryItem)
        {
            ItemInstanceId = inventoryItem.ItemInstanceId;
            Amount = inventoryItem.Amount;
            ItemVNum = inventoryItem.ItemVNum;
        }

        #endregion

        #region Public Properties

        public Item Item
        {
            get
            {
                if (item == null) item = ServerManager.GetItem(this.ItemVNum);
                return item;
            }
        }

        #endregion

        #region Public Methods

        public ItemInstance DeepCopy()
        {
            return (ItemInstance)this.MemberwiseClone();
        }

        public void Save()
        {
        }

        #endregion
    }
}