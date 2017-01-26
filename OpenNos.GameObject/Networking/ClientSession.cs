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
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Domain;
using OpenNos.WebApi.Reference;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;

namespace OpenNos.GameObject
{
    public class ClientSession
    {
        #region Members

        public bool HealthStop = false;

        private static EncryptionBase _encryptor;
        private Account _account;
        private Character _character;
        private INetworkClient _client;
        private IDictionary<string, HandlerMethodReference> _handlerMethods;
        private Random _random;
        private ConcurrentQueue<byte[]> _receiveQueue;
        private object _receiveQueueObservable;
        private IList<string> _waitForPacketList = new List<string>();

        // Packetwait Packets
        private int? _waitForPacketsAmount;

        // private byte countPacketReceived;
        private long lastPacketReceive;

        #endregion

        #region Instantiation

        public ClientSession(INetworkClient client)
        {
            // set last received
            lastPacketReceive = DateTime.Now.Ticks;

            // lag mode
            _random = new Random((int)client.ClientId);

            // initialize lagging mode
            bool isLagMode = System.Configuration.ConfigurationManager.AppSettings["LagMode"].ToLower() == "true";

            // initialize network client
            _client = client;

            // absolutely new instantiated Client has no SessionId
            SessionId = 0;

            // register for NetworkClient events
            _client.MessageReceived += OnNetworkClientMessageReceived;

            // start observer for receiving packets
            _receiveQueue = new ConcurrentQueue<byte[]>();
            _receiveQueueObservable = Observable.Interval(new TimeSpan(0, 0, 0, 0, isLagMode ? 1000 : 10))
                .Subscribe(x => HandlePackets());
        }

        #endregion

        #region Properties

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
                if (_character == null || !HasSelectedCharacter)
                {
                    // cant access an
                    Logger.Log.Warn("Uninitialized Character cannot be accessed.");
                }

                return _character;
            }

