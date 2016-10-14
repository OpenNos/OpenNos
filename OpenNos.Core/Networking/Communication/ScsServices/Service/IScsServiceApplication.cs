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
    /// Represents a SCS Service Application that is used to construct and manage a SCS service.
    /// </summary>
    public interface IScsServiceApplication
    {
        #region Events

        /// <summary>
        /// This event is raised when a new client connected to the service.
        /// </summary>
        event EventHandler<ServiceClientEventArgs> ClientConnected;

        /// <summary>
        /// This event is raised when a client disconnected from the service.
        /// </summary>
        event EventHandler<ServiceClientEventArgs> ClientDisconnected;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a service object to this service application. Only single service object can be
        /// added for a service interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <typeparam name="TServiceClass">
        /// Service class type. Must be delivered from ScsService and must implement TServiceInterface.
        /// </typeparam>
        /// <param name="service">An instance of TServiceClass.</param>
        void AddService<TServiceInterface, TServiceClass>(TServiceClass service)
            where TServiceClass : ScsService, TServiceInterface
            where TServiceInterface : class;

        /// <summary>
        /// Removes a previously added service object from this service application. It removes
        /// object according to interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <returns>True: removed. False: no service object with this interface</returns>
        bool RemoveService<TServiceInterface>() where TServiceInterface : class;

        /// <summary>
        /// Starts service application.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops service application.
        /// </summary>
        void Stop();

        #endregion
    }
}