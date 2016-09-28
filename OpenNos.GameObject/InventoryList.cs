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
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class InventoryList
    {
        #region Instantiation

        private Random _random;

        public InventoryList(Character Character)
        {
            Inventory = new List<Inventory>();
            Owner = Character;
            _random = new Random();
        }

        #endregion

        #region Properties

        public List<Inventory> Inventory { get; set; }

        public Character Owner { get; set; }

        #endregion

        #region Methods

        public static ItemInstance CreateItemInstance(short vnum)
        {
            ItemInstance iteminstance = new ItemInstance() { ItemVNum = vnum, Amount = 1 };
            if (iteminstance.Item != null)
            {
                switch (iteminstance.Item.Type)
                {
                    case (byte)InventoryType.Wear:
                        if (iteminstance.Item.ItemType == (byte)ItemType.Specialist)
                        {
                            iteminstance = new SpecialistInstance() { ItemVNum = vnum, SpLevel = 1, Amount = 1 };
                        }
                        else
                        {
                            iteminstance = new WearableInstance() { ItemVNum = vnum, Amount = 1 };
                        }
                        break;
                }
            }
            return iteminstance;
        }

        public Inventory AddNewItemToInventory(short vnum, int amount = 1)
        {
            Logger.Debug(vnum.ToString(), Owner.Session.SessionId);
            short Slot = -1;
            IEnumerable<ItemInstance> slotfree = null;
            Inventory inv = null;
            ItemInstance newItem = CreateItemInstance(vnum);
            newItem.Amount = amount;
            if (newItem.Item.Type != 0)
            {
                slotfree = Owner.LoadBySlotAllowed(newItem.ItemVNum, newItem.Amount);
                inv = GetFirstSlot(slotfree);
            }
            if (inv != null)
            {
                inv.ItemInstance.Amount += newItem.Amount;
                Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(vnum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade, 0));
            }
            else
            {
                Slot = GetFirstPlace(newItem.Item.Type, Owner.BackPack);
                if (Slot != -1)
                {
                    inv = AddToInventoryWithSlotAndType(newItem, newItem.Item.Type, Slot);
                    inv.ItemInstance.Id = inv.Id; // set id because its a one to one
                }
            }
            return inv;
        }

        public Inventory AddToInventory(ItemInstance newItem)
        {
            Logger.Debug(newItem.ItemVNum.ToString(), Owner.Session.SessionId);
            short Slot = -1;
            IEnumerable<ItemInstance> slotfree = null;
            Inventory inv = null;
            if (newItem.Item.Type != 0)
            {
                slotfree = Owner.LoadBySlotAllowed(newItem.ItemVNum, newItem.Amount);
                inv = GetFirstSlot(slotfree);
            }
            if (inv != null)
            {
                inv.ItemInstance.Amount = (byte)(newItem.Amount + inv.ItemInstance.Amount);
            }
            else
            {
                Slot = GetFirstPlace(newItem.Item.Type, Owner.BackPack);
                if (Slot != -1)
                {
                    inv = AddToInventoryWithSlotAndType(newItem, newItem.Item.Type, Slot);
                    inv.ItemInstance.Id = inv.Id; // set id because its a one to one
                }
            }

            return inv;
        }

        public Inventory AddToInventoryWithSlotAndType(ItemInstance iteminstance, InventoryType type, short slot)
        {
            Logger.Debug($"Slot: {slot} Type: {type} VNUM: {iteminstance.ItemVNum}", Owner.Session.SessionId);
            Inventory inv = new Inventory() { Type = type, Slot = slot, ItemInstance = iteminstance, CharacterId = Owner.CharacterId };
            string inventoryPacket = Owner.Session.Character.GenerateInventoryAdd(iteminstance.ItemVNum, inv.ItemInstance.Amount, type, slot, iteminstance.Rare, iteminstance.Design, 0, 0);
            if (!String.IsNullOrEmpty(inventoryPacket))
            {
                Owner.Session.SendPacket(inventoryPacket);
            }

            if (Inventory.Any(s => s.Slot == slot && s.Type == type))
            {
                return null;
            }

            inv.ItemInstance.Id = inv.Id; // set id because its a one to one
            Inventory.Add(inv);
            return inv;
        }

        public int CountItem(int v)
        {
            int count = 0;
            foreach (Inventory inv in Inventory.Where(s => s.ItemInstance.ItemVNum == v))
            {
                count += inv.ItemInstance.Amount;
            }
            return count;
        }

        public InventoryList DeepCopy()
        {
            InventoryList clonedList = (InventoryList)this.MemberwiseClone();
            clonedList.Inventory = this.Inventory.Select(x => x.DeepCopy()).ToList();

            return clonedList;
        }

        public Tuple<short, InventoryType> DeleteByInventoryItemId(Guid id)
        {
            Logger.Debug(id.ToString(), Owner.Session.SessionId);
            Tuple<short, InventoryType> removedPlace = new Tuple<short, InventoryType>(0, 0);
            Inventory inv = Inventory.FirstOrDefault(i => i.ItemInstance.Id.Equals(id));

            if (inv != null)
            {
                removedPlace = new Tuple<short, InventoryType>(inv.Slot, inv.Type);
                Inventory.Remove(inv);
            }

            return removedPlace;
        }

        public void DeleteFromSlotAndType(short slot, InventoryType type)
        {
            Logger.Debug($"Slot: {slot} Type: {type}", Owner.Session.SessionId);
            Inventory inv = Inventory.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));

            if (inv != null)
            {
                Inventory.Remove(inv);
            }
        }

        public Inventory GetFirstSlot(IEnumerable<ItemInstance> slotfree)
        {
            List<Guid> inventoryitemids = new List<Guid>();
            foreach (ItemInstance itemfree in slotfree)
            {
                inventoryitemids.Add(itemfree.Id);
            }
            return Inventory.Where(i => inventoryitemids.Contains(i.ItemInstance.Id)).OrderBy(i => i.Slot).FirstOrDefault();
        }

        public bool GetFreePlaceAmount(List<ItemInstance> item, int backPack)
        {
            short[] place = new short[item.Count()];
            for (byte k = 0; k < item.Count(); k++)
            {
                place[k] = (byte)(48 + (backPack * 12));
                for (short i = 0; i < 48 + (backPack * 12); i++)
                {
                    Inventory result = LoadInventoryBySlotAndType(i, (InventoryType)item[k].Item.Type);
                    if (result != null && result.Type == 0)
                    {
                        place[k]--;
                    }
                    else if (result != null)
                    {
                        bool check = false;
                        foreach (ItemInstance itemins in item)
                        {
                            if (itemins.Item.Type != 0 && itemins.Amount + result.ItemInstance.Amount <= 99)
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

        public Inventory GetInventoryByItemInstanceId(Guid id)
        {
            return Inventory.FirstOrDefault(i => i.ItemInstance.Id.Equals(id));
        }

        public bool IsEmpty()
        {
            return !Inventory.Any();
        }

        public T LoadByItemInstance<T>(Guid id)
                    where T : ItemInstanceDTO
        {
            return (T)Inventory.FirstOrDefault(i => i.ItemInstance.Id.Equals(id))?.ItemInstance;
        }

        public T LoadBySlotAndType<T>(short slot, InventoryType type)
                    where T : ItemInstance
        {
            return (T)Inventory.FirstOrDefault(i => i.ItemInstance.GetType().Equals(typeof(T)) && i.Slot == slot && i.Type == type)?.ItemInstance;
        }

        public Inventory LoadInventoryBySlotAndType(short slot, InventoryType type)
        {
            return Inventory.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));
        }

        public Inventory MoveInventory(Inventory inv, InventoryType desttype, short destslot)
        {
            Logger.Debug($"Inventory: {inv.Id} Desttype: {desttype} Destslot: {destslot}", Owner.Session.SessionId);
            if (inv != null)
            {
                Item iteminfo = (inv.ItemInstance as ItemInstance).Item;
                Inventory invdest = LoadInventoryBySlotAndType(destslot, desttype);

                if (invdest == null && ((desttype == InventoryType.Sp && iteminfo.ItemType == 4) || (desttype == InventoryType.Costume && iteminfo.ItemType == 2) || desttype == 0))
                {
                    inv.Slot = destslot;
                    inv.Type = desttype;
                    return inv;
                }
            }
            return null;
        }

        public void MoveItem(InventoryType type, short sourceSlot, byte amount, short destinationSlot, out Inventory sourceInventory, out Inventory destinationInventory)
        {
            Logger.Debug($"type: {type} sourceSlot: {sourceSlot} amount: {amount} destinationSlot: {destinationSlot}", Owner.Session.SessionId);

            // load source and destination slots
            sourceInventory = LoadInventoryBySlotAndType(sourceSlot, type);
            destinationInventory = LoadInventoryBySlotAndType(destinationSlot, type);
            if (sourceInventory != null && amount <= sourceInventory.ItemInstance.Amount)
            {
                if (destinationInventory == null)
                {
                    if (sourceInventory.ItemInstance.Amount == amount)
                    {
                        sourceInventory.Slot = destinationSlot;
                    }
                    else
                    {
                        ItemInstance itemDest = (sourceInventory.ItemInstance as ItemInstance).DeepCopy();
                        sourceInventory.ItemInstance.Amount -= amount;
                        itemDest.Amount = amount;
                        itemDest.Id = Guid.NewGuid();
                        destinationInventory = AddToInventoryWithSlotAndType(itemDest, sourceInventory.Type, destinationSlot);
                    }
                }
                else
                {
                    if (destinationInventory.ItemInstance.ItemVNum == sourceInventory.ItemInstance.ItemVNum && sourceInventory.Type != 0)
                    {
                        if (destinationInventory.ItemInstance.Amount + amount > 99)
                        {
                            int saveItemCount = destinationInventory.ItemInstance.Amount;
                            destinationInventory.ItemInstance.Amount = 99;
                            sourceInventory.ItemInstance.Amount = (byte)(saveItemCount + sourceInventory.ItemInstance.Amount - 99);
                        }
                        else
                        {
                            destinationInventory.ItemInstance.Amount += amount;
                            sourceInventory.ItemInstance.Amount -= amount;

                            // item with amount of 0 should be removed
                            if (sourceInventory.ItemInstance.Amount == 0)
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
        public void PutInventory(Inventory inventory)
        {
            Inventory.Add(inventory);
        }

        public MapItem PutItem(byte type, short slot, byte amount, ref Inventory inv)
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

            if (amount > 0 && amount <= inv.ItemInstance.Amount)
            {
                droppedItem = new MapItem(MapX, MapY)
                {
                    ItemInstance = (inv.ItemInstance as ItemInstance).DeepCopy()
                };
                droppedItem.ItemInstance.Id = random2;
                droppedItem.ItemInstance.Amount = amount;
                while (Owner.Session.CurrentMap.DroppedList.ContainsKey(droppedItem.ItemInstance.TransportId))
                {
                    droppedItem.ItemInstance.TransportId = 0; // reset transportId
                }

                Owner.Session.CurrentMap.DroppedList.TryAdd(droppedItem.ItemInstance.TransportId, droppedItem);
                inv.ItemInstance.Amount -= amount;
            }
            return droppedItem;
        }

        public void RemoveItemAmount(int vnum, int amount)
        {
            Logger.Debug($"vnum: {vnum} amount: {amount}", Owner.Session.SessionId);
            for (int i = 0; i < Inventory.Where(s => s.ItemInstance.ItemVNum == vnum).OrderBy(s => s.Slot).Count(); i++)
            {
                Inventory inv = Inventory.Where(s => s.ItemInstance.ItemVNum == vnum).OrderBy(s => s.Slot).ElementAt(i);
                if (inv.ItemInstance.Amount > amount)
                {
                    inv.ItemInstance.Amount -= (byte)amount;
                    amount = 0;
                    Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade, 0));
                }
                else
                {
                    amount -= inv.ItemInstance.Amount;
                    DeleteByInventoryItemId(inv.ItemInstance.Id);
                    Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0, 0));
                }
            }
        }

        public Inventory RemoveItemAmountFromInventory(byte amount, Guid id)
        {
            Logger.Debug($"InventoryId: {id} amount: {amount}", Owner.Session.SessionId);
            Inventory inv = Inventory.FirstOrDefault(i => i.Id.Equals(id));

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

        public void Reorder(ClientSession Session, InventoryType inventoryType)
        {
            List<Inventory> templist = new List<Inventory>();
            switch (inventoryType)
            {
                case InventoryType.Costume:
                    templist = Inventory.Where(s => s.Type == InventoryType.Costume).OrderBy(s => s.ItemInstance.ItemVNum).ToList();
                    break;

                case InventoryType.Sp:
                    templist = Inventory.Where(s => s.Type == InventoryType.Sp).OrderBy(s => ServerManager.GetItem(s.ItemInstance.ItemVNum).LevelJobMinimum).ToList();
                    break;
            }
            short i = 0;
            foreach (Inventory invtemp in templist)
            {
                Inventory temp = new GameObject.Inventory();
                Inventory temp2 = new GameObject.Inventory();
                if (invtemp.Slot != i)
                {
                    MoveItem(inventoryType, invtemp.Slot, 1, i, out temp, out temp2);

                    if (temp2 == null || temp == null)
                    {
                        return;
                    }
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(temp2.ItemInstance.ItemVNum, temp2.ItemInstance.Amount, inventoryType, temp2.Slot, temp2.ItemInstance.Rare, temp2.ItemInstance.Design, temp2.ItemInstance.Upgrade, 0));
                    Session.SendPacket(Session.Character.GenerateInventoryAdd(temp.ItemInstance.ItemVNum, temp.ItemInstance.Amount, inventoryType, temp.Slot, temp.ItemInstance.Rare, temp.ItemInstance.Design, temp.ItemInstance.Upgrade, 0));
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
        public Inventory TakeInventory(short slot, InventoryType type)
        {
            Inventory inventoryToTake = Inventory.SingleOrDefault(i => i.Slot == slot && i.Type == type);
            Inventory.Remove(inventoryToTake);
            return inventoryToTake;
        }

        private short GetFirstPlace(InventoryType type, int backPack)
        {
            Inventory result;
            for (short i = 0; i < 48 + (backPack * 12); i++)
            {
                result = Inventory.FirstOrDefault(c => c.Type == type && c.Slot.Equals(i));
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