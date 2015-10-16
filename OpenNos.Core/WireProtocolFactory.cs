using Hik.Communication.Scs.Communication.Protocols;

namespace OpenNos.Core
{
    public class WireProtocolFactory<EncryptorT> : IScsWireProtocolFactory
        where EncryptorT : EncryptionBase
    {
        public IScsWireProtocol CreateWireProtocol()
        {
            return new WireProtocol<EncryptorT>();
        }
    }
}
