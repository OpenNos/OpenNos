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

using OpenNos.Core.Networking.Communication.Scs.Client;
using OpenNos.Core.Networking.Communication.Scs.Client.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Server;
using OpenNos.Core.Networking.Communication.Scs.Server.Tcp;
using System;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp
{
    /// <summary>
    /// Represens a TCP end point in SCS.
    /// </summary>
    public class ScsTcpEndPoint : ScsEndPoint
    {
        #region Instantiation

        /// <summary>
        /// Creates a new ScsTcpEndPoint object with specified port number.
        /// </summary>
        /// <param name="tcpPort">Listening TCP Port for incoming connection requests on server</param>
        public ScsTcpEndPoint(int tcpPort)
        {
            TcpPort = tcpPort;
        }

        public ScsTcpEndPoint()
        {
        }

        /// <summary>
        /// Creates a new ScsTcpEndPoint object with specified IP address and port number.
        /// </summary>
        /// <param name="ipAddress">IP address of the server</param>
        /// <param name="port">Listening TCP Port for incoming connection requests on server</param>
        public ScsTcpEndPoint(string ipAddress, int port)
        {
            IpAddress = ipAddress;
            TcpPort = port;
        }

        /// <summary>
        /// Creates a new ScsTcpEndPoint from a string address. Address format must be like
        /// IPAddress:Port (For example: 127.0.0.1:10085).
        /// </summary>
        /// <param name="address">TCP end point Address</param>
        /// <returns>Created ScsTcpEndpoint object</returns>
        public ScsTcpEndPoint(string address)
        {
            var splittedAddress = address.Trim().Split(':');
            IpAddress = splittedAddress[0].Trim();
            TcpPort = Convert.ToInt32(splittedAddress[1].Trim());
        }

        #endregion

        #region Properties

        /// <summary>
        /// IP address of the server.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Listening TCP Port for incoming connection requests on server.
        /// </summary>
        public int TcpPort { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a Scs Client that uses this end point to connect to server.
        /// </summary>
        /// <returns>Scs Client</returns>
        public override IScsClient CreateClient()
        {
            return new ScsTcpClient(this);
        }

        /// <summary>
        /// Creates a Scs Server that uses this end point to listen incoming connections.
        /// </summary>
        /// <returns>Scs Server</returns>
        public override IScsServer CreateServer()
        {
            return new ScsTcpServer(this);
        }

        public override bool Equals(object obj)
        {
            return ((ScsTcpEndPoint)obj).IpAddress == IpAddress
                && ((ScsTcpEndPoint)obj).TcpPort == TcpPort;
        }

        public override int GetHashCode()
        {
            return IpAddress.GetHashCode() + TcpPort.GetHashCode();
        }

        /// <summary>
        /// Generates a string representation of this end point object.
        /// </summary>
        /// <returns>String representation of this end point object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(IpAddress) ? "tcp://" + TcpPort : "tcp://" + IpAddress + ":" + TcpPort;
        }

        #endregion
    }
}