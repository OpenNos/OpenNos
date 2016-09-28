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
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public abstract class BroadcastableBase
    {
        #region Instantiation

        public BroadcastableBase()
        {
            Sessions = new List<ClientSession>();
            LastUnregister = DateTime.Now.AddMinutes(-1);
        }

        #endregion

        #region Events

        private event EventHandler BroadcastEvent;

        #endregion

        #region Properties

        public DateTime LastUnregister { get; set; }

        public List<ClientSession> Sessions { get; set; }

        #endregion

        #region Methods

        public void Broadcast(string content)
        {
            Broadcast(null, content);
        }

        public void Broadcast(ClientSession client, string content, ReceiverType receiver = ReceiverType.All, string characterName = "", long characterId = -1)
        {
            try
            {
                BroadcastEvent?.Invoke(new BroadcastPacket(client, content, receiver, characterName, characterId), new EventArgs());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public virtual void RegisterSession(ClientSession session)
        {
            if (session != null && !Sessions.Contains(session))
            {
                BroadcastEvent += session.OnSessionBroadcast;
                Sessions.Add(session);
            }
        }

        public virtual void UnregisterSession(ClientSession session)
        {
            if (session != null && Sessions.Contains(session))
            {
                LastUnregister = DateTime.Now;
                BroadcastEvent -= session.OnSessionBroadcast;
                Sessions.Remove(session);
            }
        }

        #endregion
    }
}