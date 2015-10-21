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

            CustomScsServerClient customClient = e.Client as CustomScsServerClient;

            //Register to MessageReceived event to receive messages from new client
            e.Client.MessageReceived += Client_MessageReceived;

            //dynamically create instances of packethandlers
            foreach (Type handler in _packetHandlers)
            {
                customClient.Handlers.Add(handler.ToString(), Activator.CreateInstance(handler, new object[] { e.Client }));
            }
        }

        static void Server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            Logger.Log.Info("A client is has been disconnected! CliendId = " + e.Client.ClientId);
        }

        static void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ScsRawDataMessage; //Server only accepts text messages
            if (message == null)
            {
                return;
            }

            //Get a reference to the client
            CustomScsServerClient client = sender as CustomScsServerClient;

            //determine first packet
            if (_encryptor.HasCustomParameter && client.SessionId == 0)
            {
                string sessionPacket = _encryptor.DecryptCustomParameter(message.MessageData, message.MessageData.Length);
                string[] sessionParts = sessionPacket.Split(' ');
                client.LastKeepAliveIdentity = Convert.ToInt32(sessionParts[0]);

                //set the SessionId if Session Packet arrives
                client.SessionId = Convert.ToInt32(sessionParts[1].Split('\\').FirstOrDefault());
                Logger.Log.DebugFormat("Client arrived, SessionId: {0}", client.SessionId);

                foreach (Type type in _packetHandlers)
                {
                    MethodInfo methodInfo = GetMethodInfo("OpenNos.EntryPoint", type);
                    object result = methodInfo.Invoke(client.Handlers.SingleOrDefault(h => h.Key.Equals(type.ToString())).Value, new object[] { client.SessionId });
                    //Send reply message to the client
                    ScsTextMessage resultMessage = (ScsTextMessage)result;
                    Logger.Log.DebugFormat("Message sent {0} to client {1}", resultMessage.Text, client.SessionId);

                    if (!String.IsNullOrEmpty(resultMessage.Text))
                    {
                        ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(resultMessage.Text));
                        client.SendMessage(rawMessage);
                    }
                }

                return;
            }

            string packet = _encryptor.Decrypt(message.MessageData, message.MessageData.Length, (int)client.SessionId);
            Logger.Log.DebugFormat("Message received {0} on client {1}", packet, client.ClientId);

            string packetHeader = packet.Split(' ')[0];

            if (_packetHandlers != null)
            {
                foreach (Type type in _packetHandlers)
                {
                    MethodInfo methodInfo = GetMethodInfo(packetHeader, type);

                    if (methodInfo != null)
                    {
                        object result = methodInfo.Invoke(client.Handlers.SingleOrDefault(h => h.Key.Equals(type.ToString())).Value, new object[] { packet, client.SessionId });
                        //Send reply message to the client
                        ScsTextMessage resultMessage = (ScsTextMessage)result;
                        if (!String.IsNullOrEmpty(resultMessage.Text))
                        {
                            Logger.Log.DebugFormat("Message sent {0} to client {1}", resultMessage.Text, client.SessionId);
                            ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(resultMessage.Text));
                            client.SendMessage(rawMessage);
                        }
                    }
                    else
                    {
                        Logger.Log.ErrorFormat("No Method found for Packet Header: {0}", packetHeader);
                    }
                }
            }
        }

        private static MethodInfo GetMethodInfo(string packetHeader, Type t)
        {
            return t.GetMethods().
                Where(x => x.GetCustomAttributes(false).OfType<Packet>().Any())
                .FirstOrDefault(x => x.GetCustomAttributes(false).OfType<Packet>().First().Header.Equals(packetHeader));
        }
    }
}
