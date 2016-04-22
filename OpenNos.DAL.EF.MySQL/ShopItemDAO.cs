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

using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;
using System.Linq;
using System;
using OpenNos.DAL.EF.MySQL.DB;

namespace OpenNos.DAL.EF.MySQL
{
    public class ShopItemDAO : IShopItemDAO
    {
        #region Methods

        public DeleteResult DeleteById(int ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                ShopItem Item = context.ShopItem.FirstOrDefault(i => i.ShopItemId.Equals(ItemId));

                if (Item != null)
                {
                    context.ShopItem.Remove(Item);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
       

        public ShopItemDTO LoadById(int ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<ShopItemDTO>(context.ShopItem.FirstOrDefault(i => i.ShopItemId.Equals(ItemId)));
            }
        }

        public IEnumerable<ShopItemDTO> LoadByShopId(int ShopId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ShopItem ShopItem in context.ShopItem.Where(i => i.ShopId.Equals(ShopId)))
                {
                    yield return Mapper.Map<ShopItemDTO>(ShopItem);
                }
            }
        }
        public ShopItemDTO Insert(ShopItemDTO Item)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                ShopItem entity = Mapper.Map<ShopItem>(Item);
                context.ShopItem.Add(entity);
                context.SaveChanges();
                return Mapper.Map<ShopItemDTO>(entity);
            }
        }

        private ShopItemDTO Update(ShopItem entity, ShopItemDTO ShopItem, OpenNosContext context)
        {
            using (context)
            {
                var result = context.ShopItem.FirstOrDefault(c => c.ShopItemId.Equals(ShopItem.ShopItemId));
                if (result != null)
                {
                    result = Mapper.Map<ShopItemDTO, ShopItem>(ShopItem, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<ShopItemDTO>(entity);
        }

        public void Insert(List<ShopItemDTO> Items)
        {
            using (var context = DataAccessHelper.CreateContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (ShopItemDTO Item in Items)
                {
                    ShopItem entity = Mapper.Map<ShopItem>(Item);
                    context.ShopItem.Add(entity);
                }
                context.SaveChanges();

            }
        }

        #endregion
    }
}