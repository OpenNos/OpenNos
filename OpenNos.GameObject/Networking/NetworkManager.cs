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
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class NetworkManager<EncryptorT> : SessionManager
        where EncryptorT : EncryptionBase
    {
        #region Members

        private IDictionary<string, DateTime> _connectionLog;
        private EncryptorT _encryptor;
        private EncryptionBase _fallbackEncryptor;
        private IScsServer _server;

        #endregion

        #region Instantiation

        public NetworkManager(string ipAddress, int port, Type packetHandler, Type fallbackEncryptor, bool isWorldServer) : base(packetHandler, isWorldServer)
        {
            _encryptor = (EncryptorT)Activator.CreateInstance(typeof(EncryptorT));

            if (fallbackEncryptor != null)
            {
                _fallbackEncryptor = (EncryptionBase)Activator.CreateInstance(fallbackEncryptor);
            }

            _server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(ipAddress, port));

            // Register events of the server to be informed about clients
            _server.ClientConnected += OnServerClientConnected;
            _server.ClientDisconnected += OnServerClientDisconnected;
            _server.WireProtocolFactory = new WireProtocolFactory<EncryptorT>();

            // Start the server
            _server.Start();

            Logger.Log.Info(Language.Instance.GetMessageFromKey("STARTED"));
        }

        #endregion

        #region Properties

        private IDictionary<string, DateTime> ConnectionLog
        {
            get
            {
                return _connectionLog ?? (_connectionLog = new Dictionary<string, DateTime>());
            }
        }

        #endregion

        #region Methods

        public override void StopServer()
        {
            _server.Stop();
            _server.ClientConnected -= OnServerClientDisconnected;
            _server.ClientDisconnected -= OnServerClientConnected;
        }

        protected override ClientSession IntializeNewSession(INetworkClient client)
        {
            if (!CheckGeneralLog(client))
            {
                Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("FORCED_DISCONNECT"), client.ClientId);
                client.Initialize(_fallbackEncryptor);
                client.SendPacket($"fail {Language.Instance.GetMessageFromKey("CONNECTION_LOST")}");
                client.Disconnect();
                return null;
            }

            ClientSession session = new ClientSession(client);
            session.Initialize(_encryptor, _packetHandler, IsWorldServer);

            return session;
        }

        private bool CheckGeneralLog(INetworkClient client)
        {
            if (!client.IpAddress.Contains("127.0.0.1"))
            {
                if (ConnectionLog.Any())
                {
                    foreach (var item in ConnectionLog.Where(cl => cl.Key.Equals(client.IpAddress) && (DateTime.Now - cl.Value).Seconds > 3).ToList())
                    {
                        ConnectionLog.Remove(item.Key);
                    }
                }

                if (ConnectionLog.Any(c=>c.Key.Contains(client.IpAddress.Split(':')[0])))
                {
                    return false;
                }
                ConnectionLog.Add(client.IpAddress, DateTime.Now);
                return true;
            }

            return true;
        }

        private void OnServerClientConnected(object sender, ServerClientEventArgs e)
        {
            AddSession(e.Client as NetworkClient);
        }

        private void OnServerClientDisconnected(object sender, ServerClientEventArgs e)
        {
            RemoveSession(e.Client as NetworkClient);
        }

        #endregion
    }
}