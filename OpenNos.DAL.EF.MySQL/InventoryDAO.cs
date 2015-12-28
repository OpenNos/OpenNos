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
    public class InventoryDAO : IInventoryDAO
    {
        public SaveResult InsertOrUpdate(ref InventoryDTO inventory)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {

                    long inventoryId = inventory.InventoryId;
          
                    Inventory entity = context.inventory.SingleOrDefault(c => c.InventoryId.Equals(inventoryId));

                    if (entity == null) //new entity
                    {
                        inventory = Insert(inventory, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        inventory = Update(entity, inventory, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_ACCOUNT_ERROR"), inventory.InventoryId, e.Message);
                return SaveResult.Error;
            }
        }
        public DeleteResult DeleteFromSlotAndType(long characterId, short slot, short type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Inventory inv = context.inventory.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type) && i.CharacterId.Equals(characterId));

                if (inv != null)
                {
                    context.inventory.Remove(inv);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
        public InventoryDTO LoadBySlotAndType(long characterId, short slot,short type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<InventoryDTO>(context.inventory.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type) && i.CharacterId.Equals(characterId)));
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
        public IEnumerable<InventoryDTO> LoadByType(long characterId, short type)
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

 

        public short getFirstPlace(long characterId, byte type,int backpack)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
               Inventory result;
                for (short i = 0; i < 48 + backpack * 12; i++)
                {
                    result = context.inventory.SingleOrDefault(c => c.CharacterId.Equals(characterId) && c.Type.Equals(type) && c.Slot.Equals(i));
                    if (result == null)
                        return i;
                }
                   
            }
            return -1;
        }

        public InventoryDTO LoadByItemInstance(short itemInstanceId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<InventoryDTO>(context.inventory.SingleOrDefault(i => i.ItemInstanceId.Equals(itemInstanceId)));
            }
        }

        public InventoryDTO getFirstSlot(List<short> iteminstanceids)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<InventoryDTO>(context.inventory.Where(i => iteminstanceids.Contains(i.ItemInstanceId)).OrderBy(i => i.Slot).FirstOrDefault());
            }
        }
    }
}
