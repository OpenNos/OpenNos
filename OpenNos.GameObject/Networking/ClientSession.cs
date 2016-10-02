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
using OpenNos.Core.Threading;
using OpenNos.Domain;
using OpenNos.ServiceRef.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
        private IDictionary<PacketAttribute, Tuple<Action<object, string>, object>> _handlerMethods;
        private SequentialItemProcessor<byte[]> _queue;
        private IList<String> _waitForPacketList = new List<String>();

        // Packetwait Packets
        private int? _waitForPacketsAmount;

        private long lastPacketReceive;

        #endregion

        #region Instantiation

        public ClientSession(INetworkClient client)
        {
            // set last received
            lastPacketReceive = DateTime.Now.Ticks;

            // initialize network client
            _client = client;

            // absolutely new instantiated Client has no SessionId
            SessionId = 0;

            // register for NetworkClient events
            _client.MessageReceived += OnNetworkClientMessageReceived;

            // start queue
            _queue = new SequentialItemProcessor<byte[]>(HandlePacket);
            _queue.Start();

            // register WCF events
            ServiceFactory.Instance.CommunicationCallback.CharacterConnectedEvent += CommunicationCallback_CharacterConnectedEvent;
            ServiceFactory.Instance.CommunicationCallback.CharacterDisconnectedEvent += CommunicationCallback_CharacterDisconnectedEvent;
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
                return _character;
            }

            set
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

        public IDictionary<PacketAttribute, Tuple<Action<object, string>, object>> HandlerMethods
        {
            get
            {
                if (_handlerMethods == null)
                {
                    _handlerMethods = new Dictionary<PacketAttribute, Tuple<Action<object, string>, object>>();
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
            if (Character != null)
            {
                Character.CloseShop();

                // disconnect client
                ServiceFactory.Instance.CommunicationService.DisconnectCharacter(Character.Name);

                // unregister from map if registered
                if (CurrentMap != null)
                {
                    CurrentMap.UnregisterSession(this.ClientId);
                    CurrentMap = null;
                }
            }

            if (Account != null)
            {
                ServiceFactory.Instance.CommunicationService.DisconnectAccount(Account.Name);
            }
            ServerManager.Instance.UnregisterSession(this.ClientId);
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
        }

        /// <summary>
        /// Handle Broadcast from Broadcastable
        /// </summary>
        public void ReceiveBroadcast(BroadcastPacket sentPacket)
        {
            if (!IsDisposing && sentPacket != null)
            {
                switch (sentPacket.Receiver)
                {
                    case ReceiverType.All:
                        SendPacket(sentPacket.Content);
                        break;

                    case ReceiverType.AllExceptMe:
                        if (sentPacket.Sender != this)
                        {
                            SendPacket(sentPacket.Content);
                        }
                        break;

                    case ReceiverType.OnlySomeone:
                        {
                            if ((sentPacket.SomeonesCharacterId > 0 || !String.IsNullOrEmpty(sentPacket.SomeonesCharacterName))
                                && (this.Character != null && (this.Character.CharacterId == sentPacket.SomeonesCharacterId
                                || this.Character.Name == sentPacket.SomeonesCharacterName)))
                            {
                                SendPacket(sentPacket.Content);
                            }
                            break;
                        }
                    case ReceiverType.AllNoEmoBlocked:
                        if (!this.Character.EmoticonsBlocked)
                        {
                            SendPacket(sentPacket.Content);
                        }
                        break;

                    case ReceiverType.AllNoHeroBlocked:
                        if (!this.Character.HeroChatBlocked)
                        {
                            SendPacket(sentPacket.Content);
                        }
                        break;

                    case ReceiverType.Group:
                        if (sentPacket.Sender.Character.Group != null && Character.Group != null && Character.Group.GroupId == sentPacket.Sender.Character.Group.GroupId)
                        {
                            SendPacket(sentPacket.Content);
                        }
                        break;
                }
            }
        }

        public void SendPacket(string packet)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(packet);
            }
        }

        public void SendPacketFormat(string packet, params object[] param)
        {
            if (!IsDisposing)
            {
                _client.SendPacketFormat(packet, param);
            }
        }

        public void SendPackets(IEnumerable<String> packets)
        {
            if (!IsDisposing)
            {
                _client.SendPackets(packets);
            }
        }

        private void CommunicationCallback_CharacterConnectedEvent(object sender, EventArgs e)
        {
            // TODO: filter for friendlist
            string characterNameWhichHasBeenLoggedIn = (string)sender;

            if (Character != null && !Character.Name.Equals(characterNameWhichHasBeenLoggedIn))
            {
                _client.SendPacket(Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_IN"), characterNameWhichHasBeenLoggedIn), 10));
            }
        }

        private void CommunicationCallback_CharacterDisconnectedEvent(object sender, EventArgs e)
        {
            // TODO: filter for friendlist
            string characterNameWhichHasBeenLoggedIn = (string)sender;

            if (Character != null && !Character.Name.Equals(characterNameWhichHasBeenLoggedIn))
            {
                _client.SendPacket(Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_OUT"), characterNameWhichHasBeenLoggedIn), 10));
            }
        }

        private void GenerateHandlerReferences(Type type, bool isWorldServer)
        {
            IEnumerable<Type> handlerTypes = !isWorldServer ? type.Assembly.GetTypes().Where(t => t.Name.Equals("LoginPacketHandler")) // shitty but it works
                                                            : type.Assembly.GetTypes().Where(p => !p.IsInterface && type.GetInterfaces().FirstOrDefault().IsAssignableFrom(p));

            // iterate thru each type in the given assembly, the IPacketHandler is expected in the
            // same dll
            foreach (Type handlerType in handlerTypes)
            {
                object handler = Activator.CreateInstance(handlerType, new object[] { this });

                foreach (MethodInfo methodInfo in handlerType.GetMethods().Where(x => x.GetCustomAttributes(false).OfType<PacketAttribute>().Any()))
                {
                    PacketAttribute Packet = methodInfo.GetCustomAttributes(false).OfType<PacketAttribute>().FirstOrDefault();

                    if (Packet != null)
                    {
                        HandlerMethods.Add(Packet, new Tuple<Action<object, string>, object>(DelegateBuilder.BuildDelegate<Action<object, string>>(methodInfo), handler));
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

            long currentPacketReceive = DateTime.Now.Ticks;

            // ignore a packet which has been sent 40ms after the last one
            if (currentPacketReceive - lastPacketReceive < 400000 && !IsLocalhost)
            {
                Logger.Log.Warn($"[AntiSpam]: Packet has been ignored, access was too fast. Last: {lastPacketReceive}, Current: {currentPacketReceive}, Difference: {currentPacketReceive - lastPacketReceive}, SessionId: {SessionId}");
                Disconnect();
                return;
            }

            _queue.EnqueueMessage(message.MessageData);
            lastPacketReceive = DateTime.Now.Ticks;
        }

        private void TriggerHandler(string packetHeader, string packet, bool force)
        {
            if (!IsDisposing)
            {
                KeyValuePair<PacketAttribute, Tuple<Action<object, string>, object>> action = HandlerMethods.FirstOrDefault(h => h.Key.Header.Equals(packetHeader));

                if (action.Value != null)
                {
                    if (!force && action.Key.Amount > 1 && !_waitForPacketsAmount.HasValue)
                    {
                        // we need to wait for more
                        _waitForPacketsAmount = action.Key.Amount;
                        _waitForPacketList.Add(packet != String.Empty ? packet : $"1 {packetHeader} ");
                        return;
                    }
                    try
                    {
                        if (HasSelectedCharacter || action.Value.Item2.GetType().Name == "CharacterScreenPacketHandler" || action.Value.Item2.GetType().Name == "LoginPacketHandler")
                        {
                            // call actual handler method
                            action.Value.Item1(action.Value.Item2, packet);
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