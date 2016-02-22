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
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class Shop : ShopDTO, IGameObject
    {
        public List<ShopItem> ShopItems
        {
            get; set;
        }

        #region Instantiation

        public Shop(int shopId)
        {
            Mapper.CreateMap<ShopDTO, Shop>();
            Mapper.CreateMap<Shop, ShopDTO>();
            ShopItems = new List<ShopItem>();
            ShopId = shopId;
            foreach (ShopItemDTO item in DAOFactory.ShopItemDAO.LoadByShopId(ShopId))
            {
                ShopItems.Add(new ShopItem() { ItemVNum = item.ItemVNum, Rare = item.Rare, ShopItemId = item.ShopItemId, Slot = item.Slot, Upgrade = item.Upgrade, Color = item.Color });
            }
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