using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Concurrent;
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
                                              item.Design = (short)(4433 + (i > 2 ? 5 - i : i));
                                              map.MapDesignObjects.Add(new MapDesignObject()
                                              {
                                                  ItemInstance = item,
                                                  ItemInstanceId = item.Id,
                                                  CharacterId = member.Session.Character.CharacterId,
                                                  MapX = (short)(i > 2 ? 120 : 19),
                                                  MapY = (short)(i > 2 ? 35 + (i % 3) * 4 : 36 + (i % 3) * 4),
                                              });
                                          }
                                          ConcurrentBag<ArenaTeamMember> arenateam = new ConcurrentBag<ArenaTeamMember>();
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
                                                      if (o.GroupId == s.GroupId)
                                                      {
                                                          o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ZENAS"), 10));
                                                          arenateam.Add(new ArenaTeamMember(o.Session, ArenaTeamType.ZENAS, null));
                                                          Observable.Interval(TimeSpan.FromSeconds(5)).Subscribe(effect =>
                                                          {
                                                              map.Broadcast(o.Session.Character.GenerateEff(3013));
                                                          });
                                                      }
                                                      else
                                                      {
                                                          o.Session.SendPacket(o.Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ERENIA"), 10));
                                                          arenateam.Add(new ArenaTeamMember(o.Session, ArenaTeamType.ERENIA, null));
                                                          Observable.Interval(TimeSpan.FromSeconds(5)).Subscribe(effect =>
                                                          {
                                                              map.Broadcast(o.Session.Character.GenerateEff(3012));
                                                          });
                                                      }
                                                      o.Session.SendPacket(UserInterfaceHelper.Instance.GenerateTaM(0, 0));
                                                      o.Session.SendPacket("ta_sv 0");
                                                      o.Session.SendPacket("ta_st 0");
                                                      byte timer = 60;
                                                      o.Session.SendPacket(UserInterfaceHelper.Instance.GenerateTaM(3, timer));
                                                      string groups = string.Empty;

                                                      Observable.Interval(TimeSpan.FromMilliseconds(timer * 100)).Subscribe(start2 =>
                                                        {
                                                            bool resettap = false;
                                                            map.MapDesignObjects.ForEach(e =>
                                                                {
                                                                    if (e.ItemInstance.Design >= 4433 && e.ItemInstance.Design <= 4435)
                                                                    {
                                                                        Character chara = map.GetCharactersInRange(e.MapX, e.MapY, 0).FirstOrDefault();
                                                                        if (chara != null)
                                                                        {
                                                                            resettap = true;
                                                                            ArenaTeamMember teammember = arenateam.FirstOrDefault(at => at.Session == chara.Session);
                                                                            if (teammember != null && !arenateam.Any(at => at.Order == (e.ItemInstance.ItemVNum - 4433) && at.ArenaTeamType == (e.MapX == 120 ? ArenaTeamType.ERENIA : ArenaTeamType.ZENAS)))
                                                                            {
                                                                                if (teammember.Order != null)
                                                                                {
                                                                                    MapDesignObject obj = map.MapDesignObjects.FirstOrDefault(mapobj => mapobj.ItemInstance.ItemVNum == e.ItemInstance.ItemVNum && e.MapX == (teammember.ArenaTeamType == ArenaTeamType.ERENIA ? 120 : 19));
                                                                                    if (obj != null)
                                                                                    {
                                                                                        obj.ItemInstance.Design = obj.ItemInstance.ItemVNum;
                                                                                    }
                                                                                }
                                                                                teammember.Order = (byte)(e.ItemInstance.ItemVNum - 4433);
                                                                            }
                                                                        }
                                                                    }
                                                                    else if (e.ItemInstance.Design == 4436)
                                                                    {
                                                                        if (!map.GetCharactersInRange(e.MapX, e.MapY, 0).Any())
                                                                        {
                                                                            resettap = true;
                                                                            ArenaTeamMember teammember = arenateam.FirstOrDefault(at => at.Order == (e.ItemInstance.ItemVNum - 4433) && at.ArenaTeamType == (e.MapX == 120 ? ArenaTeamType.ERENIA : ArenaTeamType.ZENAS));
                                                                            if (teammember != null)
                                                                            {
                                                                                teammember.Order = null;
                                                                            }
                                                                        }
                                                                    }
                                                                    if (!arenateam.Any(at => at.Order == (e.ItemInstance.ItemVNum - 4433) && at.ArenaTeamType == (e.MapX == 120 ? ArenaTeamType.ERENIA : ArenaTeamType.ZENAS)))
                                                                    {
                                                                        if (e.ItemInstance.Design == 4436)
                                                                        {
                                                                            e.ItemInstance.Design = e.ItemInstance.ItemVNum;
                                                                            map.Broadcast(e.GenerateEffect(false));
                                                                        }
                                                                    }
                                                                    else if (e.ItemInstance.Design != 4436)
                                                                    {
                                                                        e.ItemInstance.Design = 4436;
                                                                        map.Broadcast(e.GenerateEffect(false));
                                                                    }
                                                                });

                                                            if (resettap)
                                                            {
                                                                arenateam.ToList().ForEach(arenauser => { arenauser.Session.SendPacket(UserInterfaceHelper.Instance.GenerateTaP(2, arenateam, arenauser.ArenaTeamType, false)); });
                                                            }
                                                        });

                                                      Observable.Timer(TimeSpan.FromSeconds(timer)).Subscribe(start =>
                                                      {
                                                          arenateam.ToList().ForEach(arenauser =>
                                                          {
                                                              if (arenauser.Order == null)
                                                              {
                                                                  for (byte x = 0; x < 3; x++)
                                                                  {
                                                                      if (!arenateam.Any(at => at.ArenaTeamType == arenauser.ArenaTeamType && at.Order == x))
                                                                      {
                                                                          arenauser.Order = x;
                                                                      }
                                                                  }
                                                              }
                                                              arenauser.Session.SendPacket(UserInterfaceHelper.Instance.GenerateTaP(2, arenateam, arenauser.ArenaTeamType, true));
                                                          });
                                                          map.MapDesignObjects.ToArray().ToList().ForEach(md => map.Broadcast(md.GenerateEffect(true)));
                                                          map.MapDesignObjects.RemoveAll(md => true);
                                                      });
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