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
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class NetworkManager<EncryptorT>
        where EncryptorT : EncryptionBase
    {
        #region Members

        private EncryptorT _encryptor;
        private EncryptionBase _fallbackEncryptor;
        private IDictionary<string, DateTime> _generalLog;
        private ConcurrentDictionary<Guid, Map> _maps = new ConcurrentDictionary<Guid, Map>();
        private Type _packetHandler;
        private ConcurrentDictionary<long, ClientSession> _sessions = new ConcurrentDictionary<long, ClientSession>();

        #endregion

        #region Instantiation

        public NetworkManager(string ipAddress, int port, Type packetHandler, Type fallbackEncryptor, bool isWorldServer)
        {
            IsWorldServer = isWorldServer;
            _packetHandler = packetHandler;
            _encryptor = (EncryptorT)Activator.CreateInstance(typeof(EncryptorT));

            if (fallbackEncryptor != null)
                _fallbackEncryptor = (EncryptionBase)Activator.CreateInstance(fallbackEncryptor);

            var server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(ipAddress, port));
            //Register events of the server to be informed about clients
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.WireProtocolFactory = new WireProtocolFactory<EncryptorT>();

            server.Start(); //Start the server

            Logger.Log.Info(Language.Instance.GetMessageFromKey("STARTED"));
        }

        #endregion

        #region Properties

        public IDictionary<string, DateTime> GeneralLog
        {
            get
            {
                if (_generalLog == null)
                {
                    _generalLog = new Dictionary<string, DateTime>();
                }

                return _generalLog;
            }
            set
            {
                if (_generalLog != value)
                {
                    _generalLog = value;
                }
            }
        }

        public bool IsWorldServer { get; set; }

        #endregion

        #region Methods

        private bool CheckGeneralLog(NetworkClient client)
        {
            ScsTcpEndPoint currentEndpoint = client.RemoteEndPoint as ScsTcpEndPoint;

            if (GeneralLog.Any())
            {
                foreach (var item in GeneralLog.Where(cl => cl.Key.Equals(currentEndpoint.IpAddress) && (DateTime.Now - cl.Value).Seconds > 3).ToList())
                {
                    GeneralLog.Remove(item.Key);
                }
            }

            if (GeneralLog.ContainsKey(currentEndpoint.IpAddress))
            {
                return false;
            }
            else
            {
                GeneralLog.Add(currentEndpoint.IpAddress, DateTime.Now);
                return true;
            }
        }

        private void Server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            Logger.Log.Info(Language.Instance.GetMessageFromKey("NEW_CONNECT") + e.Client.ClientId);
            NetworkClient customClient = e.Client as NetworkClient;

            if (!CheckGeneralLog(customClient))
            {
                Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("FORCED_DISCONNECT"), customClient.ClientId);
                customClient.Initialize(_fallbackEncryptor);
                customClient.SendPacket($"fail {Language.Instance.GetMessageFromKey("CONNECTION_LOST")}");
                customClient.Disconnect();
                customClient = null;
                return;
            }

            ClientSession session = new ClientSession(customClient);
            session.Initialize(_encryptor, _packetHandler);

            if (IsWorldServer)
            {
                ServerManager.Instance.RegisterSession(session);
                if (!_sessions.TryAdd(customClient.ClientId, session))
                {
                    Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("FORCED_DISCONNECT"), customClient.ClientId);
                    customClient.Disconnect();
                    _sessions.TryRemove(customClient.ClientId, out session);
                    ServerManager.Instance.UnregisterSession(session);
                    return;
                };
            }
        }

        private void Server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            ClientSession session;
            _sessions.TryRemove(e.Client.ClientId, out session);

            //check if session hasnt been already removed
            if (session != null)
            {
                session.IsDisposing = true;


                if (IsWorldServer)
                {
                    if (session.Character != null)
                    {
                        if (ServerManager.Instance.Groups.FirstOrDefault(s => s.IsMemberOfGroup(session.Character.CharacterId)) != null)
                        {
                            ServerManager.Instance.GroupLeave(session);
                        }
                        session.Character.Save();

                        //only remove the character from map if the character has been set
                        session.CurrentMap.Broadcast(session, session.Character.GenerateOut(), ReceiverType.AllExceptMe);

                        if (session.HealthTask != null)
                        {
                            session.healthStop = true;
                            session.HealthTask.Dispose();
                        }
                    }
                }

                session.Destroy();
                e.Client.Disconnect();
                Logger.Log.Info(Language.Instance.GetMessageFromKey("DISCONNECT") + e.Client.ClientId);
                session = null;
            }
        }

        #endregion
    }
}