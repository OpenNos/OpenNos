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
using OpenNos.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public static class ServerManager
    {
        #region Members

        private static ConcurrentDictionary<Guid, Map> _maps = new ConcurrentDictionary<Guid, Map>();

        #endregion

        #region Event Handlers

        #endregion

        #region Methods

        public static void Initialize()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(@"./Resource/zones");
                FileInfo[] files = dir.GetFiles();
                
                foreach (FileInfo file in files)
                {
                   

                    Guid guid = Guid.NewGuid();
                    Map newMap = new Map(Convert.ToInt16(file.Name), guid);
                    //register for broadcast
                    NotifyChildren += newMap.GetNotification;
                    _maps.TryAdd(guid, newMap);
                }
              
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MAP_LOADED"), files.Length));
            }
            catch (Exception ex) { Logger.Log.Error(ex.Message); }

        }

        public static Map GetMap(short id)
        {
            return _maps.SingleOrDefault(m => m.Value.MapId.Equals(id)).Value;
        }

        public static ConcurrentDictionary<Guid, Map> GetAllMap()
        {
            return _maps;
        }
      
        public static void OnBroadCast(MapPacket mapPacket)
        {
            var handler = NotifyChildren;
            if (handler != null)
            {
                handler(mapPacket, new EventArgs());
            }
        }

        #endregion

        public static EventHandler NotifyChildren { get; set; }
    }
}
