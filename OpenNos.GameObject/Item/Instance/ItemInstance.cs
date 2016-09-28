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

        private long _transportId;
        private Item item;
        private Random _random;
        #endregion

        #region Instantiation

        public ItemInstance()
        {
            _random = new Random();
        }

        public ItemInstance(ItemInstanceDTO inventoryItem)
        {
            Id = inventoryItem.Id;
            Amount = inventoryItem.Amount;
            ItemVNum = inventoryItem.ItemVNum;
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
                if (item == null)
                {
                    item = ServerManager.GetItem(this.ItemVNum);
                }
                return item;
            }
        }

        public long TransportId
        {
            get
            {
                if (_transportId == 0)
                {
                    // create transportId thru factory
                    _transportId = TransportFactory.Instance.GenerateTransportId();
                }

                return _transportId;
            }

            set
            {
                if (value != _transportId)
                {
                    _transportId = value;
                }
            }
        }

        #endregion

        //// TODO: create Interface

        #region Methods

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