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
        private Account _account;
        private Character _character;
        private IDictionary<Packet, Tuple<MethodInfo, object>> _packetHandlers;
        private static EncryptionBase _encryptor;
        private SequentialItemProcessor<byte[]> _queue;

        //Packetwait Packets
        private int? _waitForPacketsAmount;
        private IList<String> _waitForPacketList = new List<String>();

        #endregion

        #region Instantiation

        public ClientSession(NetworkClient client)
        {
            _client = client;

            //absolutely new instantiated Client has no SessionId
            SessionId = 0;

            //register for NetworkClient events
            _client.MessageReceived += NetworkClient_MessageReceived;

            //start queue
            _queue = new SequentialItemProcessor<byte[]>(HandlePacket);
            _queue.Start();
        }

        #endregion

        #region Properties

        public int SessionId { get; set; }
        public int LastKeepAliveIdentity { get; set; }

        public Map CurrentMap { get; set; }

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
        public Character Character
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

        #endregion

        #region Methods

        /// <summary>
        /// This will be triggered when the underlying NetworkCleint receives a packet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetworkClient_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ScsRawDataMessage;
            if (message == null)
            {
                return;
            }

            _queue.EnqueueMessage(message.MessageData);
        }

        /// <summary>
        /// Handle the packet received by the Client.
        /// </summary>
        /// <param name="packetData"></param>
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

                if (!_waitForPacketsAmount.HasValue)
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

                    if (_waitForPacketsAmount.HasValue)
                    {
                        if (_waitForPacketList.Count != _waitForPacketsAmount - 1)
                        {
                            _waitForPacketList.Add(packet);
                        }
                        else
                        {
                            _waitForPacketList.Add(packet);
                            _waitForPacketsAmount = null;
                            string queuedPackets = String.Join(" ", _waitForPacketList.ToArray());
                            string header = queuedPackets.Split(' ')[1];
                            TriggerHandler(header, queuedPackets, true);
                            _waitForPacketList.Clear();
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

        /// <summary>
        /// Destroy ClientSession
        /// </summary>
        internal void Destroy()
        {
            //do everything necessary before removing client, DB save, Whatever
        }

        /// <summary>
        /// Register for Map notifications.
        /// </summary>
        public void RegisterForMapNotification()
        {
            CurrentMap.NotifyClients += GetNotification;
        }

        /// <summary>
        /// Unregister for Map notifications.
        /// </summary>
        public void UnregisterForMapNotification()
        {
            CurrentMap.NotifyClients -= GetNotification;
        }

        /// <summary>
        /// Get notificated from outside the Session.
        /// </summary>
        /// <param name="sender">Sender of the packet.</param>
        /// <param name="e">Eventargs e.</param>
        private void GetNotification(object sender, EventArgs e)
        {
            MapPacket mapPacket = (MapPacket)sender;

            switch (mapPacket.Receiver)
            {
                case ReceiverType.All:
                    {
                        _client.SendPacket(mapPacket.Content);
                        break;
                    }
                case ReceiverType.AllExceptMe:
                    {
                        if (mapPacket.Session.Client.ClientId != this.Client.ClientId)
                        {
                            _client.SendPacket(mapPacket.Content);
                        }
                        break;
                    }
                case ReceiverType.OnlyMe:
                    {
                        if (mapPacket.Session.Client.ClientId == this.Client.ClientId)
                        {
                            _client.SendPacket(mapPacket.Content);
                        }
                        break;
                    }
                default:
                    {
                        Logger.Log.ErrorFormat("Unknown Notification ReceiverType for client, {0}");
                        break;
                    }
            }
        }

        public void Initialize(EncryptionBase encryptor, IList<Type> packetHandlers)
        {
            _encryptor = encryptor;
            _client.Initialize(encryptor);

            //dynamically create instances of packethandlers
            GenerateHandlerReferences(packetHandlers);
        }

        private bool TriggerHandler(string packetHeader, string packet, bool force)
        {
            KeyValuePair<Packet, Tuple<MethodInfo, object>> methodInfo = Handlers.SingleOrDefault(h => h.Key.Header.Equals(packetHeader));

            if (methodInfo.Value != null)
            {
                if (!force && methodInfo.Key.Amount > 1 && !_waitForPacketsAmount.HasValue)
                {
                    //we need to wait for more
                    _waitForPacketsAmount = methodInfo.Key.Amount;
                    _waitForPacketList.Add(packet != String.Empty ? packet : String.Format("1 {0} ", packetHeader));
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

        #endregion
    }
}
