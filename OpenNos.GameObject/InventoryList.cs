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

        public InventoryList()
        {
            Inventory = new List<Inventory>();
        }
        #endregion

        #region Methods
        public bool isEmpty()
        {
            return Inventory.Count > 0 ? false : true;
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
                Inventory.Remove(result);
                Inventory.Add(inventory);

            }


            return inventory;
        }


        public short getFirstPlace(byte type, int backPack, InventoryItem item)
        {
            Inventory result;
            for (short i = 0; i < 48 + backPack * 12; i++)
            {
                result = Inventory.SingleOrDefault(c => c.Type.Equals(type) && c.Slot.Equals(i));
                if (result == null)
                    return i;
            }
            return -1;
        }

        public bool getFreePlaceAmount(List<InventoryItem> item, int backPack)
        {
            short[] place = new short[10];
            for (short k = 0; k < place.Count(); k++)
            {
                place[k] = (short)(48 + backPack * 12);
                for (short i = 0; i < 48 + backPack * 12; i++)
                {
                    Inventory result = LoadBySlotAndType(i, k);
                    if (result != null && result.Type == 0)
                        place[k]--;
                    else if (result != null)
                    {
                        bool test = false;
                        //si un item colle
                        foreach (InventoryItem itemins in item)
                        {
                            if (ServerManager.GetItem(itemins.ItemVNum).Type != 0 && itemins.Amount + result.InventoryItem.Amount <= 99)
                                test = true;
                        }
                        if (!test)
                            place[k]--;
                    }
                }
            }
            bool test2 = true;
            foreach (InventoryItem itemins in item)
            {
                if (place[ServerManager.GetItem(itemins.ItemVNum).Type] == 0)
                    test2 = false;
            }
            return test2;
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

        public Inventory getFirstSlot(IEnumerable<InventoryItem> slotfree)
        {
            List<long> inventoryitemids = new List<long>();
            foreach (InventoryItem itemfree in slotfree)
            {
                inventoryitemids.Add(itemfree.InventoryItemId);
            }
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
                inventoryitemId = (long)((r.NextDouble() * 2.0 - 1.0) * long.MaxValue);
                foreach (Inventory inv in Inventory)
                {
                    if (inv.InventoryItemId == inventoryitemId)
                    { boolean = true; break; }
                }
            }
            return inventoryitemId;
        }

        public Inventory CreateItem(InventoryItem newItem, Character character)
        {
            short Slot = -1;
            IEnumerable<InventoryItem> slotfree = null;
            Inventory inv = null;
            if (ServerManager.GetItem(newItem.ItemVNum).Type != 0)
            {
                slotfree = character.LoadBySlotAllowed(newItem.ItemVNum, newItem.Amount);
                inv = getFirstSlot(slotfree);
            }
            bool modified = false;
            Inventory newInventory = null;
            if (inv != null)
            {
                Slot = inv.Slot;
                newItem.Amount = (short)(newItem.Amount + inv.InventoryItem.Amount);
                modified = true;
            }
            else
                Slot = getFirstPlace(ServerManager.GetItem(newItem.ItemVNum).Type, character.BackPack, newItem);
            if (Slot != -1)
            {

                if (modified == false)
                    newInventory = new Inventory()
                    {
                        CharacterId = character.CharacterId,
                        InventoryItemId = newItem.InventoryItemId,
                        Slot = Slot,
                        Type = ServerManager.GetItem(newItem.ItemVNum).Type,
                        InventoryItem = newItem,
                        InventoryId = generateInventoryId(),
                    };
                else
                    newInventory = new Inventory()
                    {
                        CharacterId = character.CharacterId,
                        InventoryItemId = newItem.InventoryItemId,
                        Slot = Slot,
                        Type = ServerManager.GetItem(newItem.ItemVNum).Type,
                        InventoryItem = newItem,
                        InventoryId = inv.InventoryId,
                    };
                InsertOrUpdate(ref newInventory);

            }
            return newInventory;
        }

        public MapItem PutItem(ClientSession Session, short type, short slot, short amount, out Inventory inv)
        {
            Random rnd = new Random();
            int random = 0;
            inv = Session.Character.InventoryList.LoadBySlotAndType(slot, type);
            MapItem DroppedItem = null;
            if (amount <= inv.InventoryItem.Amount)
            {
                DroppedItem = new MapItem((short)(rnd.Next(Session.Character.MapX - 2, Session.Character.MapX + 3)), (short)(rnd.Next(Session.Character.MapY - 2, Session.Character.MapY + 3)))
                {
                    Amount = amount,
                    Color = inv.InventoryItem.Color,
                    Concentrate = inv.InventoryItem.Concentrate,
                    CriticalLuckRate = inv.InventoryItem.CriticalLuckRate,
                    CriticalRate = inv.InventoryItem.CriticalRate,
                    DamageMaximum = inv.InventoryItem.DamageMaximum,
                    DamageMinimum = inv.InventoryItem.DamageMinimum,
                    DarkElement = inv.InventoryItem.DarkElement,
                    DistanceDefence = inv.InventoryItem.DistanceDefence,
                    DefenceDodge = inv.InventoryItem.DefenceDodge,
                    DistanceDefenceDodge = inv.InventoryItem.DistanceDefenceDodge,
                    ElementRate = inv.InventoryItem.ElementRate,
                    FireElement = inv.InventoryItem.FireElement,
                    HitRate = inv.InventoryItem.HitRate,
                    WaterElement = inv.InventoryItem.WaterElement,
                    SlHit = inv.InventoryItem.SlHit,
                    ItemVNum = inv.InventoryItem.ItemVNum,
                    LightElement = inv.InventoryItem.LightElement,
                    MagicDefence = inv.InventoryItem.MagicDefence,
                    RangeDefence = inv.InventoryItem.RangeDefence,
                    Rare = inv.InventoryItem.Rare,
                    SlDefence = inv.InventoryItem.SlDefence,
                    SlElement = inv.InventoryItem.SlElement,
                    SlHP = inv.InventoryItem.SlHP,
                    Upgrade = inv.InventoryItem.Upgrade
                };
                while (Session.CurrentMap.DroppedList.ContainsKey(random = rnd.Next(1, 999999)))
                { }
                DroppedItem.InventoryItemId = random;
                Session.CurrentMap.DroppedList.Add(random, DroppedItem);
                inv.InventoryItem.Amount = (short)(inv.InventoryItem.Amount - amount);
                Session.Character.InventoryList.InsertOrUpdate(ref inv);


            }
            return DroppedItem;
        }

        internal Inventory AmountMinusFromSlotAndType(short amount, short invSlot, short invType)
        {
            Inventory inv = Inventory.SingleOrDefault(i => i.Slot.Equals(invSlot) && i.Type.Equals(invType));

            if (inv != null)
            {
                inv.InventoryItem.Amount -= amount;
            }
            if (inv.InventoryItem.Amount <= 0)
            {
                Inventory.Remove(inv);
                return null;
            }
            return inv;
        }

        public Inventory moveInventory(short type, short slot, short desttype, short destslot)
        {
            Inventory inv = LoadBySlotAndType(slot, type);
            if (inv != null)
            {
                Item iteminfo = ServerManager.GetItem(inv.InventoryItem.ItemVNum);
                Inventory invdest = LoadBySlotAndType(destslot, desttype);

                if (invdest == null && ((desttype == 6 && iteminfo.ItemType == 4) || (desttype == 7 && iteminfo.ItemType == 2) || desttype == 0))
                {
                    inv.Slot = destslot;
                    inv.Type = desttype;
                    InsertOrUpdate(ref inv);

                }
            }
            return inv;
        }

        public void MoveItem(Character character, short type, short slot, short amount, short destslot, out Inventory inv, out Inventory invdest)
        {
            inv = LoadBySlotAndType(slot, type);
            invdest = LoadBySlotAndType(destslot, type);
            if (amount <= inv.InventoryItem.Amount)
            {
                if (invdest == null)
                {
                    if (inv.InventoryItem.Amount == amount)
                    {
                        inv.Slot = destslot;
                        InsertOrUpdate(ref inv);
                    }
                    else
                    {
                        inv.InventoryItem.Amount = (short)(inv.InventoryItem.Amount - amount);

                        InventoryItem itemDest = new InventoryItem
                        {
                            Amount = amount,
                            Color = inv.InventoryItem.Color,
                            Concentrate = inv.InventoryItem.Concentrate,
                            CriticalLuckRate = inv.InventoryItem.CriticalLuckRate,
                            CriticalRate = inv.InventoryItem.CriticalRate,
                            DamageMaximum = inv.InventoryItem.DamageMaximum,
                            DamageMinimum = inv.InventoryItem.DamageMinimum,
                            DarkElement = inv.InventoryItem.DarkElement,
                            DistanceDefence = inv.InventoryItem.DistanceDefence,
                            DistanceDefenceDodge = inv.InventoryItem.DistanceDefenceDodge,
                            DefenceDodge = inv.InventoryItem.DefenceDodge,
                            ElementRate = inv.InventoryItem.ElementRate,
                            FireElement = inv.InventoryItem.FireElement,
                            HitRate = inv.InventoryItem.HitRate,
                            ItemVNum = inv.InventoryItem.ItemVNum,
                            LightElement = inv.InventoryItem.LightElement,
                            MagicDefence = inv.InventoryItem.MagicDefence,
                            RangeDefence = inv.InventoryItem.RangeDefence,
                            Rare = inv.InventoryItem.Rare,
                            SlDefence = inv.InventoryItem.SlDefence,
                            SlElement = inv.InventoryItem.SlElement,
                            SlHit = inv.InventoryItem.SlHit,
                            SlHP = inv.InventoryItem.SlHP,
                            Upgrade = inv.InventoryItem.Upgrade,
                            WaterElement = inv.InventoryItem.WaterElement,
                            InventoryItemId = generateInventoryItemId(),


                        };


                        InsertOrUpdate(ref inv);

                        Inventory invDest = new Inventory
                        {
                            CharacterId = character.CharacterId,
                            InventoryItemId = itemDest.InventoryItemId,
                            Slot = destslot,
                            Type = inv.Type,
                            InventoryId = generateInventoryId(),
                            InventoryItem = itemDest,
                        };
                        InsertOrUpdate(ref invDest);
                        invdest = invDest;
                    }

                }
                else
                {

                    if (invdest.InventoryItem.ItemVNum == inv.InventoryItem.ItemVNum && inv.Type != 0)
                    {

                        if (invdest.InventoryItem.Amount + amount > 99)
                        {
                            short saveItemCount = invdest.InventoryItem.Amount;
                            invdest.InventoryItem.Amount = 99;
                            inv.InventoryItem.Amount = (short)(saveItemCount + inv.InventoryItem.Amount - 99);

                            InsertOrUpdate(ref inv);
                            InsertOrUpdate(ref invdest);

                        }
                        else
                        {
                            short saveItemCount = invdest.InventoryItem.Amount;
                            invdest.InventoryItem.Amount = (short)(saveItemCount + amount);
                            inv.InventoryItem.Amount = (short)(inv.InventoryItem.Amount - amount);
                            InsertOrUpdate(ref inv);
                            InsertOrUpdate(ref invdest);


                        }
                    }
                    else
                    {
                        invdest.Slot = inv.Slot;
                        inv.Slot = 99;
                        InsertOrUpdate(ref inv);
                        InsertOrUpdate(ref invdest);
                        inv.Slot = destslot;
                        InsertOrUpdate(ref inv);
                    }
                }
            }
            inv = LoadBySlotAndType(slot, type);
            invdest = LoadBySlotAndType(destslot, type);
        }

        public void DeleteByInventoryItemId(long inventoryItemId)
        {
            Inventory inv = Inventory.SingleOrDefault(i => i.InventoryItemId.Equals(inventoryItemId));

            if (inv != null)
            {
                Inventory.Remove(inv);
            }
        }

        public Inventory getInventoryByInventoryItemId(long inventoryItemId)
        {
            return Inventory.SingleOrDefault(i => i.InventoryItemId.Equals(inventoryItemId));

        }




        #endregion
    }
}
