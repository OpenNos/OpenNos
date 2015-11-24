using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenNos.GameObject
{
    public class Map
    {
        #region Members

        private char[,] _grid;
        private short _mapId;
        private int _xLength;
        private int _yLength;
        private Guid _uniqueIdentifier;
        private List<Portal> _portals;
        private ThreadedBase<MapPacket> threadedBase;

        #endregion

        #region Instantiation
        public Map(short mapId, Guid uniqueIdentifier)
        {
            threadedBase = new ThreadedBase<MapPacket>(500, HandlePacket);
            _mapId = mapId;
            _uniqueIdentifier = uniqueIdentifier;
            LoadZone();
            IEnumerable<PortalDTO> portalsDTO = DAOFactory.PortalDAO.LoadFromMap(_mapId);
            _portals = new List<Portal>();
            foreach (PortalDTO portal in portalsDTO)
            {
                _portals.Add( new GameObject.Portal()
                {
                    DestMap = portal.DestMap,
                    SrcMap = portal.SrcMap,
                    SrcX = portal.SrcX,
                    SrcY = portal.SrcX,
                    DestX = portal.DestX,
                    DestY = portal.DestX,
                    Type = portal.Type,
                    PortalId = portal.PortalId

                });
            }
        }

        #endregion

        #region Properties

        public short MapId
        {
            get
            {
                return _mapId;
            }
        }

        public EventHandler NotifyClients { get; set; }

        #endregion

        #region Methods

        public List<Portal> Portals
        {
            get
            {
                return _portals;
            }
        }

        public bool IsBlockedZone(int x, int y)
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
        public void OnBroadCast(MapPacket mapPacket)
        {
            var handler = NotifyClients;
            if (handler != null)
            {
                handler(mapPacket, new EventArgs());
            }
        }

        /// <summary>
        /// Sequentialitemsprocessor triggers this tasked based
        /// </summary>
        /// <param name="parameter"></param>
        public void HandlePacket(MapPacket parameter)
        {
            //handle iterative operations

            //notify clients about changes
            OnBroadCast(parameter);
        }

        /// <summary>
        /// Enqueue a packet for the Map.
        /// </summary>
        /// <param name="mapPacket"></param>
        private void QueuePacket(MapPacket mapPacket)
        {
            threadedBase.Queue.EnqueueMessage(mapPacket);
        }

        /// <summary>
        /// Inform client(s) about the Packet.
        /// </summary>
        /// <param name="session">Session of the sender.</param>
        /// <param name="packet">The packet content to send.</param>
        /// <param name="receiver">The receiver(s) of the Packet.</param>
        public void BroadCast(ClientSession session, string packet, ReceiverType receiver)
        {
            QueuePacket(new MapPacket(session, packet, receiver));
        }

        /// <summary>
        /// Send packet to all clients
        /// </summary>
        /// <param name="mapPacket">The MapPacket to send.</param>
        public void BroadCast(MapPacket mapPacket)
        {
            QueuePacket(mapPacket);
        }

        /// <summary>
        /// Get notificated from outside the Session.
        /// </summary>
        /// <param name="sender">Sender of the packet.</param>
        /// <param name="e">Eventargs e.</param>
        public void GetNotification(object sender, EventArgs e)
        {
            //pass thru to clients
            QueuePacket((MapPacket)sender);
        }

        #endregion
    }
}
