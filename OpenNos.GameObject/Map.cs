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
        private char[,] _grid;
        private short _mapId;
        private int _xLength;
        private int _yLength;
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
        public bool isBlockedZone(int x, int y)
        {
            if (x > _xLength || x < 1 || y > _yLength || y < 1 || _grid[y - 1, x - 1] == 1)
            {
                return true;
            }

            return false;
        }
        public void LoadZone()
        {
            FileStream fsSource = new FileStream("Resource/zones/" + _mapId, FileMode.Open, FileAccess.Read);

            byte[] bytes = new byte[fsSource.Length];
            int numBytesToRead = 1;
            int numBytesRead = 0;


            fsSource.Read(bytes, numBytesRead, numBytesToRead);
            _xLength = bytes[0];
            fsSource.Read(bytes, numBytesRead, numBytesToRead);
            fsSource.Read(bytes, numBytesRead, numBytesToRead);
            _yLength = bytes[0];

            _grid = new char[_yLength, _xLength];
            for (int i = 0; i < _yLength; ++i)
            {

                for (int t = 0; t < _xLength; ++t)
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
