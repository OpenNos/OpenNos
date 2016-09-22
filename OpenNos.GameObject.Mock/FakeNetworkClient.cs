using OpenNos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;

namespace OpenNos.GameObject.Mock
{
    public class FakeNetworkClient : INetworkClient
    {
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

        public event EventHandler<MessageEventArgs> MessageReceived;

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
    }
}
