using AutoMapper;
using OpenNos.Core;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.EF
{
    public class GameObjectMappingBaseDAO<TEntity, TDTO> : IGameObjectMappingBaseDAO
        where TDTO : GameObjectMappingBaseDTO
    {
        protected IMapper _mapper;

        protected IDictionary<Type, Type> mappings = new Dictionary<Type, Type>();

        public void RegisterMapping(Type gameObjectType)
        {
            try
            {
                Type targetType = Assembly.GetExecutingAssembly().GetTypes().SingleOrDefault(t => t.Name.Equals(gameObjectType.Name));
                Type itemInstanceType = typeof(TEntity);
                mappings.Add(gameObjectType, targetType);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public virtual void InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                foreach (KeyValuePair<Type, Type> entry in mappings)
                {
                    // GameObject -> Entity
                    cfg.CreateMap(typeof(TDTO), entry.Value);

                    // Entity -> GameObject
                    cfg.CreateMap(entry.Value, typeof(TDTO))
                        .AfterMap((src, dest) => ((GameObjectMappingBaseDTO)dest).Initialize()).As(entry.Key);
                }
            });

            _mapper = config.CreateMapper();
        }
    }
}
