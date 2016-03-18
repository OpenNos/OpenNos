using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols.BinarySerialization;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.Protocols
{
    /// <summary>
    /// This class is used to get default protocols.
    /// </summary>
    public static class WireProtocolManager
    {
        #region Methods

        /// <summary>
        /// Creates a default wire protocol object to be used on communicating of applications.
        /// </summary>
        /// <returns>A new instance of default wire protocol</returns>
        public static IScsWireProtocol GetDefaultWireProtocol()
        {
            return new BinarySerializationProtocol();
        }

        /// <summary>
        /// Creates a default wire protocol factory object to be used on communicating of applications.
        /// </summary>
        /// <returns>A new instance of default wire protocol</returns>
        public static IScsWireProtocolFactory GetDefaultWireProtocolFactory()
        {
            return new BinarySerializationProtocolFactory();
        }

        #endregion
    }
}