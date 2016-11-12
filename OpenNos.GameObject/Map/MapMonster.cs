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

using EpPathFinding.cs;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MapMonster : MapMonsterDTO
    {
        #region Members

        private int _movetime;
        private Random _random;

        #endregion

        #region Instantiation

        public MapMonster()
        {
        }

        #endregion

        #region Properties

        public bool Alive { get; set; }

        public int CurrentHp { get; set; }

        public int CurrentMp { get; set; }

        public IDictionary<long, long> DamageList { get; set; }

        public DateTime Death { get; set; }

        public short FirstX { get; set; }

        public short FirstY { get; set; }

        public bool InWaiting { get; set; }

        public DateTime LastEffect { get; set; }

        public DateTime LastMove { get; set; }

        public Map Map { get; set; }

        public NpcMonster Monster { get; set; }

        public List<GridPos> Path { get; set; }

        public bool? Respawn { get; set; }

        public List<NpcMonsterSkill> Skills { get; set; }

        public long Target { get; set; }

        #endregion

        #region Methods

        public string GenerateEff(int effect)
        {
            return $"eff 3 {MapMonsterId} {effect}";
        }

        public string GenerateIn3()
        {
            if (Alive && !IsDisabled)
            {
                return $"in 3 {MonsterVNum} {MapMonsterId} {MapX} {MapY} {Position} {(int)(((float)CurrentHp / (float)Monster.MaxHP) * 100)} {(int)(((float)CurrentMp / (float)Monster.MaxMP) * 100)} 0 0 0 -1 1 0 -1 - 0 -1 0 0 0 0 0 0 0 0";
            }
            else
            {
                return String.Empty;
            }
        }

        public string GenerateMv3()
        {
            return $"mv 3 {MapMonsterId} {MapX} {MapY} {Monster.Speed}";
        }

        public void Initialize(Map currentMap)
        {
            Map = currentMap;
            Initialize();
        }

        public override void Initialize()
        {
            FirstX = MapX;
            FirstY = MapY;
            LastEffect = LastMove = DateTime.Now;
            Target = -1;
            Path = new List<GridPos>();
            Alive = true;
            Respawn = (Respawn.HasValue? Respawn.Value : true);
            Monster = ServerManager.GetNpc(MonsterVNum);
            CurrentHp = Monster.MaxHP;
            CurrentMp = Monster.MaxMP;
            Skills = Monster.Skills.ToList();
            DamageList = new Dictionary<long, long>();
            _random = new Random(MapMonsterId);
            _movetime = _random.Next(400, 3200);
        }

        public bool IsInRange(short mapX, short mapY, byte distance)
        {
            return Map.GetDistance(
             new MapCell()
             {
                 X = mapX,
                 Y = mapY
             }, new MapCell()
             {
                 X = MapX,
                 Y = MapY
             }) <= distance + 1;
        }

        internal void MonsterLife()
        {
            // Respawn
            if (!Alive && Respawn.Value)
            {
                double timeDeath = (DateTime.Now - Death).TotalSeconds;
                if (timeDeath >= Monster.RespawnTime / 10)
                {
                    DamageList = new Dictionary<long, long>();
                    Alive = true;
                    Target = -1;
                    CurrentHp = Monster.MaxHP;
                    CurrentMp = Monster.MaxMP;
                    MapX = FirstX;
                    MapY = FirstY;
                    Path = new List<GridPos>();
                    Map.Broadcast(GenerateIn3());
                    Map.Broadcast(GenerateEff(7), MapX, MapY, 10);
                }
                return;
            }
            else if (Target == -1)
            {
                // Normal Move Mode
                if (!Alive)
                {
                    return;
                }
                if (IsMoving && Monster.Speed > 0)
                {
                    double time = (DateTime.Now - LastMove).TotalMilliseconds;

                    if (Path.Where(s => s != null).Any())
                    {
                        int timetowalk = 1000 / (2 * Monster.Speed);
                        if (time > timetowalk)
                        {
                            int mapX = Path.ElementAt(0).x;
                            int mapY = Path.ElementAt(0).y;
                            Path.RemoveAt(0);

                            Task.Factory.StartNew(async () =>
                            {
                                await Task.Delay(timetowalk);
                                MapX = (short)mapX;
                                MapY = (short)mapY;
                            });
                            LastMove = DateTime.Now;
                            Map.Broadcast(new BroadcastPacket(null, GenerateMv3(), ReceiverType.AllInRange, xCoordinate: mapX, yCoordinate: mapY));
                            return;
                        }
                    }
                    else if (time > _movetime)
                    {
                        _movetime = _random.Next(400, 3200);
                        byte point = (byte)_random.Next(2, 4);
                        byte fpoint = (byte)_random.Next(0, 2);

                        byte xpoint = (byte)_random.Next(fpoint, point);
                        byte ypoint = (byte)(point - xpoint);

                        short mapX = FirstX;
                        short mapY = FirstY;
                        if (Map?.GetFreePosition(ref mapX, ref mapY, xpoint, ypoint) ?? false)
                        {
                            Task.Factory.StartNew(async () =>
                            {
                                await Task.Delay(1000 * (xpoint + ypoint) / (2 * Monster.Speed));
                                this.MapX = mapX;
                                this.MapY = mapY;
                            });
                            LastMove = DateTime.Now.AddSeconds((xpoint + ypoint) / (2 * Monster.Speed));
                            Map.Broadcast(new BroadcastPacket(null, GenerateMv3(), ReceiverType.AllInRange, xCoordinate: mapX, yCoordinate: mapY));
                        }
                    }
                }
                if (Monster.IsHostile)
                {
                    Character character = ServerManager.Instance.Sessions.FirstOrDefault(s => s != null && s.Character != null && s.Character.Hp > 0 && !s.Character.InvisibleGm && !s.Character.Invisible && s.Character.MapId == MapId && Map.GetDistance(new MapCell() { X = MapX, Y = MapY }, new MapCell() { X = s.Character.MapX, Y = s.Character.MapY }) < Monster.NoticeRange)?.Character;
                    if (character != null)
                    {
                        Target = character.CharacterId;
                        if (!Monster.NoAggresiveIcon)
                        {
                            character.Session.SendPacket(GenerateEff(5000));
                        }
                    }
                }
            }
            else
            {
                ClientSession targetSession = Map.GetSessionByCharacterId(Target);

                if (targetSession == null || targetSession.Character.Invisible || targetSession.Character.Hp <= 0)
                {
                    Target = -1;
                    Path = Map.StraightPath(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = FirstX, y = FirstY });
                    if (!Path.Any())
                    {
                        Path = Map.JPSPlus(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = FirstX, y = FirstY });
                    }
                }
                NpcMonsterSkill npcMonsterSkill = null;
                if (_random.Next(10) > 8)
                {
                    npcMonsterSkill = Skills.Where(s => (DateTime.Now - s.LastSkillUse).TotalMilliseconds >= 100 * s.Skill.Cooldown).OrderBy(rnd => _random.Next()).FirstOrDefault();
                }

                int damage = 100;

                if (targetSession != null && !targetSession.Character.InvisibleGm && !targetSession.Character.Invisible && targetSession.Character.Hp > 0 && ((npcMonsterSkill != null && CurrentMp - npcMonsterSkill.Skill.MpCost >= 0 && Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) < npcMonsterSkill.Skill.Range) || (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) <= Monster.BasicRange)))
                {
                    if (((DateTime.Now - LastEffect).TotalMilliseconds >= 1000 + Monster.BasicCooldown * 200 && !Skills.Any()) || npcMonsterSkill != null)
                    {
                        if (npcMonsterSkill != null)
                        {
                            npcMonsterSkill.LastSkillUse = DateTime.Now;
                            CurrentMp -= npcMonsterSkill.Skill.MpCost;
                            Map.Broadcast($"ct 3 {MapMonsterId} 1 {Target} {npcMonsterSkill.Skill.CastAnimation} {npcMonsterSkill.Skill.CastEffect} {npcMonsterSkill.Skill.SkillVNum}");
                        }
                        LastMove = DateTime.Now;

                        // deal 0 damage to GM with GodMode
                        damage = targetSession.Character.HasGodMode ? 0 : 100;
                        if (targetSession.Character.IsSitting)
                        {
                            targetSession.Character.IsSitting = false;
                            Map.Broadcast(targetSession.Character.GenerateRest());
                            Thread.Sleep(500);
                        }
                        if (npcMonsterSkill != null && npcMonsterSkill.Skill.CastEffect != 0)
                        {
                            Map.Broadcast(GenerateEff(npcMonsterSkill.Skill.CastEffect), MapX, MapY, 10);
                            Thread.Sleep(npcMonsterSkill.Skill.CastTime * 100);
                        }
                        Path = new List<GridPos>();
                        targetSession.Character.LastDefence = DateTime.Now;
                        targetSession.Character.GetDamage(damage);

                        Map.Broadcast(null, ServerManager.Instance.GetUserMethod<string>(Target, "GenerateStat"), ReceiverType.OnlySomeone, "", Target);

                        if (npcMonsterSkill != null)
                        {
                            Map.Broadcast($"su 3 {MapMonsterId} 1 {Target} {npcMonsterSkill.SkillVNum} {npcMonsterSkill.Skill.Cooldown} {npcMonsterSkill.Skill.AttackAnimation} {npcMonsterSkill.Skill.Effect} {this.MapX} {this.MapY} {(targetSession.Character.Hp > 0 ? 1 : 0)} { (int)(targetSession.Character.Hp / targetSession.Character.HPLoad() * 100) } {damage} 0 0");
                        }
                        else
                        {
                            Map.Broadcast($"su 3 {MapMonsterId} 1 {Target} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(targetSession.Character.Hp > 0 ? 1 : 0)} { (int)(targetSession.Character.Hp / targetSession.Character.HPLoad() * 100) } {damage} 0 0");
                        }

                        LastEffect = DateTime.Now;
                        if (targetSession.Character.Hp <= 0)
                        {
                            Thread.Sleep(1000);
                            ServerManager.Instance.AskRevive(targetSession.Character.CharacterId);
                            Path = Map.StraightPath(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = FirstX, y = FirstY });
                            if (!Path.Any())
                            {
                                Path = Map.JPSPlus(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = FirstX, y = FirstY });
                            }
                            Target = -1;
                        }
                        if (npcMonsterSkill != null && (npcMonsterSkill.Skill.Range > 0 || npcMonsterSkill.Skill.TargetRange > 0))
                        {
                            foreach (Character chara in Map.GetListPeopleInRange(npcMonsterSkill.Skill.TargetRange == 0 ? this.MapX : targetSession.Character.MapX, npcMonsterSkill.Skill.TargetRange == 0 ? this.MapY : targetSession.Character.MapY, (byte)(npcMonsterSkill.Skill.TargetRange + npcMonsterSkill.Skill.Range)).Where(s => s.CharacterId != Target && s.Hp > 0))
                            {
                                if (chara.IsSitting)
                                {
                                    chara.IsSitting = false;
                                    Map.Broadcast(chara.GenerateRest());
                                    Thread.Sleep(500);
                                }
                                damage = chara.HasGodMode ? 0 : 100;
                                bool AlreadyDead2 = chara.Hp <= 0;
                                chara.GetDamage(damage);
                                chara.LastDefence = DateTime.Now;
                                Map.Broadcast(null, chara.GenerateStat(), ReceiverType.OnlySomeone, "", chara.CharacterId);
                                Map.Broadcast($"su 3 {MapMonsterId} 1 {chara.CharacterId} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(chara.Hp > 0 ? 1 : 0)} { (int)(chara.Hp / chara.HPLoad() * 100) } {damage} 0 0");
                                if (chara.Hp <= 0 && !AlreadyDead2)
                                {
                                    Thread.Sleep(1000);
                                    ServerManager.Instance.AskRevive(chara.CharacterId);
                                }
                            }
                        }
                    }
                }
                else
                {
                    int distance = 0;
                    if (targetSession != null)
                    {
                        distance = Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY });
                    }
                    if (IsMoving)
                    {
                        short maxDistance = 22;
                        if (Path.Count() == 0 && targetSession != null && distance > 1 && distance < maxDistance)
                        {
                            short xoffset = (short)_random.Next(-1, 1);
                            short yoffset = (short)_random.Next(-1, 1);

                            Path = Map.StraightPath(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = (short)(targetSession.Character.MapX + xoffset), y = (short)(targetSession.Character.MapY + yoffset) });
                            if (!Path.Any())
                            {
                                Path = Map.JPSPlus(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = (short)(targetSession.Character.MapX + xoffset), y = (short)(targetSession.Character.MapY + yoffset) });
                            }
                        }
                        if (DateTime.Now > LastMove && Monster.Speed > 0 && Path.Any())
                        {
                            short mapX;
                            short mapY;
                            int maxindex = Path.Count > Monster.Speed / 2 ? Monster.Speed / 2 : Path.Count;
                            mapX = (short)Path.ElementAt(maxindex - 1).x;
                            mapY = (short)Path.ElementAt(maxindex - 1).y;
                            double waitingtime = (double)(Map.GetDistance(new MapCell() { X = mapX, Y = mapY, MapId = MapId }, new MapCell() { X = MapX, Y = MapY, MapId = MapId })) / (double)(Monster.Speed);
                            Map.Broadcast(new BroadcastPacket(null, $"mv 3 {this.MapMonsterId} {mapX} {mapY} {Monster.Speed}", ReceiverType.AllInRange, xCoordinate: mapX, yCoordinate: mapY));
                            LastMove = DateTime.Now.AddSeconds((waitingtime > 1 ? 1 : waitingtime));
                            Task.Factory.StartNew(async () =>
                            {
                                await Task.Delay((int)((waitingtime > 1 ? 1 : waitingtime) * 1000));
                                this.MapX = mapX;
                                this.MapY = mapY;
                            });

                            for (int j = maxindex; j > 0; j--)
                            {
                                Path.RemoveAt(0);
                            }
                        }
                        if (Path.Count() == 0 && (targetSession == null || MapId != targetSession.Character.MapId || distance > maxDistance))
                        {
                            Path = Map.StraightPath(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = FirstX, y = FirstY });
                            if (!Path.Any())
                            {
                                Path = Map.JPSPlus(new GridPos() { x = this.MapX, y = this.MapY }, new GridPos() { x = FirstX, y = FirstY });
                            }
                            Target = -1;
                        }
                    }
                }
            }
        }

        #endregion
    }
}