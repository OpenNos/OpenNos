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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenNos.DAL.EF.MySQL
{
    public class InventoryDAO : IInventoryDAO
    {
        #region Members

        private Type _baseType;
        private IMapper _mapper;
        private IDictionary<Type, Type> itemInstanceMappings = new Dictionary<Type, Type>();

        #endregion

        #region Methods

        public DeleteResult DeleteFromSlotAndType(long characterId, short slot, byte type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Inventory inv = context.Inventory.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type) && i.CharacterId.Equals(characterId));
                ItemInstance invItem = context.ItemInstance.FirstOrDefault(i => i.Inventory.InventoryId == inv.InventoryId);
                if (inv != null)
                {
                    context.Inventory.Remove(inv);
                    context.ItemInstance.Remove(invItem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public ItemInstanceDTO Id(long inventoryId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                var itemInstance = context.ItemInstance.Include(nameof(Inventory)).FirstOrDefault(i => i.Inventory.InventoryId.Equals(inventoryId));
                return _mapper.Map<ItemInstanceDTO>(itemInstance);
            }
        }

        public void InitializeMapper(Type baseType)
        {
            _baseType = baseType;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(baseType, typeof(ItemInstance))
                .ForMember("Item", opts => opts.Ignore());

                Type itemInstanceType = typeof(ItemInstance);
                foreach (KeyValuePair<Type, Type> entry in itemInstanceMappings)
                {
                    //GameObject -> Entity
                    cfg.CreateMap(entry.Key, entry.Value).ForMember("Item", opts => opts.Ignore())
                            .IncludeBase(baseType, typeof(ItemInstance));

                    //Entity -> GameObject
                    cfg.CreateMap(entry.Value, entry.Key)
                            .IncludeBase(typeof(ItemInstance), baseType);

                    Type retrieveDTOType = Type.GetType($"OpenNos.Data.{entry.Key.Name}DTO, OpenNos.Data");
                    //Entity -> DTO
                    cfg.CreateMap(entry.Value, retrieveDTOType)
                            .IncludeBase(typeof(ItemInstance), typeof(ItemInstanceDTO));
                }

                //Inventory Mappings
                cfg.CreateMap<InventoryDTO, Inventory>();
                cfg.CreateMap<Inventory, InventoryDTO>();
            });

            _mapper = config.CreateMapper();
        }

        public SaveResult InsertOrUpdate(ref InventoryDTO inventory)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long InventoryId = inventory.InventoryId;
                    byte Type = inventory.Type;
                    short Slot = inventory.Slot;
                    long CharacterId = inventory.CharacterId;
                    Inventory entity = context.Inventory.FirstOrDefault(c => c.InventoryId == InventoryId);
                    if (entity == null) //new entity
                    {
                        Inventory delete = context.Inventory.FirstOrDefault(s => s.CharacterId == CharacterId && s.Slot == Slot && s.Type == Type);
                        if (delete != null)
                        {
                            ItemInstance deleteItem = context.ItemInstance.FirstOrDefault(s => s.Inventory.InventoryId == delete.InventoryId);
                            context.ItemInstance.Remove(deleteItem);
                            context.Inventory.Remove(delete);
                        }
                        inventory = Insert(inventory, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        entity.ItemInstance = context.ItemInstance.FirstOrDefault(c => c.Inventory.InventoryId == entity.InventoryId);
                        inventory = Update(entity, inventory, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message);
                return SaveResult.Error;
            }
        }

        public IEnumerable<InventoryDTO> LoadByCharacterId(long characterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Inventory inventory in context.Inventory.Where(i => i.CharacterId.Equals(characterId)))
                {
                    yield return _mapper.Map<InventoryDTO>(inventory);
                }
            }
        }

        public InventoryDTO LoadById(long inventoryId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return _mapper.Map<InventoryDTO>(context.Inventory.FirstOrDefault(i => i.InventoryId.Equals(inventoryId)));
            }
        }

        public InventoryDTO LoadBySlotAndType(long characterId, short slot, byte type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return _mapper.Map<InventoryDTO>(context.Inventory.FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type) && i.CharacterId.Equals(characterId)));
            }
        }

        public IEnumerable<InventoryDTO> LoadByType(long characterId, byte type)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Inventory Inventoryobject in context.Inventory.Where(i => i.Type.Equals(type) && i.CharacterId.Equals(characterId)))
                {
                    yield return _mapper.Map<InventoryDTO>(Inventoryobject);
                }
            }
        }

        public void RegisterMapping(Type gameObjectType)
        {
            Type targetType = Assembly.GetExecutingAssembly().GetTypes().SingleOrDefault(t => t.Name.Equals(gameObjectType.Name));
            Type itemInstanceType = typeof(ItemInstance);
            itemInstanceMappings.Add(gameObjectType, targetType);
        }

        private InventoryDTO Insert(InventoryDTO inventory, OpenNosContext context)
        {
            Inventory entity = Mapper.Map<Inventory>(inventory);
            KeyValuePair<Type, Type> targetMapping = itemInstanceMappings.FirstOrDefault(k => k.Key.Equals(inventory.ItemInstance.GetType()));
            if (targetMapping.Key != null)
            {
                entity.ItemInstance = _mapper.Map(inventory.ItemInstance, targetMapping.Key, targetMapping.Value) as ItemInstance;
            }
            else
            {
                entity.ItemInstance = _mapper.Map(inventory.ItemInstance, _baseType, typeof(ItemInstance)) as ItemInstance;
            }
            context.Inventory.Add(entity);
            context.SaveChanges();
            return _mapper.Map<InventoryDTO>(entity);
        }

        private InventoryDTO Update(Inventory entity, InventoryDTO inventory, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(inventory, entity);
                context.SaveChanges();
            }

            return _mapper.Map<InventoryDTO>(entity);
        }

        #endregion
    }
}