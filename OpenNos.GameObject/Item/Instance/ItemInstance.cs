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
using OpenNos.Domain;
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
                return item ?? (item = ServerManager.Instance.GetItem(ItemVNum));
            }
        }

        #endregion

        //// TODO: create Interface

        #region Methods

        public ItemInstance DeepCopy()
        {
            return (ItemInstance)MemberwiseClone();
        }

        public string GenerateFStash()
        {
            return $"f_stash {GenerateStashPacket()}";
        }

        public string GenerateInventoryAdd()
        {
            switch (Type)
            {
                case InventoryType.Equipment:
                    return $"ivn 0 {Slot}.{ItemVNum}.{Rare}.{(Item.IsColored ? Design : Upgrade)}.0";

                case InventoryType.Main:
                    return $"ivn 1 {Slot}.{ItemVNum}.{Amount}.0";

                case InventoryType.Etc:
                    return $"ivn 2 {Slot}.{ItemVNum}.{Amount}.0";

                case InventoryType.Miniland:
                    return $"ivn 3 {Slot}.{ItemVNum}.{Amount}";

                case InventoryType.Specialist:
                    return $"ivn 6 {Slot}.{ItemVNum}.{Rare}.{Upgrade}.{(this as SpecialistInstance)?.SpStoneUpgrade}";

                case InventoryType.Costume:
                    return $"ivn 7 {Slot}.{ItemVNum}.{Rare}.{Upgrade}.0";
            }
            return string.Empty;
        }

        public string GeneratePStash()
        {
            return $"pstash {GenerateStashPacket()}";
        }

        public string GenerateStash()
        {
            return $"stash {GenerateStashPacket()}";
        }

        public string GenerateStashPacket()
        {
            string packet = $"{Slot}.{ItemVNum}.{(byte)Item.Type}";
            switch (Item.Type)
            {
                case InventoryType.Equipment:
                    return packet + $".{Amount}.{Rare}.{Upgrade}";

                case InventoryType.Specialist:
                    SpecialistInstance sp = this as SpecialistInstance;
                    return packet + $".{Upgrade}.{sp?.SpStoneUpgrade ?? 0}.0";

                default:
                    return packet + $".{Amount}.0.0";
            }
        }

        public void Save()
        {
        }

        #endregion
    }
}