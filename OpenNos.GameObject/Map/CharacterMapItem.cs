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

namespace OpenNos.GameObject
{
    public class CharacterMapItem : MapItem
    {
        #region Instantiation

        public CharacterMapItem(short x, short y, ItemInstance itemInstance) : base(x, y)
        {
            ItemInstance = itemInstance;
        }

        #endregion

        #region Properties

        public override byte Amount
        {
            get
            {
                return ItemInstance.Amount;
            }

            set
            {
                ItemInstance.Amount = Amount;
            }
        }

        public ItemInstance ItemInstance { get; set; }

        public override short ItemVNum
        {
            get
            {
                return ItemInstance.ItemVNum;
            }

            set
            {
                ItemInstance.ItemVNum = value;
            }
        }

        public override long TransportId
        {
            get
            {
                return base.TransportId;
            }

            set
            {
                // cannot set TransportId
            }
        }

        #endregion

        #region Methods

        public override ItemInstance GetItemInstance()
        {
            return ItemInstance;
        }

        #endregion
    }
}