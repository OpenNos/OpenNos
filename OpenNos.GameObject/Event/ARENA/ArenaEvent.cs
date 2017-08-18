using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
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

                                                  o.Session.SendPacket(o.Session.Character.GenerateBsInfo(0, 2, s.Time, 8));
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
                                          ArenaMember[] arenamembers = ServerManager.Instance.ArenaMembers.Where(o => o.GroupId == member.GroupId || o.GroupId == s.GroupId).OrderBy(o => o.GroupId).ToArray();
                                          for (int i = 0; i < 6; i++)
                                          {
                                              ItemInstance item = Inventory.InstantiateItemInstance((short)(4433 + (i > 2 ? 5 - i : i)), member.Session.Character.CharacterId);
                                              map.MapDesignObjects.Add(new MapDesignObject()
                                              {
                                                  ItemInstance = item,
                                                  ItemInstanceId = item.Id,
                                                  CharacterId = member.Session.Character.CharacterId,
                                                  MapX = (short)(i > 2 ? 120 : 19),
                                                  MapY = (short)(i > 2 ? 35 + (i % 3) * 4 : 36 + (i % 3) * 4),
                                              });
                                          }

                                          arenamembers.ToList().ForEach(o =>
                                          {
                                              o.Session.SendPacket(o.Session.Character.GenerateBsInfo(0, 2, s.Time, 2));
                                              o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RIVAL_ARENA_TEAM_FOUND"), 10));

                                              Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(time =>
                                              {
                                                  o.Session.SendPacket("ta_close");
                                                  Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(time2 =>
                                                  {
                                                      int i = Array.IndexOf(arenamembers, o) + 1;
                                                      ServerManager.Instance.ChangeMapInstance(o.Session.Character.CharacterId, map.MapInstanceId, o.GroupId == member.GroupId ? 125 : 14, (o.GroupId == member.GroupId ? 37 : 38) + (i % 3) * 2);
                                                      o.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SELECT_ORDER_ARENA_TIME"), 0));
                                                      o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SELECT_ORDER_ARENA"), 10));
                                                      o.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SELECT_ORDER_ARENA"), 0));
                                                      if(o.GroupId == s.GroupId)
                                                      {
                                                          o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ZENAS"), 10));
                                                      }
                                                      else
                                                      {
                                                          o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ERENIA"), 10));
                                                      }
                                                      o.Session.SendPacket("ta_m 0 0 0 0 0");
                                                      o.Session.SendPacket("ta_p 0 2 5 5 -1.-1.-1.-1.-1 -1.-1.-1.-1.-1 -1.-1.-1.-1.-1 -1.-1.-1.-1.-1 -1.-1.-1.-1.-1 -1.-1.-1.-1.-1");
                                                      o.Session.SendPacket("ta_sv 0");
                                                      o.Session.SendPacket("ta_st 0");
                                                      o.Session.SendPacket("ta_m 3 0 0 60 0");
                                                  });
                                              });
                                          });
                                          ServerManager.Instance.ArenaMembers.RemoveAll(o => o.GroupId == member.GroupId || o.GroupId == s.GroupId);
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
                                      s.Session.SendPacket(s.Session.Character.GenerateBsInfo(1, 2, s.Time, 8));
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