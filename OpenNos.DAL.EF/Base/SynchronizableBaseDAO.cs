using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public abstract class SynchronizableBaseDao<TEntity, TDto> : MappingBaseDao<TEntity, TDto>, ISynchronizableBaseDAO<TDto>
        where TDto : SynchronizableBaseDTO
        where TEntity : SynchronizableBaseEntity
    {
        #region Methods

        public virtual DeleteResult Delete(Guid id)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                TEntity entity = context.Set<TEntity>().FirstOrDefault(i => i.Id == id);
                if (entity != null)
                {
                    context.Set<TEntity>().Remove(entity);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public IEnumerable<TDto> InsertOrUpdate(IEnumerable<TDto> dtos)
        {
            try
            {
                IList<TDto> results = new List<TDto>();
                using (var context = DataAccessHelper.CreateContext())
                {
                    foreach (TDto dto in dtos)
                    {
                        results.Add(InsertOrUpdate(context, dto));
                    }
                }

                return results;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message), e);
                return Enumerable.Empty<TDto>();
            }
        }

        public TDto InsertOrUpdate(TDto dto)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return InsertOrUpdate(context, dto);
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message), e);
                return null;
            }
        }

        public TDto LoadById(Guid id)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<TDto>(context.Set<TEntity>().FirstOrDefault(i => i.Id.Equals(id)));
            }
        }

        protected virtual TDto Insert(TDto dto, OpenNosContext context)
        {
            TEntity entity = MapEntity(dto);
            context.Set<TEntity>().Add(entity);
            context.SaveChanges();
            return Mapper.Map<TDto>(entity);
        }

        protected virtual TDto InsertOrUpdate(OpenNosContext context, TDto dto)
        {
            Guid primaryKey = dto.Id;
            TEntity entity = context.Set<TEntity>().FirstOrDefault(c => c.Id == primaryKey);
            if (entity == null)
            {
                dto = Insert(dto, context);
            }
            else
            {
                dto = Update(entity, dto, context);
            }

            return dto;
        }

        protected virtual TEntity MapEntity(TDto dto)
        {
            return Mapper.Map<TEntity>(dto);
        }

        protected virtual TDto Update(TEntity entity, TDto inventory, OpenNosContext context)
        {
            if (entity != null)
            {
                Mapper.Map(inventory, entity);
                context.SaveChanges();
            }

            return Mapper.Map<TDto>(entity);
        }

        #endregion
    }
}