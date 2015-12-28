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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Inventory : InventoryDTO, IGameObject
    {
        public ItemInstance ItemInstance {
            get; set; }

        #region Instantiation
        

        public Inventory(short itemInstanceId)
        {
            Mapper.CreateMap<InventoryDTO, Inventory>();
            Mapper.CreateMap<Inventory, InventoryDTO>();
            ItemInstanceDTO item = DAOFactory.ItemInstanceDAO.LoadById(itemInstanceId);
            ItemInstance = new ItemInstance()
            {
             
                Amount = item.Amount,
                Color = item.Color,
                Concentrate = item.Concentrate,
                CriticalLuckRate = item.CriticalLuckRate,
                CriticalRate = item.CriticalRate,
                DamageMaximum = item.DamageMaximum,
                DamageMinimum = item.DamageMinimum,
                DarkElement = item.DarkElement,
                DistanceDefence = item.DistanceDefence,
                Dodge = item.Dodge,
                ElementRate = item.ElementRate,
                FireElement = item.FireElement,
                HitRate = item.HitRate,
                ItemInstanceId = item.ItemInstanceId,
                ItemVNum = item.ItemVNum,
                LightElement = item.LightElement,
                MagicDefence = item.MagicDefence,
                RangeDefence = item.RangeDefence,
                Rare = item.Rare,
                SlDefence = item.SlDefence,
                SlElement = item.SlElement,
                SlHit = item.SlHit,
                Upgrade = item.Upgrade,
                SlHP = item.SlHP,
                WaterElement = item.WaterElement,
            };
            ItemDTO iteminfo = DAOFactory.ItemDAO.LoadById(ItemInstance.ItemVNum);
            ItemInstance.Item = new Item { Blocked = iteminfo.Blocked, Classe = iteminfo.Classe, Colored = iteminfo.Colored, Concentrate = iteminfo.Concentrate, CriticalLuckRate = iteminfo.CriticalLuckRate, CriticalRate = iteminfo.CriticalRate, DamageMaximum = iteminfo.DamageMaximum, DamageMinimum = iteminfo.DamageMinimum, DarkElement = iteminfo.DarkElement, DarkResistance = iteminfo.DarkResistance, DimOposantResistance = iteminfo.DimOposantResistance, DistanceDefence = iteminfo.DistanceDefence, Dodge = iteminfo.Dodge, Droppable = iteminfo.Droppable, Element = iteminfo.Element, ElementRate = iteminfo.ElementRate, EquipmentSlot = iteminfo.EquipmentSlot,FireElement=iteminfo.FireElement,FireResistance=iteminfo.FireResistance,HitRate=iteminfo.HitRate,Hp=iteminfo.Hp,HpRegeneration=iteminfo.HpRegeneration,Inventory=iteminfo.Inventory,isConsumable=iteminfo.isConsumable,isWareHouse=iteminfo.isWareHouse,ItemType=iteminfo.ItemType,
            LevelMinimum=iteminfo.LevelMinimum,LightElement=iteminfo.LightElement,LightResistance=iteminfo.LightResistance,MagicDefence=iteminfo.MagicDefence,MaxCellon=iteminfo.MaxCellon,MaxCellonLvl=iteminfo.MaxCellonLvl,MinilandObject=iteminfo.MinilandObject,MoreHp=iteminfo.MoreHp,MoreMp=iteminfo.MoreMp,Morph=iteminfo.Morph,Mp=iteminfo.Mp,MpRegeneration=iteminfo.MpRegeneration,Name=iteminfo.Name,Price=iteminfo.Price,PvpDefence=iteminfo.PvpDefence,PvpStrength=iteminfo.PvpStrength,RangeDefence=iteminfo.RangeDefence,Soldable=iteminfo.Soldable,Speed=iteminfo.Speed,Transaction=iteminfo.Transaction,Type=iteminfo.Type,VNum=iteminfo.VNum,WaterElement=iteminfo.WaterElement,WaterResistance=iteminfo.WaterResistance};
            
        }


        #endregion

        #region Methods
     
        public void Save()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