            private set
            {
                _character = value;
            }
        }

        public long ClientId
        {
            get
            {
                return _client.ClientId;
            }
        }

        public MapInstance CurrentMapInstance { get; set; }

        public IDictionary<string, HandlerMethodReference> HandlerMethods
        {
            get
            {
                return _handlerMethods ?? (_handlerMethods = new Dictionary<string, HandlerMethodReference>());
            }

            set
            {
                _handlerMethods = value;
            }
        }

        public bool HasCurrentMapInstance
        {
            get
            {
                return CurrentMapInstance != null;
            }
        }

        public bool HasSelectedCharacter { get; set; }

        public bool HasSession
        {
            get
            {
                return _client != null;
            }
        }

        public string IpAddress
        {
            get
            {
                return _client.IpAddress;
            }
        }

        public bool IsAuthenticated { get; set; }

        public bool IsConnected
        {
            get
            {
                return _client.IsConnected;
            }
        }

        public bool IsDisposing
        {
            get
            {
                return _client.IsDisposing;
            }

            set
            {
                _client.IsDisposing = value;
            }
        }

        public bool IsLocalhost
        {
            get
            {
                return IpAddress.Contains("127.0.0.1");
            }
        }

        public bool IsOnMap
        {
            get
            {
                return CurrentMapInstance != null;
            }
        }

        public int LastKeepAliveIdentity { get; set; }

        public int SessionId { get; set; }
        public DateTime RegisterTime { get; internal set; }

        #endregion

        #region Methods

        public void ClearLowPriorityQueue()
        {
            _client.ClearLowPriorityQueue();
        }

        public void Destroy()
        {
            // unregister from WCF events
            ServerCommunicationClient.Instance.CharacterConnectedEvent -= OnOtherCharacterConnected;
            ServerCommunicationClient.Instance.CharacterDisconnectedEvent -= OnOtherCharacterDisconnected;

            // do everything necessary before removing client, DB save, Whatever
            if (HasSelectedCharacter)
            {
                Character.Dispose();

                // TODO Check why ExchangeInfo.TargetCharacterId is null Character.CloseTrade();
                // disconnect client
                ServerCommunicationClient.Instance.HubProxy.Invoke("DisconnectCharacter", ServerManager.ServerGroup, Character.Name, Character.CharacterId).Wait();

                // unregister from map if registered
                if (CurrentMapInstance != null)
                {
                    CurrentMapInstance.UnregisterSession(Character.CharacterId);
                    CurrentMapInstance = null;
                    ServerManager.Instance.UnregisterSession(Character.CharacterId);
                }
            }

            if (Account != null)
            {
                ServerCommunicationClient.Instance.HubProxy.Invoke("DisconnectAccount", Account.Name).Wait();
            }

            ClearReceiveQueue();
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void Initialize(EncryptionBase encryptor, Type packetHandler, bool isWorldServer)
        {
            _encryptor = encryptor;
            _client.Initialize(encryptor);

            // dynamically create packethandler references
            GenerateHandlerReferences(packetHandler, isWorldServer);
        }

        public void InitializeAccount(Account account)
        {
            Account = account;
            ServerCommunicationClient.Instance.HubProxy.Invoke("ConnectAccount", ServerManager.Instance.WorldId, account.Name, SessionId);
            IsAuthenticated = true;
        }

        // [Obsolete("Primitive string operations will be removed in future, use PacketDefinition
        // SendPacket instead. SendPacket with string parameter should only be used for debugging.")]
        public void SendPacket(string packet, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(packet, priority);
            }
        }

        public void SendPacket(PacketDefinition packet, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(PacketFactory.Serialize(packet), priority);
            }
        }

        public void SendPacketAfterWait(string packet, int Millisecond)
        {
            if (!IsDisposing)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(Millisecond))
                .Subscribe(
                 o =>
                 {
                     SendPacket(packet);
                 });
            }
        }

        public void SendPacketFormat(string packet, params object[] param)
        {
            if (!IsDisposing)
            {
                _client.SendPacketFormat(packet, param);
            }
        }

        public void SendPackets(IEnumerable<string> packets, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPackets(packets, priority);
            }
        }

        public void SetCharacter(Character character)
        {
            Character = character;

            // register WCF events
            ServerCommunicationClient.Instance.CharacterConnectedEvent += OnOtherCharacterConnected;
            ServerCommunicationClient.Instance.CharacterDisconnectedEvent += OnOtherCharacterDisconnected;

            HasSelectedCharacter = true;

            // register for servermanager
            ServerManager.Instance.RegisterSession(this);
            Character.SetSession(this);
            Character.Buff = new Buff.BuffContainer(this);
        }

        private void ClearReceiveQueue()
        {
            byte[] outPacket;
            while (_receiveQueue.TryDequeue(out outPacket))
            {
            }
        }

        private void OnOtherCharacterConnected(object sender, EventArgs e)
        {
            Tuple<string, string, long> loggedInCharacter = (Tuple<string, string, long>)sender;
            if(ServerManager.ServerGroup != loggedInCharacter.Item1)
            {
                return;
            }
            if (Character.IsFriendOfCharacter(loggedInCharacter.Item3))
            {
                if (Character != null && Character.Name != loggedInCharacter.Item1)
                {
                    _client.SendPacket(Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_IN"), loggedInCharacter.Item1), 10));
                    _client.SendPacket(Character.GenerateFinfo(loggedInCharacter.Item3, true));
                }
            }
            if (Character.Family != null)
            {
                FamilyCharacter chara = Character.Family.FamilyCharacters.FirstOrDefault(s => s.CharacterId == loggedInCharacter.Item3);
                if (chara != null && loggedInCharacter.Item3 != Character?.CharacterId)
                {
                    _client.SendPacket(Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("CHARACTER_FAMILY_LOGGED_IN"), loggedInCharacter.Item1, Language.Instance.GetMessageFromKey(chara.Authority.ToString().ToUpper())), 10));
                }
            }
        }
        private void OnOtherCharacterDisconnected(object sender, EventArgs e)
        {
            Tuple<string, string, long> loggedOutCharacter = (Tuple<string, string, long>)sender;
            if (ServerManager.ServerGroup != loggedOutCharacter.Item1)
            {
                return;
            }
            if (Character.IsFriendOfCharacter(loggedOutCharacter.Item3))

                if (Character != null && Character.Name != loggedOutCharacter.Item2)
                {
                    _client.SendPacket(Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_OUT"), loggedOutCharacter.Item3), 10));
                    _client.SendPacket(Character.GenerateFinfo(loggedOutCharacter.Item3, false));
                }
        }

        private void GenerateHandlerReferences(Type type, bool isWorldServer)
        {
            IEnumerable<Type> handlerTypes = !isWorldServer ? type.Assembly.GetTypes().Where(t => t.Name.Equals("LoginPacketHandler")) // shitty but it works
                                                            : type.Assembly.GetTypes().Where(p =>
                                                            {
                                                                Type interfaceType = type.GetInterfaces().FirstOrDefault();
                                                                return interfaceType != null && !p.IsInterface && interfaceType.IsAssignableFrom(p);
                                                            });

            // iterate thru each type in the given assembly
            foreach (Type handlerType in handlerTypes)
            {
                IPacketHandler handler = (IPacketHandler)Activator.CreateInstance(handlerType, this);

                // include PacketDefinition
                foreach (MethodInfo methodInfo in handlerType.GetMethods().Where(x => x.GetCustomAttributes(false).OfType<PacketAttribute>().Any() || x.GetParameters().FirstOrDefault()?.ParameterType.BaseType == typeof(PacketDefinition)))
                {
                    List<PacketAttribute> packetAttributes = methodInfo.GetCustomAttributes(false).OfType<PacketAttribute>().ToList();

                    // assume PacketDefinition based handler method
                    if (!packetAttributes.Any())
                    {
                        HandlerMethodReference methodReference = new HandlerMethodReference(DelegateBuilder.BuildDelegate<Action<object, object>>(methodInfo), handler, methodInfo.GetParameters().FirstOrDefault()?.ParameterType);
                        HandlerMethods.Add(methodReference.Identification, methodReference);
                    }
                    else
                    {
                        // assume string based handler method
                        foreach (PacketAttribute packetAttribute in packetAttributes)
                        {
                            HandlerMethodReference methodReference = new HandlerMethodReference(DelegateBuilder.BuildDelegate<Action<object, object>>(methodInfo), handler, packetAttribute);
                            HandlerMethods.Add(methodReference.Identification, methodReference);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle the packet received by the Client.
        /// </summary>
        private void HandlePackets()
        {
            byte[] packetData;
            while (_receiveQueue.TryDequeue(out packetData))
            {
                // determine first packet
                if (_encryptor.HasCustomParameter && SessionId == 0)
                {
                    string sessionPacket = _encryptor.DecryptCustomParameter(packetData);

                    string[] sessionParts = sessionPacket.Split(' ');
                    if (!sessionParts.Any())
                    {
                        return;
                    }
                    int lastka;
                    if (!int.TryParse(sessionParts[0], out lastka))
                    {
                        Disconnect();
                    }
                    LastKeepAliveIdentity = lastka;

                    // set the SessionId if Session Packet arrives
                    if (sessionParts.Length < 2)
                    {
                        return;
                    }
                    SessionId = Convert.ToInt32(sessionParts[1].Split('\\').FirstOrDefault());
                    Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("CLIENT_ARRIVED"), SessionId);

                    if (!_waitForPacketsAmount.HasValue)
                    {
                        TriggerHandler("OpenNos.EntryPoint", string.Empty, false);
                    }

                    return;
                }

                string packetConcatenated = _encryptor.Decrypt(packetData, SessionId);

                foreach (string packet in packetConcatenated.Split(new[] { (char)0xFF }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] packetsplit = packet.Split(' ', '^');
                    string packetstring = packet;

                    if (_encryptor.HasCustomParameter)
                    {
                        // keep alive
                        string nextKeepAliveRaw = packetsplit[0];
                        int nextKeepaliveIdentity;
                        if (!int.TryParse(nextKeepAliveRaw, out nextKeepaliveIdentity) && nextKeepaliveIdentity != LastKeepAliveIdentity + 1)
                        {
                            Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("CORRUPTED_KEEPALIVE"), _client.ClientId);
                            _client.Disconnect();
                            return;
                        }
                        if (nextKeepaliveIdentity == 0)
                        {
                            if (LastKeepAliveIdentity == ushort.MaxValue)
                            {
                                LastKeepAliveIdentity = nextKeepaliveIdentity;
                            }
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
                                string queuedPackets = string.Join(" ", _waitForPacketList.ToArray());
                                string header = queuedPackets.Split(' ', '^')[1];
                                TriggerHandler(header, queuedPackets, true);
                                _waitForPacketList.Clear();
                                return;
                            }
                        }
                        else
                        {
                            string[] packetHeader = packet.Split(new[] { ' ', '^' }, StringSplitOptions.RemoveEmptyEntries);

                            // 1 is a keep alive packet with no content to handle
                            int permit = 1;
                            if (packetHeader.Length > 1)
                            {
                                if (packetHeader[1][0] == '$')
                                {
                                    if (Account != null && Account.Authority != AuthorityType.Admin)
                                    {
                                        permit = 0;
                                    }
                                }

                                if (packetHeader[1][0] == '/' || packetHeader[1][0] == ':' || packetHeader[1][0] == ';')
                                {
                                    packetHeader[1] = packetHeader[1][0].ToString();
                                    packetstring = packet.Insert(packet.IndexOf(' ') + 2, " ");
                                }

                                if (permit == 1)
                                {
                                    if (packetHeader[1] != "0")
                                    {
                                        TriggerHandler(packetHeader[1], packetstring, false);
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        string packetHeader = packetstring.Split(' ')[0];
                        // simple messaging
                        if (packetHeader[0] == '/' || packetHeader[0] == ':' || packetHeader[0] == ';')
                        {
                            packetHeader = packetHeader[0].ToString();
                            packetstring = packet.Insert(packet.IndexOf(' ') + 2, " ");
                        }

                        TriggerHandler(packetHeader, packetstring, false);

                    }
                }
            }
        }

        /// <summary>
        /// This will be triggered when the underlying NetworkCleint receives a packet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNetworkClientMessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ScsRawDataMessage;
            if (message == null)
            {
                return;
            }

            if (message.MessageData.Any() && message.MessageData.Length > 2)
            {
                _receiveQueue.Enqueue(message.MessageData);
            }

            lastPacketReceive = e.ReceivedTimestamp.Ticks;
        }

        private void TriggerHandler(string packetHeader, string packet, bool force)
        {
            if (!IsDisposing)
            {
                HandlerMethodReference methodReference = HandlerMethods.ContainsKey(packetHeader) ? HandlerMethods[packetHeader] : null;

                if (methodReference != null)
                {
                    if (methodReference.HandlerMethodAttribute != null && !force && methodReference.HandlerMethodAttribute.Amount > 1 && !_waitForPacketsAmount.HasValue)
                    {
                        // we need to wait for more
                        _waitForPacketsAmount = methodReference.HandlerMethodAttribute.Amount;
                        _waitForPacketList.Add(packet != string.Empty ? packet : $"1 {packetHeader} ");
                        return;
                    }
                    try
                    {
                        if (HasSelectedCharacter || methodReference.ParentHandler.GetType().Name == "CharacterScreenPacketHandler" || methodReference.ParentHandler.GetType().Name == "LoginPacketHandler")
                        {
                            // call actual handler method
                            if (methodReference.PacketDefinitionParameterType != null)
                            {
                                object serializedPacket = PacketFactory.Deserialize(packet, methodReference.PacketDefinitionParameterType, true);

                                if (serializedPacket != null || methodReference.PassNonParseablePacket)
                                {
                                    methodReference.HandlerMethod(methodReference.ParentHandler, serializedPacket);
                                }
                                else
                                {
                                    Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("CORRUPT_PACKET"), packetHeader, packet);
                                }
                            }
                            else
                            {
                                methodReference.HandlerMethod(methodReference.ParentHandler, packet);
                            }
                        }
                    }
                    catch (DivideByZeroException ex)
                    {
                        // disconnect if something unexpected happens
                        Logger.Log.Error("Handler Error SessionId: " + SessionId, ex);
                        Disconnect();
                    }
                }
                else
                {
                    Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader);
                }
            }
            else
            {
                Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("CLIENTSESSION_DISPOSING"), packetHeader);
            }
        }

        #endregion
    }
}