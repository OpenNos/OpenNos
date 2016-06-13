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

using System.Threading;

namespace OpenNos.Core.Networking.Communication.Scs.Server
{
    /// <summary>
    /// Provides some functionality that are used by servers.
    /// </summary>
    public static class ScsServerManager
    {
        #region Members

        /// <summary>
        /// Used to set an auto incremential unique identifier to clients.
        /// </summary>
        private static long _lastClientId;

        #endregion

        #region Methods

        /// <summary>
        /// Gets an unique number to be used as idenfitier of a client.
        /// </summary>
        /// <returns></returns>
        public static long GetClientId()
        {
            return Interlocked.Increment(ref _lastClientId);
        }

        #endregion
    }
}