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
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;

namespace OpenNos.GameObject.Event
{
    public class LOD
    {
        #region Methods

        public static void GenerateLod(int lodtime = 60)
        {
            const int hornTime = 30;
            const int hornRepawn = 4;
            const int hornStay = 1;
            EventHelper.Instance.RunEvent(new EventContainer(ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(98)), EventActionType.NPCSEFFECTCHANGESTATE, true));
            LODThread lodThread = new LODThread();
            Observable.Timer(TimeSpan.FromMinutes(0)).Subscribe(X => lodThread.Run(lodtime * 60, hornTime * 60, hornRepawn * 60, hornStay * 60));
        }

        #endregion
    }

    public class LODThread
    {
        #region Methods

        public void Run(int lodTime, int hornTime, int hornRespawn, int hornStay)
        {
            const int interval = 30;
            int dhspawns = 0;

            while (lodTime > 0)
            {
                RefreshLOD(lodTime);

                if (lodTime == hornTime || lodTime == hornTime - hornRespawn * dhspawns)
                {
                    SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
                    foreach (Family fam in ServerManager.Instance.FamilyList.ToArray())
                    {
                        if (fam.LandOfDeath == null)
                        {
                            continue;
                        }
                        EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.CHANGEXPRATE, 3));
                        EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.CHANGEDROPRATE, 3));
                        SpawnDH(fam.LandOfDeath);
                    }
                }        
                else if (lodTime == hornTime - hornRespawn * dhspawns - hornStay)
                {
                    SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
                    foreach (Family fam in ServerManager.Instance.FamilyList.ToArray())
                    {
                        if (fam.LandOfDeath == null)
                        {
                            continue;
                        }
                        DespawnDH(fam.LandOfDeath);
                        dhspawns++;
                    }
                }

                lodTime -= interval;
                Thread.Sleep(interval * 1000);
            }
            EndLOD();
        }

        private void DespawnDH(MapInstance landOfDeath)
        {
            EventHelper.Instance.RunEvent(new EventContainer(ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(98)), EventActionType.NPCSEFFECTCHANGESTATE, false));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_DISAPEAR"), 0)));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.UNSPAWNMONSTERS, 443));
        }

        private void EndLOD()
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            foreach (Family fam in ServerManager.Instance.FamilyList.ToArray())
            {
                if (fam.LandOfDeath == null)
                {
                    continue;
                }
                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.DISPOSEMAP, null));
                fam.LandOfDeath = null;
            }
            ServerManager.Instance.StartedEvents.Remove(EventType.LOD);
            ServerManager.Instance.StartedEvents.Remove(EventType.LODDH);
        }

        private void RefreshLOD(int remaining)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            foreach (Family fam in ServerManager.Instance.FamilyList.ToArray())
            {
                if (fam.LandOfDeath == null)
                {
                    fam.LandOfDeath = ServerManager.Instance.GenerateMapInstance(150, MapInstanceType.LodInstance, new InstanceBag());
                }
                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.CLOCK, remaining * 10));
                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.STARTCLOCK,
                    new Tuple<List<EventContainer>, List<EventContainer>>(new List<EventContainer>(),
                        new List<EventContainer>())));
            }
        }

        private void SpawnDH(MapInstance landOfDeath)
        {
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SPAWNONLASTENTRY, 443));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SENDPACKET, "df 2"));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_APPEAR"), 0)));
        }

        #endregion
    }
}