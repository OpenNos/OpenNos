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
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class ClientSession : IDisposable
    {
        #region Members

        public Boolean healthStop = false;
        private static EncryptionBase _encryptor;
        private Account _account;
        private Character _character;
        private NetworkClient _client;
        private IDictionary<Packet, Tuple<Action<object, string>, object>> _handlerMethods;
        private SequentialItemProcessor<byte[]> _queue;
        private IList<String> _waitForPacketList = new List<String>();

        //Packetwait Packets
        private int? _waitForPacketsAmount;

        private Task healthTask;

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

            //register WCF events
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

        public IDictionary<Packet, Tuple<Action<object, string>, object>> HandlerMethods
        {
            get
            {
                if (_handlerMethods == null)
                {
                    _handlerMethods = new Dictionary<Packet, Tuple<Action<object, string>, object>>();
                }

                return _handlerMethods;
            }

            set
            {
                _handlerMethods = value;
            }
        }

        public Task HealthTask
        {
            get
            {
                return healthTask;
            }
            set
            {
                healthTask = value;
            }
        }

        public int LastKeepAliveIdentity { get; set; }

        public int SessionId { get; set; }

        #endregion

        #region Methods

        public bool CallbackSessionRequest(SessionPacket sessionPacket)
        {
            switch (sessionPacket.Receiver)
            {
                case ReceiverType.All:
                    Client.SendPacket(sessionPacket.Content);
                    break;

                case ReceiverType.AllExceptMe:
                    if (Client.ClientId != sessionPacket.Sender.Client.ClientId)
                    {
                        Client.SendPacket(sessionPacket.Content);
                    }
                    break;

                case ReceiverType.OnlyMe: //necessary?
                    if (Client.ClientId == sessionPacket.Sender.Client.ClientId)
                    {
                        Client.SendPacket(sessionPacket.Content);
                    }
                    break;

                case ReceiverType.OnlySomeone:
                    if (Character != null && (Character.CharacterId == sessionPacket.SomeonesCharacterId || 
                         Character.Name.Equals(sessionPacket.SomeonesCharacterName)))
                    {
                        Client.SendPacket(sessionPacket.Content);
                    }
                    return true;

                case ReceiverType.AllNoEmoBlocked:
                    if (Character != null && !Character.EmoticonsBlocked)
                    {
                        Client.SendPacket(sessionPacket.Content);
                    }
                    break;

                case ReceiverType.AllNoHeroBlocked:
                    if (Character != null && !Character.HeroChatBlocked)
                    {
                        Client.SendPacket(sessionPacket.Content);
                    }
                    break;

                case ReceiverType.Group:
                    if (Character.Group != null && Character.Group.GroupId.Equals(sessionPacket.Sender.Character.Group.GroupId))
                    {
                        Client.SendPacket(sessionPacket.Content);
                    }
                    break;
            }
            return true;
        }

        /// <summary>
        /// Destroy ClientSession
        /// </summary>
        public void Destroy()
        {
            //unregister from WCF events
            ServiceFactory.Instance.CommunicationCallback.CharacterConnectedEvent -= CommunicationCallback_CharacterConnectedEvent;
            ServiceFactory.Instance.CommunicationCallback.CharacterDisconnectedEvent -= CommunicationCallback_CharacterDisconnectedEvent;

            //do everything necessary before removing client, DB save, Whatever

            if (Character != null)
            {
                //disconnect client
                KeyValuePair<long, MapShop> shop = this.CurrentMap.ShopUserList.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(this.Character.CharacterId));
                if (!shop.Equals(default(KeyValuePair<long, MapShop>)))
                {
                    this.CurrentMap.ShopUserList.Remove(shop.Key);

                    ClientLinkManager.Instance.Broadcast(this, Character.GenerateShopEnd(), ReceiverType.All);
                    ClientLinkManager.Instance.Broadcast(this, Character.GeneratePlayerFlag(0), ReceiverType.AllExceptMe);
                }
                ServiceFactory.Instance.CommunicationService.DisconnectCharacter(Character.Name);
                CurrentMap = null;
            }

            if (Account != null)
            {
                ServiceFactory.Instance.CommunicationService.DisconnectAccount(Account.Name);
            }
        }

        public void Dispose()
        {
        }

        public void Initialize(EncryptionBase encryptor, Type packetHandler)
        {
            _encryptor = encryptor;
            _client.Initialize(encryptor);

            //dynamically create packethandler references
            GenerateHandlerReferences(packetHandler);
        }

        public void InitializeAccount(Account account)
        {
            Account = account;
            ServiceFactory.Instance.CommunicationService.ConnectAccount(account.Name, SessionId);
        }

        private void CommunicationCallback_CharacterConnectedEvent(object sender, EventArgs e)
        {
            //TODO filter for friendlist
            string characterNameWhichHasBeenLoggedIn = (string)sender;

            if (Character != null && !Character.Name.Equals(characterNameWhichHasBeenLoggedIn))
            {
                _client.SendPacket(Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_IN"), characterNameWhichHasBeenLoggedIn), 10));
            }
        }

        private void CommunicationCallback_CharacterDisconnectedEvent(object sender, EventArgs e)
        {
            //TODO filter for friendlist
            string characterNameWhichHasBeenLoggedIn = (string)sender;

            if (Character != null && !Character.Name.Equals(characterNameWhichHasBeenLoggedIn))
            {
                _client.SendPacket(Character.GenerateSay(String.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_OUT"), characterNameWhichHasBeenLoggedIn), 10));
            }
        }

        private void GenerateHandlerReferences(Type type)
        {
            //iterate thru each type in the given assembly, the IPacketHandler is expected in the same dll
            foreach (Type handlerType in type.Assembly.GetTypes().Where(p => !p.IsInterface && type.GetInterfaces().FirstOrDefault().IsAssignableFrom(p)))
            {
                object handler = Activator.CreateInstance(handlerType, new object[] { this });

                foreach (MethodInfo methodInfo in handlerType.GetMethods().Where(x => x.GetCustomAttributes(false).OfType<Packet>().Any()))
                {
                    Packet packetAttribute = methodInfo.GetCustomAttributes(false).OfType<Packet>().FirstOrDefault();

                    if (packetAttribute != null)
                    {
                        HandlerMethods.Add(packetAttribute, new Tuple<Action<object, string>, object>(DelegateBuilder.BuildDelegate<Action<object, string>>(methodInfo), handler));
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
            //determine first packet
            if (_encryptor.HasCustomParameter && this.SessionId == 0)
            {
                string sessionPacket = _encryptor.DecryptCustomParameter(packetData);
                Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("PACKET_ARRIVED"), sessionPacket);

                string[] sessionParts = sessionPacket.Split(' ');
                if (sessionParts.Count() < 1)
                    return;
                this.LastKeepAliveIdentity = Convert.ToInt32(sessionParts[0]);

                //set the SessionId if Session Packet arrives
                if (sessionParts.Count() < 2)
                    return;
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
                        ClientLinkManager.Instance.Broadcast(this, Encoding.UTF8.GetString(Convert.FromBase64String("bXNnIDEwIFRoaXMgaXMgYSBHUEwgUFJPSkVDVCAtIE9QRU5OT1Mh")), ReceiverType.All);
                        return;
                    }
                    Logger.Log.DebugFormat(Language.Instance.GetMessageFromKey("MESSAGE_RECEIVED"), packet, _client.ClientId);
                }

                if (_encryptor.HasCustomParameter)
                {
                    //keep alive
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
                            string header = queuedPackets.Split(' ', '^')[1];
                            TriggerHandler(header, queuedPackets, true);
                            _waitForPacketList.Clear();
                            return;
                        }
                    }
                    else
                    {
                        string packetHeader = packet.Split(' ', '^')[1];
                        // 0 is a keep alive packet with no content to handle
                        int permit = 1;
                        if (packetHeader.Length > 0)
                        {
                            if (packetHeader[0] == '$')
                            {
                                if (Account.Authority != AuthorityType.Admin)
                                    permit = 0;
                            }

                            if (packetHeader[0] == '/' || packetHeader[0] == ':' || packetHeader[0] == ';')
                            {
                                TriggerHandler(packetHeader[0].ToString(), packet, false);
                            }
                            else
                            if (permit == 1)
                            {
                                if (packetHeader != "0" && !TriggerHandler(packetHeader, packet, false))
                                {
                                    Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader);
                                }
                            }
                        }
                    }
                }
                else
                {
                    //simple messaging
                    string packetHeader = packet.Split(' ')[0];
                    if (packetHeader[0] == '/' || packetHeader[0] == ':' || packetHeader[0] == ';')
                    {
                        TriggerHandler(packetHeader[0].ToString(), packet, false);
                    }
                    else
                    if (!TriggerHandler(packetHeader, packet, false))
                    {
                        Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader);
                    }
                }
            }
        }

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

        private bool TriggerHandler(string packetHeader, string packet, bool force)
        {
            KeyValuePair<Packet, Tuple<Action<object, string>, object>> action = HandlerMethods.FirstOrDefault(h => h.Key.Header.Equals(packetHeader));

            if (action.Value != null)
            {
                if (!force && action.Key.Amount > 1 && !_waitForPacketsAmount.HasValue)
                {
                    //we need to wait for more
                    _waitForPacketsAmount = action.Key.Amount;
                    _waitForPacketList.Add(packet != String.Empty ? packet : $"1 {packetHeader} ");
                    return false;
                }
                try
                {
                    //call actual handler method
                    action.Value.Item1(action.Value.Item2, packet);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                }
                return true;
            }

            return false;
        }

        #endregion
    }
}