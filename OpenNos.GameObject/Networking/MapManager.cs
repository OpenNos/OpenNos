using OpenNos.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public static class MapManager
    {
        private static ConcurrentDictionary<Guid, Map> _maps = new ConcurrentDictionary<Guid, Map>();

        public static void Initialize()
        {
            for (short i = 0; i < 10; i++)
            {
                Guid guid = Guid.NewGuid();
                Map newMap = new Map(i, guid);
                _maps.TryAdd(guid, newMap);
            }
        }

        public static Map GetMap(short id)
        {
            return _maps.SingleOrDefault(m => m.Value.MapId.Equals(id)).Value;
        }
    }
}
