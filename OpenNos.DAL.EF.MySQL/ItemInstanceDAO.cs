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
    public class ItemInstanceDAO : IItemInstanceDAO
    {
        public SaveResult InsertOrUpdate(ref ItemInstanceDTO item)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {

                    short ItemInstanceId = item.ItemInstanceId;
                    ItemInstance entity = context.iteminstance.SingleOrDefault(c => c.ItemInstanceId.Equals(ItemInstanceId));

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
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_ACCOUNT_ERROR"), item.ItemInstanceId, e.Message);
                return SaveResult.Error;
            }
        }
        public DeleteResult DeleteById(short ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                ItemInstance item = context.iteminstance.SingleOrDefault(i => i.ItemInstanceId.Equals(ItemId));

                if (item != null)
                {
                    context.iteminstance.Remove(item);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
        public ItemInstanceDTO LoadById(short ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<ItemInstanceDTO>(context.iteminstance.SingleOrDefault(i => i.ItemInstanceId.Equals(ItemId)));
            }
        }
        private ItemInstanceDTO Insert(ItemInstanceDTO iteminstance, OpenNosContainer context)
        {
            ItemInstance entity = Mapper.Map<ItemInstance>(iteminstance);
            context.iteminstance.Add(entity);
            context.SaveChanges();
            return Mapper.Map<ItemInstanceDTO>(entity);
        }

        private ItemInstanceDTO Update(ItemInstance entity, ItemInstanceDTO iteminstance, OpenNosContainer context)
        {
            using (context)
            {
                var result = context.iteminstance.SingleOrDefault(c => c.ItemInstanceId.Equals(iteminstance.ItemInstanceId));
                if (result != null)
                {
                    result = Mapper.Map<ItemInstanceDTO, ItemInstance>(iteminstance, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<ItemInstanceDTO>(entity);
        }

    }
}
