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
using System.Linq;

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

        #region Properties

        public DateTime LastUnregister { get; set; }

        public List<ClientSession> Sessions { get; set; }

        #endregion

        #region Methods

        public void Broadcast(string message)
        {
            Broadcast(null, message);
        }

        public void Broadcast(ClientSession client, string message, ReceiverType receiver = ReceiverType.All, string characterName = "", long characterId = -1)
        {
            try
            {
                switch (receiver)
                {
                    case ReceiverType.All:
                        for (int i = Sessions.Where(s => s != null && s.Character != null && !s.IsDisposing).Count() - 1; i >= 0; i--)
                        {
                            Sessions.Where(s => s != null).ElementAt(i).SendPacket(message);
                        }
                        break;

                    case ReceiverType.AllExceptMe:
                        for (int i = Sessions.Where(s => s != null && s.Character != null && s != client && !s.IsDisposing).Count() - 1; i >= 0; i--)
                            Sessions.Where(s => s != null && s != client).ElementAt(i).SendPacket(message);
                        break;

                    case ReceiverType.OnlySomeone:
                        {
                            ClientSession targetSession = Sessions.FirstOrDefault(s => s.Character != null && !s.IsDisposing && (s.Character.Name.Equals(characterName) || s.Character.CharacterId.Equals(characterId)));

                            if (targetSession == null) return;

                            targetSession.SendPacket(message);
                            break;
                        }
                    case ReceiverType.AllNoEmoBlocked:
                        foreach (ClientSession session in Sessions.Where(s => s.Character != null && !s.IsDisposing && s.Character.MapId.Equals(client.Character.MapId) && !s.Character.EmoticonsBlocked))
                            session.SendPacket(message);
                        break;

                    case ReceiverType.AllNoHeroBlocked:
                        foreach (ClientSession session in Sessions.Where(s => s.Character != null && !s.IsDisposing && !s.Character.HeroChatBlocked))
                            session.SendPacket(message);
                        break;

                    case ReceiverType.Group:
                        foreach (ClientSession session in Sessions.Where(s => s.Character != null && !s.IsDisposing && s.Character.Group != null && s.Character.Group.GroupId.Equals(client.Character.Group.GroupId)))
                            session.SendPacket(message);
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public virtual void RegisterSession(ClientSession session)
        {
            if (!Sessions.Contains(session))
            {
                Sessions.Add(session);
            }
        }

        public virtual void UnregisterSession(ClientSession session)
        {
            LastUnregister = DateTime.Now;
            if (Sessions.Contains(session))
            {
                Sessions.Remove(session);
            }
        }

        #endregion
    }
}