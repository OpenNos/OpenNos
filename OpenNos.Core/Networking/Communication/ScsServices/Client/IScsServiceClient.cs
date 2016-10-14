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

namespace OpenNos.Core.Networking.Communication.ScsServices.Client
{
    /// <summary>
    /// Represents a service client that consumes a SCS service.
    /// </summary>
    /// <typeparam name="T">Type of service interface</typeparam>
    public interface IScsServiceClient<out T> : IConnectableClient where T : class
    {
        #region Properties

        /// <summary>
        /// Reference to the service proxy to invoke remote service methods.
        /// </summary>
        T ServiceProxy { get; }

        /// <summary>
        /// Timeout value when invoking a service method. If timeout occurs before end of remote
        /// method call, an exception is thrown. Use -1 for no timeout (wait indefinite). Default
        /// value: 60000 (1 minute).
        /// </summary>
        int Timeout { get; set; }

        #endregion
    }
}