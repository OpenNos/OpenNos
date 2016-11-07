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

        public Item()
        {
        }

        public Item(ItemDTO item)
        {
            InitializeItem(item);
        }

        #endregion

        #region Methods

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
            this.FireElement = item.FireElement;
            this.FireResistance = item.FireResistance;
            this.HitRate = item.HitRate;
            this.Hp = item.Hp;
            this.HpRegeneration = item.HpRegeneration;
            this.IsBlocked = item.IsBlocked;
            this.IsColored = item.IsColored;
            this.IsConsumable = item.IsConsumable;
            this.IsDroppable = item.IsDroppable;
            this.IsHeroic = item.IsHeroic;
            this.IsMinilandObject = item.IsMinilandObject;
            this.IsSoldable = item.IsSoldable;
            this.IsTradable = item.IsTradable;
            this.IsWarehouse = item.IsWarehouse;
            this.ItemSubType = item.ItemSubType;
            this.ItemType = item.ItemType;
            this.ItemValidTime = item.ItemValidTime;
            this.LevelJobMinimum = item.LevelJobMinimum;
            this.LevelMinimum = item.LevelMinimum;
            this.LightElement = item.LightElement;
            this.LightResistance = item.LightResistance;
            this.MagicDefence = item.MagicDefence;
            this.MaxCellon = item.MaxCellon;
            this.MaxCellonLvl = item.MaxCellonLvl;
            this.MaxElementRate = item.MaxElementRate;
            this.MaximumAmmo = item.MaximumAmmo;
            this.MoreHp = item.MoreHp;
            this.MoreMp = item.MoreMp;
            this.Morph = item.Morph;
            this.Mp = item.Mp;
            this.MpRegeneration = item.MpRegeneration;
            this.Name = item.Name;
            this.Price = item.Price;
            this.PvpDefence = item.PvpDefence;
            this.PvpStrength = item.PvpStrength;
            this.ReduceOposantResistance = item.ReduceOposantResistance;
            this.ReputationMinimum = item.ReputationMinimum;
            this.ReputPrice = item.ReputPrice;
            this.SecondaryElement = item.SecondaryElement;
            this.Sex = item.Sex;
            this.Speed = item.Speed;
            this.SpType = item.SpType;
            this.Type = item.Type;
            this.VNum = item.VNum;
            this.WaitDelay = item.WaitDelay;
            this.WaterElement = item.WaterElement;
            this.WaterResistance = item.WaterResistance;
        }

        public abstract void Use(ClientSession Session, ref ItemInstance Inv, bool DelayUsed = false, string[] packetsplit = null);

        #endregion
    }
}