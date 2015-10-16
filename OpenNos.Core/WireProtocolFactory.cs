using Hik.Communication.Scs.Communication.Protocols;

namespace OpenNos.Core
{
    public class WireProtocolFactory<EncryptorT> : IScsWireProtocolFactory
        where EncryptorT : EncryptionBase
    {
        private bool _useFraming;

        public WireProtocolFactory(bool useFraming)
        {
            _useFraming = useFraming;
        }

        public IScsWireProtocol CreateWireProtocol()
        {
            return new WireProtocol<EncryptorT>(255, _useFraming);
        }
    }
}
