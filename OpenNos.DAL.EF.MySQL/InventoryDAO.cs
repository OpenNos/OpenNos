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
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenNos.DAL.EF.MySQL
{
    public class InventoryDAO : SynchronizableBaseDAO<Inventory, InventoryDTO>, IInventoryDAO
    {
        #region Members

        private Type _baseType;
        private IDictionary<Type, Type> itemInstanceMappings = new Dictionary<Type, Type>();

        #endregion

        #region Methods

        public override DeleteResult Delete(Guid id)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Inventory entity = context.Set<Inventory>().FirstOrDefault(i => i.Id.Equals(id));
                    ItemInstance instance = context.Set<ItemInstance>().FirstOrDefault(i => i.Id.Equals(id));
                    if (entity != null)
                    {
                        context.Set<Inventory>().Remove(entity);
                        context.Set<ItemInstance>().Remove(instance);
                        context.SaveChanges();
                    }
                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public DeleteResult DeleteFromSlotAndType(long characterId, short slot, InventoryType type)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Inventory inv = context.Inventory.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type) && i.CharacterId.Equals(characterId));
                    ItemInstance invItem = context.ItemInstance.FirstOrDefault(i => i.Inventory.Id == inv.Id);
                    if (inv != null)
                    {
                        context.Inventory.Remove(inv);
                        context.ItemInstance.Remove(invItem);
                        context.SaveChanges();
                    }
                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public void InitializeMapper(Type baseType)
        {
            _baseType = baseType;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(baseType, typeof(ItemInstance))
                    .ForMember("Item", opts => opts.Ignore());

                cfg.CreateMap(typeof(ItemInstance), typeof(ItemInstanceDTO)).As(baseType);

                Type itemInstanceType = typeof(ItemInstance);
                foreach (KeyValuePair<Type, Type> entry in itemInstanceMappings)
                {
                    // GameObject -> Entity
                    cfg.CreateMap(entry.Key, entry.Value).ForMember("Item", opts => opts.Ignore())
                                    .IncludeBase(baseType, typeof(ItemInstance));

                    // Entity -> GameObject
                    cfg.CreateMap(entry.Value, entry.Key)
                                    .IncludeBase(typeof(ItemInstance), baseType);

                    Type retrieveDTOType = Type.GetType($"OpenNos.Data.{entry.Key.Name}DTO, OpenNos.Data");

                    // Entity -> DTO
                    cfg.CreateMap(entry.Value, typeof(ItemInstanceDTO)).As(entry.Key);
                }

                // Inventory Mappings
                cfg.CreateMap<InventoryDTO, Inventory>();
                cfg.CreateMap<Inventory, InventoryDTO>();
            });

            _mapper = config.CreateMapper();
        }

        public IEnumerable<InventoryDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Inventory inventory in context.Inventory.Include(nameof(ItemInstance)).Where(i => i.CharacterId.Equals(characterId)))
                {
                    yield return _mapper.Map<InventoryDTO>(inventory);
                }
            }
        }

        public InventoryDTO LoadById(long inventoryId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<InventoryDTO>(context.Inventory.Include(nameof(ItemInstance)).FirstOrDefault(i => i.Id.Equals(inventoryId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public InventoryDTO LoadBySlotAndType(long characterId, short slot, InventoryType type)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<InventoryDTO>(context.Inventory.Include(nameof(ItemInstance)).FirstOrDefault(i => i.Slot.Equals(slot) && i.Type == type && i.CharacterId.Equals(characterId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<InventoryDTO> LoadByType(long characterId, InventoryType type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Inventory inventoryEntry in context.Inventory.Include(nameof(ItemInstance)).Where(i => i.Type == type && i.CharacterId.Equals(characterId)))
                {
                    yield return _mapper.Map<InventoryDTO>(inventoryEntry);
                }
            }
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return context.Inventory.Where(i => i.CharacterId.Equals(characterId)).Select(c => c.Id).ToList();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public void RegisterMapping(Type gameObjectType)
        {
            try
            {
                Type targetType = Assembly.GetExecutingAssembly().GetTypes().SingleOrDefault(t => t.Name.Equals(gameObjectType.Name));
                Type itemInstanceType = typeof(ItemInstance);
                itemInstanceMappings.Add(gameObjectType, targetType);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        protected override InventoryDTO InsertOrUpdate(OpenNosContext context, InventoryDTO inventory)
        {
            try
            {
                Guid primaryKey = inventory.Id;
                InventoryType Type = inventory.Type;
                short Slot = inventory.Slot;
                long CharacterId = inventory.CharacterId;
                Inventory entity = context.Inventory.FirstOrDefault(c => c.Id == primaryKey);

                if (entity == null)
                {
                    Inventory delete = context.Inventory.FirstOrDefault(s => s.CharacterId == CharacterId && s.Slot == Slot && s.Type == Type);
                    if (delete != null)
                    {
                        ItemInstance deleteItem = context.ItemInstance.FirstOrDefault(s => s.Inventory.Id == delete.Id);
                        context.ItemInstance.Remove(deleteItem);
                        context.Inventory.Remove(delete);
                        context.SaveChanges();
                    }
                    inventory = Insert(inventory, context);
                }
                else
                {
                    entity.ItemInstance = context.ItemInstance.FirstOrDefault(c => c.Inventory.Id == entity.Id);
                    inventory = Update(entity, inventory, context);
                }
                return inventory;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        protected override Inventory MapEntity(InventoryDTO dto)
        {
            try
            {
                Inventory entity = _mapper.Map<Inventory>(dto);
                KeyValuePair<Type, Type> targetMapping = itemInstanceMappings.FirstOrDefault(k => k.Key.Equals(dto.ItemInstance.GetType()));
                if (targetMapping.Key != null)
                {
                    entity.ItemInstance = _mapper.Map(dto.ItemInstance, targetMapping.Key, targetMapping.Value) as ItemInstance;
                }

                // stupid references -> maybe using mapper to ignore property?
                entity.ItemInstance.Item = null;

                return entity;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        #endregion
    }
}