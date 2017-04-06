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

using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using System;
using System.Collections.Generic;

namespace OpenNos.Data
{
    public class WorldserverDTO
    {
        #region Instantiation

        public WorldserverDTO(Guid id, ScsTcpEndPoint endpoint, int accountLimit)
        {
            ConnectedAccounts = new Dictionary<string, long>();
            ConnectedCharacters = new Dictionary<string, long>();
            Id = id;
            Endpoint = endpoint;
            AccountLimit = accountLimit;
        }

        #endregion

        #region Properties

        public int AccountLimit { get; set; }

        public int ChannelId { get; set; }

        public Dictionary<string, long> ConnectedAccounts { get; set; }

        public Dictionary<string, long> ConnectedCharacters { get; set; }

        public ScsTcpEndPoint Endpoint { get; set; }

        public Guid Id { get; set; }

        #endregion
    }
}