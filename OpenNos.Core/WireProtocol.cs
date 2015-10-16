using System.Text;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Protocols.BinarySerialization;
using System;

namespace OpenNos.Core
{
    /// <summary>
    /// This class is a sample custom wire protocol to use as wire protocol in SCS framework.
    /// It extends BinarySerializationProtocol.
    /// It is used just to send/receive ScsTextMessage messages.
    /// 
    /// Since BinarySerializationProtocol automatically writes message length to the beggining
    /// of the message, a message format of this class is:
    /// 
    /// [Message length (4 bytes)][UTF-8 encoded text (N bytes)]
    /// 
    /// So, total length of the message = (N + 4) bytes;
    /// </summary>
    public class WireProtocol<EncryptorT> : BinarySerializationProtocol
        where EncryptorT : EncryptionBase
    {

        private EncryptionBase _encryptor;

        public WireProtocol()
        {
            _encryptor = (EncryptorT)Activator.CreateInstance(typeof(EncryptorT));
        }

        protected override byte[] SerializeMessage(IScsMessage message)
        {
            return _encryptor.Encrypt(((ScsTextMessage)message).Text);
        }

        protected override IScsMessage DeserializeMessage(byte[] bytes)
        {
            //TODO: optimize, this endecoding stuff is pretty slow
            byte[] differentEncoding = Encoding.Default.GetBytes(System.Text.Encoding.Default.GetString(bytes));

            return new ScsTextMessage(_encryptor.Decrypt(differentEncoding, differentEncoding.Length));
        }
    }
}
