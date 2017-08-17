using OpenNos.Core;
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
                double groupid = 0;
                int seconds = 0;
                while (seconds < 60 * 60 * 7)
                {
                    ServerManager.Instance.ArenaMembers.Where(s => s.ArenaType == EventType.TALENTARENA).ToList().ForEach(s =>
                      {
                          s.Time -= 1;
                          if (s.GroupId == null)
                          {
                              ArenaMember member = ServerManager.Instance.ArenaMembers.FirstOrDefault(e => e.Session != s.Session && e.ArenaType == EventType.TALENTARENA && e.Session.Character.Level <= s.Session.Character.Level + 5 && e.Session.Character.Level >= s.Session.Character.Level - 5);
                              if (member != null)
                              {
                                  if (member.GroupId == null)
                                  {
                                      groupid++;
                                      member.GroupId = groupid;
                                  }
                                  s.GroupId = member.GroupId;
                                  ServerManager.Instance.ArenaMembers.Where(e => e.ArenaType == EventType.TALENTARENA && e.GroupId == member.GroupId).ToList().ForEach(o=>o.Session.SendPacket(o.Session.Character.GenerateSay("UTILISATEUR TROUVé",10)));//TODO REPLACE BY THE CORRECT PACKET
                                  s.Session.SendPacket(s.Session.Character.GenerateBsInfo(1, 2, s.Time, 7));
                              }
                          }
                          if (s.Time == 0)
                          {
                              if (s.GroupId == null)
                              {
                                  s.Session.SendPacket(s.Session.Character.GenerateBsInfo(1, 2, s.Time, 7));
                                  s.Session.SendPacket(s.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_TEAM_ARENA"), 10));
                                  Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(time =>
                                  {
                                      s.Time = 300;
                                      s.Session.SendPacket(s.Session.Character.GenerateBsInfo(1, 2, s.Time, 5));
                                      s.Session.SendPacket(s.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SEARCH_ARENA_TEAM"), 10));
                                  });
                              }
                              else if (ServerManager.Instance.ArenaMembers.Count(g => g.GroupId == s.GroupId) < 3)
                              {
                                  s.Session.SendPacket(s.Session.Character.GenerateBsInfo(1, 2, -1, 4));
                                  s.Time = 300;
                                  s.Session.SendPacket(s.Session.Character.GenerateBsInfo(0, 2, s.Time, 5));
                                  s.Session.SendPacket(s.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RETRY_SEARCH_ARENA_TEAM"), 10));
                              }
                              else
                              {
                                  s.Time = 300;
                                  s.Session.SendPacket(s.Session.Character.GenerateBsInfo(0, 2, s.Time, 3));
                                  s.Session.SendPacket(s.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SEARCH_RIVAL_ARENA_TEAM"), 10));
                              }
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