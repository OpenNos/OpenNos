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
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenNos.DAL.EF
{
    public class ItemInstanceDAO : SynchronizableBaseDAO<ItemInstance, ItemInstanceDTO>, IItemInstanceDAO
    {
        #region Members

        private Type _baseType;
        private IDictionary<Type, Type> itemInstanceMappings = new Dictionary<Type, Type>();

        #endregion

        #region Methods

        public DeleteResult DeleteFromSlotAndType(long characterId, short slot, InventoryType type)
        {
            try
            {
                ItemInstanceDTO dto = LoadBySlotAndType(characterId, slot, type);
                return Delete(dto.Id);
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
            });

            _mapper = config.CreateMapper();
        }

        public IEnumerable<ItemInstanceDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (var itemInstance in context.ItemInstance.Where(i => i.CharacterId.Equals(characterId)))
                {
                    yield return _mapper.Map<ItemInstanceDTO>(itemInstance);
                }
            }
        }

        public ItemInstanceDTO LoadBySlotAndType(long characterId, short slot, InventoryType type)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    byte inventoryType = (byte)type;
                    byte equipmentType = (byte)slot;
                    ItemInstance entity = context.ItemInstance.FirstOrDefault(i => i.CharacterId == characterId && i.Slot == equipmentType && i.Type == inventoryType);
                    return _mapper.Map<ItemInstanceDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ItemInstanceDTO> LoadByType(long characterId, InventoryType type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                byte inventoryType = (byte)type;
                foreach (var itemInstance in context.ItemInstance.Where(i => i.CharacterId == characterId && i.Type == inventoryType))
                {
                    yield return _mapper.Map<ItemInstanceDTO>(itemInstance);
                }
            }
        }

        public IList<Guid> LoadSlotAndTypeByCharacterId(long characterId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return context.ItemInstance.Where(i => i.CharacterId.Equals(characterId)).Select(i => i.Id).ToList();
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

        protected override ItemInstanceDTO InsertOrUpdate(OpenNosContext context, ItemInstanceDTO itemInstance)
        {
            try
            {
                var entity = context.ItemInstance.FirstOrDefault(c => c.Id == itemInstance.Id);

                if (entity == null)
                {
                    itemInstance = Insert(itemInstance, context);
                }
                else
                {
                    itemInstance = Update(entity, itemInstance, context);
                }
                return itemInstance;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        protected override ItemInstance MapEntity(ItemInstanceDTO dto)
        {
            try
            {
                var entity = _mapper.Map<ItemInstance>(dto);
                KeyValuePair<Type, Type> targetMapping = itemInstanceMappings.FirstOrDefault(k => k.Key.Equals(dto.GetType()));
                if (targetMapping.Key != null)
                {
                    entity = _mapper.Map(dto, targetMapping.Key, targetMapping.Value) as ItemInstance;
                }

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