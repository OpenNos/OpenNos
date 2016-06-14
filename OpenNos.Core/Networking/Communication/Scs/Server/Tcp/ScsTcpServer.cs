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

namespace OpenNos.Core.Networking.Communication.Scs.Server.Tcp
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