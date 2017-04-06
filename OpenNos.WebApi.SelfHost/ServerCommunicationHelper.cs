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

using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.WebApi.SelfHost
{
    public class ServerCommunicationHelper
    {
        #region Members

        private static ServerCommunicationHelper _instance;

        private Dictionary<string, long> _registeredAccountLogins;

        private List<WorldserverGroupDTO> _worldserverGroups;

        private List<WorldserverDTO> _worldservers;

        #endregion

        #region Properties

        public static ServerCommunicationHelper Instance => _instance ?? (_instance = new ServerCommunicationHelper());

        public Dictionary<string, long> RegisteredAccountLogins
        {
            get
            {
                return _registeredAccountLogins ?? (_registeredAccountLogins = new Dictionary<string, long>());
            }
            set
            {
                _registeredAccountLogins = value;
            }
        }

        public List<WorldserverGroupDTO> WorldserverGroups
        {
            get
            {
                return _worldserverGroups ?? (_worldserverGroups = new List<WorldserverGroupDTO>());
            }
            set
            {
                _worldserverGroups = value;
            }
        }

        public List<WorldserverDTO> Worldservers
        {
            get
            {
                return _worldservers ?? (_worldservers = new List<WorldserverDTO>());
            }
            set
            {
                _worldservers = value;
            }
        }

        #endregion

        #region Methods

        public WorldserverDTO GetWorldserverById(Guid id)
        {
            return Worldservers.SingleOrDefault(w => w.Id == id);
        }

        #endregion
    }
}