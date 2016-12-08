/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

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
            return new TcpCommunicationChannel(TcpHelper.ConnectToServer(new IPEndPoint(IPAddress.Parse(_serverEndPoint.IpAddress), _serverEndPoint.TcpPort), ConnectTimeout));
        }

        #endregion
    }
}