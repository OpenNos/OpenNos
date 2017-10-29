using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Collections;

namespace OpenNos.DAL.EF
{
    public class GenericDAO<TEntity, TDTO> : IGenericDAO<TEntity, TDTO> where TEntity : class
    {
        protected readonly IDictionary<Type, Type> Mappings = new Dictionary<Type, Type>();
        protected IMapper Mapper;

        private PropertyInfo PrimaryKey { get; set; }

        public virtual void InitializeMapper()
        {
            MapperConfiguration config = new MapperConfiguration(cfg =>
            {
                foreach (KeyValuePair<Type, Type> entry in Mappings)
                {
                    // GameObject -> Entity
                    cfg.CreateMap(typeof(TDTO), entry.Value);

                    // Entity -> GameObject
                    cfg.CreateMap(entry.Value, typeof(TDTO))
                        .AfterMap((src, dest) => ((MappingBaseDTO)dest).Initialize()).As(entry.Key);
                }
            });

            Mapper = config.CreateMapper();
        }

        public virtual IGenericDAO<TEntity, TDTO> RegisterMapping(Type gameObjectType)
        {
            try
            {
                Type targetType = typeof(TEntity);
                Mappings.Add(gameObjectType, targetType);

                foreach (PropertyInfo pi in gameObjectType.GetProperties())
                {
                    object[] attrs = pi.GetCustomAttributes(typeof(KeyAttribute), false);
                    if (attrs.Length != 1)
                    {
                        continue;
                    }
                    PrimaryKey = pi;
                    break;
                }
                if (PrimaryKey != null)
                {
                    return this;
                }
                throw new KeyNotFoundException();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

      

        public DeleteResult Delete(object dtokey)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {

                DbSet<TEntity> dbset = context.Set<TEntity>();

                if (dtokey is IEnumerable)
                {
                    foreach (object key in dtokey as IEnumerable)
                    {
                        TEntity entityfound = dbset.Find(key);

                        if (entityfound != null)
                        {
                            dbset.Remove(entityfound);
                            context.SaveChanges();
                        }
                    }
                }
                else
                {
                    TEntity entityfound = dbset.Find(dtokey);

                    if (entityfound != null)
                    {
                        dbset.Remove(entityfound);
                    }
                }
                context.SaveChanges();

                return DeleteResult.Deleted;
            }
        }

        public TDTO FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    DbSet<TEntity> dbset = context.Set<TEntity>();
                    IEnumerable<TEntity> entities = Enumerable.Empty<TEntity>();

                    return Mapper.Map<TDTO>(dbset.FirstOrDefault(predicate));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return default(TDTO);
            }
        }

        public SaveResult InsertOrUpdate(ref TDTO dto)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    TEntity entity = Mapper.Map<TEntity>(dto);
                    DbSet<TEntity> dbset = context.Set<TEntity>();

                    object value = PrimaryKey.GetValue(dto, null);
                    TEntity entityfound = null;
                    if (value is object[])
                    {
                        entityfound = dbset.Find((object[])value);
                    }
                    else
                    {
                        entityfound = dbset.Find(value);
                    }
                    if (entityfound != null)
                    {
                        Mapper.Map(entity, entityfound);

                        context.Entry(entityfound).CurrentValues.SetValues(entity);
                        context.SaveChanges();

                        return SaveResult.Updated;
                    }
                    if (value == null || entityfound == null)
                    {
                        dbset.Add(entity);
                    }
                    context.SaveChanges();
                    dto = Mapper.Map<TDTO>(entity);

                    return SaveResult.Inserted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(IEnumerable<TDTO> dtos)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;

                    DbSet<TEntity> dbset = context.Set<TEntity>();
                    foreach (TDTO dto in dtos)
                    {
                        TEntity entity = Mapper.Map<TEntity>(dto);
                        object value = PrimaryKey.GetValue(dto, null);

                        TEntity entityfound = null;
                        if (value is object[])
                        {
                            entityfound = dbset.Find((object[])value);
                        }
                        else
                        {
                            entityfound = dbset.Find(value);
                        }

                        if (entityfound != null)
                        {
                            Mapper.Map(entity, entityfound);

                            context.Entry(entityfound).CurrentValues.SetValues(entity);
                            context.SaveChanges();

                            return SaveResult.Updated;
                        }

                        if (value == null || entityfound == null)
                        {
                            dbset.Add(entity);
                            context.SaveChanges();
                        }
                    }

                    return SaveResult.Inserted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }


        public IEnumerable<TDTO> LoadAll()
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                DbSet<TEntity> dbset = context.Set<TEntity>();
                foreach (TEntity t in dbset)
                {
                    yield return Mapper.Map<TDTO>(t);
                }
            }
        }

        public IEnumerable<TDTO> Where(Expression<Func<TEntity, bool>> predicate)
        {

            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                DbSet<TEntity> dbset = context.Set<TEntity>();
                IEnumerable<TEntity> entities = Enumerable.Empty<TEntity>();
                try
                {
                    entities = dbset.Where(predicate).ToList();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                foreach (TEntity t in entities)
                {
                    yield return Mapper.Map<TDTO>(t);
                }
            }

        }
    }
}
