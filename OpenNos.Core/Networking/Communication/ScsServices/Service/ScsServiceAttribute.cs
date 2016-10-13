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
    /// Any SCS Service interface class must has this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class ScsServiceAttribute : Attribute
    {
        #region Instantiation

        /// <summary>
        /// Creates a new ScsServiceAttribute object.
        /// </summary>
        public ScsServiceAttribute()
        {
            Version = "NO_VERSION";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Service Version. This property can be used to indicate the code version. This value is
        /// sent to client application on an exception, so, client application can know that service
        /// version is changed. Default value: NO_VERSION.
        /// </summary>
        public string Version { get; set; }

        #endregion
    }
}