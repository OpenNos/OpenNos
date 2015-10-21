using OpenNos.Core.Communication.Scs.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core.Communication.Scs.Communication.Channels;
using OpenNos.Core.Communication.Scs.Communication.Messages;
using System.Reflection;
using System.Threading;

namespace OpenNos.Core
{
    public class NetworkClient : ScsServerClient
    {
        private IDictionary<Type, object> _PacketHandlers { get; set; }
        private static EncryptionBase _encryptor;

        public int SessionId { get; set; }
        public int LastKeepAliveIdentity { get; set; }

        public NetworkClient(ICommunicationChannel communicationChannel) : base(communicationChannel)
        {
            //absolutely new instantiated Client has no SessionId
            SessionId = 0;
            base.MessageReceived += CustomScsServerClient_MessageReceived;
        }

        public void Initialize(EncryptionBase encryptor, IList<Type> _packetHandlers)
        {
            _encryptor = encryptor;

            //dynamically create instances of packethandlers
            foreach (Type handler in _packetHandlers)
            {
                Handlers.Add(handler, Activator.CreateInstance(handler, new object[] { this }));
            }
        }

        private void CustomScsServerClient_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ScsRawDataMessage; //Server only accepts text messages
            if (message == null)
            {
                return;
            }

            //determine first packet
            if (_encryptor.HasCustomParameter && this.SessionId == 0)
            {
                string sessionPacket = _encryptor.DecryptCustomParameter(message.MessageData, message.MessageData.Length);
                string[] sessionParts = sessionPacket.Split(' ');
                this.LastKeepAliveIdentity = Convert.ToInt32(sessionParts[0]);

                //set the SessionId if Session Packet arrives
                this.SessionId = Convert.ToInt32(sessionParts[1].Split('\\').FirstOrDefault());
                Logger.Log.DebugFormat("Client arrived, SessionId: {0}", this.SessionId);

                if(!TriggerHandler("OpenNos.EntryPoint", String.Empty))
                {
                    Logger.Log.ErrorFormat("No EntryPoint found");
                }

                return;
            }

            string packet = _encryptor.Decrypt(message.MessageData, message.MessageData.Length, (int)this.SessionId);
            Logger.Log.DebugFormat("Message received {0} on client {1}", packet, this.ClientId);

            string packetHeader = packet.Split(' ')[0];

            if (!TriggerHandler(packetHeader, packet))
            {
                Logger.Log.ErrorFormat("Could not found Handler implementation for Packet with Header {0}", packetHeader);
            }
        }

        private bool TriggerHandler(string packetHeader, string packet)
        {
            foreach (KeyValuePair<Type, Object> handler in _PacketHandlers)
            {
                MethodInfo methodInfo = GetMethodInfo(packetHeader, handler.Key);

                if (methodInfo != null)
                {
                    object result = methodInfo.Invoke(handler.Value, new object[] { packet, this.SessionId });

                    if(result != null)
                    {
                        //Send reply message to the client
                        ScsTextMessage resultMessage = (ScsTextMessage)result;
                        Logger.Log.DebugFormat("Message sent {0} to client {1}", resultMessage.Text, this.SessionId);

                        if (!String.IsNullOrEmpty(resultMessage.Text))
                        {
                            ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(resultMessage.Text));
                            this.SendMessage(rawMessage);
                        }
                    }
                    
                    return true;
                }
            }

            return false;
        }

        private static MethodInfo GetMethodInfo(string packetHeader, Type t)
        {
            return t.GetMethods().
                Where(x => x.GetCustomAttributes(false).OfType<Packet>().Any())
                .FirstOrDefault(x => x.GetCustomAttributes(false).OfType<Packet>().First().Header.Equals(packetHeader));
        }

        public IDictionary<Type, object> Handlers
        {
            get
            {
                if (_PacketHandlers == null)
                {
                    _PacketHandlers = new Dictionary<Type, object>();
                }

                return _PacketHandlers;
            }

            set
            {
                _PacketHandlers = value;
            }
        }
    }
}
