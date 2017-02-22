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
using System;

namespace OpenNos.GameObject
{
    public class ItemInstance : ItemInstanceDTO
    {
        #region Members

        private Random _random;
        private Item item;

        #endregion

        #region Instantiation

        public ItemInstance()
        {
            _random = new Random();
        }

        public ItemInstance(short vNum, byte amount)
        {
            ItemVNum = vNum;
            Amount = amount;
            Type = Item.Type;
            _random = new Random();
        }

        #endregion

        #region Properties

        public bool IsBound
        {
            get
            {
                return BoundCharacterId.HasValue;
            }
        }

        public Item Item
        {
            get
            {
                return item ?? (item = ServerManager.GetItem(ItemVNum));
            }
        }

        #endregion

        //// TODO: create Interface

        #region Methods

        public ItemInstance DeepCopy()
        {
            return (ItemInstance)MemberwiseClone();
        }

        public void Save()
        {
        }

        #endregion
    }
}