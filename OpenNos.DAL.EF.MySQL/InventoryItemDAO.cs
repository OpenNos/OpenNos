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
    public class InventoryItemDAO : IInventoryItemDAO
    {
        public SaveResult InsertOrUpdate(ref InventoryItemDTO item)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {

                    long InventoryItemId = item.InventoryItemId;
                    InventoryItem entity = context.inventoryitem.SingleOrDefault(c => c.InventoryItemId.Equals(InventoryItemId));

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
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_ACCOUNT_ERROR"), item.InventoryItemId, e.Message);
                return SaveResult.Error;
            }
        }
        public DeleteResult DeleteById(short ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                InventoryItem item = context.inventoryitem.SingleOrDefault(i => i.InventoryItemId.Equals(ItemId));

                if (item != null)
                {
                    context.inventoryitem.Remove(item);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
        public InventoryItemDTO LoadById(long ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<InventoryItemDTO>(context.inventoryitem.SingleOrDefault(i => i.InventoryItemId.Equals(ItemId)));
            }
        }
        private InventoryItemDTO Insert(InventoryItemDTO inventoryitem, OpenNosContainer context)
        {
            InventoryItem entity = Mapper.Map<InventoryItem>(inventoryitem);
            context.inventoryitem.Add(entity);
            context.SaveChanges();
            return Mapper.Map<InventoryItemDTO>(entity);
        }

        private InventoryItemDTO Update(InventoryItem entity, InventoryItemDTO inventoryitem, OpenNosContainer context)
        {
            using (context)
            {
                var result = context.inventoryitem.SingleOrDefault(c => c.InventoryItemId.Equals(inventoryitem.InventoryItemId));
                if (result != null)
                {
                    result = Mapper.Map<InventoryItemDTO, InventoryItem>(inventoryitem, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<InventoryItemDTO>(entity);
        }
        public IEnumerable<InventoryItemDTO> LoadBySlotAllowed( short itemVNum, short amount)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (InventoryItem inventoryitemobject in context.inventoryitem.Where(i => i.ItemVNum.Equals(itemVNum) && i.Amount + amount < 100))
                {
                    yield return Mapper.Map<InventoryItemDTO>(inventoryitemobject);
                }
            }
        }
        
    }
}
