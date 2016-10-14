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

using OpenNos.Core;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Inventory : List<ItemInstance>
    {
        #region Members

        private Random _random;

        #endregion

        #region Instantiation

        public Inventory(Character Character) : base(new List<ItemInstance>())
        {
            Owner = Character;
            _random = new Random();
        }

        #endregion

        #region Properties

        public Character Owner { get; set; }

        #endregion

        #region Methods

        public ItemInstance AddNewToInventory(short vnum, byte amount = 1)
        {
            Logger.Debug(vnum.ToString(), Owner.Session.SessionId);

            ItemInstance newItem = new ItemInstance() { ItemVNum = vnum, Amount = amount };
            if (newItem.Item != null)
            {
                switch (newItem.Item.Type)
                {
                    case (byte)InventoryType.Wear:
                        if (newItem.Item.ItemType == (byte)ItemType.Specialist)
                        {
                            newItem = new SpecialistInstance() { ItemVNum = vnum, SpLevel = 1, Amount = amount };
                        }
                        else
                        {
                            newItem = new WearableInstance() { ItemVNum = vnum, Amount = amount };
                        }
                        break;
                }
            }

            return AddToInventory(newItem);
        }

        public ItemInstance AddToInventory(ItemInstance newItem)
        {
            Logger.Debug(newItem.ItemVNum.ToString(), Owner.Session.SessionId);
            IEnumerable<ItemInstance> slotfree = null;
            ItemInstance inv = null;
            if (newItem.Item.Type != 0)
            {
                slotfree = Owner.LoadBySlotAllowed(newItem.ItemVNum, newItem.Amount);
                inv = GetFirstSlot(slotfree);
            }
            if (inv != null)
            {
                inv.Amount = (byte)(newItem.Amount + inv.Amount);
            }
            else
            {
                short slot = GetFirstPlace(newItem.Item.Type, Owner.BackPack);
                if (slot != -1)
                {
                    inv = AddToInventoryWithSlotAndType(newItem, newItem.Item.Type, slot);
                }
            }

            return inv;
        }

        public ItemInstance AddToInventoryWithSlotAndType(ItemInstance iteminstance, InventoryType type, short slot)
        {
            Logger.Debug($"Slot: {slot} Type: {type} VNUM: {iteminstance.ItemVNum}", Owner.Session.SessionId);
            ItemInstance inv = new ItemInstance() { Type = type, Slot = slot, CharacterId = Owner.CharacterId };
            string inventoryPacket = Owner.Session.Character.GenerateInventoryAdd(iteminstance.ItemVNum, inv.Amount, type, slot, iteminstance.Rare, iteminstance.Design, 0, 0);
            if (!String.IsNullOrEmpty(inventoryPacket))
            {
                Owner.Session.SendPacket(inventoryPacket);
            }

            if (this.Any(s => s.Slot == slot && s.Type == type))
            {
                return null;
            }

            Add(inv);
            return inv;
        }

        public int CountItem(int v)
        {
            int count = 0;
            foreach (ItemInstance inv in this.Where(s => s.ItemVNum == v))
            {
                count += inv.Amount;
            }
            return count;
        }

        public Inventory DeepCopy()
        {
            Inventory clonedList = (Inventory)this.MemberwiseClone();

            return clonedList;
        }

        public Tuple<short, InventoryType> DeleteByInventoryItemId(Guid id)
        {
            Logger.Debug(id.ToString(), Owner.Session.SessionId);
            Tuple<short, InventoryType> removedPlace = new Tuple<short, InventoryType>(0, 0);
            ItemInstance inv = this.FirstOrDefault(i => i.Id.Equals(id));

            if (inv != null)
            {
                removedPlace = new Tuple<short, InventoryType>(inv.Slot, inv.Type);
                this.Remove(inv);
            }

            return removedPlace;
        }

        public void DeleteFromSlotAndType(short slot, InventoryType type)
        {
            Logger.Debug($"Slot: {slot} Type: {type}", Owner.Session.SessionId);
            ItemInstance inv = this.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));

            if (inv != null)
            {
                this.Remove(inv);
            }
        }

        public ItemInstance GetFirstSlot(IEnumerable<ItemInstance> slotfree)
        {
            List<Guid> inventoryitemids = new List<Guid>();
            foreach (ItemInstance itemfree in slotfree)
            {
                inventoryitemids.Add(itemfree.Id);
            }
            return this.Where(i => inventoryitemids.Contains(i.Id)).OrderBy(i => i.Slot).FirstOrDefault();
        }

        public bool GetFreePlaceAmount(List<ItemInstance> item, int backPack)
        {
            short[] place = new short[item.Count()];
            for (byte k = 0; k < item.Count(); k++)
            {
                place[k] = (byte)(48 + (backPack * 12));
                for (short i = 0; i < 48 + (backPack * 12); i++)
                {
                    ItemInstance result = LoadInventoryBySlotAndType(i, (InventoryType)item[k].Item.Type);
                    if (result != null && result.Type == 0)
                    {
                        place[k]--;
                    }
                    else if (result != null)
                    {
                        bool check = false;
                        foreach (ItemInstance itemins in item)
                        {
                            if (itemins.Item.Type != 0 && itemins.Amount + result.Amount <= 99)
                            {
                                check = true;
                            }
                        }
                        if (!check)
                        {
                            place[k]--;
                        }
                    }
                }
            }
            for (int i = 0; i < item.Count(); i++)
            {
                if (place[i] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public ItemInstance GetInventoryByItemInstanceId(Guid id)
        {
            return this.FirstOrDefault(i => i.Id.Equals(id));
        }

        public bool IsEmpty()
        {
            return !this.Any();
        }

        public T LoadByItemInstance<T>(Guid id)
                    where T : ItemInstance
        {
            return (T)this.FirstOrDefault(i => i.Id.Equals(id));
        }

        public T LoadBySlotAndType<T>(short slot, InventoryType type)
                    where T : ItemInstance
        {
            return (T)this.FirstOrDefault(i => i.GetType().Equals(typeof(T)) && i.Slot == slot && i.Type == type);
        }

        public ItemInstance LoadInventoryBySlotAndType(short slot, InventoryType type)
        {
            return this.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));
        }

        public ItemInstance MoveInventory(ItemInstance inv, InventoryType desttype, short destslot)
        {
            if (Owner.Session != null && inv != null)
            {
                Logger.Debug($"Inventory: {inv.Id} Desttype: {desttype} Destslot: {destslot}", Owner.Session.SessionId);
                Item iteminfo = inv.Item;
                ItemInstance invdest = LoadInventoryBySlotAndType(destslot, desttype);

                if (invdest == null && ((desttype == InventoryType.Sp && iteminfo.ItemType == 4) || (desttype == InventoryType.Costume && iteminfo.ItemType == 2) || desttype == 0))
                {
                    inv.Slot = destslot;
                    inv.Type = desttype;
                    return inv;
                }
            }
            return null;
        }

        public void MoveItem(InventoryType type, short sourceSlot, byte amount, short destinationSlot, out ItemInstance sourceInventory, out ItemInstance destinationInventory)
        {
            Logger.Debug($"type: {type} sourceSlot: {sourceSlot} amount: {amount} destinationSlot: {destinationSlot}", Owner.Session.SessionId);

            // load source and destination slots
            sourceInventory = LoadInventoryBySlotAndType(sourceSlot, type);
            destinationInventory = LoadInventoryBySlotAndType(destinationSlot, type);
            if (sourceInventory != null && amount <= sourceInventory.Amount)
            {
                if (destinationInventory == null)
                {
                    if (sourceInventory.Amount == amount)
                    {
                        sourceInventory.Slot = destinationSlot;
                    }
                    else
                    {
                        ItemInstance itemDest = sourceInventory.DeepCopy();
                        sourceInventory.Amount -= amount;
                        itemDest.Amount = amount;
                        itemDest.Id = Guid.NewGuid();
                        destinationInventory = AddToInventoryWithSlotAndType(itemDest, sourceInventory.Type, destinationSlot);
                    }
                }
                else
                {
                    if (destinationInventory.ItemVNum == sourceInventory.ItemVNum && sourceInventory.Type != 0)
                    {
                        if (destinationInventory.Amount + amount > 99)
                        {
                            int saveItemCount = destinationInventory.Amount;
                            destinationInventory.Amount = 99;
                            sourceInventory.Amount = (byte)(saveItemCount + sourceInventory.Amount - 99);
                        }
                        else
                        {
                            destinationInventory.Amount += amount;
                            sourceInventory.Amount -= amount;

                            // item with amount of 0 should be removed
                            if (sourceInventory.Amount == 0)
                            {
                                DeleteFromSlotAndType(sourceInventory.Slot, sourceInventory.Type);
                            }
                        }
                    }
                    else
                    {
                        // add and remove save inventory
                        destinationInventory = TakeInventory(destinationInventory.Slot, destinationInventory.Type);
                        destinationInventory.Slot = sourceSlot;
                        sourceInventory = TakeInventory(sourceInventory.Slot, sourceInventory.Type);
                        sourceInventory.Slot = destinationSlot;
                        PutInventory(destinationInventory);
                        PutInventory(sourceInventory);
                    }
                }
            }
            sourceInventory = LoadInventoryBySlotAndType(sourceSlot, type);
            destinationInventory = LoadInventoryBySlotAndType(destinationSlot, type);
        }

        /// <summary>
        /// Puts a Single Invenory including ItemInstance to the List
        /// </summary>
        /// <param name="inventory"></param>
        public void PutInventory(ItemInstance inventory)
        {
            this.Add(inventory);
        }

        public MapItem PutItem(byte type, short slot, byte amount, ref ItemInstance inv)
        {
            Logger.Debug($"type: {type} slot: {slot} amount: {amount}", Owner.Session.SessionId);
            Guid random2 = Guid.NewGuid();
            MapItem droppedItem = null;
            List<MapCell> Possibilities = new List<MapCell>();

            for (short x = -2; x < 3; x++)
            {
                for (short y = -2; y < 3; y++)
                {
                    Possibilities.Add(new MapCell() { X = x, Y = y });
                }
            }
            short MapX = 0;
            short MapY = 0;
            foreach (MapCell possibilitie in Possibilities.OrderBy(s => _random.Next()))
            {
                MapX = (short)(Owner.MapX + possibilitie.X);
                MapY = (short)(Owner.MapY + possibilitie.Y);
                if (!Owner.Session.CurrentMap.IsBlockedZone(MapX, MapY))
                {
                    break;
                }
            }

            if (amount > 0 && amount <= inv.Amount)
            {
                droppedItem = new MapItem(MapX, MapY)
                {
                    ItemInstance = inv.DeepCopy()
                };
                droppedItem.ItemInstance.Id = random2;
                droppedItem.ItemInstance.Amount = amount;
                while (Owner.Session.CurrentMap.DroppedList.ContainsKey(droppedItem.ItemInstance.TransportId))
                {
                    droppedItem.ItemInstance.TransportId = 0; // reset transportId
                }

                Owner.Session.CurrentMap.DroppedList.TryAdd(droppedItem.ItemInstance.TransportId, droppedItem);
                inv.Amount -= amount;
            }
            return droppedItem;
        }

        public void RemoveItemAmount(int vnum, int amount = 1)
        {
            Logger.Debug($"vnum: {vnum} amount: {amount}", Owner.Session.SessionId);
            int remainingAmount = amount;

            foreach (ItemInstance inventory in this.Where(s => s.ItemVNum == vnum).OrderBy(i => i.Slot))
            {
                if (remainingAmount > 0)
                {
                    Logger.Debug($"Remaining {remainingAmount}/{amount}, removing item {inventory.ItemVNum} from Slot {inventory.Slot} with amount {inventory.Amount}");
                    if (inventory.Amount > remainingAmount)
                    {
                        // amount completely removed
                        inventory.Amount -= (byte)remainingAmount;
                        remainingAmount = 0;
                        Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(inventory.ItemVNum,
                            inventory.Amount, inventory.Type, inventory.Slot, inventory.Rare, inventory.Design,
                            inventory.Upgrade, 0));
                    }
                    else
                    {
                        // amount partly removed
                        remainingAmount -= inventory.Amount;
                        DeleteByInventoryItemId(inventory.Id);
                        Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(-1, 0, inventory.Type, inventory.Slot, 0, 0, 0, 0));
                    }
                }
                else
                {
                    // amount to remove reached
                    break;
                }
            }
        }

        public ItemInstance RemoveItemAmountFromInventory(byte amount, Guid id)
        {
            Logger.Debug($"InventoryId: {id} amount: {amount}", Owner.Session.SessionId);
            ItemInstance inv = this.FirstOrDefault(i => i.Id.Equals(id));

            if (inv != null)
            {
                inv.Amount -= amount;
                if (inv.Amount <= 0)
                {
                    this.Remove(inv);
                    return null;
                }
            }

            return inv;
        }

        public void Reorder(ClientSession Session, InventoryType inventoryType)
        {
            List<ItemInstance> templist = new List<ItemInstance>();
            switch (inventoryType)
            {
                case InventoryType.Costume:
                    templist = this.Where(s => s.Type == InventoryType.Costume).OrderBy(s => s.ItemVNum).ToList();
                    break;

                case InventoryType.Sp:
                    templist = this.Where(s => s.Type == InventoryType.Sp).OrderBy(s => ServerManager.GetItem(s.ItemVNum).LevelJobMinimum).ToList();
                    break;
            }
            short i = 0;
            foreach (ItemInstance invtemp in templist)
            {
                ItemInstance temp = new GameObject.ItemInstance();
                ItemInstance temp2 = new GameObject.ItemInstance();
                if (invtemp.Slot != i)
                {
                    MoveItem(inventoryType, invtemp.Slot, 1, i, out temp, out temp2);

                    if (temp2 == null || temp == null)
                    {
                        return;
                    }
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(temp2.ItemVNum, temp2.Amount, inventoryType, temp2.Slot, temp2.Rare, temp2.Design, temp2.Upgrade, 0));
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(temp.ItemVNum, temp.Amount, inventoryType, temp.Slot, temp.Rare, temp.Design, temp.Upgrade, 0));
                }
                i++;
            }
        }

        /// <summary>
        /// Takes a Single Inventory including ItemInstance from the List and removes it.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ItemInstance TakeInventory(short slot, InventoryType type)
        {
            ItemInstance inventoryToTake = this.SingleOrDefault(i => i.Slot == slot && i.Type == type);
            this.Remove(inventoryToTake);
            return inventoryToTake;
        }

        private short GetFirstPlace(InventoryType type, int backPack)
        {
            ItemInstance result;
            for (short i = 0; i < 48 + (backPack * 12); i++)
            {
                result = this.FirstOrDefault(c => c.Type == type && c.Slot.Equals(i));
                if (result == null)
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion
    }
}