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
using OpenNos.Core.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public abstract class BroadcastableBase
    {
        #region Members

        /// <summary>
        /// List of all connected clients.
        /// </summary>
        private readonly ThreadSafeSortedList<long, ClientSession> _sessions;

        #endregion

        #region Instantiation

        public BroadcastableBase()
        {
            LastUnregister = DateTime.Now.AddMinutes(-1);
            _sessions = new ThreadSafeSortedList<long, ClientSession>();
        }

        #endregion

        #region Properties

        public DateTime LastUnregister { get; set; }

        public List<ClientSession> Sessions
        {
            get
            {
                return _sessions.GetAllItems();
            }
        }

        #endregion

        #region Methods

        public void Broadcast(string content)
        {
            Broadcast(null, content);
        }

        public void Broadcast(PacketBase content)
        {
            Broadcast(null, content);
        }

        public void Broadcast(ClientSession client, PacketBase content, ReceiverType receiver = ReceiverType.All, string characterName = "", long characterId = -1)
        {
            Broadcast(client, PacketFactory.Deserialize(content), receiver, characterName, characterId);
        }

        public void Broadcast(ClientSession client, string content, ReceiverType receiver = ReceiverType.All, string characterName = "", long characterId = -1)
        {
            // Send message to all online users
            Task.Factory.StartNew(
                () =>
                {
                    foreach (var session in _sessions.GetAllItems())
                    {
                        try
                        {
                            session.ReceiveBroadcast(new BroadcastPacket(client, content, receiver, characterName, characterId));
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                });
        }

        public virtual void RegisterSession(ClientSession session)
        {
            if (session != null)
            {
                // Create a ChatClient and store it in a collection
                _sessions[session.ClientId] = session;
            }
        }

        public virtual void UnregisterSession(long clientId)
        {
            // Get client from client list, if not in list do not continue
            var session = _sessions[clientId];
            if (session == null)
            {
                return;
            }

            // Remove client from online clients list
            _sessions.Remove(clientId);

            LastUnregister = DateTime.Now;
        }

        #endregion
    }
}