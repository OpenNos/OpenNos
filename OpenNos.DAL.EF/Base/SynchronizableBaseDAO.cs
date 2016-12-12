using OpenNos.Core;
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public abstract class SynchronizableBaseDAO<TEntity, TDTO> : MappingBaseDAO<TEntity, TDTO>, ISynchronizableBaseDAO<TDTO>
        where TDTO : SynchronizableBaseDTO
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

        public IEnumerable<TDTO> InsertOrUpdate(IEnumerable<TDTO> dtos)
        {
            try
            {
                IList<TDTO> results = new List<TDTO>();
                using (var context = DataAccessHelper.CreateContext())
                {
                    foreach (TDTO dto in dtos)
                    {
                        results.Add(InsertOrUpdate(context, dto));
                    }
                }

                return results;
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message), e);
                return Enumerable.Empty<TDTO>();
            }
        }

        public TDTO InsertOrUpdate(TDTO dto)
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

        public TDTO LoadById(Guid id)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return _mapper.Map<TDTO>(context.Set<TEntity>().FirstOrDefault(i => i.Id.Equals(id)));
            }
        }

        protected virtual TDTO Insert(TDTO dto, OpenNosContext context)
        {
            TEntity entity = MapEntity(dto);
            context.Set<TEntity>().Add(entity);
            context.SaveChanges();
            return _mapper.Map<TDTO>(entity);
        }

        protected virtual TDTO InsertOrUpdate(OpenNosContext context, TDTO dto)
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

        protected virtual TEntity MapEntity(TDTO dto)
        {
            return _mapper.Map<TEntity>(dto);
        }

        protected virtual TDTO Update(TEntity entity, TDTO inventory, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(inventory, entity);
                context.SaveChanges();
            }

            return _mapper.Map<TDTO>(entity);
        }

        #endregion
    }
}