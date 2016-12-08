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
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace OpenNos.Handler
{
    public class ServerConfig : IConfigurationSectionHandler
    {
        #region Methods

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            List<Server> liste = new List<Server>();
            foreach (XmlNode server in section.ChildNodes)
            {
                liste.Add(GetServer(server));
            }
            return liste;
        }

        public Server GetServer(XmlNode str)
        {
            Server result = new Server();

            result.Name = str.Attributes["Name"].Value;
            result.WorldIp = str.Attributes["WorldIp"].Value;
            result.ChannelAmount = Convert.ToInt32(str.Attributes["channelAmount"].Value);
            result.WorldPort = Convert.ToInt32(str.Attributes["WorldPort"].Value);
            return result;
        }

        #endregion

        #region Classes

        public class Server
        {
            #region Properties

            public int ChannelAmount { get; set; }

            public string Name { get; set; }

            public string WorldIp { get; set; }

            public int WorldPort { get; set; }

            #endregion
        }

        #endregion
    }
}