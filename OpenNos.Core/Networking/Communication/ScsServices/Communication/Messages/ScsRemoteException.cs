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
using System.Runtime.Serialization;

namespace OpenNos.Core.Networking.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// Represents a SCS Remote Exception. This exception is used to send an exception from an
    /// application to another application.
    /// </summary>
    [Serializable]
    public class ScsRemoteException : Exception
    {
        #region Instantiation

        /// <summary>
        /// Contstructor.
        /// </summary>
        public ScsRemoteException()
        {
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        public ScsRemoteException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public ScsRemoteException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public ScsRemoteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}