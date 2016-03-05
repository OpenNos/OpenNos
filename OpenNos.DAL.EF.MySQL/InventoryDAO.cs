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
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class InventoryDAO : IInventoryDAO
    {
        #region Methods

        public DeleteResult DeleteFromSlotAndType(long characterId, short slot, byte type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Inventory inv = context.inventory.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type) && i.CharacterId.Equals(characterId));
                InventoryItem invitem = context.inventoryitem.SingleOrDefault(i => i.inventory.InventoryId == inv.InventoryId);
                if (inv != null)
                {
                    context.inventory.Remove(inv);
                    context.inventoryitem.Remove(invitem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public SaveResult InsertOrUpdate(ref InventoryDTO inventory)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long InventoryId = inventory.InventoryId;
                    Inventory entity = context.inventory.SingleOrDefault(c => c.InventoryId == InventoryId);
                    if (entity == null) //new entity
                    {
                        inventory = Insert(inventory, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        entity.inventoryitem = context.inventoryitem.SingleOrDefault(c => c.inventory.InventoryId == entity.InventoryId);
                        inventory = Update(entity, inventory, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message);
                return SaveResult.Error;
            }
        }

        public IEnumerable<InventoryDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Inventory inventoryobject in context.inventory.Where(i => i.CharacterId.Equals(characterId)))
                {
                    yield return Mapper.Map<InventoryDTO>(inventoryobject);
                }
            }
        }


        public InventoryDTO LoadBySlotAndType(long characterId, short slot, byte type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<InventoryDTO>(context.inventory.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type) && i.CharacterId.Equals(characterId)));
            }
        }

        public IEnumerable<InventoryDTO> LoadByType(long characterId, byte type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Inventory inventoryobject in context.inventory.Where(i => i.Type.Equals(type) && i.CharacterId.Equals(characterId)))
                {
                    yield return Mapper.Map<InventoryDTO>(inventoryobject);
                }
            }
        }

        private InventoryDTO Insert(InventoryDTO inventory, OpenNosContainer context)
        {

            Inventory entity = Mapper.Map<Inventory>(inventory);
            context.inventory.Add(entity);
            context.SaveChanges();
            return Mapper.Map<InventoryDTO>(entity);
        }

        private InventoryDTO Update(Inventory entity, InventoryDTO inventory, OpenNosContainer context)
        {
            using (context)
            {
                var result = context.inventory.SingleOrDefault(c => c.InventoryId == inventory.InventoryId);
                if (result != null)
                {
                    result = Mapper.Map<InventoryDTO, Inventory>(inventory, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<InventoryDTO>(entity);
        }

        #endregion
    }
}