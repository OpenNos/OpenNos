using OpenNos.Core.Communication.Scs.Communication.Channels;
using OpenNos.Core.Communication.Scs.Communication.Channels.Tcp;
using OpenNos.Core.Communication.Scs.Communication.EndPoints.Tcp;

namespace OpenNos.Core.Communication.Scs.Server.Tcp
{
    /// <summary>
    /// This class is used to create a TCP server.
    /// </summary>
    public class ScsTcpServer : ScsServerBase
    {
        #region Members

        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ScsTcpServer object.
        /// </summary>
        /// <param name="endPoint">The endpoint address of the server to listen incoming connections</param>
        public ScsTcpServer(ScsTcpEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a TCP connection listener.
        /// </summary>
        /// <returns>Created listener object</returns>
        protected override IConnectionListener CreateConnectionListener()
        {
            return new TcpConnectionListener(_endPoint);
        }

        #endregion
    }
}