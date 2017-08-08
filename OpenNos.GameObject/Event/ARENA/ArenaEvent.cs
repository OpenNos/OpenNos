using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace OpenNos.GameObject.Event.ARENA
{
    class ArenaEvent
    {
        internal static void GenerateTalentArena()
        {
            Observable.Timer(TimeSpan.FromMinutes(0)).Subscribe(X =>
            {
                int seconds = 0;
                while (seconds < 60 * 60 * 7)
                {
                    ServerManager.Instance.ArenaTeam.ForEach(s =>
                    {
                        s.Time -= 1;
                        if (s.Time == 0)
                        {
                            if (s.GroupId == null)
                            {
                                //bsinfo 1 2 0 7
                                //say 1 -1 10 Aucun participant trouvé
                                //sleep 1 
                                //bsinfo 1 2 300 5
                                //say 1 -1 10 Trouver des participants

                            }
                            else if(ServerManager.Instance.ArenaTeam.Count(g=>g.GroupId == s.GroupId) < 3)
                            {
                                //bsinfo 1 2 -1 4
                                //bsinfo 0 2 300 5
                                //say 1 - 1 10 Essaie à nouveau de former l'équipe.
                            }
                            else
                            {
                                //bsinfo 0 2 300 3
                                //say 1 - 1 10 Essaie de trouver equipe adverse
                            }
                            s.Time = 300;
                        }
                    });

                    seconds++;
                    Thread.Sleep(1000);
                }
                ServerManager.Instance.StartedEvents.Remove(EventType.TALENTARENA);
            });
        }
    }
}