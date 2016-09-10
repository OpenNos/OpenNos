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

        public InventoryList(Character Character)
        {
            Inventory = new List<Inventory>();
            Owner = Character;
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
                            iteminstance = new SpecialistInstance() { ItemVNum = vnum, SpLevel = 1, Amount = 1 };
                        else
                            iteminstance = new WearableInstance() { ItemVNum = vnum, Amount = 1 };
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
                Owner.Session.Client.SendPacket(Owner.Session.Character.GenerateInventoryAdd(vnum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade, 0));
            }
            else
            {
                Slot = GetFirstPlace(newItem.Item.Type, Owner.BackPack);
                if (Slot != -1)
                {
                    inv = AddToInventoryWithSlotAndType(newItem, newItem.Item.Type, Slot);
                }
            }

            inv.ItemInstance.Id = inv.Id; //set id because its a one to one
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
                }
            }

            inv.ItemInstance.Id = inv.Id; //set id because its a one to one
            return inv;
        }

        public Inventory AddToInventoryWithSlotAndType(ItemInstance iteminstance, byte type, short slot)
        {
            Logger.Debug($"Slot: {slot} Type: {type} VNUM: {iteminstance.ItemVNum}", Owner.Session.SessionId);
            Inventory inv = new Inventory() { Type = type, Slot = slot, ItemInstance = iteminstance, CharacterId = Owner.CharacterId };
            string inventoryPacket = Owner.Session.Character.GenerateInventoryAdd(iteminstance.ItemVNum, inv.ItemInstance.Amount, type, slot, iteminstance.Rare, iteminstance.Design, 0, 0);
            if (!String.IsNullOrEmpty(inventoryPacket))
            {
                Owner.Session.Client.SendPacket(inventoryPacket);
            }

            if (Inventory.Any(s => s.Slot == slot && s.Type == type))
                return null;

            inv.ItemInstance.Id = inv.Id; //set id because its a one to one
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

        public Tuple<short, byte> DeleteByInventoryItemId(Guid id)
        {
            Logger.Debug(id.ToString(), Owner.Session.SessionId);
            Tuple<short, byte> removedPlace = new Tuple<short, byte>(0, 0);
            Inventory inv = Inventory.FirstOrDefault(i => i.ItemInstance.Id.Equals(id));

            if (inv != null)
            {
                removedPlace = new Tuple<short, byte>(inv.Slot, inv.Type);
                Inventory.Remove(inv);
            }

            return removedPlace;
        }

        public void DeleteFromSlotAndType(short slot, byte type)
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
                    Inventory result = LoadInventoryBySlotAndType(i, item[k].Item.Type);
                    if (result != null && result.Type == 0)
                        place[k]--;
                    else if (result != null)
                    {
                        bool check = false;
                        foreach (ItemInstance itemins in item)
                        {
                            if (itemins.Item.Type != 0 && itemins.Amount + result.ItemInstance.Amount <= 99)
                                check = true;
                        }
                        if (!check)
                            place[k]--;
                    }
                }
            }
            for (int i = 0; i < item.Count(); i++)
            {
                if (place[i] == 0)
                    return false;
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

        public T LoadBySlotAndType<T>(short slot, byte type)
            where T : ItemInstance
        {
            return (T)Inventory.FirstOrDefault(i => i.ItemInstance.GetType().Equals(typeof(T)) && i.Slot == slot && i.Type == type)?.ItemInstance;
        }

        public Inventory LoadInventoryBySlotAndType(short slot, byte type)
        {
            return Inventory.SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));
        }

        public Inventory MoveInventory(Inventory inv, byte desttype, short destslot)
        {
            Logger.Debug($"Inventory: {inv.Id} Desttype: {desttype} Destslot: {destslot}", Owner.Session.SessionId);
            if (inv != null)
            {
                Item iteminfo = (inv.ItemInstance as ItemInstance).Item;
                Inventory invdest = LoadInventoryBySlotAndType(destslot, desttype);

                if (invdest == null && ((desttype == 6 && iteminfo.ItemType == 4) || (desttype == 7 && iteminfo.ItemType == 2) || desttype == 0))
                {
                    inv.Slot = destslot;
                    inv.Type = desttype;
                    return inv;
                }
            }
            return null;
        }

        public void MoveItem(byte type, short slot, byte amount, short destslot, out Inventory inv, out Inventory invdest)
        {
            Logger.Debug($"type: {type} slot: {slot} amount: {amount} destslot: {destslot}", Owner.Session.SessionId);
            inv = LoadInventoryBySlotAndType(slot, type);
            invdest = LoadInventoryBySlotAndType(destslot, type);
            if (inv != null && amount <= inv.ItemInstance.Amount)
            {
                if (invdest == null)
                {
                    if (inv.ItemInstance.Amount == amount)
                    {
                        inv.Slot = destslot;
                    }
                    else
                    {
                        ItemInstance itemDest = (inv.ItemInstance as ItemInstance).DeepCopy();
                        inv.ItemInstance.Amount -= amount;
                        itemDest.Amount = amount;
                        itemDest.Id = Guid.NewGuid();
                        invdest = AddToInventoryWithSlotAndType(itemDest, inv.Type, destslot);
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
                        }
                        else
                        {
                            invdest.ItemInstance.Amount += amount;
                            inv.ItemInstance.Amount -= amount;
                        }
                    }
                    else
                    {
                        invdest.Slot = slot;
                        inv.Slot = destslot;
                    }
                }
            }
            inv = LoadInventoryBySlotAndType(slot, type);
            invdest = LoadInventoryBySlotAndType(destslot, type);
        }

        public MapItem PutItem(byte type, short slot, byte amount, ref Inventory inv)
        {
            Logger.Debug($"type: {type} slot: {slot} amount: {amount}", Owner.Session.SessionId);
            Random rnd = new Random();
            Guid random = Guid.NewGuid();
            int i = 0;
            MapItem droppedItem = null;
            short MapX = (short)(rnd.Next(Owner.MapX - 1, Owner.MapX + 2));
            short MapY = (short)(rnd.Next(Owner.MapY - 1, Owner.MapY + 2));
            while (ServerManager.GetMap(Owner.MapId).IsBlockedZone(MapX, MapY) && i < 5)
            {
                MapX = (short)(rnd.Next(Owner.MapX - 1, Owner.MapX + 2));
                MapY = (short)(rnd.Next(Owner.MapY - 1, Owner.MapY + 2));
                i++;
            }
            if (i == 5)
                return null;
            if (amount > 0 && amount <= inv.ItemInstance.Amount)
            {
                droppedItem = new MapItem(MapX, MapY, false)
                {
                    ItemInstance = (inv.ItemInstance as ItemInstance).DeepCopy()
                };
                droppedItem.ItemInstance.Id = random;
                droppedItem.ItemInstance.Amount = amount;
                ServerManager.GetMap(Owner.MapId).DroppedList.Add(droppedItem.ItemInstance.TransportId, droppedItem);//todo create TransportId
                inv.ItemInstance.Amount -= amount;
            }
            return droppedItem;
        }

        public void RemoveItemAmount(int v, int amount)
        {
            Logger.Debug($"v: {v} amount: {amount}", Owner.Session.SessionId);
            for (int i = 0; i < Inventory.Where(s => s.ItemInstance.ItemVNum == v).OrderBy(s => s.Slot).Count(); i++)
            {
                Inventory inv = Inventory.Where(s => s.ItemInstance.ItemVNum == v).OrderBy(s => s.Slot).ElementAt(i);
                if (inv.ItemInstance.Amount > amount)
                {
                    inv.ItemInstance.Amount -= (byte)amount;
                    amount = 0;
                    Owner.Session.Client.SendPacket(Owner.Session.Character.GenerateInventoryAdd(inv.ItemInstance.ItemVNum, inv.ItemInstance.Amount, inv.Type, inv.Slot, inv.ItemInstance.Rare, inv.ItemInstance.Design, inv.ItemInstance.Upgrade, 0));
                }
                else
                {
                    amount -= inv.ItemInstance.Amount;
                    DeleteByInventoryItemId(inv.ItemInstance.Id);
                    Owner.Session.Client.SendPacket(Owner.Session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0, 0));
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

        private short GetFirstPlace(byte type, int backPack)
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

        #endregion
    }
}