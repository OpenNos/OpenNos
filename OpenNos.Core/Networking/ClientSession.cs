using OpenNos.Core.Communication.Scs.Communication.Messages;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class ClientSession
    {

        #region Members

        private NetworkClient _client;
        private Guid _uniqueIdentifier;
        private Account _account;
        private IDictionary<Type, object> _PacketHandlers { get; set; }
        private static EncryptionBase _encryptor;
        private SequentialItemProcessor<byte[]> _processor;

        #endregion

        public ClientSession(NetworkClient client)
        {
            _client = client;
            //absolutely new instantiated Client has no SessionId
            SessionId = 0;
            _client.MessageReceived += NetworkClient_MessageReceived;
            _processor = new SequentialItemProcessor<byte[]>(HandlePacket);
            _processor.Start();
        }

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

        public Account Account
        {
            get
            {
                return _account;
            }
            set
            {
                _account = value;
            }
        }

        public NetworkClient Client
        {
            get
            {
                return _client;
            }
            set
            {
                _client = value;
            }
        }

        #endregion

        private void NetworkClient_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ScsRawDataMessage;
            if (message == null)
            {
                return;
            }

            _processor.EnqueueMessage(message.MessageData);
        }

        private void HandlePacket(byte[] packetData)
        {
            //determine first packet
            if (_encryptor.HasCustomParameter && this.SessionId == 0)
            {
                string sessionPacket = _encryptor.DecryptCustomParameter(packetData);
                Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("PACKET_ARRIVED"), sessionPacket);

                string[] sessionParts = sessionPacket.Split(' ');
                this.LastKeepAliveIdentity = Convert.ToInt32(sessionParts[0]);

                //set the SessionId if Session Packet arrives
                this.SessionId = Convert.ToInt32(sessionParts[1].Split('\\').FirstOrDefault());
                Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("CLIENT_ARRIVED"), this.SessionId);

                if (!TriggerHandler("OpenNos.EntryPoint", String.Empty))
                {
                    Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("NO_ENTRY"));
                }

                return;
            }

            string packetConcatenated = _encryptor.Decrypt(packetData, packetData.Length, (int)this.SessionId);

            foreach (string packet in packetConcatenated.Split(new char[] { (char)0xFF }, StringSplitOptions.RemoveEmptyEntries))
            {
                Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("MESSAGE_RECEIVED"), packet, _client.ClientId);

                if (_encryptor.HasCustomParameter)
                {
                    //keep alive
                    string nextKeepAliveRaw = packet.Split(' ')[0];
                    Int32 nextKeepaliveIdentity;
                    if (!Int32.TryParse(nextKeepAliveRaw, out nextKeepaliveIdentity) && nextKeepaliveIdentity != (this.LastKeepAliveIdentity + 1))
                    {
                        Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("CORRUPT_KEEPALIVE"), _client.ClientId);
                        _client.Disconnect();
                        return;
                    }
                    else if (nextKeepaliveIdentity == 0)
                    {
                        if (LastKeepAliveIdentity == UInt16.MaxValue)
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
                        Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader);
                    }
                }
                else
                {
                    //simple messaging
                    string packetHeader = packet.Split(' ')[0];

                    if (!TriggerHandler(packetHeader, packet))
                    {
                        Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader);
                    }
                }

            }
        }

        public void Initialize(EncryptionBase encryptor, IList<Type> _packetHandlers, Guid uniqueIdentifier)
        {
            _encryptor = encryptor;
            _uniqueIdentifier = uniqueIdentifier;
            _client.Initialize(encryptor);

            //dynamically create instances of packethandlers
            foreach (Type handler in _packetHandlers)
            {
                Handlers.Add(handler, Activator.CreateInstance(handler, new object[] { this }));
            }
        }

        private bool TriggerHandler(string packetHeader, string packet)
        {
            foreach (KeyValuePair<Type, Object> handler in _PacketHandlers)
            {
                MethodInfo methodInfo = GetMethodInfo(packetHeader, handler.Key);

                if (methodInfo != null)
                {
                    string result = (string)methodInfo.Invoke(handler.Value, new object[] { packet });

                    //check for returned packet
                    if (!String.IsNullOrEmpty(result))
                    {
                        //Send reply message to the client
                        Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("MSG_SENT"), result);
                        _client.SendPacket(result);
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

    }
}
