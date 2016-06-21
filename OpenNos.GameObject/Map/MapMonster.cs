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
        #region Instantiation

        public MapMonster(Map parent)
        {
            LastEffect = LastMove = DateTime.Now;
            Target = -1;
            path = new List<MapCell>();
            LifeTaskIsRunning = false;
            Map = parent;
        }

        #endregion

        #region Properties

        public bool Alive { get; set; }
        public int CurrentHp { get; set; }
        public int CurrentMp { get; set; }
        public DateTime Death { get; set; }
        public short firstX { get; set; }
        public short firstY { get; set; }
        public DateTime LastEffect { get; private set; }
        public DateTime LastMove { get; private set; }
        public bool LifeTaskIsRunning { get; internal set; }
        public Map Map { get; set; }
        public List<MapCell> path { get; set; }
        public long Target { get; set; }

        #endregion

        #region Methods

        public static int GenerateMapMonsterId()
        {
            Random rnd = new Random();
            List<int> test = new List<int>();

            for (int i = ServerManager.Monsters.Count - 1; i >= 0; i--)
            {
                test.Add(ServerManager.Monsters[i].MapMonsterId);
            }

            for (int i = 20000; i < int.MaxValue; i++)
                if (!test.Contains(i))
                    return i;
            return -1;
        }

        public string GenerateEff(int Effect)
        {
            return $"eff 3 {MapMonsterId} {Effect}";
        }

        public string GenerateIn3()
        {
            NpcMonster monsterinfo = ServerManager.GetNpc(this.MonsterVNum);
            if (monsterinfo != null && Alive)
                return $"in 3 {MonsterVNum} {MapMonsterId} {MapX} {MapY} {Position} {(int)(((float)CurrentHp / (float)monsterinfo.MaxHP) * 100)} {(int)(((float)CurrentMp / (float)monsterinfo.MaxMP) * 100)} 0 0 0 -1 1 0 -1 - 0 -1 0 0 0 0 0 0 0 0";
            else return "";
        }

        internal void MonsterLife()
        {
            LifeTaskIsRunning = true;
            NpcMonster monster = ServerManager.GetNpc(this.MonsterVNum);

            //Respawn
            if (!Alive)
            {
                double timeDeath = (DateTime.Now - Death).TotalSeconds;
                if (timeDeath >= monster.RespawnTime / 10)
                {
                    Alive = true;
                    Target = -1;
                    CurrentHp = monster.MaxHP;
                    CurrentMp = monster.MaxMP;
                    MapX = firstX;
                    MapY = firstY;

                    Map.Broadcast(GenerateIn3());
                    Map.Broadcast(GenerateEff(7));
                }
                LifeTaskIsRunning = false;
                return;
            }
            else if (Target == -1)
            {
                //Normal Move Mode
                if (monster == null || Alive == false)
                {
                    LifeTaskIsRunning = false;
                    return;
                }
                Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                double time = (DateTime.Now - LastMove).TotalSeconds;
                int MoveFrequent = 5 - (int)Math.Round((double)(monster.Speed / 5));
                if (MoveFrequent < 1)
                    MoveFrequent = 1;
                if (IsMoving)
                {
                    if (path.Count > 0)
                    {
                        if ((DateTime.Now - LastMove).TotalSeconds > 1.0 / monster.Speed)
                        {
                            short MapX = path.ElementAt(0).X;
                            short MapY = path.ElementAt(0).Y;
                            path.RemoveAt(0);
                            LastMove = DateTime.Now;
                            Map.Broadcast($"mv 3 {this.MapMonsterId} {this.MapX} {this.MapY} {monster.Speed}");

                            Task.Factory.StartNew(async () =>
                            {
                                await Task.Delay(500);
                                this.MapX = MapX;
                                this.MapY = MapY;
                            });
                            LifeTaskIsRunning = false;
                            return;
                        }
                    }
                    else if (time > r.Next(1, MoveFrequent) + 1)
                    {
                        int MoveDistance = (int)Math.Round((double)monster.Speed / 2);
                        byte xpoint = (byte)(r.Next(1, MoveDistance));
                        byte ypoint = (byte)(r.Next(1, MoveDistance));

                        short MapX = firstX;
                        short MapY = firstY;
                        if (ServerManager.GetMap(MapId).GetFreePosition(ref MapX, ref MapY, xpoint, ypoint))
                        {
                            LastMove = DateTime.Now;

                            string movepacket = $"mv 3 {this.MapMonsterId} {MapX} {MapY} {monster.Speed}";
                            Map.Broadcast(movepacket);

                            Task.Factory.StartNew(async () =>
                            {
                                await Task.Delay(500);
                                this.MapX = MapX;
                                this.MapY = MapY;
                            });
                        }
                    }
                }
                if (monster.IsHostile)
                {
                    Character character = ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.Hp > 0).OrderBy(s => Map.GetDistance(new MapCell() { X = MapX, Y = MapY }, new MapCell() { X = s.Character.MapX, Y = s.Character.MapY })).FirstOrDefault(s => s.Character != null && !s.Character.Invisible && s.Character.MapId == MapId)?.Character;
                    if (character != null)
                    {
                        if (Map.GetDistance(new MapCell() { X = character.MapX, Y = character.MapY }, new MapCell() { X = MapX, Y = MapY }) < 7)
                        {
                            Target = character.CharacterId;
                            ServerManager.Instance.Sessions.FirstOrDefault(s => s != null && s.Client != null && s.Character != null && s.Character.CharacterId.Equals(Target)).Client.SendPacket(GenerateEff(5000));
                        }
                    }
                }
            }
            else
            {
                short? MapX = ServerManager.Instance.GetProperty<short?>(Target, "MapX");
                short? MapY = ServerManager.Instance.GetProperty<short?>(Target, "MapY");
                int? Hp = ServerManager.Instance.GetProperty<int?>(Target, "Hp");
                short? mapId = ServerManager.Instance.GetProperty<short?>(Target, "MapId");
                bool? invisible = ServerManager.Instance.GetProperty<bool?>(Target, "Invisible");

                if (MapX == null || MapY == null || Hp <= 0 || invisible != null && (bool)invisible) { Target = -1; LifeTaskIsRunning = false; return; }

                int damage = 100;
                Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                NpcMonsterSkill ski = monster.Skills.Where(s => !s.Used && (DateTime.Now - s.LastUse).TotalMilliseconds >= 100 * ServerManager.GetSkill(s.SkillVNum).Cooldown).OrderBy(rnd => r.Next()).FirstOrDefault();
                Skill sk = null;
                if (ski != null)
                {
                    sk = ServerManager.GetSkill(ski.SkillVNum);
                }

                ClientSession targetSession = Map.Sessions.Single(s => s.Character.CharacterId == Target);

                Thread thread = targetSession.Character.ThreadCharChange;

                if (thread != null && thread.IsAlive)
                    thread.Abort();

                if ((sk != null && Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = (short)MapX, Y = (short)MapY }) < sk.Range) || (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = (short)MapX, Y = (short)MapY }) <= monster.BasicRange))
                {
                    if ((sk != null && ((DateTime.Now - LastEffect).TotalMilliseconds >= sk.Cooldown * 100 + 1000)) || ((DateTime.Now - LastEffect).TotalMilliseconds >= (monster.BasicCooldown < 4 ? 4 : monster.BasicCooldown) * 100 + 100))
                    {
                        if (ski != null)
                        {
                            ski.Used = true;
                            ski.LastUse = DateTime.Now;
                            Map.Broadcast($"ct 3 {MapMonsterId} 1 {Target} {sk.CastAnimation} -1 {sk.SkillVNum}");
                        }

                        LastMove = DateTime.Now;

                        if (sk != null && sk.CastEffect != 0)
                        {
                            Map.Broadcast(GenerateEff(sk.CastEffect));
                            Thread.Sleep(sk.CastTime * 100);
                        }
                        path = new List<MapCell>();
                        targetSession.Character.LastDefence = DateTime.Now;
                        targetSession.Character.GetDamage(damage);
                        Map.Broadcast(null, ServerManager.Instance.GetUserMethod<string>(Target, "GenerateStat"), ReceiverType.OnlySomeone, "", Target);

                        if (sk != null)
                            Map.Broadcast($"su 3 {MapMonsterId} 1 {Target} {ski.SkillVNum} {sk.Cooldown} {sk.AttackAnimation} {sk.Effect} {this.MapX} {this.MapY} {(targetSession.Character.Hp > 0 ? 1 : 0)} {(int)((double)targetSession.Character.Hp / ServerManager.Instance.GetUserMethod<double>(Target, "HPLoad"))} {damage} 0 0");
                        else
                            Map.Broadcast($"su 3 {MapMonsterId} 1 {Target} 0 {monster.BasicCooldown} 11 {monster.BasicSkill} 0 0 {(targetSession.Character.Hp > 0 ? 1 : 0)} {(int)((double)targetSession.Character.Hp / ServerManager.Instance.GetUserMethod<double>(Target, "HPLoad"))} {damage} 0 0");

                        if (ski != null)
                            ski.Used = false;
                        LastEffect = DateTime.Now;
                        if (targetSession.Character.Hp <= 0)
                        {
                            Thread.Sleep(1000);
                            ServerManager.Instance.AskRevive(Target);
                            Target = -1;
                        }
                        if ((sk != null && (sk.Range > 0 || sk.TargetRange > 0)))
                            foreach (Character chara in ServerManager.GetMap(MapId).GetListPeopleInRange(sk.TargetRange == 0 ? this.MapX : (short)MapX, sk.TargetRange == 0 ? this.MapY : (short)MapY, (byte)(sk.TargetRange + sk.Range)).Where(s => s.CharacterId != Target))
                            {
                                damage = 100;
                                bool AlreadyDead2 = chara.Hp <= 0;
                                chara.GetDamage(damage);
                                chara.LastDefence = DateTime.Now;
                                Map.Broadcast(null, chara.GenerateStat(), ReceiverType.OnlySomeone, "", chara.CharacterId);
                                Map.Broadcast($"su 3 {MapMonsterId} 1 {chara.CharacterId} 0 {monster.BasicCooldown} 11 {monster.BasicSkill} 0 0 {(chara.Hp > 0 ? 1 : 0)} {(int)((double)chara.Hp / chara.HPLoad())} {damage} 0 0");
                                if (chara.Hp <= 0 && !AlreadyDead2)
                                {
                                    Thread.Sleep(1000);
                                    ServerManager.Instance.AskRevive(chara.CharacterId);
                                }
                            }
                    }
                }
                else
                {
                    if (IsMoving == true)
                    {
                        short maxdistance = 20;

                        if (path.Count() == 0)
                        {
                            path = ServerManager.GetMap(MapId).AStar(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = (short)MapX, Y = (short)MapY, MapId = this.MapId });
                        }
                        if (path.Count > 0 && Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = (short)MapX, Y = (short)MapY, MapId = this.MapId }) > 1)
                        {
                            this.MapX = path.ElementAt(0).X;
                            this.MapY = path.ElementAt(0).Y;
                            path.RemoveAt(0);
                        }
                        if (MapId != mapId || (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = (short)MapX, Y = (short)MapY }) > maxdistance))
                        {
                            path = ServerManager.GetMap(MapId).AStar(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = firstX, Y = firstY, MapId = this.MapId });
                            Target = -1;
                        }
                        else
                        {
                            if ((DateTime.Now - LastMove).TotalSeconds > 1.0 / monster.Speed)
                            {
                                LastMove = DateTime.Now;
                                Map.Broadcast($"mv 3 {this.MapMonsterId} {this.MapX} {this.MapY} {monster.Speed}");
                            }
                        }
                    }
                }
            }
            LifeTaskIsRunning = false;
        }

        #endregion
    }
}