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

using OpenNos.Core.Networking.Communication.Scs.Communication.Messengers;
using OpenNos.Core.Networking.Communication.Scs.Server;

namespace OpenNos.Core.Networking.Communication.ScsServices.Service
{
    /// <summary>
    /// This class is used to create service client objects that is used in server-side.
    /// </summary>
    public static class ScsServiceClientFactory
    {
        #region Methods

        /// <summary>
        /// Creates a new service client object that is used in server-side.
        /// </summary>
        /// <param name="serverClient">Underlying server client object</param>
        /// <param name="requestReplyMessenger">
        /// RequestReplyMessenger object to send/receive messages over serverClient
        /// </param>
        /// <returns></returns>
        public static IScsServiceClient CreateServiceClient(IScsServerClient serverClient, RequestReplyMessenger<IScsServerClient> requestReplyMessenger)
        {
            return new ScsServiceClient(serverClient, requestReplyMessenger);
        }

        #endregion
    }
}