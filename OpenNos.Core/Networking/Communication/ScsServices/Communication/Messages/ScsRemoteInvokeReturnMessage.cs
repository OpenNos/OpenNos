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
    /// This message is sent as response message to a ScsRemoteInvokeMessage. It is used to send
    /// return value of method invocation.
    /// </summary>
    [Serializable]
    public class ScsRemoteInvokeReturnMessage : ScsMessage
    {
        #region Properties

        /// <summary>
        /// If any exception occured during method invocation, this field contains Exception object.
        /// If no exception occured, this field is null.
        /// </summary>
        public ScsRemoteException RemoteException { get; set; }

        /// <summary>
        /// Return value of remote method invocation.
        /// </summary>
        public object ReturnValue { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Represents this object as string.
        /// </summary>
        /// <returns>String representation of this object</returns>
        public override string ToString()
        {
            return $"ScsRemoteInvokeReturnMessage: Returns {ReturnValue}, Exception = {RemoteException}";
        }

        #endregion
    }
}