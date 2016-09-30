using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenNos.DAL.Mock
{
    public class InventoryDAO : BaseDAO<InventoryDTO>, IInventoryDAO
    {
        #region Members

        private IDictionary<Type, Type> itemInstanceMappings = new Dictionary<Type, Type>();

        #endregion

        #region Methods

        public DeleteResult Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void InitializeMapper(Type baseType)
        {
            /*
           _baseType = baseType;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(baseType, typeof(ItemInstanceDTO)).ForMember("Item", opts => opts.Ignore());
                cfg.CreateMap(typeof(ItemInstanceDTO), typeof(ItemInstanceDTO)).As(baseType);
                Type itemInstanceType = typeof(ItemInstanceDTO);
                foreach (KeyValuePair<Type, Type> entry in itemInstanceMappings)
                {
                    cfg.CreateMap(entry.Key, entry.Value).ForMember("Item", opts => opts.Ignore()).IncludeBase(baseType, typeof(ItemInstanceDTO));
                    cfg.CreateMap(entry.Value, entry.Key).IncludeBase(typeof(ItemInstanceDTO), baseType);
                    Type retrieveDTOType = Type.GetType($"OpenNos.Data.{entry.Key.Name}DTO, OpenNos.Data");
                    cfg.CreateMap(entry.Value, typeof(ItemInstanceDTO)).As(entry.Key);
                }
            });
            _mapper = config.CreateMapper();
            */
        }

        public IEnumerable<InventoryDTO> InsertOrUpdate(IEnumerable<InventoryDTO> dtos)
        {
            throw new NotImplementedException();
        }

        public InventoryDTO InsertOrUpdate(InventoryDTO dto)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<InventoryDTO> LoadByCharacterId(long characterId)
        {
            throw new NotImplementedException();
        }

        public InventoryDTO LoadById(Guid id)
        {
            throw new NotImplementedException();
        }

        public InventoryDTO LoadBySlotAndType(long characterId, short slot, Domain.InventoryType type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<InventoryDTO> LoadByType(long characterId, Domain.InventoryType type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            throw new NotImplementedException();
        }

        public void RegisterMapping(Type gameObjectType)
        {
            Type targetType = Assembly.GetExecutingAssembly().GetTypes().SingleOrDefault(t => t.Name.Equals(gameObjectType.Name));
            Type itemInstanceType = typeof(ItemInstanceDTO);
            itemInstanceMappings.Add(gameObjectType, targetType);
        }

        #endregion
    }
}