using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Core.Networking.Communication.Scs.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public class NetworkClient : ScsServerClient
    {
        #region Members

        private EncryptionBase _encryptor;

        #endregion

        #region Instantiation

        public NetworkClient(ICommunicationChannel communicationChannel) : base(communicationChannel)
        {
        }

        public void Initialize(EncryptionBase encryptor)
        {
            _encryptor = encryptor;
        }

        #endregion

        #region Methods

        public void SendPacketFormat(string packet, params object[] param)
        {
             SendPacket(String.Format(packet, param));
        }

        public void SendPacket(string packet)
        {
            Task task = new Task(()=>TaskSendPacket(packet));
            task.Start();      
        }

        private void TaskSendPacket(string packet)
        {

            try
            {
                ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(packet));
                SendMessage(rawMessage);
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("PACKET_FAILURE"), packet, ClientId, e.Message);
            }
        }

        public void SendPackets(IEnumerable<String> packets)
        {
           
            //TODO maybe send at once with delimiter
            foreach (string packet in packets)
            {
                SendPacket(packet);
            }
            
        }

        #endregion
    }
}