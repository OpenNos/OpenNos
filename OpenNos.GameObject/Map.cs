using OpenNos.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Map // ThreadedBase<KeyValuePair<String, ClientSession>>
    {
        private bool _isRunning;
        public char[,] _grid;
        private short _mapId;
        private Guid _uniqueIdentifier;

        public SequentialItemProcessor<KeyValuePair<String, ClientSession>> Queue;

        public Map(short mapId, Guid uniqueIdentifier)
        {
            _isRunning = true;
            _mapId = mapId;
            _uniqueIdentifier = uniqueIdentifier;
            LoadZone();
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
        public void LoadZone()
        {
            FileStream fsSource = new FileStream("Resource/zones/" + _mapId, FileMode.Open, FileAccess.Read);

                byte[] bytes = new byte[fsSource.Length];
                int numBytesToRead = 1;
                int numBytesRead = 0;

                
                fsSource.Read(bytes, numBytesRead, numBytesToRead);
                byte xLength = bytes[0];
                fsSource.Read(bytes, numBytesRead, numBytesToRead);
                fsSource.Read(bytes, numBytesRead, numBytesToRead);
                byte yLength = bytes[0];

               _grid = new char[yLength, xLength];
                for (int i = 0; i < yLength; ++i)
                {

                    for (int t = 0; t < xLength; ++t)
                    {
                        fsSource.Read(bytes, numBytesRead, numBytesToRead);
                       _grid[i, t] = Convert.ToChar(bytes[0]);
                    }
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
