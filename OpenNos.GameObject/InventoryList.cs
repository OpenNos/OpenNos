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
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class InventoryList
    {

        #region Instantiation
        public List<Inventory> Inventory { get; set; }
  
        public InventoryList(){
                Inventory = new List<Inventory>();
            }

        public void InsertOrUpdate(ref Inventory newInventory)
        {
                    long inventoryId = newInventory.InventoryId;

                    Inventory entity = Inventory.SingleOrDefault(c => c.InventoryId.Equals(inventoryId));

                    if (entity == null) //new entity
                    {
                        newInventory = Insert(newInventory);
                    }
                    else //existing entity
                    {
                         newInventory = Update(entity, newInventory);
                    }
        }

        private Inventory Insert(Inventory inventory)
        {
            Inventory entity = inventory;
            Inventory.Add(entity);
            return entity;
        }

        private Inventory Update(Inventory entity, Inventory inventory)
        {
           
                var result = Inventory.SingleOrDefault(c => c.InventoryId == inventory.InventoryId);
                if (result != null)
                {
                    result = Mapper.Map<Inventory, Inventory>(inventory, entity);
                 
                }
            

            return entity;
        }


        public short getFirstPlace( byte type, int backPack)
        {
            Inventory result;
            for (short i = 0; i < 48 + backPack * 12; i++)
            {
                result =Inventory.SingleOrDefault(c=> c.Type.Equals(type) && c.Slot.Equals(i));
                if (result == null)
                    return i;
            }
            return -1;
        }

        public short getFreePlaceAmount(InventoryItem item, int backPack)
        {
            Inventory result;
            short j = (short)(48 + backPack * 12);
            for (short i = 0; i < 48 + backPack * 12; i++)
            {
               
                    result = Inventory.SingleOrDefault(c => c.Type.Equals(ServerManager.GetItem(item.ItemVNum).Type) && c.Slot.Equals(i));     
                if (( ServerManager.GetItem(item.ItemVNum).Type == 0 && result != null) || (ServerManager.GetItem(item.ItemVNum).Type != 0 && result != null && result.InventoryItem.ItemVNum == item.ItemVNum && result.InventoryItem.Amount + item.Amount > 99))
                    j--;
              
            }
            return j;
        }
        public Inventory LoadBySlotAndType(short slot, short type)
        {
           
                return Inventory.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));
            
        }

        public void DeleteFromSlotAndType(short slot, short type)
        {
            Inventory inv = Inventory.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));

            if (inv != null)
            {
                Inventory.Remove(inv);
            }
            
        }

        public Inventory getFirstSlot(List<long> inventoryitemids)
        {
            return Inventory.Where(i => inventoryitemids.Contains(i.InventoryItemId)).OrderBy(i => i.Slot).FirstOrDefault();
        }

        public Inventory LoadByInventoryItem(long InventoryItemId)
        {

            return Inventory.SingleOrDefault(i => i.InventoryItemId.Equals(InventoryItemId));

        }

        public long generateInventoryId()
        {
            Random r = new Random();
            bool boolean = true;
            long inventoryId = -1;
            while (boolean)
            {
                boolean = false;
                inventoryId = (long)((r.NextDouble() * 2.0 - 1.0) * long.MaxValue);
                foreach (Inventory inv in Inventory)
                {
                    if (inv.InventoryId == inventoryId)
                    { boolean = true; break; }
                }
            }
            return inventoryId;
        }

        public long generateInventoryItemId()
        {
            Random r = new Random();
            bool boolean = true;
            long inventoryitemId = -1;
            while (boolean)
            {
                boolean = false;
                 inventoryitemId =  (long)((r.NextDouble() * 2.0 - 1.0) * long.MaxValue);
                foreach(Inventory inv in Inventory)
                {
                    if (inv.InventoryItemId == inventoryitemId)
                    { boolean = true; break; }
                }
            }
            return inventoryitemId;
        }
        #endregion

        #region Methods


        #endregion
    }
}
