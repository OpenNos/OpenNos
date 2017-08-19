using System;
using System.Collections.Concurrent;
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

        public static void RemoveAll<T>(this ConcurrentBag<T> queue, Func<T, bool> predicate)
        {
            var Temp = new ConcurrentBag<T>();
            Parallel.ForEach(queue.Where(predicate), Line =>
            {
                Temp.Add(Line);
            });
            queue = Temp;
        }

        #endregion
    }
}