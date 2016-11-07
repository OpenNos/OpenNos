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
    public abstract class Item : ItemDTO
    {
        #region Instantiation

        public Item() { }

        public void InitializeItem(ItemDTO item)
        {
            //manual mapping to avoid automapper outside of DAO
            this.BasicUpgrade = item.BasicUpgrade;
            this.CellonLvl = item.CellonLvl;
            this.Class = item.Class;
            this.CloseDefence = item.CloseDefence;
            this.Color = item.Color;
            this.Concentrate = item.Concentrate;
            this.CriticalLuckRate = item.CriticalLuckRate;
            this.DamageMaximum = item.DamageMaximum;
            this.DamageMinimum = item.DamageMinimum;
            this.DarkElement = item.DarkElement;
            this.DarkResistance = item.DarkResistance;
            this.DefenceDodge = item.DefenceDodge;
            this.DistanceDefence = item.DistanceDefence;
            this.DistanceDefenceDodge = item.DistanceDefenceDodge;
            this.Effect = item.Effect;
            this.EffectValue = item.EffectValue;
            this.Element = item.Element;
            this.ElementRate = item.ElementRate;
            this.EquipmentSlot = item.EquipmentSlot;
        }

        #endregion

        #region Methods

        public abstract void Use(ClientSession Session, ref ItemInstance Inv, bool DelayUsed = false, string[] packetsplit = null);

        #endregion
    }
}