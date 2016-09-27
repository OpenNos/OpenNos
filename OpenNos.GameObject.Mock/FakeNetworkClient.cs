using OpenNos.Core;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject.Mock
{
    public class FakeNetworkClient : INetworkClient
    {
        #region Events

        public event EventHandler<MessageEventArgs> MessageReceived;

        #endregion

        #region Properties

        public long ClientId
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string IpAddress
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsConnected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsDisposing
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Methods

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Initialize(EncryptionBase encryptor)
        {
            throw new NotImplementedException();
        }

        public void SendPacket(string packet)
        {
            throw new NotImplementedException();
        }

        public void SendPacketFormat(string packet, params object[] param)
        {
            throw new NotImplementedException();
        }

        public void SendPackets(IEnumerable<string> packets)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}