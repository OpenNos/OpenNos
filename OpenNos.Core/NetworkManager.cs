using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Server;
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

        private static Dictionary<String, Object> _packetHandlers;

        #endregion

        #region Instantiation

        public NetworkManager(string ipAddress, int port, Dictionary<String, Object> packetHandlers, bool useFraming)
        {
            _packetHandlers = packetHandlers;

            //Create a server that listens 10085 TCP port for incoming connections
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
            Logger.Log.Info("A new client is connected. Client Id = " + e.Client.ClientId);

            //Register to MessageReceived event to receive messages from new client
            e.Client.MessageReceived += Client_MessageReceived;
            e.Client.Handlers = _packetHandlers;
        }

        static void Server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            Logger.Log.Info("A client is disconnected! Client Id = " + e.Client.ClientId);
        }

        static void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ScsTextMessage; //Server only accepts text messages
            if (message == null)
            {
                return;
            }

            //Get a reference to the client
            var client = (IScsServerClient)sender;

            Logger.Log.DebugFormat("Message received {0} on client {1}", message, client.ClientId);

            string packetHeader = message.Text.Split(' ')[0];

            Assembly handlerAssembly = Assembly.Load("OpenNos.Handler");

            if (handlerAssembly != null)
            {
                foreach (Type type in handlerAssembly.GetTypes())
                {
                    MethodInfo methodInfo = GetMethodInfo(packetHeader, type);

                    if (methodInfo != null)
                    {
                        object result = methodInfo.Invoke(client.Handlers.SingleOrDefault(h => h.Key.Equals(type.ToString())).Value, new object[] { message.Text, client.ClientId });
                        //Send reply message to the client
                        ScsMessage resultMessage = (ScsMessage)result;
                        Logger.Log.DebugFormat("Message sent {0} to client {1}", resultMessage, client.ClientId);
                        client.SendMessage(resultMessage);
                    }
                    else
                    {
                        Logger.Log.ErrorFormat("No Method found for Packet Header: {0}", packetHeader);
                    }
                }
            }
            else
            {
                Logger.Log.Error("OpenNos.Handler not found, could not retrieve Packet handlers.");
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
