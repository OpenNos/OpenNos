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

using System.ServiceModel;

namespace OpenNos.ServiceRef.Internal
{
    public class ServiceFactory
    {
        #region Members

        private static ServiceFactory _instance;
        private CommunicationServiceReference.CommunicationServiceClient _communicationServiceClient;
        private CommunicationCallback _instanceCallback;
        private InstanceContext _instanceContext;

        #endregion

        #region Instantiation

        public ServiceFactory()
        {
            // callback instance will be instantiated once per process
            _instanceCallback = new CommunicationCallback();
            _instanceContext = new InstanceContext(_instanceCallback);
        }

        #endregion

        #region Properties

        public static ServiceFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceFactory();
                }

                return _instance;
            }
        }

        public CommunicationCallback CommunicationCallback
        {
            get
            {
                return _instanceCallback;
            }
        }

        public CommunicationServiceReference.CommunicationServiceClient CommunicationService
        {
            get
            {
                if (_communicationServiceClient == null || _communicationServiceClient.State == CommunicationState.Faulted)
                {
                    _communicationServiceClient = new CommunicationServiceReference.CommunicationServiceClient(_instanceContext);
                }

                return _communicationServiceClient;
            }
        }

        #endregion
    }
}