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

        public static ConcurrentBag<T> Where<T>(this ConcurrentBag<T> queue, Func<T, bool> predicate)
        {
            var Temp = new ConcurrentBag<T>();
            queue.Where(predicate).ToList().ForEach(Line =>
            {
                Temp.Add(Line);
            });
            return Temp;
        }

        #endregion
    }
}