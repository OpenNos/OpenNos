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
using OpenNos.Core.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Communication.Scs.Communication.Messages;
using OpenNos.Core.Communication.Scs.Server;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class NetworkManager<EncryptorT>
        where EncryptorT : EncryptionBase
    {

        #region Members

        private static IList<Type> _packetHandlers;
        private static EncryptorT _encryptor;

        #endregion

        #region Instantiation

        public NetworkManager(string ipAddress, int port, IList<Type> packetHandlers, bool useFraming)
        {
            _packetHandlers = packetHandlers;
            _encryptor = (EncryptorT)Activator.CreateInstance(typeof(EncryptorT));

            var server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(ipAddress, port));

            //Register events of the server to be informed about clients
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.WireProtocolFactory = new WireProtocolFactory<EncryptorT>(useFraming);

            server.Start(); //Start the server

            Logger.Log.Info("Server is started successfully.");
        }

        #endregion

        static void Server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            Logger.Log.Info("A new client is connected. ClientId = " + e.Client.ClientId);

            NetworkClient customClient = e.Client as NetworkClient;
            customClient.Initialize(_encryptor, _packetHandlers);
        }

        static void Server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {

            e.Client.Disconnect();
            Logger.Log.Info("A client is has been disconnected! CliendId = " + e.Client.ClientId);
        }
    }
}
