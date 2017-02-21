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
using System.Reactive.Linq;
using OpenNos.Core;
using OpenNos.Domain;

namespace OpenNos.GameObject.Event
{
    public class LOD
    {
        #region Methods

        public static void GenerateLod(int lodtime = 120)
        {
            const int HornTime = 30;
            const int HornRepawn = 4;
            const int HornStay = 1;
            ServerManager.Instance.EnableMapEffect(98, true);
            foreach (Family fam in ServerManager.Instance.FamilyList)
            {
                fam.LandOfDeath = ServerManager.GenerateMapInstance(150, MapInstanceType.LodInstance);
                fam.LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(0), EventActionType.CLOCK, TimeSpan.FromMinutes(lodtime).TotalSeconds);
                fam.LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(lodtime - HornTime), EventActionType.XPRATE, 3);
                fam.LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(lodtime - HornTime), EventActionType.DROPRATE, 3);
                fam.LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(lodtime), EventActionType.DISPOSE, null);
                for (int i = 0; i < 8; i++)
                {
                    Observable.Timer(TimeSpan.FromMinutes(lodtime - HornTime + HornRepawn * i)).Subscribe(
                    x =>
                    {
                        SpawnDH(fam.LandOfDeath, HornStay);
                    });
                }
            }
            Observable.Timer(TimeSpan.FromMinutes(lodtime)).Subscribe(x => { ServerManager.Instance.StartedEvents.Remove(EventType.LOD); ServerManager.Instance.EnableMapEffect(98, false); });
        }

        private static void SpawnDH(MapInstance LandOfDeath, int HornStay)
        {
            LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(0), EventActionType.SPAWNONLASTENTRY, 443);
            LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(0), EventActionType.MESSAGE, "df 2");
            LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(0), EventActionType.MESSAGE, ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_APPEAR"), 0));
            LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(HornStay), EventActionType.MESSAGE, ServerManager.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_DISAPEAR"), 0));
            LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(HornStay), EventActionType.LOCK, true);
            LandOfDeath.StartMapEvent(TimeSpan.FromMinutes(HornStay), EventActionType.UNSPAWN, 443);
        }

        #endregion
    }
}