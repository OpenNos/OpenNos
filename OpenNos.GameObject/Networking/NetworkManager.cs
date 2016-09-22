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

        #endregion

        #region Instantiation

        public NetworkManager(string ipAddress, int port, Type packetHandler, Type fallbackEncryptor, bool isWorldServer) : base(packetHandler, isWorldServer)
        {
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

        public IDictionary<string, DateTime> ConnectionLog
        {
            get
            {
                if (_connectionLog == null)
                {
                    _connectionLog = new Dictionary<string, DateTime>();
                }

                return _connectionLog;
            }
            set
            {
                if (_connectionLog != value)
                {
                    _connectionLog = value;
                }
            }
        }

        #endregion

        #region Methods

        protected override ClientSession IntializeNewSession(INetworkClient client)
        {
            if (!CheckGeneralLog(client))
            {
                Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("FORCED_DISCONNECT"), client.ClientId);
                client.Initialize(_fallbackEncryptor);
                client.SendPacket($"fail {Language.Instance.GetMessageFromKey("CONNECTION_LOST")}");
                client.Disconnect();
                client = null;
                return null;
            }

            ClientSession session = new ClientSession(client);
            session.Initialize(_encryptor, _packetHandler, IsWorldServer);

            return session;
        }

        private bool CheckGeneralLog(INetworkClient client)
        {
            if (ConnectionLog.Any())
            {
                foreach (var item in ConnectionLog.Where(cl => cl.Key.Equals(client.IpAddress) && (DateTime.Now - cl.Value).Seconds > 3).ToList())
                {
                    ConnectionLog.Remove(item.Key);
                }
            }

            if (ConnectionLog.ContainsKey(client.IpAddress))
            {
                return false;
            }
            else
            {
                ConnectionLog.Add(client.IpAddress, DateTime.Now);
                return true;
            }
        }

        private void Server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            AddSession(e.Client as NetworkClient);
        }

        private void Server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            RemoveSession(e.Client as NetworkClient);
        }

        #endregion
    }
}