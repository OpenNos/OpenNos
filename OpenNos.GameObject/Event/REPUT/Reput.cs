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
using System.Linq;

namespace OpenNos.GameObject.Event
{
    public class Reput
    {
        #region Methods

        public static void GenerateReput()
        {
            foreach (var genlog in ServerManager.GeneralLogs.Where(s => s.LogData == "MINILAND" && s.Timestamp > DateTime.Now.AddDays(-1)).GroupBy(s => s.CharacterId))
            {
                if (genlog.Key != null)
                {
                    ClientSession Session = ServerManager.Instance.GetSessionByCharacterId((long)genlog.Key);
                    if (Session != null)
                    {
                        Session.Character.GetReput(2 * genlog.Count());
                    }
                    else if (!ServerCommunicationClient.Instance.HubProxy.Invoke<bool>("CharacterIsConnected", ServerManager.ServerGroup, (long)genlog.Key).Result)
                    {
                        CharacterDTO chara = DAOFactory.CharacterDAO.LoadById((long)genlog.Key);
                        if (chara != null)
                        {
                            chara.Reput += 2 * genlog.Count();
                            DAOFactory.CharacterDAO.InsertOrUpdate(ref chara);
                        }
                    }
                }
                ServerManager.Instance.StartedEvents.Remove(EventType.REPUTEVENT);
            }
        }

        #endregion
    }
}