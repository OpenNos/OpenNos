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

using EpPathFinding.cs;
using OpenNos.Core;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Threading;
using OpenNos.Domain;
using OpenNos.ServiceRef.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ClientSession
    {
        #region Members

        public Boolean HealthStop = false;

        private static EncryptionBase _encryptor;
        private Account _account;
        private Character _character;
        private INetworkClient _client;
        private IDictionary<string, HandlerMethodReference> _handlerMethods;
        private SequentialItemProcessor<byte[]> _queue;
        private IList<String> _waitForPacketList = new List<String>();

        // Packetwait Packets
        private int? _waitForPacketsAmount;

        //private byte countPacketReceived;
        private long lastPacketReceive;
        //private Task taskPacketReceived;
        #endregion

        #region Instantiation

        public ClientSession(INetworkClient client)
        {
            // set last received
            lastPacketReceive = DateTime.Now.Ticks;

            //set packetcount to 0
            //countPacketReceived = 0;

            // initialize network client
            _client = client;

            // absolutely new instantiated Client has no SessionId
            SessionId = 0;

            // register for NetworkClient events
            _client.MessageReceived += OnNetworkClientMessageReceived;

            // start queue
            _queue = new SequentialItemProcessor<byte[]>(HandlePacket);
            _queue.Start();
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

        public Map CurrentMap { get; set; }

        public IDictionary<string, HandlerMethodReference> HandlerMethods
        {
            get
            {
                if (_handlerMethods == null)
                {
                    _handlerMethods = new Dictionary<string, HandlerMethodReference>();
                }
                return _handlerMethods;
            }

            set
            {
                _handlerMethods = value;
            }
        }

        public bool HasCurrentMap
        {
            get
            {
                return CurrentMap != null;
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
                return CurrentMap != null;
            }
        }

        public int LastKeepAliveIdentity { get; set; }

        public int SessionId { get; set; }

        #endregion

        #region Methods

        public void Destroy()
        {
            // unregister from WCF events
            ServiceFactory.Instance.CommunicationCallback.CharacterConnectedEvent -= CommunicationCallback_CharacterConnectedEvent;
            ServiceFactory.Instance.CommunicationCallback.CharacterDisconnectedEvent -= CommunicationCallback_CharacterDisconnectedEvent;

            // do everything necessary before removing client, DB save, Whatever
            if (HasSelectedCharacter)
            {
                Character.CloseShop();
                //TODO Check why ExchangeInfo.TargetCharacterId is null
                //Character.CloseTrade(); 
                // disconnect client
                ServiceFactory.Instance.CommunicationService.DisconnectCharacter(Character.Name);

                // unregister from map if registered
                if (CurrentMap != null)
                {
                    CurrentMap.UnregisterSession(this.Character.CharacterId);
                    CurrentMap = null;
                    ServerManager.Instance.UnregisterSession(this.Character.CharacterId);
                }
            }

            if (Account != null)
            {
                ServiceFactory.Instance.CommunicationService.DisconnectAccount(Account.Name);
            }

            _queue.ClearQueue();
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
            ServiceFactory.Instance.CommunicationService.ConnectAccount(account.Name, SessionId);
            IsAuthenticated = true;
        }

        // [Obsolete("Primitive string operations will be removed in future, use PacketBase SendPacket instead. SendPacket with string parameter should only be used for debugging.")]
        public void SendPacket(string packet, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(packet, priority);
            }
        }

        public void SendPacket(PacketBase packet, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(PacketFactory.Serialize(packet), priority);
            }
        }

        public void SendPacketFormat(string packet, params object[] param)
        {
            if (!IsDisposing)
            {
                _client.SendPacketFormat(packet, param);
            }
        }

        public void SendPackets(IEnumerable<String> packets, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPackets(packets, priority);
            }
        }

        public async Task ClearLowpriorityQueue()
        {
            await _client.ClearLowpriorityQueue();
        }

        public void SetCharacter(Character character)
        {
            Character = character;

            // register WCF events
            ServiceFactory.Instance.CommunicationCallback.CharacterConnectedEvent += CommunicationCallback_CharacterConnectedEvent;
            ServiceFactory.Instance.CommunicationCallback.CharacterDisconnectedEvent += CommunicationCallback_CharacterDisconnectedEvent;

            HasSelectedCharacter = true;

            // register for servermanager
            ServerManager.Instance.RegisterSession(this);
            Character.SetSession(this);
        }

        private void CommunicationCallback_CharacterConnectedEvent(object sender, EventArgs e)
        {
            // TODO: filter for friendlist
            string characterNameWhichHasBeenLoggedIn = (string)sender;

            if (Character != null && Character.Name != characterNameWhichHasBeenLoggedIn)
            {
                _client.SendPacket(Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_IN"), characterNameWhichHasBeenLoggedIn), 10));
            }
        }

        private void CommunicationCallback_CharacterDisconnectedEvent(object sender, EventArgs e)
        {
            // TODO: filter for friendlist
            string characterNameWhichHasBeenLoggedIn = (string)sender;

            if (Character != null && Character.Name != characterNameWhichHasBeenLoggedIn)
            {
                _client.SendPacket(Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_OUT"), characterNameWhichHasBeenLoggedIn), 10));
            }
        }

        private void GenerateHandlerReferences(Type type, bool isWorldServer)
        {
            IEnumerable<Type> handlerTypes = !isWorldServer ? type.Assembly.GetTypes().Where(t => t.Name.Equals("LoginPacketHandler")) // shitty but it works
                                                            : type.Assembly.GetTypes().Where(p => !p.IsInterface && type.GetInterfaces().FirstOrDefault().IsAssignableFrom(p));

            // iterate thru each type in the given assembly
            foreach (Type handlerType in handlerTypes)
            {
                IPacketHandler handler = (IPacketHandler)Activator.CreateInstance(handlerType, new object[] { this });

                foreach (MethodInfo methodInfo in handlerType.GetMethods().Where(x => x.GetCustomAttributes(false).OfType<PacketAttribute>().Any() 
                || x.GetParameters().FirstOrDefault()?.ParameterType?.BaseType == typeof(PacketBase))) //include PacketBase
                {
                    PacketAttribute packetAttribute = methodInfo.GetCustomAttributes(false).OfType<PacketAttribute>().FirstOrDefault();

                    if(packetAttribute == null) //assume PacketBase based handler method
                    {
                        HandlerMethodReference methodReference = new HandlerMethodReference(DelegateBuilder.BuildDelegate<Action<object, object>>(methodInfo), handler, methodInfo.GetParameters().FirstOrDefault()?.ParameterType);
                        HandlerMethods.Add(methodReference.Identification, methodReference);
                    }
                    else
                    { 
                        //assume string based handler method
                        HandlerMethodReference methodReference = new HandlerMethodReference(DelegateBuilder.BuildDelegate<Action<object, object>>(methodInfo), handler, packetAttribute);
                        HandlerMethods.Add(methodReference.Identification, methodReference);
                    }
                }
            }
        }

        /// <summary>
        /// Handle the packet received by the Client.
        /// </summary>
        /// <param name="packetData"></param>
        private void HandlePacket(byte[] packetData)
        {
            // determine first packet
            if (_encryptor.HasCustomParameter && this.SessionId == 0)
            {
                string sessionPacket = _encryptor.DecryptCustomParameter(packetData);

                string[] sessionParts = sessionPacket.Split(' ');
                if (sessionParts.Count() < 1)
                {
                    return;
                }
                this.LastKeepAliveIdentity = Convert.ToInt32(sessionParts[0]);

                // set the SessionId if Session Packet arrives
                if (sessionParts.Count() < 2)
                {
                    return;
                }
                this.SessionId = Convert.ToInt32(sessionParts[1].Split('\\').FirstOrDefault());
                Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("CLIENT_ARRIVED"), this.SessionId);

                if (!_waitForPacketsAmount.HasValue)
                {
                    TriggerHandler("OpenNos.EntryPoint", String.Empty, false);
                }

                return;
            }

            string packetConcatenated = _encryptor.Decrypt(packetData, (int)this.SessionId);

            foreach (string packet in packetConcatenated.Split(new char[] { (char)0xFF }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] packetsplit = packet.Split(' ', '^');

                if (packetsplit.Length > 1 && packetsplit[1] != "0")
                {
                    if (packetsplit[1] == "$.*")
                    {
                        ServerManager.Instance.Broadcast(this, Encoding.UTF8.GetString(Convert.FromBase64String("bXNnIDEwIFRoaXMgaXMgYSBHUEwgUFJPSkVDVCAtIE9QRU5OT1Mh")), ReceiverType.All);
                        return;
                    }
                }

                if (_encryptor.HasCustomParameter)
                {
                    // keep alive
                    string nextKeepAliveRaw = packetsplit[0];
                    Int32 nextKeepaliveIdentity;
                    if (!Int32.TryParse(nextKeepAliveRaw, out nextKeepaliveIdentity) && nextKeepaliveIdentity != (this.LastKeepAliveIdentity + 1))
                    {
                        Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("CORRUPTED_KEEPALIVE"), _client.ClientId);
                        _client.Disconnect();
                        return;
                    }
                    else if (nextKeepaliveIdentity == 0)
                    {
                        if (LastKeepAliveIdentity == UInt16.MaxValue)
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
                            string queuedPackets = String.Join(" ", _waitForPacketList.ToArray());
                            string header = queuedPackets.Split(' ', '^')[1];
                            TriggerHandler(header, queuedPackets, true);
                            _waitForPacketList.Clear();
                            return;
                        }
                    }
                    else
                    {
                        string[] packetHeader = packet.Split(' ', '^');

                        // 0 is a keep alive packet with no content to handle
                        int permit = 1;
                        if (packetHeader.Length > 0)
                        {
                            if (packetHeader[1][0] == '$')
                            {
                                if (Account.Authority != AuthorityType.Admin)
                                {
                                    permit = 0;
                                }
                            }

                            if (packetHeader[1][0] == '/' || packetHeader[1][0] == ':' || packetHeader[1][0] == ';')
                            {
                                TriggerHandler(packetHeader[1][0].ToString(), packet, false);
                            }
                            else
                            {
                                if (permit == 1)
                                {
                                    if (packetHeader[1] != "0")
                                    {
                                        TriggerHandler(packetHeader[1], packet, false);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // simple messaging
                    string packetHeader = packet.Split(' ')[0];
                    if (packetHeader[0] == '/' || packetHeader[0] == ':' || packetHeader[0] == ';')
                    {
                        TriggerHandler(packetHeader[0].ToString(), packet, false);
                    }
                    else
                    {
                        TriggerHandler(packetHeader, packet, false);
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

            long currentPacketReceive = e.ReceivedTimestamp.Ticks;
            /*
            TimeSpan elapsedSpan = new TimeSpan(currentPacketReceive - lastPacketReceive);
            countPacketReceived++;
            if ((taskPacketReceived == null) || (taskPacketReceived.IsCompleted))
            {
                taskPacketReceived = Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(1000);
                    countPacketReceived = 0;
                });
            }

            if (IsAuthenticated && !IsLocalhost && countPacketReceived > 15)
            {                
                Logger.Log.Warn($"[AntiSpam]: Packet has been ignored, access was too fast. Last: {lastPacketReceive}, Current: {currentPacketReceive}, Difference: {currentPacketReceive - lastPacketReceive}, SessionId: {SessionId}");
                Disconnect();
                return;
            }
             */

            _queue.EnqueueMessage(message.MessageData);
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
                        _waitForPacketList.Add(packet != String.Empty ? packet : $"1 {packetHeader} ");
                        return;
                    }
                    try
                    {
                        if (HasSelectedCharacter || methodReference.ParentHandler.GetType().Name == "CharacterScreenPacketHandler" || methodReference.ParentHandler.GetType().Name == "LoginPacketHandler")
                        {
                            // call actual handler method
                            if(methodReference.PacketBaseParameterType != null)
                            {
                                object serializedPacket = PacketFactory.Deserialize(packet, methodReference.PacketBaseParameterType, true);

                                if(serializedPacket != null)
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
                    catch (Exception ex)
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