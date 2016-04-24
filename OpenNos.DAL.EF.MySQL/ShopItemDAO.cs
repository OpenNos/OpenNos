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
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class ShopItemDAO : IShopItemDAO
    {
        #region Methods

        public DeleteResult DeleteById(int itemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                ShopItem Item = context.ShopItem.FirstOrDefault(i => i.ShopItemId.Equals(itemId));

                if (Item != null)
                {
                    context.ShopItem.Remove(Item);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public ShopItemDTO Insert(ShopItemDTO item)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                ShopItem entity = Mapper.DynamicMap<ShopItem>(item);
                context.ShopItem.Add(entity);
                context.SaveChanges();
                return Mapper.DynamicMap<ShopItemDTO>(entity);
            }
        }

        public void Insert(List<ShopItemDTO> items)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (ShopItemDTO Item in items)
                {
                    ShopItem entity = Mapper.DynamicMap<ShopItem>(Item);
                    context.ShopItem.Add(entity);
                }
                context.SaveChanges();
            }
        }

        public ShopItemDTO LoadById(int itemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<ShopItemDTO>(context.ShopItem.FirstOrDefault(i => i.ShopItemId.Equals(itemId)));
            }
        }

        public IEnumerable<ShopItemDTO> LoadByShopId(int shopId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ShopItem ShopItem in context.ShopItem.Where(i => i.ShopId.Equals(shopId)))
                {
                    yield return Mapper.DynamicMap<ShopItemDTO>(ShopItem);
                }
            }
        }

        private ShopItemDTO Update(ShopItem entity, ShopItemDTO shopItem, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.DynamicMap(shopItem, entity);
                context.SaveChanges();
            }

            return Mapper.DynamicMap<ShopItemDTO>(entity);
        }

        #endregion
    }
}