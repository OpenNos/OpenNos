using OpenNos.Core;
using OpenNos.Core.Communication.Scs.Communication.Messages;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
namespace OpenNos.GameObject
{
    public class ClientSession
    {

        #region Members

        private NetworkClient _client;
        private Guid _uniqueIdentifier;
        private Account _account;
        private Character _character;
        private IDictionary<Packet, Tuple<MethodInfo, object>> _packetHandlers;
        private static EncryptionBase _encryptor;
        private SequentialItemProcessor<byte[]> _processor;

        private bool _waitForPackets;
        private int _waitForPacketsAmount;
        private IList<String> _packetQueue = new List<String>();

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

        public IDictionary<Packet, Tuple<MethodInfo, object>> Handlers
        {
            get
            {
                if (_packetHandlers == null)
                {
                    _packetHandlers = new Dictionary<Packet, Tuple<MethodInfo, object>>();
                }

                return _packetHandlers;
            }

            set
            {
                _packetHandlers = value;
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
        public Character character
        {
            get
            {
                return _character;
            }
            set
            {
                _character = value;
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

        public Map CurrentMap { get; set; }

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

                if (!_waitForPackets)
                {
                    TriggerHandler("OpenNos.EntryPoint", String.Empty, false);
                }

                return;
            }

            string packetConcatenated = _encryptor.Decrypt(packetData, packetData.Length, (int)this.SessionId);

            foreach (string packet in packetConcatenated.Split(new char[] { (char)0xFF }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] packetsplit = packet.Split(' ');

                if (packetsplit[1] != "0")
                    Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("MESSAGE_RECEIVED"), packet, _client.ClientId);

                if (_encryptor.HasCustomParameter)
                {
                    //keep alive
                    string nextKeepAliveRaw = packetsplit[0];
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

                    if (_waitForPackets)
                    {
                        if (_packetQueue.Count != _waitForPacketsAmount - 1)
                        {
                            _packetQueue.Add(packet);
                        }
                        else
                        {
                            _packetQueue.Add(packet);
                            _waitForPackets = false;
                            string queuedPackets = String.Join(" ", _packetQueue.ToArray());
                            string header = queuedPackets.Split(' ')[1];
                            TriggerHandler(header, queuedPackets, true);
                            _packetQueue.Clear();
                            return;
                        }
                    }
                    else
                    {
                        string packetHeader = packet.Split(' ')[1];
                        //0 is a keep alive packet with no content to handle
                        if (packetHeader != "0" && !TriggerHandler(packetHeader, packet, false))
                        {
                            Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader);
                        }
                    }                 
                }
                else
                {
                    //simple messaging
                    string packetHeader = packet.Split(' ')[0];

                    if (!TriggerHandler(packetHeader, packet, false))
                    {
                        Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader);
                    }
                }

            }
        }

        public void RegisterForMapNotification()
        {
            CurrentMap.NotifyClients += GetNotification;
        }

        private void GetNotification(object sender, EventArgs e)
        {
            KeyValuePair<String, ClientSession> packet = (KeyValuePair<String, ClientSession>)sender;

            //exclude myself from passive notification
            if (packet.Value.Client.ClientId != this.Client.ClientId)
                _client.SendPacket(packet.Key);
        }

        public void Initialize(EncryptionBase encryptor, IList<Type> packetHandlers, Guid uniqueIdentifier)
        {
            _encryptor = encryptor;
            _uniqueIdentifier = uniqueIdentifier;
            _client.Initialize(encryptor);

            //dynamically create instances of packethandlers
            GenerateHandlerReferences(packetHandlers);
        }

        private bool TriggerHandler(string packetHeader, string packet, bool force)
        {
            KeyValuePair<Packet, Tuple<MethodInfo, object>> methodInfo = Handlers.SingleOrDefault(h => h.Key.Header.Equals(packetHeader));

            if (methodInfo.Value != null)
            {
                if (!force && methodInfo.Key.Amount > 1 && !_waitForPackets)
                {
                    //we need to wait for more
                    _waitForPackets = true;
                    _waitForPacketsAmount = methodInfo.Key.Amount;
                    _packetQueue.Add(packet != String.Empty ? packet : String.Format("1 {0} ", packetHeader));
                    return false;
                }

                string result = (string)methodInfo.Value.Item1.Invoke(methodInfo.Value.Item2, new object[] { packet });

                //check for returned packet
                if (!String.IsNullOrEmpty(result))
                {
                    //Send reply message to the client
                    Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("MSG_SENT"), result);
                    _client.SendPacket(result);
                }

                return true;
            }

            return false;
        }

        private bool GenerateHandlerReferences(IList<Type> handlerTypes)
        {
            foreach (Type handlerType in handlerTypes)
            {
                object handler = Activator.CreateInstance(handlerType, new object[] { this });

                foreach (MethodInfo methodInfo in handlerType.GetMethods().Where(x => x.GetCustomAttributes(false).OfType<Packet>().Any()))
                {
                    Packet packetAttribute = methodInfo.GetCustomAttributes(false).OfType<Packet>().SingleOrDefault();

                    if (packetAttribute != null)
                    {
                        Handlers.Add(packetAttribute, new Tuple<MethodInfo, object>(methodInfo, handler));
                    }
                }
            }

            return false;
        }
    }
}
