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

using OpenNos.DAL;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

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

        #region Properties
        public List<BCard> BCards { get; set; }
        public List<RollGeneratedItemDTO> RollGeneratedItems { get; set; }
        #endregion

        #region Methods

        public void InitializeItem(ItemDTO item)
        {
            // manual mapping to avoid automapper outside of DAO
            Height = item.Height;
            Width = item.Width;
            MinilandObjectPoint = item.MinilandObjectPoint;
            BasicUpgrade = item.BasicUpgrade;
            CellonLvl = item.CellonLvl;
            Class = item.Class;
            CloseDefence = item.CloseDefence;
            Color = item.Color;
            Concentrate = item.Concentrate;
            CriticalRate = item.CriticalRate;
            CriticalLuckRate = item.CriticalLuckRate;
            DamageMaximum = item.DamageMaximum;
            DamageMinimum = item.DamageMinimum;
            DarkElement = item.DarkElement;
            DarkResistance = item.DarkResistance;
            DefenceDodge = item.DefenceDodge;
            DistanceDefence = item.DistanceDefence;
            DistanceDefenceDodge = item.DistanceDefenceDodge;
            Effect = item.Effect;
            EffectValue = item.EffectValue;
            Element = item.Element;
            ElementRate = item.ElementRate;
            EquipmentSlot = item.EquipmentSlot;
            FireElement = item.FireElement;
            FireResistance = item.FireResistance;
            HitRate = item.HitRate;
            Hp = item.Hp;
            HpRegeneration = item.HpRegeneration;
            IsBlocked = item.IsBlocked;
            IsColored = item.IsColored;
            IsConsumable = item.IsConsumable;
            IsDroppable = item.IsDroppable;
            IsHeroic = item.IsHeroic;
            IsMinilandObject = item.IsMinilandObject;
            IsSoldable = item.IsSoldable;
            IsTradable = item.IsTradable;
            IsHolder = item.IsHolder;
            ItemSubType = item.ItemSubType;
            ItemType = item.ItemType;
            ItemValidTime = item.ItemValidTime;
            LevelJobMinimum = item.LevelJobMinimum;
            LevelMinimum = item.LevelMinimum;
            LightElement = item.LightElement;
            LightResistance = item.LightResistance;
            MagicDefence = item.MagicDefence;
            MaxCellon = item.MaxCellon;
            MaxCellonLvl = item.MaxCellonLvl;
            MaxElementRate = item.MaxElementRate;
            MaximumAmmo = item.MaximumAmmo;
            MoreHp = item.MoreHp;
            MoreMp = item.MoreMp;
            Morph = item.Morph;
            Mp = item.Mp;
            MpRegeneration = item.MpRegeneration;
            Name = item.Name;
            Price = item.Price;
            PvpDefence = item.PvpDefence;
            PvpStrength = item.PvpStrength;
            ReduceOposantResistance = item.ReduceOposantResistance;
            ReputationMinimum = item.ReputationMinimum;
            ReputPrice = item.ReputPrice;
            SecondaryElement = item.SecondaryElement;
            Sex = item.Sex;
            Speed = item.Speed;
            SpType = item.SpType;
            Type = item.Type;
            VNum = item.VNum;
            WaitDelay = item.WaitDelay;
            WaterElement = item.WaterElement;
            WaterResistance = item.WaterResistance;
            BCards = new List<BCard>();
            DAOFactory.BCardDAO.LoadByItemVNum(item.VNum).ToList().ForEach(o => BCards.Add((BCard)o));
            RollGeneratedItems = DAOFactory.RollGeneratedItemDAO.LoadByItemVNum(item.VNum).ToList();
        }

        //TODO: Convert to PacketDefinition
        public abstract void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null);

        #endregion
    }
}