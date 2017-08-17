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
                          List<double> groupids = new List<double>();
                          ServerManager.Instance.ArenaMembers.Where(o => o.GroupId != null).ToList().ForEach(o =>
                          {
                              if (ServerManager.Instance.ArenaMembers.Count(g => g.GroupId == o.GroupId) == 3)
                              {
                                  groupids.Add((double)o.GroupId);
                              }
                          });

                          if (s.Time > 0)
                          {
                              if (s.GroupId == null)
                              {
                                  List<ArenaMember> members = ServerManager.Instance.ArenaMembers.Where(e => e.Session != s.Session && e.ArenaType == EventType.TALENTARENA && e.Session.Character.Level <= s.Session.Character.Level + 5 && e.Session.Character.Level >= s.Session.Character.Level - 5).ToList();
                                  members.RemoveAll(o => o.GroupId != null && groupids.Contains((double)o.GroupId));
                                  ArenaMember member = members.FirstOrDefault();
                                  if (member != null)
                                  {
                                      if (member.GroupId == null)
                                      {
                                          groupid++;
                                          member.GroupId = groupid;
                                      }
                                      s.GroupId = member.GroupId;
                                      ServerManager.Instance.ArenaMembers.Where(e => e.ArenaType == EventType.TALENTARENA && e.GroupId == member.GroupId).ToList().ForEach(o =>
                                      {
                                          o.Session.SendPacket(o.Session.Character.GenerateBsInfo(1, 2, -1, 6));
                                          o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ARENA_TEAM_FOUND"), 10));
                                          Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(time =>
                                          {
                                              s.Time = 300;
                                              if (ServerManager.Instance.ArenaMembers.Count(g => g.GroupId == s.GroupId) < 3)
                                              {

                                                  o.Session.SendPacket(o.Session.Character.GenerateBsInfo(0, 2, s.Time, 5));
                                                  o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SEARCH_ARENA_TEAM"), 10));
                                              }
                                              else
                                              {
                                                  o.Session.SendPacket(o.Session.Character.GenerateBsInfo(0, 2, s.Time, 1));
                                                  o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SEARCH_RIVAL_ARENA_TEAM"), 10));
                                              }
                                          });
                                      });
                                  }
                              }
                              else
                              {
                                  if (ServerManager.Instance.ArenaMembers.Count(g => g.GroupId == s.GroupId) == 3)
                                  {
                                      ArenaMember member = ServerManager.Instance.ArenaMembers.FirstOrDefault(o => o.GroupId != null && o.GroupId != (double)s.GroupId && groupids.Contains((double)o.GroupId) && o.Session.Character.Level <= s.Session.Character.Level + 5 && o.Session.Character.Level >= s.Session.Character.Level - 5);
                                      if (member != null)
                                      {
                                          MapInstance map = ServerManager.Instance.GenerateMapInstance(2015, MapInstanceType.TalentArenaMapInstance, new InstanceBag());

                                          ServerManager.Instance.ArenaMembers.Where(o => o.GroupId == member.GroupId || o.GroupId == s.GroupId).ToList().ForEach(o =>
                                          {
                                              o.Session.SendPacket(o.Session.Character.GenerateBsInfo(0, 2, s.Time, 2));
                                              Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(time =>
                                              {
                                                  ServerManager.Instance.ChangeMapInstance(o.Session.Character.CharacterId, map.MapInstanceId, o.GroupId == member.GroupId ? 130 : 10, 40);
                                              });
                                          });
                                      }
                                  }
                              }
                          }
                          else
                          {
                              if (s.GroupId == null)
                              {
                                  if (s.Time != -1)
                                  {
                                      s.Session.SendPacket(s.Session.Character.GenerateBsInfo(1, 2, s.Time, 7));
                                      s.Session.SendPacket(s.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_TEAM_ARENA"), 10));
                                  }
                                  s.Time = 300;
                                  s.Session.SendPacket(s.Session.Character.GenerateBsInfo(1, 2, s.Time, 5));
                                  s.Session.SendPacket(s.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SEARCH_ARENA_TEAM"), 10));
                              }
                              else if (ServerManager.Instance.ArenaMembers.Count(g => g.GroupId == s.GroupId) < 3)
                              {
                                  s.Session.SendPacket(s.Session.Character.GenerateBsInfo(1, 2, -1, 4));
                                  Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(time =>
                                  {
                                      s.Time = 300;
                                      s.Session.SendPacket(s.Session.Character.GenerateBsInfo(1, 2, s.Time, 5));
                                      s.Session.SendPacket(s.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RETRY_SEARCH_ARENA_TEAM"), 10));
                                  });
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