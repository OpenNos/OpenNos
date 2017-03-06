using AutoMapper;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public abstract class BaseDAO<TDTO> : IMappingBaseDAO
    {
        #region Members

        protected IMapper _mapper;
        protected Dictionary<Type, Type> _mappings = new Dictionary<Type, Type>();

        #endregion

        #region Instantiation

        public BaseDAO()
        {
            Container = new List<TDTO>();
        }

        #endregion

        #region Properties

        public IList<TDTO> Container { get; set; }

        #endregion

        #region Methods

        public virtual void InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                foreach (KeyValuePair<Type, Type> entry in _mappings)
                {
                    // GameObject -> Entity
                    cfg.CreateMap(typeof(TDTO), entry.Value);

                    // Entity -> GameObject
                    cfg.CreateMap(entry.Value, typeof(TDTO))
                        .AfterMap((src, dest) => ((MappingBaseDTO)dest).Initialize()).As(entry.Key);
                }
            });

            _mapper = config.CreateMapper();
        }

        public void Insert(IEnumerable<TDTO> dtos)
        {
            foreach (TDTO dto in dtos)
            {
                Insert(dto);
            }
        }

        public virtual TDTO Insert(TDTO dto)
        {
            Container.Add(dto);
            return dto;
        }

        public IEnumerable<TDTO> LoadAll()
        {
            foreach (TDTO dto in Container)
            {
                yield return MapEntity(dto);
            }
        }

        public virtual IMappingBaseDAO RegisterMapping(Type gameObjectType)
        {
            try
            {
                Type targetType = typeof(TDTO);
                _mappings.Add(gameObjectType, targetType);
                return this;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Map a DTO to a GO
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        protected virtual TDTO MapEntity(TDTO dto)
        {
            return _mapper.Map<TDTO>(dto);
        }

        #endregion
    }
}