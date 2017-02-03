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

using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class MonsterMapItem : MapItem
    {
        #region Instantiation

        public MonsterMapItem(short x, short y, short itemVNum, int amount = 1, long ownerId = -1) : base(x, y)
        {
            ItemVNum = itemVNum;
            if (amount < 100)
            {
                Amount = (byte)amount;
            }
            GoldAmount = amount;
            OwnerId = ownerId;
        }

        #endregion

        #region Properties

        public sealed override byte Amount { get; set; }

        public int GoldAmount { get; private set; }

        public sealed override short ItemVNum { get; set; }

        public long? OwnerId { get; }

        #endregion

        #region Methods

        public override ItemInstance GetItemInstance()
        {
            if (_itemInstance == null && OwnerId != null)
            {
                _itemInstance = Inventory.InstantiateItemInstance(ItemVNum, OwnerId.Value, Amount);
            }
            return _itemInstance;
        }

        public void Rarify(ClientSession session)
        {
            ItemInstance instance = GetItemInstance();
            if (instance.Item.Type == InventoryType.Equipment && (instance.Item.ItemType == ItemType.Weapon || instance.Item.ItemType == ItemType.Armor))
            {
                WearableInstance wearableInstance = instance as WearableInstance;
                wearableInstance?.RarifyItem(session, RarifyMode.Drop, RarifyProtection.None);
            }
        }

        #endregion
    }
}