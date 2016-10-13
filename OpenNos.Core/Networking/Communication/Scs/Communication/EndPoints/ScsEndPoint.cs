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
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Server;
using System;

namespace OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints
{
    /// <summary>
    /// Represents a server side end point in SCS.
    /// </summary>
    public abstract class ScsEndPoint
    {
        #region Methods

        /// <summary>
        /// Create a Scs End Point from a string. Address must be formatted as: protocol://address
        /// For example: tcp://89.43.104.179:10048 for a TCP endpoint with IP 89.43.104.179 and port 10048.
        /// </summary>
        /// <param name="endPointAddress">Address to create endpoint</param>
        /// <returns>Created end point</returns>
        public static ScsEndPoint CreateEndPoint(string endPointAddress)
        {
            // Check if end point address is null
            if (string.IsNullOrEmpty(endPointAddress))
            {
                throw new ArgumentNullException("endPointAddress");
            }

            // If not protocol specified, assume TCP.
            var endPointAddr = endPointAddress;
            if (!endPointAddr.Contains("://"))
            {
                endPointAddr = "tcp://" + endPointAddr;
            }

            // Split protocol and address parts
            var splittedEndPoint = endPointAddr.Split(new[] { "://" }, StringSplitOptions.RemoveEmptyEntries);
            if (splittedEndPoint.Length != 2)
            {
                throw new ApplicationException(endPointAddress + " is not a valid endpoint address.");
            }

            // Split end point, find protocol and address
            var protocol = splittedEndPoint[0].Trim().ToLower();
            var address = splittedEndPoint[1].Trim();
            switch (protocol)
            {
                case "tcp":
                    return new ScsTcpEndPoint(address);

                default:
                    throw new ApplicationException("Unsupported protocol " + protocol + " in end point " + endPointAddress);
            }
        }

        /// <summary>
        /// Creates a Scs Server that uses this end point to connect to server.
        /// </summary>
        /// <returns>Scs Client</returns>
        public abstract IScsClient CreateClient();

        /// <summary>
        /// Creates a Scs Server that uses this end point to listen incoming connections.
        /// </summary>
        /// <returns>Scs Server</returns>
        public abstract IScsServer CreateServer();

        #endregion
    }
}