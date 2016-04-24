using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using System.Net;

namespace OpenNos.Core.Networking.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    public class ScsTcpClient : ScsClientBase
    {
        #region Members

        /// <summary>
        /// The endpoint address of the server.
        /// </summary>
        private readonly ScsTcpEndPoint _serverEndPoint;

        #endregion

        #region Instantiation

        /// <summary>
        /// Creates a new ScsTcpClient object.
        /// </summary>
        /// <param name="serverEndPoint">The endpoint address to connect to the server</param>
        public ScsTcpClient(ScsTcpEndPoint serverEndPoint)
        {
            _serverEndPoint = serverEndPoint;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a communication channel using ServerIpAddress and ServerPort.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected override ICommunicationChannel CreateCommunicationChannel()
        {
            return new TcpCommunicationChannel(
                TcpHelper.ConnectToServer(
                    new IPEndPoint(IPAddress.Parse(_serverEndPoint.IpAddress), _serverEndPoint.TcpPort),
                    ConnectTimeout
                    ));
        }

        #endregion
    }
}