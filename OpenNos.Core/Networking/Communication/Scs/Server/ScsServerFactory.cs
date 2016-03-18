using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints;

namespace OpenNos.Core.Networking.Communication.Scs.Server
{
    /// <summary>
    /// This class is used to create SCS servers.
    /// </summary>
    public static class ScsServerFactory
    {
        #region Methods

        /// <summary>
        /// Creates a new SCS Server using an EndPoint.
        /// </summary>
        /// <param name="endPoint">Endpoint that represents address of the server</param>
        /// <returns>Created TCP server</returns>
        public static IScsServer CreateServer(ScsEndPoint endPoint)
        {
            return endPoint.CreateServer();
        }

        #endregion
    }
}