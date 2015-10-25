using OpenNos.Core.Communication.Scs.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core.Communication.Scs.Communication.Channels;
using OpenNos.Core.Communication.Scs.Communication.Messages;
using System.Reflection;
using System.Threading;

namespace OpenNos.Core
{
    public class NetworkClient : ScsServerClient
    {
        #region Members

        private EncryptionBase _encryptor;

        #endregion

        #region Methods

        public NetworkClient(ICommunicationChannel communicationChannel) : base(communicationChannel)
        {

        }

        public void Initialize(EncryptionBase encryptor)
        {
            _encryptor = encryptor;
        }

        public bool SendPacketFormat(string packet, params object[] param)
        {
            return SendPacket(String.Format(packet, param));
        }

        public bool SendPacket(string packet)
        {
            try
            {
                ScsRawDataMessage rawMessage = new ScsRawDataMessage(_encryptor.Encrypt(packet));
                SendMessage(rawMessage);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("PACKET_FAILURE"), packet, ClientId, e.Message);
                return false;
            }
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
