using OpenNos.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Map // ThreadedBase<KeyValuePair<String, ClientSession>>
    {
        private bool _isRunning;
        private short _mapId;
        private Guid _uniqueIdentifier;

        public SequentialItemProcessor<KeyValuePair<String, ClientSession>> Queue;

        public Map(short mapId, Guid uniqueIdentifier)
        {
            _isRunning = true;
            _mapId = mapId;
            _uniqueIdentifier = uniqueIdentifier;
            Queue = new SequentialItemProcessor<KeyValuePair<string, ClientSession>>(OnBroadCast);
            Queue.Start();
        }

        public short MapId
        {
            get
            {
                return _mapId;
            }
        }

        protected virtual void OnBroadCast(KeyValuePair<string, ClientSession> packet)
        {
            var handler = NotifyClients;
            if (handler != null)
            {
                handler(packet.Key, new EventArgs());
            }
        }

        public EventHandler NotifyClients { get; set; }
    }
}
