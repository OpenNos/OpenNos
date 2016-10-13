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

using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints;

namespace OpenNos.Core.Networking.Communication.ScsServices.Client
{
    /// <summary>
    /// This class is used to build service clients to remotely invoke methods of a SCS service.
    /// </summary>
    public class ScsServiceClientBuilder
    {
        #region Methods

        /// <summary>
        /// Creates a client to connect to a SCS service.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpoint">EndPoint of the server</param>
        /// <param name="clientObject">
        /// Client-side object that handles remote method calls from server to client. May be null if
        /// client has no methods to be invoked by server
        /// </param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(ScsEndPoint endpoint, object clientObject = null) where T : class
        {
            return new ScsServiceClient<T>(endpoint.CreateClient(), clientObject);
        }

        /// <summary>
        /// Creates a client to connect to a SCS service.
        /// </summary>
        /// <typeparam name="T">Type of service interface for remote method call</typeparam>
        /// <param name="endpointAddress">EndPoint address of the server</param>
        /// <param name="clientObject">
        /// Client-side object that handles remote method calls from server to client. May be null if
        /// client has no methods to be invoked by server
        /// </param>
        /// <returns>Created client object to connect to the server</returns>
        public static IScsServiceClient<T> CreateClient<T>(string endpointAddress, object clientObject = null) where T : class
        {
            return CreateClient<T>(ScsEndPoint.CreateEndPoint(endpointAddress), clientObject);
        }

        #endregion
    }
}