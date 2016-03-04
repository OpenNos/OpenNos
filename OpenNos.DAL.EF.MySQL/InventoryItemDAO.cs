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
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class InventoryItemDAO : IInventoryItemDAO
    {
        #region Methods

        public DeleteResult DeleteById(long ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                InventoryItem item = context.inventoryitem.SingleOrDefault(i => i.InventoryItemId.Equals(ItemId));

                if (item != null)
                {
                    Inventory inv = context.inventory.FirstOrDefault(s => s.inventoryitem.InventoryItemId == item.InventoryItemId);
                    if (inv != null)
                    {
                        context.inventory.Remove(inv);
                    }
                    context.inventoryitem.Remove(item);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public SaveResult InsertOrUpdate(ref InventoryItemDTO item)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long InventoryItemId = item.InventoryItemId;
                    InventoryItem entity = context.inventoryitem.SingleOrDefault(c => c.InventoryItemId.Equals(InventoryItemId));

                    if (entity == null) //new entity
                    {
                        item = Insert(item, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        item = Update(entity, item, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_INVENTORY_ERROR"), item.InventoryItemId, e.Message);
                return SaveResult.Error;
            }
        }

        public InventoryItemDTO LoadById(long ItemId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<InventoryItemDTO>(context.inventoryitem.SingleOrDefault(i => i.InventoryItemId.Equals(ItemId)));
            }
        }

        private InventoryItemDTO Insert(InventoryItemDTO inventoryitem, OpenNosContainer context)
        {
            InventoryItem entity = new InventoryItem()
            {
                Ammo = inventoryitem.Ammo,
                FireElement = inventoryitem.FireElement,
                IsFixed = inventoryitem.IsFixed,
                Design = inventoryitem.Design,
                Concentrate = inventoryitem.Concentrate,
                Amount = inventoryitem.Amount,
                CriticalLuckRate = inventoryitem.CriticalLuckRate,
                CriticalRate = inventoryitem.CriticalRate,
                DamageMaximum = inventoryitem.DamageMaximum,
                DamageMinimum = inventoryitem.DamageMinimum,
                DarkElement = inventoryitem.DarkElement,
                DefenceDodge = inventoryitem.DefenceDodge,
                DistanceDefence = inventoryitem.DistanceDefence,
                DistanceDefenceDodge = inventoryitem.DistanceDefenceDodge,
                ElementRate = inventoryitem.ElementRate,
                HitRate = inventoryitem.HitRate,
                ItemVNum = inventoryitem.ItemVNum,
                LightElement = inventoryitem.LightElement,
                MagicDefence = inventoryitem.MagicDefence,
                Rare = inventoryitem.Rare,
                SlDefence = inventoryitem.SlDefence,
                SlElement = inventoryitem.SlElement,
                SlDamage = inventoryitem.SlDamage,
                SlHP = inventoryitem.SlHP,
                SpLevel = inventoryitem.SpLevel,
                SpXp = inventoryitem.SpXp,
                Upgrade = inventoryitem.Upgrade,
                WaterElement = inventoryitem.WaterElement,

                CloseDefence = inventoryitem.CloseDefence,
            };

            context.inventoryitem.Add(entity);
            try
            {
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(e.Message);
            }

            return Mapper.Map<InventoryItemDTO>(entity);
        }

        private InventoryItemDTO Update(InventoryItem entity, InventoryItemDTO inventoryitem, OpenNosContainer context)
        {
            using (context)
            {
                var result = context.inventoryitem.SingleOrDefault(c => c.InventoryItemId.Equals(inventoryitem.InventoryItemId));
                if (result != null)
                {
                    result = Mapper.Map<InventoryItemDTO, InventoryItem>(inventoryitem, entity);
                    context.SaveChanges();
                }
            }

            return Mapper.Map<InventoryItemDTO>(entity);
        }

        #endregion
    }
}