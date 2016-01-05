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
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.Interface;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using AutoMapper;
using OpenNos.Core;

namespace OpenNos.DAL.EF.MySQL
{
    public class ShopItemDAO : IShopItemDAO
    {
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

        public SaveResult InsertOrUpdate(ref ShopItemDTO item)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {

                    long ShopItemId = item.ShopItemId;
                    ShopItem entity = context.shopitem.SingleOrDefault(c => c.ShopItemId.Equals(ShopItemId));

                    if (entity == null) //new entity
                    {
                        item = Insert(item, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        item = Update(entity, item, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_ACCOUNT_ERROR"), item.ShopItemId, e.Message);
                return SaveResult.Error;
            }
        }
        private ShopItemDTO Insert(ShopItemDTO shopitem, OpenNosContainer context)
        {
            ShopItem entity = Mapper.Map<ShopItem>(shopitem);
            context.shopitem.Add(entity);
            context.SaveChanges();
            return Mapper.Map<ShopItemDTO>(entity);
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


        public ShopItemDTO LoadById(int ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<ShopItemDTO>(context.shopitem.SingleOrDefault(i => i.ShopItemId.Equals(ItemId)));
            }
        }
    }
}
