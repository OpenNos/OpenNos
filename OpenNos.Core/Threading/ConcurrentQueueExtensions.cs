using System.Collections.Concurrent;

namespace OpenNos.Core
{
    internal static class ConcurrentQueueExtensions
    {
        #region Methods

        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item))
            {
                // do nothing
            }
        }

        #endregion
    }
}