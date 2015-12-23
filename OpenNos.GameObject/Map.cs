/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using AutoMapper;

namespace OpenNos.GameObject
{
    public class Map : MapDTO, IGameObject
    {
        #region Members

        private char[,] _grid;
        private int _xLength;
        private int _yLength;
        private Guid _uniqueIdentifier;
        private List<Portal> _portals;
        private List<Npc> _npcs;
        private ThreadedBase<MapPacket> threadedBase;

        #endregion

        #region Instantiation
        public Map(short mapId, Guid uniqueIdentifier)
        {

            Mapper.CreateMap<MapDTO, Map>();
            Mapper.CreateMap<Map, MapDTO>();

            threadedBase = new ThreadedBase<MapPacket>(500, HandlePacket);
            MapId = mapId;
            _uniqueIdentifier = uniqueIdentifier;
            LoadZone();
            IEnumerable<PortalDTO> portalsDTO = DAOFactory.PortalDAO.LoadFromMap(MapId);
            _portals = new List<Portal>();
            foreach (PortalDTO portal in portalsDTO)
            {
                _portals.Add(new GameObject.Portal()
                {
                    DestinationMapId = portal.DestinationMapId,
                    SourceMapId = portal.SourceMapId,
                    SourceX = portal.SourceX,
                    SourceY = portal.SourceY,
                    DestinationX = portal.DestinationX,
                    DestinationY = portal.DestinationY,
                    Type = portal.Type,
                    PortalId = portal.PortalId

                });
            }
            IEnumerable<NpcDTO> npcsDTO = DAOFactory.NpcDAO.LoadFromMap(MapId);
            _npcs = new List<Npc>();
            foreach (NpcDTO npc in npcsDTO)
            {
                _npcs.Add(new GameObject.Npc()
                {
                    Dialog = npc.Dialog,
                    MapId = npc.MapId,
                    MapX = npc.MapX,
                    MapY = npc.MapY,
                    Name = npc.Name,
                    Level = npc.Level,
                    NpcId = npc.NpcId,
                    Position = npc.Position,
                    Vnum = npc.Vnum
                });
            }
        }

        #endregion

        #region Properties

        public EventHandler NotifyClients { get; set; }

        public List<Portal> Portals
        {
            get
            {
                return _portals;
            }
        }
        public List<Npc> Npcs
        {
            get
            {
                return _npcs;
            }
        }

        public int IsDancing { get; set; }

        #endregion

        #region Methods

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
            FileStream fsSource = new FileStream("Resource/zones/" + MapId, FileMode.Open, FileAccess.Read);

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

        public void Save()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
