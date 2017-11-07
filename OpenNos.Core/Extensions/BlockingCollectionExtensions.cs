using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public static class ConcurrentBagExtensions
    {
        #region Methods

        public static void Clear<T>(this ConcurrentBag<T> queue)
        {
            while (queue.Count > 0)
            {
                queue.TryTake(out T item);
            }
        }

        public static ConcurrentBag<T> Replace<T>(this ConcurrentBag<T> queue, Func<T, bool> predicate)
        {
            return new ConcurrentBag<T>(queue.ToList().Where(predicate));
        }
        #endregion
    }
}