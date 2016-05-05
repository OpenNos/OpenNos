using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Server;
using OpenNos.Core.Threading;
using System;
using System.Collections.Generic;

namespace OpenNos.Core
{
    public class NetworkClient : ScsServerClient
    {
        #region Members

        private EncryptionBase _encryptor;
        private SequentialItemProcessor<string> _queue;

        #endregion

        #region Instantiation

        public NetworkClient(ICommunicationChannel communicationChannel) : base(communicationChannel)
        {
            _queue = new SequentialItemProcessor<string>(Send);
            _queue.Start();
        }

        #endregion

        #region Methods

        public void Initialize(EncryptionBase encryptor)
        {
            _encryptor = encryptor;
        }

        public void Send(string packet)
        {
            ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(packet));
            SendMessage(rawMessage);
        }

        public bool SendPacket(string packet)
        {
            try
            {
                _queue.EnqueueMessage(packet);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendPacketFormat(string packet, params object[] param)
        {
            return SendPacket(String.Format(packet, param));
        }

        public bool SendPackets(IEnumerable<String> packets)
        {
            bool result = true;

            //TODO maybe send at once with delimiter
            foreach (string packet in packets)
            {
                result = result && SendPacket(packet);
            }

            return result;
        }

        #endregion
    }
}