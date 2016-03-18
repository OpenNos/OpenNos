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

        public DeleteResult DeleteById(int ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                ShopItem item = context.shopitem.SingleOrDefault(i => i.ShopItemId.Equals(ItemId));

                if (item != null)
                {
                    context.shopitem.Remove(item);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
       

        public ShopItemDTO LoadById(int ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<ShopItemDTO>(context.shopitem.SingleOrDefault(i => i.ShopItemId.Equals(ItemId)));
            }
        }

        public IEnumerable<ShopItemDTO> LoadByShopId(int ShopId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ShopItem shopitem in context.shopitem.Where(i => i.ShopId.Equals(ShopId)))
                {
                    yield return Mapper.Map<ShopItemDTO>(shopitem);
                }
            }
        }
        public ShopItemDTO Insert(ShopItemDTO item)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                ShopItem entity = Mapper.Map<ShopItem>(item);
                context.shopitem.Add(entity);
                context.SaveChanges();
                return Mapper.Map<ShopItemDTO>(entity);
            }
        }

        private ShopItemDTO Update(ShopItem entity, ShopItemDTO shopitem, OpenNosContainer context)
        {
            using (context)
            {
                var result = context.shopitem.SingleOrDefault(c => c.ShopItemId.Equals(shopitem.ShopItemId));
                if (result != null)
                {
                    result = Mapper.Map<ShopItemDTO, ShopItem>(shopitem, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<ShopItemDTO>(entity);
        }

        #endregion
    }
}