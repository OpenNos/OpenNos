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

using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using System;

namespace OpenNos.Core.Networking.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// This message is sent to invoke a method of a remote application.
    /// </summary>
    [Serializable]
    public class ScsRemoteInvokeMessage : ScsMessage
    {
        #region Properties

        /// <summary>
        /// Method of remote application to invoke.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Parameters of method.
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// Name of the remove service class.
        /// </summary>
        public string ServiceClassName { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Represents this object as string.
        /// </summary>
        /// <returns>String representation of this object</returns>
        public override string ToString()
        {
            return $"ScsRemoteInvokeMessage: {ServiceClassName}.{MethodName}(...)";
        }

        #endregion
    }
}