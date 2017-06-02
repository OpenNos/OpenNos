using System.Collections.Concurrent;

namespace OpenNos.Core
{
    internal static class ConcurrentQueueExtensions
    {
        #region Methods

        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            while (queue.TryDequeue(out T item))
            {
                // do nothing
            }
        }

        #endregion
    }
}