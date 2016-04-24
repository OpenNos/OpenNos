namespace OpenNos.Core.Networking.Communication.Scs.Communication.Protocols.BinarySerialization
{
    /// <summary>
    /// This class is used to create Binary Serialization Protocol objects.
    /// </summary>
    public class BinarySerializationProtocolFactory : IScsWireProtocolFactory
    {
        #region Methods

        /// <summary>
        /// Creates a new Wire Protocol object.
        /// </summary>
        /// <returns>Newly created wire protocol object</returns>
        public IScsWireProtocol CreateWireProtocol()
        {
            return new BinarySerializationProtocol();
        }

        #endregion
    }
}