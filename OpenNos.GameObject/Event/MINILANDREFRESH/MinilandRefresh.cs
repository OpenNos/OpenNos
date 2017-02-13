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

using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.WebApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject.Event
{
    public class MinilandRefresh
    {
        #region Methods

        public static void GenerateMinilandEvent()
        {
            ServerManager.Instance.SaveAll();
            GeneralLogDTO gen = DAOFactory.GeneralLogDAO.LoadByAccount(null).LastOrDefault(s => s.LogData == "MinilandRefresh" && s.LogType == "World");

            foreach (var genlog in ServerManager.Instance.Sessions.Where(s=>s.HasSelectedCharacter).SelectMany(c => c.Character.GeneralLogs.Where(s => s.LogData == "MINILAND" && s.Timestamp > DateTime.Now.AddDays(-1)).GroupBy(s => s.CharacterId)))
            {
                if (genlog.Key != null)
                {
                    ClientSession Session = ServerManager.Instance.GetSessionByCharacterId((long)genlog.Key);
                    if (Session != null)
                    {
                        Session.Character.GetReput(2 * genlog.Count());
                        Session.Character.MinilandPoint = 2000;
                    }
                    else if (!ServerCommunicationClient.Instance.HubProxy.Invoke<bool>("CharacterIsConnected", ServerManager.ServerGroup, (long)genlog.Key).Result)
                    {
                        if (gen == null || gen.Timestamp.Day != DateTime.Now.Day)
                        {
                            CharacterDTO chara = DAOFactory.CharacterDAO.LoadById((long)genlog.Key);
                            if (chara != null)
                            {
                                chara.Reput += 2 * genlog.Count();
                                chara.MinilandPoint = 2000;
                                DAOFactory.GeneralLogDAO.Insert(new GeneralLogDTO { IpAddress = Session.IpAddress, LogData = "MinilandRefresh", LogType = "World", Timestamp = DateTime.Now });
                                DAOFactory.CharacterDAO.InsertOrUpdate(ref chara);
                            }
                        }
                    }
                }
                
            }
            ServerManager.Instance.StartedEvents.Remove(EventType.MINILANDREFRESHEVENT);
        }

        #endregion
    }
}