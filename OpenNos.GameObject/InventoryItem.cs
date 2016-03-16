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

using AutoMapper;
using OpenNos.Data;

namespace OpenNos.GameObject
{
    public class InventoryItem : InventoryItemDTO, IGameObject
    {
        #region Instantiation

        public InventoryItem()
        {
            Mapper.CreateMap<InventoryItemDTO, InventoryItem>();
            Mapper.CreateMap<InventoryItem, InventoryItemDTO>();
        }

        public InventoryItem(InventoryItemDTO inventoryItem)
        {
            InventoryItemId = inventoryItem.InventoryItemId;
            Amount = inventoryItem.Amount;
            ElementRate = inventoryItem.ElementRate;
            HitRate = inventoryItem.HitRate;
            Design = inventoryItem.Design;
            Concentrate = inventoryItem.Concentrate;
            CriticalLuckRate = inventoryItem.CriticalLuckRate;
            CriticalRate = inventoryItem.CriticalRate;
            DamageMaximum = inventoryItem.DamageMaximum;
            DamageMinimum = inventoryItem.DamageMinimum;
            DarkElement = inventoryItem.DarkElement;
            DistanceDefence = inventoryItem.DistanceDefence;
            DistanceDefenceDodge = inventoryItem.DistanceDefenceDodge;
            DefenceDodge = inventoryItem.DefenceDodge;
            FireElement = inventoryItem.FireElement;
            ItemVNum = inventoryItem.ItemVNum;
            LightElement = inventoryItem.LightElement;
            MagicDefence = inventoryItem.MagicDefence;
            CloseDefence = inventoryItem.CloseDefence;
            Rare = inventoryItem.Rare;
            SpXp = inventoryItem.SpXp;
            SpLevel = inventoryItem.SpLevel;
            SlDefence = inventoryItem.SlDefence;
            SlElement = inventoryItem.SlElement;
            SlDamage = inventoryItem.SlDamage;
            SlHP = inventoryItem.SlHP;
            Upgrade = inventoryItem.Upgrade;
            WaterElement = inventoryItem.WaterElement;
            Ammo = inventoryItem.Ammo;
            Cellon = inventoryItem.Cellon;
            CriticalDodge = inventoryItem.CriticalDodge;
            DarkResistance = inventoryItem.DarkResistance;
            FireResistance = inventoryItem.FireResistance;
            HP = inventoryItem.HP;
            IsEmpty = inventoryItem.IsEmpty;
            IsFixed = inventoryItem.IsFixed;
            IsUsed = inventoryItem.IsUsed;
            ItemDeleteTime = inventoryItem.ItemDeleteTime;
            LightResistance = inventoryItem.LightResistance;
            MP = inventoryItem.MP;
            SpDamage = inventoryItem.SpDamage;
            SpDark = inventoryItem.SpDark;
            SpDefence = inventoryItem.SpDefence;
            SpElement = inventoryItem.SpElement;
            SpFire = inventoryItem.SpFire;
            SpHP = inventoryItem.SpHP;
            SpLight = inventoryItem.SpLight;
            SpStoneUpgrade = inventoryItem.SpStoneUpgrade;
            SpWater = inventoryItem.SpWater;
            WaterResistance = inventoryItem.WaterResistance;
        }

        #endregion

        #region Enums

        public enum RarifyMode
        {
            Normal,
            Reduced,
            Free
        }

        public enum RarifyProtection
        {
            None,
            BlueAmulet,
            RedAmulet,
            Scroll
        }

        public enum UpgradeMode
        {
            Normal,
            Reduced,
            Free
        }

        public enum UpgradeProtection
        {
            None,
            Protected
        }

        #endregion

        #region Methods

        public void Save()
        {
        }

        #endregion
    }
}