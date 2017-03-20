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

using EpPathFinding;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class MapNpc : MapNpcDTO
    {
        #region Members

        public NpcMonster Npc;
        private int _movetime;
        private Random _random;

        #endregion

        #region Properties

        public short FirstX { get; set; }

        public short FirstY { get; set; }

        public JumpPointParam JumpPointParameters { get; set; }

        public DateTime LastEffect { get; private set; }

        public DateTime LastMove { get; private set; }

        public IDisposable LifeEvent { get; set; }

        public MapInstance MapInstance { get; set; }

        public List<GridPos> Path { get; set; }

        public List<Recipe> Recipes { get; set; }

        public Shop Shop { get; set; }

        public bool IsHostile { get; set; }
        public long Target { get; set; }

        public List<TeleporterDTO> Teleporters { get; set; }

        public bool EffectActivated { get; set; }
        public List<EventContainer> OnDeathEvents { get; set; }

        public bool IsMate { get; set; }
        public bool IsProtected { get; set; }
        #endregion

        #region Methods

        public string GenerateIn()
        {
            NpcMonster npcinfo = ServerManager.Instance.GetNpc(NpcVNum);
            if (npcinfo != null && !IsDisabled)
            {
                return $"in 2 {NpcVNum} {MapNpcId} {MapX} {MapY} {Position} 100 100 {Dialog} 0 0 -1 1 {(IsSitting ? 1 : 0)} -1 - 0 -1 0 0 0 0 0 0 0 0";
            }
            return string.Empty;
        }

        public void RunDeathEvent()
        {
            MapInstance.InstanceBag.NpcsKilled++;
            OnDeathEvents.ForEach(e =>
            {
                EventHelper.Instance.RunEvent(e);
            });
            OnDeathEvents.RemoveAll(s => s != null);
        }

        public string GetNpcDialog()
        {
            return $"npc_req 2 {MapNpcId} {Dialog}";
        }

        public void Initialize(MapInstance currentMapInstance)
        {
            MapInstance = currentMapInstance;
            Initialize();
            JumpPointParameters = new JumpPointParam(MapInstance.Map.Grid, new GridPos(0, 0), new GridPos(0, 0), false, true, true, HeuristicMode.MANHATTAN);
        }

        public override void Initialize()
        {
            _random = new Random(MapNpcId);
            Npc = ServerManager.Instance.GetNpc(NpcVNum);
            LastEffect = DateTime.Now;
            LastMove = DateTime.Now;
            IsHostile = Npc.IsHostile;
            FirstX = MapX;
            EffectActivated = true;
            FirstY = MapY;
            EffectDelay = 4000;
            _movetime = ServerManager.Instance.RandomNumber(500, 3000);
            Path = new List<GridPos>();
            Recipes = ServerManager.Instance.GetReceipesByMapNpcId(MapNpcId);
            Target = -1;
            Teleporters = ServerManager.Instance.GetTeleportersByNpcVNum((short)MapNpcId);
            Shop shop = ServerManager.Instance.GetShopByMapNpcId(MapNpcId);
            if (shop != null)
            {
                shop.Initialize();
                Shop = shop;
            }
        }

        internal void StartLife()
        {
            try
            {
                if (!MapInstance.IsSleeping)
                {
                    NpcLife();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private string GenerateMv2()
        {
            return $"mv 2 {MapNpcId} {MapX} {MapY} {Npc.Speed}";
        }
        public EffectPacket GenerateEff(int effectid)
        {
            return new EffectPacket
            {
                EffectType = 2,
                CharacterId = MapNpcId,
                Id = effectid
            };
        }

        private void NpcLife()
        {
            double time = (DateTime.Now - LastEffect).TotalMilliseconds;
            if (time > EffectDelay)
            {
                if (IsMate || IsProtected)
                {
                    MapInstance.Broadcast(GenerateEff(825), MapX, MapY);
                }
                if (Effect > 0 && EffectActivated)
                {
                    MapInstance.Broadcast(GenerateEff(Effect), MapX, MapY);
                }
                LastEffect = DateTime.Now;
            }

            time = (DateTime.Now - LastMove).TotalMilliseconds;
            if (IsMoving && Npc.Speed > 0 && time > _movetime)
            {
                _movetime = ServerManager.Instance.RandomNumber(500, 3000);
                byte point = (byte)ServerManager.Instance.RandomNumber(2, 4);
                byte fpoint = (byte)ServerManager.Instance.RandomNumber(0, 2);

                byte xpoint = (byte)ServerManager.Instance.RandomNumber(fpoint, point);
                byte ypoint = (byte)(point - xpoint);

                short mapX = FirstX;
                short mapY = FirstY;

                if (MapInstance.Map.GetFreePosition(ref mapX, ref mapY, xpoint, ypoint))
                {
                    double value = (xpoint + ypoint) / (double)(2 * Npc.Speed);
                    Observable.Timer(TimeSpan.FromMilliseconds(1000 * value))
                      .Subscribe(
                          x =>
                          {
                              MapX = mapX;
                              MapY = mapY;
                          });

                    LastMove = DateTime.Now.AddSeconds(value);
                    MapInstance.Broadcast(new BroadcastPacket(null, GenerateMv2(), ReceiverType.All, xCoordinate: mapX, yCoordinate: mapY));
                }
            }
            if (Target == -1)
            {
                if (IsHostile && Shop == null)
                {
                    MapMonster monster = MapInstance.Monsters.FirstOrDefault(s => MapInstance == s.MapInstance && Map.GetDistance(new MapCell { X = MapX, Y = MapY }, new MapCell { X = s.MapX, Y = s.MapY }) < (Npc.NoticeRange > 5 ? Npc.NoticeRange / 2 : Npc.NoticeRange));
                    ClientSession session = MapInstance.Sessions.FirstOrDefault(s => MapInstance == s.Character.MapInstance && Map.GetDistance(new MapCell { X = MapX, Y = MapY }, new MapCell { X = s.Character.PositionX, Y = s.Character.PositionY }) < Npc.NoticeRange);

                    if (monster != null && session != null)
                    {
                        Target = monster.MapMonsterId;
                    }
                }
            }
            else if (Target != -1)
            {
                MapMonster monster = MapInstance.Monsters.FirstOrDefault(s => s.MapMonsterId == Target);
                if (monster == null || monster.CurrentHp < 1)
                {
                    Target = -1;
                    return;
                }
                NpcMonsterSkill npcMonsterSkill = null;
                if (ServerManager.Instance.RandomNumber(0, 10) > 8)
                {
                    npcMonsterSkill = Npc.Skills.Where(s => (DateTime.Now - s.LastSkillUse).TotalMilliseconds >= 100 * s.Skill.Cooldown).OrderBy(rnd => _random.Next()).FirstOrDefault();
                }

                const short damage = 100;
                int distance = Map.GetDistance(new MapCell { X = MapX, Y = MapY }, new MapCell { X = monster.MapX, Y = monster.MapY });
                if (monster.CurrentHp > 0 && (npcMonsterSkill != null && distance < npcMonsterSkill.Skill.Range || distance <= Npc.BasicRange))
                {
                    if ((DateTime.Now - LastEffect).TotalMilliseconds >= 1000 + Npc.BasicCooldown * 200 && !Npc.Skills.Any() || npcMonsterSkill != null)
                    {
                        if (npcMonsterSkill != null)
                        {
                            npcMonsterSkill.LastSkillUse = DateTime.Now;
                            MapInstance.Broadcast($"ct 2 {MapNpcId} 3 {Target} {npcMonsterSkill.Skill.CastAnimation} {npcMonsterSkill.Skill.CastEffect} {npcMonsterSkill.Skill.SkillVNum}");
                        }

                        if (npcMonsterSkill != null && npcMonsterSkill.Skill.CastEffect != 0)
                        {
                            MapInstance.Broadcast(GenerateEff(Effect));
                        }

                        monster.CurrentHp -= damage;

                        MapInstance.Broadcast(npcMonsterSkill != null
                            ? $"su 2 {MapNpcId} 3 {Target} {npcMonsterSkill.SkillVNum} {npcMonsterSkill.Skill.Cooldown} {npcMonsterSkill.Skill.AttackAnimation} {npcMonsterSkill.Skill.Effect} 0 0 {(monster.CurrentHp > 0 ? 1 : 0)} {monster.CurrentHp / monster.Monster.MaxHP * 100} {damage} 0 0"
                            : $"su 2 {MapNpcId} 3 {Target} 0 {Npc.BasicCooldown} 11 {Npc.BasicSkill} 0 0 {(monster.CurrentHp > 0 ? 1 : 0)} {monster.CurrentHp / monster.Monster.MaxHP * 100} {damage} 0 0");

                        LastEffect = DateTime.Now;
                        if (monster.CurrentHp < 1)
                        {
                            if (IsMoving)
                            {
                                Path = MapInstance.Map.StraightPath(new GridPos { x = MapX, y = MapY }, new GridPos { x = FirstX, y = FirstY });
                                if (!Path.Any())
                                {
                                    Path = Map.JPSPlus(JumpPointParameters, new GridPos { x = MapX, y = MapY }, new GridPos { x = FirstX, y = FirstY });
                                }
                            }

                            monster.IsAlive = false;
                            monster.LastMove = DateTime.Now;
                            monster.CurrentHp = 0;
                            monster.CurrentMp = 0;
                            monster.Death = DateTime.Now;
                            Target = -1;
                        }
                    }
                }
                else
                {
                    int maxdistance = Npc.NoticeRange > 5 ? Npc.NoticeRange / 2 : Npc.NoticeRange;
                    if (IsMoving)
                    {
                        const short maxDistance = 5;
                        if (!Path.Any() && distance > 1 && distance < maxDistance)
                        {
                            short xoffset = (short)ServerManager.Instance.RandomNumber(-1, 1);
                            short yoffset = (short)ServerManager.Instance.RandomNumber(-1, 1);

                            Path = MapInstance.Map.StraightPath(new GridPos { x = MapX, y = MapY }, new GridPos { x = (short)(monster.MapX + xoffset), y = (short)(monster.MapY + yoffset) });
                            if (!Path.Any())
                            {
                                Path = Map.JPSPlus(JumpPointParameters, new GridPos { x = MapX, y = MapY }, new GridPos { x = (short)(monster.MapX + xoffset), y = (short)(monster.MapY + yoffset) });
                            }
                        }
                        if (DateTime.Now > LastMove && Npc.Speed > 0 && Path.Any())
                        {
                            int maxindex = Path.Count > Npc.Speed / 2 && Npc.Speed > 1 ? Npc.Speed / 2 : Path.Count;
                            short mapX = (short)Path.ElementAt(maxindex - 1).x;
                            short mapY = (short)Path.ElementAt(maxindex - 1).y;
                            double waitingtime = Map.GetDistance(new MapCell { X = mapX, Y = mapY }, new MapCell { X = MapX, Y = MapY }) / (double)Npc.Speed;
                            MapInstance.Broadcast(new BroadcastPacket(null, $"mv 2 {MapNpcId} {mapX} {mapY} {Npc.Speed}", ReceiverType.All, xCoordinate: mapX, yCoordinate: mapY));
                            LastMove = DateTime.Now.AddSeconds(waitingtime > 1 ? 1 : waitingtime);

                            Observable.Timer(TimeSpan.FromMilliseconds((int)((waitingtime > 1 ? 1 : waitingtime) * 1000)))
                            .Subscribe(
                                x =>
                                {
                                    MapX = mapX;
                                    MapY = mapY;
                                });

                            for (int j = maxindex; j > 0; j--)
                            {
                                Path.RemoveAt(0);
                            }
                        }
                        if (Path.Any() && (MapId != monster.MapId || distance > maxDistance))
                        {
                            Path = MapInstance.Map.StraightPath(new GridPos { x = MapX, y = MapY }, new GridPos { x = FirstX, y = FirstY });
                            if (!Path.Any())
                            {
                                Path = Map.JPSPlus(JumpPointParameters, new GridPos { x = MapX, y = MapY }, new GridPos { x = FirstX, y = FirstY });
                            }
                            Target = -1;
                        }
                    }
                    else
                    {
                        if (distance > maxdistance)
                        {
                            Target = -1;
                        }
                    }
                }
            }
        }

        #endregion
    }
}