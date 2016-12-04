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

using System;

namespace OpenNos.Core.Networking.Communication.ScsServices.Service
{
    /// <summary>
    /// Base class for all services that is serviced by IScsServiceApplication. A class must be
    /// derived from ScsService to serve as a SCS service.
    /// </summary>
    public abstract class ScsService
    {
        #region Members

        /// <summary>
        /// The current client for a thread that called service method.
        /// </summary>
        [ThreadStatic]
        private static IScsServiceClient _currentClient;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current client which called this service method.
        /// </summary>
        /// <remarks>
        /// This property is thread-safe, if returns correct client when called in a service method
        /// if the method is called by SCS system, else throws exception.
        /// </remarks>
        public IScsServiceClient CurrentClient
        {
            get
            {
                return GetCurrentClient();
            }

            set
            {
                _currentClient = value;
            }
        }

        #endregion

        #region Methods

        private IScsServiceClient GetCurrentClient()
        {
            if (_currentClient != null)
            {
                return _currentClient;
            }
            throw new ArgumentNullException("Client channel can not be obtained. CurrentClient property must be called by the thread which runs the service method.");
        }

        #endregion
    }
}