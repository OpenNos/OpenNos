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
        #region Members

        private IDictionary<Type, object> _PacketHandlers { get; set; }
        private static EncryptionBase _encryptor;

        #endregion

        #region Properties

        public int SessionId { get; set; }
        public int LastKeepAliveIdentity { get; set; }

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

        #endregion

        #region Methods

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

        public bool SendPacketFormat(string packet, params object[] param)
        {
            return SendPacket(String.Format(packet, param));
        }

        public bool SendPacket(string packet)
        {
            try
            {
                ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(packet));
                SendMessage(rawMessage);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat("Failed to send packet {0} to client {1}, {2}.", packet, ClientId, e.Message);
                return false;
            }
        }
        public bool SendPackets(IEnumerable<String> packets)
        {
            bool result = true;

            //TODO maybe send at once with delimiter
            foreach (string packet in packets)
            {
                result = result && SendPacket(packet);
            }

            return result;
        }

        private void CustomScsServerClient_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ScsRawDataMessage;
            if (message == null)
            {
                return;
            }

            //determine first packet
            if (_encryptor.HasCustomParameter && this.SessionId == 0)
            {
                string sessionPacket = _encryptor.DecryptCustomParameter(message.MessageData);
                Logger.Log.DebugFormat("Packet arrived, packet: {0}", sessionPacket);

                string[] sessionParts = sessionPacket.Split(' ');
                this.LastKeepAliveIdentity = Convert.ToInt32(sessionParts[0]);

                //set the SessionId if Session Packet arrives
                this.SessionId = Convert.ToInt32(sessionParts[1].Split('\\').FirstOrDefault());
                Logger.Log.DebugFormat("Client arrived, SessionId: {0}", this.SessionId);

                if (!TriggerHandler("OpenNos.EntryPoint", String.Empty))
                {
                    Logger.Log.ErrorFormat("No EntryPoint found");
                }

                return;
            }

            string packetConcatenated = _encryptor.Decrypt(message.MessageData, message.MessageData.Length, (int)this.SessionId);

            foreach (string packet in packetConcatenated.Split(new char[] { (char)0xFF }, StringSplitOptions.RemoveEmptyEntries))
            {
                Logger.Log.DebugFormat("Message received {0} on client {1}", packet, this.ClientId);
      
                if (_encryptor.HasCustomParameter)
                {
                    //keep alive
                    string nextKeepAliveRaw = packet.Split(' ')[0];
                    Int32 nextKeepaliveIdentity;
                    if(!Int32.TryParse(nextKeepAliveRaw, out nextKeepaliveIdentity) && nextKeepaliveIdentity != (this.LastKeepAliveIdentity + 1))
                    {
                        Logger.Log.ErrorFormat("Corrupted Keepalive on client {0}.", ClientId);
                        Disconnect();
                        return;
                    }
                    else if(nextKeepaliveIdentity == 0)
                    {
                        if(LastKeepAliveIdentity == UInt16.MaxValue)
                            LastKeepAliveIdentity = nextKeepaliveIdentity;
                    }
                    else
                    {
                        LastKeepAliveIdentity = nextKeepaliveIdentity;
                    }

                    string packetHeader = packet.Split(' ')[1];

                    //0 is a keep alive packet with no content to handle
                    if (packetHeader != "0" && !TriggerHandler(packetHeader, packet))
                    {
                        Logger.Log.ErrorFormat("Could not found Handler implementation for Packet with Header {0}", packetHeader);
                    }
                }
                else
                {
                    //simple messaging
                    string packetHeader = packet.Split(' ')[0];

                    if (!TriggerHandler(packetHeader, packet))
                    {
                        Logger.Log.ErrorFormat("Could not found Handler implementation for Packet with Header {0}", packetHeader);
                    }
                }

            }
        }

        private bool TriggerHandler(string packetHeader, string packet)
        {
            foreach (KeyValuePair<Type, Object> handler in _PacketHandlers)
            {
                MethodInfo methodInfo = GetMethodInfo(packetHeader, handler.Key);

                if (methodInfo != null)
                {
                    string result = (string)methodInfo.Invoke(handler.Value, new object[] { packet, this.SessionId });

                    //check for returned packet
                    if (!String.IsNullOrEmpty(result))
                    {
                        //Send reply message to the client
                        Logger.Log.DebugFormat("Message sent {0} to client {1}", result, this.ClientId);
                        SendPacket(result);
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

        #endregion
    }
}
