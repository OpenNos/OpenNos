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
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class InventoryDAO : IInventoryDAO
    {
        #region Methods

        public DeleteResult DeleteFromSlotAndType(long CharacterId, short slot, byte type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Inventory inv = context.Inventory.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type) && i.CharacterId.Equals(CharacterId));
                InventoryItem invItem = context.InventoryItem.FirstOrDefault(i => i.Inventory.InventoryId == inv.InventoryId);
                if (inv != null)
                {
                    context.Inventory.Remove(inv);
                    context.InventoryItem.Remove(invItem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public SaveResult InsertOrUpdate(ref InventoryDTO Inventory)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long InventoryId = Inventory.InventoryId;
                    byte Type = Inventory.Type;
                    short Slot = Inventory.Slot;
                    long CharacterId = Inventory.CharacterId;
                    Inventory entity = context.Inventory.FirstOrDefault(c => c.InventoryId == InventoryId);
                    if (entity == null) //new entity
                    {
                        Inventory delete = context.Inventory.FirstOrDefault(s => s.CharacterId == CharacterId && s.Slot == Slot && s.Type == Type);
                        if (delete != null)
                        {
                            InventoryItem deleteItem = context.InventoryItem.FirstOrDefault(s => s.Inventory.InventoryId == delete.InventoryId);
                            context.InventoryItem.Remove(deleteItem);
                            context.Inventory.Remove(delete);
                        }
                        Inventory = Insert(Inventory, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        entity.InventoryItem = context.InventoryItem.FirstOrDefault(c => c.Inventory.InventoryId == entity.InventoryId);
                        Inventory = Update(entity, Inventory, context);
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

        public IEnumerable<InventoryDTO> LoadByCharacterId(long CharacterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Inventory Inventoryobject in context.Inventory.Where(i => i.CharacterId.Equals(CharacterId)))
                {
                    yield return Mapper.Map<InventoryDTO>(Inventoryobject);
                }
            }
        }

        public InventoryDTO LoadBySlotAndType(long CharacterId, short slot, byte type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<InventoryDTO>(context.Inventory.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type) && i.CharacterId.Equals(CharacterId)));
            }
        }

        public IEnumerable<InventoryDTO> LoadByType(long CharacterId, byte type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Inventory Inventoryobject in context.Inventory.Where(i => i.Type.Equals(type) && i.CharacterId.Equals(CharacterId)))
                {
                    yield return Mapper.Map<InventoryDTO>(Inventoryobject);
                }
            }
        }

        private InventoryDTO Insert(InventoryDTO Inventory, OpenNosContext context)
        {
            Inventory entity = Mapper.Map<Inventory>(Inventory);
            context.Inventory.Add(entity);
            context.SaveChanges();
            return Mapper.Map<InventoryDTO>(entity);
        }

        private InventoryDTO Update(Inventory entity, InventoryDTO Inventory, OpenNosContext context)
        {
            using (context)
            {
                var result = context.Inventory.FirstOrDefault(c => c.InventoryId == Inventory.InventoryId);
                if (result != null)
                {
                    result = Mapper.Map<InventoryDTO, Inventory>(Inventory, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<InventoryDTO>(entity);
        }

        #endregion
    }
}