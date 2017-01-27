using OpenNos.Core;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
   public class LOD
    {
      public static void GenerateLod(int lodtime = 120)
        {
            int HornTime = 30;
            int HornRepawn = 4;
            int HornStay = 1;
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
                    Observable.Timer(TimeSpan.FromMinutes(lodtime - HornTime + (HornRepawn * i))).Subscribe(
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

    }
}
