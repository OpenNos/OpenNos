using OpenNos.Core;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenNos.GameObject.Mock
{
    public class FakeNetworkClient : INetworkClient
    {
        #region Members

        private long _clientId;
        private bool _isConnected;
        private Queue<string> _receivedPackets;
        private Queue<string> _sentPackets;
        private long lastKeepAliveIdentitiy;

        #endregion

        #region Instantiation

        public FakeNetworkClient()
        {
            _clientId = 0;
            _sentPackets = new Queue<string>();
            _receivedPackets = new Queue<string>();
            lastKeepAliveIdentitiy = 1;
        }

        #endregion

        #region Events

        public event EventHandler<MessageEventArgs> MessageReceived;

        #endregion

        #region Properties

        public long ClientId
        {
            get
            {
                if (_clientId == 0)
                {
                    _clientId = GameObjectMockHelper.Instance.GetNextClientId();
                }

                return _clientId;
            }

            set
            {
                if (value != _clientId)
                {
                    _clientId = value;
                }
            }
        }

        public string IpAddress
        {
            get
            {
                return "127.0.0.1";
            }
        }

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        public bool IsDisposing { get; set; }

        public Queue<string> ReceivedPackets
        {
            get
            {
                return _receivedPackets;
            }
        }

        public Queue<string> SentPackets
        {
            get
            {
                return _sentPackets;
            }
        }

        #endregion

        #region Methods

        public void Disconnect()
        {
            _isConnected = false;
        }

        public void Initialize(EncryptionBase encryptor)
        {
            // NOTHING TO DO HERE
        }

        /// <summary>
        /// Send a Packet to the Server as the Fake client receives it and triggers a Handler method.
        /// </summary>
        /// <param name="packet"></param>
        public void ReceivePacket(string packet)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] buf = encoding.GetBytes(String.Format("{0} {1}", lastKeepAliveIdentitiy, packet));
            MessageReceived?.Invoke(this, new MessageEventArgs(new ScsRawDataMessage(buf), DateTime.Now));
            lastKeepAliveIdentitiy = lastKeepAliveIdentitiy + 1;
        }

        /// <summary>
        /// Send a packet to the Server as the Fake client receives it and triggers a Handler method.
        /// </summary>
        /// <param name="packet">Packet created thru PacketFactory.</param>
        public void ReceivePacket(PacketBase packet)
        {
            ReceivePacket(PacketFactory.Deserialize(packet));
        }

        public void SendPacket(string packet)
        {
            _sentPackets.Enqueue(packet);
        }

        public void SendPacketFormat(string packet, params object[] param)
        {
            _sentPackets.Enqueue(String.Format(packet, param));
        }

        public void SendPackets(IEnumerable<string> packets)
        {
            foreach (string packet in packets)
            {
                SendPacket(packet);
            }
        }

        #endregion
    }
}