using System;
using System.Collections.Generic;
using OpenNos.Data.Enums;
using System.Linq.Expressions;

namespace OpenNos.DAL.EF
{
    public interface IGenericDAO<TEntity, TDTO> : IMappingBaseDAO where TEntity : class
    {
        IEnumerable<TDTO> LoadAll();

        IEnumerable<TDTO> Where(Expression<Func<TEntity, bool>> predicate);

        TDTO FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

        SaveResult InsertOrUpdate(ref TDTO dto);

        SaveResult InsertOrUpdate(IEnumerable<TDTO> dtos);

        DeleteResult Delete(object entitykey);
        
    }
}