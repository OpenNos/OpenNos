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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Inventory : ThreadSafeSortedList<Guid, ItemInstance>
    {
        #region Members

        private const short DEFAULT_BACKPACK_SIZE = 48;
        private const byte MAX_ITEM_AMOUNT = 99;
        private Random _random;

        #endregion

        #region Instantiation

        public Inventory(Character Character)
        {
            Owner = Character;
            _random = new Random();
        }

        #endregion

        #region Properties

        private Character Owner { get; }

        #endregion

        #region Methods

        public static ItemInstance InstantiateItemInstance(short vnum, long ownerId, byte amount = 1)
        {
            ItemInstance newItem = new ItemInstance { ItemVNum = vnum, Amount = amount, CharacterId = ownerId };
            if (newItem.Item != null)
            {
                switch (newItem.Item.Type)
                {
                    case InventoryType.Equipment:
                        newItem = newItem.Item.ItemType == ItemType.Specialist ? new SpecialistInstance
                        {
                            ItemVNum = vnum,
                            SpLevel = 1,
                            Amount = amount
                        } : newItem.Item.ItemType == ItemType.Box ? new BoxInstance
                        {
                            ItemVNum = vnum,
                            Amount = amount
                        } :
                        new WearableInstance
                        {
                            ItemVNum = vnum,
                            Amount = amount
                        };
                        break;
                }
            }

            // set default itemType
            if (newItem.Item != null)
            {
                newItem.Type = newItem.Item.Type;
            }

            return newItem;
        }

        public ItemInstance AddNewToInventory(short vnum, byte amount = 1, InventoryType? type = null)
        {
            if (Owner != null)
            {
                Logger.Debug(vnum.ToString(), Owner.Session.SessionId);
                ItemInstance newItem = InstantiateItemInstance(vnum, Owner.CharacterId, amount);
                return AddToInventory(newItem, type);
            }
            return null;
        }

        public ItemInstance AddToInventory(ItemInstance newItem, InventoryType? type = null)
        {
            if (Owner != null)
            {
                Logger.Debug(newItem.ItemVNum.ToString(), Owner.Session.SessionId);
                ItemInstance inv = null;

                // override type if necessary
                if (type.HasValue)
                {
                    newItem.Type = type.Value;
                }

                // check if item can be stapled
                if (newItem.Type != InventoryType.Equipment && newItem.Type != InventoryType.Wear)
                {
                    IEnumerable<ItemInstance> slotfree = LoadBySlotAllowed(newItem.ItemVNum, newItem.Amount);
                    inv = GetFreeSlot(slotfree.Where(s => s.Type == newItem.Type));
                }

                if (inv != null)
                {
                    // add item amount
                    inv.Amount = (byte)(newItem.Amount + inv.Amount);
                }
                else
                {
                    // create new item
                    short? freeSlot = newItem.Type == InventoryType.Wear ? (LoadBySlotAndType((short)newItem.Item.EquipmentSlot, InventoryType.Wear) == null
                                                                        ? (short?)newItem.Item.EquipmentSlot
                                                                        : null)
                                                                      : GetFreeSlot(newItem.Type, Owner.HaveBackpack() ? 1 : 0);
                    if (freeSlot.HasValue)
                    {
                        inv = AddToInventoryWithSlotAndType(newItem, newItem.Type, freeSlot.Value);
                    }
                }

                return inv;
            }
            else return null;
        }

        /// <summary>
        /// Add iteminstance to inventory with specified slot and type, iteminstance will be overridden.
        /// </summary>
        /// <param name="itemInstance"></param>
        /// <param name="type"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public ItemInstance AddToInventoryWithSlotAndType(ItemInstance itemInstance, InventoryType type, short slot)
        {
            if (Owner != null)
            {
                Logger.Debug($"Slot: {slot} Type: {type} VNUM: {itemInstance.ItemVNum}", Owner.Session.SessionId);
                itemInstance.Slot = slot;
                itemInstance.Type = type;
                itemInstance.CharacterId = Owner.Session.Character.CharacterId;

                if (ContainsKey(itemInstance.Id))
                {
                    Logger.Error(new InvalidOperationException("Cannot add the same ItemInstance twice to inventory."));
                    return null;
                }

                string inventoryPacket = Owner.Session.Character.GenerateInventoryAdd(itemInstance.ItemVNum, itemInstance.Amount, type, slot, itemInstance.Rare, itemInstance.Design, itemInstance.Upgrade, 0);
                if (!string.IsNullOrEmpty(inventoryPacket))
                {
                    Owner.Session.SendPacket(inventoryPacket);
                }

                if (GetAllItems().Any(s => s.Slot == slot && s.Type == type))
                {
                    return null;
                }
                CheckItemInstanceType(itemInstance);
                this[itemInstance.Id] = itemInstance;
                return itemInstance;
            }
            else return null;
        }

        public bool CanAddItem(short itemVnum)
        {
            InventoryType type = ServerManager.GetItem(itemVnum).Type;
            return CanAddItem(type);
        }

        private bool CanAddItem(InventoryType type)
        {
            return Owner != null && GetFreeSlot(type, Owner.HaveBackpack() ? 1 : 0).HasValue;
        }

        public int CountItem(int itemVNum)
        {
            return GetAllItems().Where(s => s.ItemVNum == itemVNum).Sum(i => i.Amount);
        }
        public int CountItemInAnInventory(InventoryType inv)
        {
            return GetAllItems().Count(s => s.Type == inv);
        }
        public Tuple<short, InventoryType> DeleteById(Guid id)
        {
            if (Owner != null)
            {
                Logger.Debug(id.ToString(), Owner.Session.SessionId);
                Tuple<short, InventoryType> removedPlace;
                ItemInstance inv = this[id];

                if (inv != null)
                {
                    removedPlace = new Tuple<short, InventoryType>(inv.Slot, inv.Type);
                    Remove(inv.Id);
                }
                else
                {
                    Logger.Error(new InvalidOperationException("Expected item wasn't deleted, Type or Slot did not match!"));
                    return null;
                }

                return removedPlace;
            }
            return null;
        }

        public void DeleteFromSlotAndType(short slot, InventoryType type)
        {
            if (Owner != null)
            {
                Logger.Debug($"Slot: {slot} Type: {type}", Owner.Session.SessionId);
                ItemInstance inv = GetAllItems().FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));

                if (inv != null)
                {
                    Remove(inv.Id);
                }
                else
                {
                    Logger.Error(new InvalidOperationException("Expected item wasn't deleted, Type or Slot did not match!"));
                }
            }
        }

        private void GenerateClearInventory(InventoryType type)
        {
            if (Owner != null)
            {
                for (short i = 0; i < DEFAULT_BACKPACK_SIZE; i++)
                {
                    Owner.Session.SendPacket(Owner.GenerateInventoryAdd(-1, 0, type, i, 0, 0, 0, 0));
                }
            }
        }

        private ItemInstance GetFreeSlot(IEnumerable<ItemInstance> slotfree)
        {
            List<Guid> inventoryitemids = new List<Guid>();
            foreach (ItemInstance itemfree in slotfree)
            {
                inventoryitemids.Add(itemfree.Id);
            }
            return GetAllItems().Where(i => inventoryitemids.Contains(i.Id)).OrderBy(i => i.Slot).FirstOrDefault();
        }

        public bool GetFreeSlotAmount(List<ItemInstance> itemInstances, int backPack)
        {
            short[] place = new short[itemInstances.Count];
            for (byte k = 0; k < itemInstances.Count; k++)
            {
                place[k] = (byte)(DEFAULT_BACKPACK_SIZE + backPack * 12);
                for (short i = 0; i < DEFAULT_BACKPACK_SIZE + backPack * 12; i++)
                {
                    ItemInstance loadedItemInstance = LoadBySlotAndType(i, itemInstances[k].Item.Type);
                    if (loadedItemInstance != null && loadedItemInstance.Type == 0)
                    {
                        place[k]--;
                    }
                    else if (loadedItemInstance != null)
                    {
                        bool check = false;
                        foreach (ItemInstance itemInstance in itemInstances)
                        {
                            if (itemInstance.Item.Type != 0 && itemInstance.Amount + loadedItemInstance.Amount <= MAX_ITEM_AMOUNT)
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
            for (int i = 0; i < itemInstances.Count; i++)
            {
                if (place[i] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public ItemInstance GetItemInstanceById(Guid id)
        {
            return this[id];
        }

        public ItemInstance AddIntoBazaarInventory(InventoryType inventory, byte slot, byte amount)
        {
            ItemInstance inv = LoadBySlotAndType(slot, inventory);
            if (inv == null || amount > inv.Amount)
                return null;

            ItemInstance invcopy = inv.DeepCopy();
            invcopy.Id = Guid.NewGuid();

            if (inv.Item.Type == InventoryType.Equipment)
            {
                for (short i = 0; i < 255; i++)
                {
                    if (LoadBySlotAndType<ItemInstance>(i, InventoryType.Bazaar) == null)
                    {
                        invcopy.Type = InventoryType.Bazaar;
                        invcopy.Slot = i;
                        invcopy.CharacterId = 1;
                        DeleteFromSlotAndType(inv.Slot, inv.Type);
                        PutItem(invcopy);
                        break;
                    }
                }
                Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(-1, 0, inventory, slot, 0, 0, 0, 0));
                return invcopy;
            }
            if (amount >= inv.Amount)
            {
                for (short i = 0; i < 255; i++)
                {
                    if (LoadBySlotAndType<ItemInstance>(i, InventoryType.Bazaar) == null)
                    {
                        invcopy.Type = InventoryType.Bazaar;
                        invcopy.Slot = i;
                        invcopy.CharacterId = 1;
                        DeleteFromSlotAndType(inv.Slot, inv.Type);
                        PutItem(invcopy);
                        break;
                    }
                }
                Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(-1, 0, inventory, slot, 0, 0, 0, 0));
                return invcopy;
            }

            invcopy.Amount = amount;
            inv.Amount -= amount;

            for (short i = 0; i < 255; i++)
            {
                if (LoadBySlotAndType<ItemInstance>(i, InventoryType.Bazaar) == null)
                {
                    invcopy.Type = InventoryType.Bazaar;
                    invcopy.Slot = i;
                    invcopy.CharacterId = 1;
                    PutItem(invcopy);
                    break;
                }
            }

            Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, 0));
            return invcopy;
        }

        public T LoadByItemInstance<T>(Guid id)
                    where T : ItemInstance
        {
            return (T)this[id];
        }

        private IEnumerable<ItemInstance> LoadBySlotAllowed(short itemVNum, int amount)
        {
            return GetAllItems().Where(i => i.ItemVNum.Equals(itemVNum) && i.Amount + amount <= MAX_ITEM_AMOUNT);
        }

        public T LoadBySlotAndType<T>(short slot, InventoryType type)
                    where T : ItemInstance
        {
            T retItem = null;
            try
            {
                retItem = (T)GetAllItems().SingleOrDefault(i => i != null && i.GetType().Equals(typeof(T)) && i.Slot == slot && i.Type == type);
            }
            catch (InvalidOperationException ioEx)
            {
                Logger.Error(ioEx);
                bool isFirstItem = true;
                foreach (ItemInstance item in GetAllItems().Where(i => i != null && i.GetType().Equals(typeof(T)) && i.Slot == slot && i.Type == type))
                {
                    if (isFirstItem)
                    {
                        retItem = (T)item;
                        isFirstItem = false;
                        continue;
                    }
                    ItemInstance iteminstance = GetAllItems().FirstOrDefault(i => i != null && i.GetType().Equals(typeof(T)) && i.Slot == slot && i.Type == type);
                    if (iteminstance != null)
                    {
                        Remove(iteminstance.Id);
                    }
                }
            }
            return retItem;
        }

        public ItemInstance LoadBySlotAndType(short slot, InventoryType type)
        {
            ItemInstance retItem = null;
            try
            {
                retItem = GetAllItems().SingleOrDefault(i => i != null && i.Slot.Equals(slot) && i.Type.Equals(type));
            }
            catch (InvalidOperationException ioEx)
            {
                Logger.Error(ioEx);
                bool isFirstItem = true;
                foreach (ItemInstance item in GetAllItems().Where(i => i != null && i.Slot.Equals(slot) && i.Type.Equals(type)))
                {
                    if (isFirstItem)
                    {
                        retItem = item;
                        isFirstItem = false;
                        continue;
                    }
                    Remove(GetAllItems().First(i => i != null && i.Slot.Equals(slot) && i.Type.Equals(type)).Id);
                }
            }
            return retItem;
        }

        /// <summary> Moves one item from one Inventory to another. Example: Equipment &lt;-&gt; Wear,
        /// Equipment &lt;-&gt; Costume, Equipment &lt;-&gt; Specialist </summary> <param
        /// name="sourceSlot"></param> <param name="sourceType"></param> <param
        /// name="targetType"></param> <param name="targetSlot"></param> <param
        /// name="wear"></param>
        public ItemInstance MoveInInventory(short sourceSlot, InventoryType sourceType, InventoryType targetType, short? targetSlot = null, bool wear = true)
        {
            var sourceInstance = LoadBySlotAndType(sourceSlot, sourceType);

            if (sourceInstance == null && wear)
            {
                Logger.Error(new InvalidOperationException("SourceInstance to move does not exist."));
                return null;
            }
            if (Owner != null && sourceInstance != null)
            {
                if (targetSlot.HasValue)
                {
                    if (wear)
                    {
                        // swap
                        var targetInstance = LoadBySlotAndType(targetSlot.Value, targetType);

                        sourceInstance.Slot = targetSlot.Value;
                        sourceInstance.Type = targetType;

                        targetInstance.Slot = sourceSlot;
                        targetInstance.Type = sourceType;
                    }
                    else
                    {
                        // move source to target
                        short? freeTargetSlot = GetFreeSlot(targetType, Owner.HaveBackpack() ? 1 : 0);
                        if (freeTargetSlot.HasValue)
                        {
                            sourceInstance.Slot = freeTargetSlot.Value;
                            sourceInstance.Type = targetType;
                        }
                    }

                    return sourceInstance;
                }

                // check for free target slot
                short? nextFreeSlot = targetType == InventoryType.Wear ? (LoadBySlotAndType((short)sourceInstance.Item.EquipmentSlot, InventoryType.Wear) == null
                        ? (short)sourceInstance.Item.EquipmentSlot
                        : (short)-1)
                    : GetFreeSlot(targetType, Owner.HaveBackpack() ? 1 : 0);
                if (nextFreeSlot.HasValue)
                {
                    sourceInstance.Type = targetType;
                    sourceInstance.Slot = nextFreeSlot.Value;
                }
                else
                {
                    return null;
                }
            }
            else return null;
            return sourceInstance;
        }

        public void MoveItem(InventoryType type, short sourceSlot, byte amount, short destinationSlot, out ItemInstance sourceInventory, out ItemInstance destinationInventory)
        {
            Logger.Debug($"type: {type} sourceSlot: {sourceSlot} amount: {amount} destinationSlot: {destinationSlot}", Owner.Session.SessionId);

            // load source and destination slots
            sourceInventory = LoadBySlotAndType(sourceSlot, type);
            destinationInventory = LoadBySlotAndType(destinationSlot, type);
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
                        AddToInventoryWithSlotAndType(itemDest, sourceInventory.Type, destinationSlot);
                    }
                }
                else
                {
                    if (destinationInventory.ItemVNum == sourceInventory.ItemVNum && sourceInventory.Type != 0)
                    {
                        if (destinationInventory.Amount + amount > MAX_ITEM_AMOUNT)
                        {
                            int saveItemCount = destinationInventory.Amount;
                            destinationInventory.Amount = MAX_ITEM_AMOUNT;
                            sourceInventory.Amount = (byte)(saveItemCount + sourceInventory.Amount - MAX_ITEM_AMOUNT);
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
                        destinationInventory = TakeItem(destinationInventory.Slot, destinationInventory.Type);
                        destinationInventory.Slot = sourceSlot;
                        sourceInventory = TakeItem(sourceInventory.Slot, sourceInventory.Type);
                        sourceInventory.Slot = destinationSlot;
                        PutItem(destinationInventory);
                        PutItem(sourceInventory);
                    }
                }
            }
            sourceInventory = LoadBySlotAndType(sourceSlot, type);
            destinationInventory = LoadBySlotAndType(destinationSlot, type);
        }

        /// <summary>
        /// Puts a Single ItemInstance to the Inventory
        /// </summary>
        /// <param name="itemInstance"></param>
        private void PutItem(ItemInstance itemInstance)
        {
            this[itemInstance.Id] = itemInstance;
        }

        public void RemoveItemAmount(int vnum, int amount = 1)
        {
            if (Owner != null)
            {
                Logger.Debug($"vnum: {vnum} amount: {amount}", Owner.Session.SessionId);
                int remainingAmount = amount;

                foreach (ItemInstance inventory in GetAllItems().Where(s => s.ItemVNum == vnum).OrderBy(i => i.Slot))
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
                            DeleteById(inventory.Id);
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
        }

        public void RemoveItemAmountFromInventory(byte amount, Guid id)
        {
            if (Owner != null)
            {
                Logger.Debug($"InventoryId: {id} amount: {amount}", Owner.Session.SessionId);
                ItemInstance inv = GetAllItems().FirstOrDefault(i => i.Id.Equals(id));

                if (inv != null)
                {
                    inv.Amount -= amount;
                    if (inv.Amount <= 0)
                    {
                        Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(-1, 0, inv.Type, inv.Slot, 0, 0, 0, 0));
                        Remove(inv.Id);
                        return;
                    }
                    Owner.Session.SendPacket(Owner.Session.Character.GenerateInventoryAdd(inv.ItemVNum, inv.Amount, inv.Type, inv.Slot, inv.Rare, inv.Design, inv.Upgrade, inv.Type == InventoryType.Specialist ? ((SpecialistInstance)inv).SpStoneUpgrade : (byte)0));
                }
            }
        }

        public void Reorder(ClientSession Session, InventoryType inventoryType)
        {
            List<ItemInstance> itemsByInventoryType = new List<ItemInstance>();
            switch (inventoryType)
            {
                case InventoryType.Costume:
                    itemsByInventoryType = GetAllItems().Where(s => s.Type == InventoryType.Costume).OrderBy(s => s.ItemVNum).ToList();
                    break;

                case InventoryType.Specialist:
                    itemsByInventoryType = GetAllItems().Where(s => s.Type == InventoryType.Specialist).OrderBy(s => s.Item.LevelJobMinimum).ToList();
                    break;
            }

            short i = 0;

            // TODO: OPTIMIZE WITH JUST REMOVE SLOT, avoid sending unneccessary empty slots
            GenerateClearInventory(inventoryType);

            foreach (ItemInstance item in itemsByInventoryType)
            {
                // remove item from inventory
                Remove(item.Id);

                // readd item to inventory
                item.Slot = i;
                Session.SendPacket(Session.Character.GenerateInventoryAdd(item.ItemVNum, item.Amount, inventoryType, item.Slot, item.Rare, item.Design, item.Upgrade, 0));
                this[item.Id] = item;

                // increment slot
                i++;
            }
        }

        /// <summary>
        /// Takes a Single Inventory including ItemInstance from the List and removes it.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private ItemInstance TakeItem(short slot, InventoryType type)
        {
            ItemInstance itemInstance = GetAllItems().SingleOrDefault(i => i.Slot == slot && i.Type == type);
            if (itemInstance != null)
            {
                Remove(itemInstance.Id);
                return itemInstance;
            }
            return null;
        }

        private static void CheckItemInstanceType(ItemInstanceDTO itemInstance)
        {
            if (itemInstance != null)
            {
                if (itemInstance.Type == InventoryType.Specialist && !(itemInstance is SpecialistInstance))
                {
                    Logger.Error(new Exception("Cannot add an item of type Specialist without beeing a SpecialistInstance."));
                    return;
                }

                if ((itemInstance.Type == InventoryType.Equipment || itemInstance.Type == InventoryType.Wear) && !(itemInstance is WearableInstance))
                {
                    Logger.Error(new Exception("Cannot add an item of type Equipment or Wear without beeing a WearableInstance."));
                }
            }
        }

        private short? GetFreeSlot(InventoryType type, int backPack)
        {
            IEnumerable<int> itemInstanceSlotsByType = GetAllItems().Where(i => i.Type == type).OrderBy(i => i.Slot).Select(i => (int)i.Slot);
            IEnumerable<int> instanceSlotsByType = itemInstanceSlotsByType as int[] ?? itemInstanceSlotsByType.ToArray();
            int nextFreeSlot = instanceSlotsByType.Any()
                                ? Enumerable.Range(0, DEFAULT_BACKPACK_SIZE + backPack * 12 + 1).Except(instanceSlotsByType).FirstOrDefault()
                                : 0;
            return (short?)nextFreeSlot < DEFAULT_BACKPACK_SIZE + backPack * 12 ? (short?)nextFreeSlot : null;
        }

        #endregion
    }
}