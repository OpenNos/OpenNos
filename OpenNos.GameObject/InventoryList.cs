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
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class InventoryList
    {
        #region Instantiation

        public InventoryList()
        {
            Inventory = new List<Inventory>();
        }

        #endregion

        #region Properties

        public List<Inventory> Inventory { get; set; }

        #endregion

        #region Methods

        public int CountItem(int v)
        {
            int count = 0;
            foreach (Inventory inv in Inventory.Where(s => s.ItemInstance.ItemVNum == v))
            {
                count += inv.ItemInstance.Amount;
            }
            return count;
        }

        public Inventory CreateItem<T>(T newItem, Character character)
            where T : ItemInstanceDTO
        {
            short Slot = -1;
            IEnumerable<ItemInstance> slotfree = null;
            Inventory inv = null;
            if (ServerManager.GetItem(newItem.ItemVNum).Type != 0)
            {
                slotfree = character.LoadBySlotAllowed(newItem.ItemVNum, newItem.Amount);
                inv = GetFirstSlot(slotfree);
            }
            bool modified = false;
            Inventory newInventory = null;
            if (inv != null)
            {
                Slot = inv.Slot;
                newItem.Amount = (byte)(newItem.Amount + inv.ItemInstance.Amount);
                modified = true;
            }
            else
                Slot = GetFirstPlace(ServerManager.GetItem(newItem.ItemVNum).Type, character.BackPack);
            if (Slot != -1)
            {
                if (modified == false)
                {
                    newInventory = new Inventory()
                    {
                        CharacterId = character.CharacterId,
                        Slot = Slot,
                        Type = ServerManager.GetItem(newItem.ItemVNum).Type,
                        ItemInstance = newItem,
                        InventoryId = GenerateInventoryId(),
                    };
                }
                else
                {
                    newItem.ItemInstanceId = inv.ItemInstance.ItemInstanceId;
                    newInventory = new Inventory()
                    {
                        CharacterId = character.CharacterId,
                        Slot = Slot,
                        Type = ServerManager.GetItem(newItem.ItemVNum).Type,
                        ItemInstance = newItem,
                        InventoryId = inv.InventoryId,
                    };
                }
                InsertOrUpdate(ref newInventory);
            }
            return newInventory;
        }

        public Tuple<short,byte> DeleteByInventoryItemId(long inventoryItemId)
        {
            Tuple<short, byte> removedPlace = new Tuple<short, byte>(0,0);
            Inventory inv = Inventory.FirstOrDefault(i => i.ItemInstance.ItemInstanceId.Equals(inventoryItemId));

            if (inv != null)
            {
                removedPlace = new Tuple<short, byte>(inv.Slot, inv.Type);
                Inventory.Remove(inv);
            }

            return removedPlace;
        }

        public void DeleteFromSlotAndType(short slot, byte type)
        {
            Inventory inv = Inventory.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));

            if (inv != null)
            {
                Inventory.Remove(inv);
            }
        }

        public long GenerateInventoryId()
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

        public long GenerateInventoryItemId()
        {
            Random r = new Random();
            bool boolean = true;
            int inventoryitemId = -1;
            while (boolean)
            {
                boolean = false;
                inventoryitemId = (int)((r.NextDouble() * 2.0 - 1.0) * int.MaxValue);
                foreach (Inventory inv in Inventory)
                {
                    if (inv.ItemInstance.ItemInstanceId == inventoryitemId)
                    { boolean = true; break; }
                }
            }
            return inventoryitemId;
        }

        public short GetFirstPlace(byte type, int backPack)
        {
            Inventory result;
            for (short i = 0; i < 48 + (backPack * 12); i++)
            {
                result = Inventory.FirstOrDefault(c => c.Type.Equals(type) && c.Slot.Equals(i));
                if (result == null)
                    return i;
            }
            return -1;
        }

        public Inventory GetFirstSlot(IEnumerable<ItemInstance> slotfree)
        {
            List<long> inventoryitemids = new List<long>();
            foreach (ItemInstance itemfree in slotfree)
            {
                inventoryitemids.Add(itemfree.ItemInstanceId);
            }
            return Inventory.Where(i => inventoryitemids.Contains(i.ItemInstance.ItemInstanceId)).OrderBy(i => i.Slot).FirstOrDefault();
        }

        public bool GetFreePlaceAmount(List<ItemInstance> item, int backPack)
        {
            short[] place = new short[10];
            for (byte k = 0; k < place.Count(); k++)
            {
                place[k] = (byte)(48 + (backPack * 12));
                for (short i = 0; i < 48 + (backPack * 12); i++)
                {
                    Inventory result = LoadInventoryBySlotAndType(i, k);
                    if (result != null && result.Type == 0)
                        place[k]--;
                    else if (result != null)
                    {
                        bool check = false;
                        // If an item stuck
                        foreach (ItemInstance itemins in item)
                        {
                            if (ServerManager.GetItem(itemins.ItemVNum).Type != 0 && itemins.Amount + result.ItemInstance.Amount <= 99)
                                check = true;
                        }
                        if (!check)
                            place[k]--;
                    }
                }
            }
            bool test2 = true;
            foreach (ItemInstance itemins in item)
            {
                if (place[ServerManager.GetItem(itemins.ItemVNum).Type] == 0)
                    test2 = false;
            }
            return test2;
        }

        public Inventory GetInventoryByInventoryItemId(long inventoryItemId)
        {
            return Inventory.FirstOrDefault(i => i.ItemInstance.ItemInstanceId.Equals(inventoryItemId));
        }

        public void InsertOrUpdate(ref Inventory newInventory)
        {
            short SLOT = newInventory.Slot;
            byte TYPE = newInventory.Type;

            Inventory entity = Inventory.FirstOrDefault(c => c.Slot.Equals(SLOT) && c.Type.Equals(TYPE));

            if (entity == null) //new entity
            {
                newInventory = Insert(newInventory);
            }
            else //existing entity
            {
                newInventory = Update(entity, newInventory);
            }
        }

        public bool IsEmpty()
        {
            return Inventory.Count > 0 ? false : true;
        }

        public T LoadByInventoryItem<T>(long InventoryItemId)
            where T : ItemInstanceDTO
        {
            return (T)Inventory.FirstOrDefault(i => i.ItemInstance.ItemInstanceId.Equals(InventoryItemId)).ItemInstance;
        }

        public Inventory LoadInventoryBySlotAndType(short slot, byte type)
        {
            return Inventory.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));
        }

        public T LoadBySlotAndType<T>(short slot, byte type)
            where T : ItemInstanceDTO
        {
            return (T)Inventory.FirstOrDefault(i => i.Type.Equals(typeof(T)) && i.Slot.Equals(slot) && i.Type.Equals(type))?.ItemInstance;
        }

        public Inventory MoveInventory(byte type, short slot, byte desttype, short destslot)
        {
            Inventory inv = LoadInventoryBySlotAndType(slot, type);
            if (inv != null)
            {
                Item iteminfo = ServerManager.GetItem(inv.ItemInstance.ItemVNum);
                Inventory invdest = LoadInventoryBySlotAndType(destslot, desttype);

                if (invdest == null && ((desttype == 6 && iteminfo.ItemType == 4) || (desttype == 7 && iteminfo.ItemType == 2) || desttype == 0))
                {
                    inv.Slot = destslot;
                    inv.Type = desttype;
                    InsertOrUpdate(ref inv);
                }
            }
            return inv;
        }

        public void MoveItem(Character character, byte type, short slot, byte amount, short destslot, out Inventory inv, out Inventory invdest)
        {
            inv = LoadInventoryBySlotAndType(slot, type);
            invdest = LoadInventoryBySlotAndType(destslot, type);
            if (inv != null && amount <= inv.ItemInstance.Amount)
            {
                if (invdest == null)
                {
                    if (inv.ItemInstance.Amount == amount)
                    {
                        inv.Slot = destslot;
                        InsertOrUpdate(ref inv);
                    }
                    else
                    {
                        inv.ItemInstance.Amount = (byte)(inv.ItemInstance.Amount - amount);

                        //TODO inventoryitem
                        //ItemInstance itemDest = Mapper.DynamicMap(inv.ItemInstance);

                        InsertOrUpdate(ref inv);

                        Inventory invDest = new Inventory
                        {
                            CharacterId = character.CharacterId,
                            Slot = destslot,
                            Type = inv.Type,
                            InventoryId = GenerateInventoryId(),
                            //ItemInstance = itemDest,
                        };
                        InsertOrUpdate(ref invDest);
                        invdest = invDest;
                    }
                }
                else
                {
                    if (invdest.ItemInstance.ItemVNum == inv.ItemInstance.ItemVNum && inv.Type != 0)
                    {
                        if (invdest.ItemInstance.Amount + amount > 99)
                        {
                            int saveItemCount = invdest.ItemInstance.Amount;
                            invdest.ItemInstance.Amount = 99;
                            inv.ItemInstance.Amount = (byte)(saveItemCount + inv.ItemInstance.Amount - 99);

                            InsertOrUpdate(ref inv);
                            InsertOrUpdate(ref invdest);
                        }
                        else
                        {
                            int saveItemCount = invdest.ItemInstance.Amount;
                            invdest.ItemInstance.Amount = (byte)(saveItemCount + amount);
                            inv.ItemInstance.Amount = (byte)(inv.ItemInstance.Amount - amount);
                            InsertOrUpdate(ref inv);
                            InsertOrUpdate(ref invdest);
                        }
                    }
                    else
                    {
                        invdest.Slot = slot;
                        inv.Slot = destslot;
                        InsertOrUpdate(ref inv);
                        InsertOrUpdate(ref invdest);
                    }
                }
            }
            inv = LoadInventoryBySlotAndType(slot, type);
            invdest = LoadInventoryBySlotAndType(destslot, type);
        }

        public MapItem PutItem(ClientSession Session, byte type, short slot, byte amount, out ItemInstance inv)
        {
            Random rnd = new Random();
            int random = 0;
            int i = 0;
            inv = Session.Character.InventoryList.LoadBySlotAndType<ItemInstance>(slot, type);
            MapItem droppedItem = null;
            short MapX = (short)(rnd.Next(Session.Character.MapX - 1, Session.Character.MapX + 1));
            short MapY = (short)(rnd.Next(Session.Character.MapY - 1, Session.Character.MapY + 1));
            while (Session.CurrentMap.IsBlockedZone(MapX, MapY) && i < 5)
            {
                MapX = (short)(rnd.Next(Session.Character.MapX - 1, Session.Character.MapX + 1));
                MapY = (short)(rnd.Next(Session.Character.MapY - 1, Session.Character.MapY + 1));
                i++;
            }
            if (i == 5)
                return null;
            if (amount > 0 && amount <= inv.Amount)
            {
                droppedItem = new MapItem(MapX, MapY)
                {
                    ItemInstance = inv
                };
                while (Session.CurrentMap.DroppedList.ContainsKey(random = rnd.Next(1, 999999)))
                { }
                droppedItem.ItemInstance.ItemInstanceId = random;
                Session.CurrentMap.DroppedList.Add(random, droppedItem);
                inv.Amount = (byte)(inv.Amount - amount);
                //TODO save ItemInstance
                //Session.Character.InventoryList.InsertOrUpdate(ref inv);
            }
            return droppedItem;
        }

        public void RemoveItemAmount(int v, int amount)
        {
            for (int i = 0; i < Inventory.Where(s => s.ItemInstance.ItemVNum == v).OrderBy(s => s.Slot).Count(); i++)
            {
                Inventory inv = Inventory.Where(s => s.ItemInstance.ItemVNum == v).OrderBy(s => s.Slot).ElementAt(i);
                if (inv.ItemInstance.Amount > amount)
                {
                    inv.ItemInstance.Amount -= (byte)amount;
                    amount = 0;
                }
                else
                {
                    amount -= inv.ItemInstance.Amount;
                    DeleteByInventoryItemId(inv.ItemInstance.ItemInstanceId);
                }
            }
        }

        internal Inventory AmountMinusFromInventory(byte amount, PersonalShopItem itemshop)
        {
            Inventory inv = Inventory.FirstOrDefault(i => i.InventoryId.Equals(itemshop.InventoryId));

            if (inv != null)
            {
                inv.ItemInstance.Amount -= amount;
                if (inv.ItemInstance.Amount <= 0)
                {
                    Inventory.Remove(inv);
                    return null;
                }
            }

            return inv;
        }

        private Inventory Insert(Inventory inventory)
        {
            Inventory entity = inventory;
            Inventory.Add(entity);
            return entity;
        }

        private Inventory Update(Inventory entity, Inventory inventory)
        {
            var result = Inventory.FirstOrDefault(c => c.InventoryId == inventory.InventoryId);
            if (result != null)
            {
                Inventory.Remove(result);
                Inventory.Add(inventory);
            }

            return inventory;
        }

        #endregion
    }
}