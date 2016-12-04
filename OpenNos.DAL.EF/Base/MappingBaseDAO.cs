using System;
using System.Collections.Generic;
using AutoMapper;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class MappingBaseDao<TEntity, TDto> : IMappingBaseDAO
        where TDto : MappingBaseDTO
    {
        #region Members

        protected IMapper Mapper;

        protected IDictionary<Type, Type> Mappings = new Dictionary<Type, Type>();

        #endregion

        #region Methods

        public virtual void InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                foreach (KeyValuePair<Type, Type> entry in Mappings)
                {
                    // GameObject -> Entity
                    cfg.CreateMap(typeof(TDto), entry.Value);

                    // Entity -> GameObject
                    cfg.CreateMap(entry.Value, typeof(TDto))
                        .AfterMap((src, dest) => ((MappingBaseDTO)dest).Initialize()).As(entry.Key);
                }
            });

            Mapper = config.CreateMapper();
        }

        public virtual IMappingBaseDAO RegisterMapping(Type gameObjectType)
        {
            try
            {
                Type targetType = typeof(TEntity);
                Mappings.Add(gameObjectType, targetType);
                return this;
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