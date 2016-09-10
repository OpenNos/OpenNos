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

        public MapMonster(Map parent, short VNum)
        {
            LastEffect = LastMove = DateTime.Now;
            Target = -1;
            path = new List<MapCell>();
            Map = parent;
            MonsterVNum = VNum;
            Monster = ServerManager.GetNpc(MonsterVNum);
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
        public Map Map { get; set; }
        public List<MapCell> path { get; set; }
        public long Target { get; set; }
        public NpcMonster Monster { get; set; }
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
            if (Alive && !IsDisabled)
                return $"in 3 {MonsterVNum} {MapMonsterId} {MapX} {MapY} {Position} {(int)(((float)CurrentHp / (float)Monster.MaxHP) * 100)} {(int)(((float)CurrentMp / (float)Monster.MaxMP) * 100)} 0 0 0 -1 1 0 -1 - 0 -1 0 0 0 0 0 0 0 0";
            else return string.Empty;
        }

        internal void MonsterLife()
        {
            //Respawn
            if (!Alive)
            {
                double timeDeath = (DateTime.Now - Death).TotalSeconds;
                if (timeDeath >= Monster.RespawnTime / 10)
                {
                    Alive = true;
                    Target = -1;
                    CurrentHp = Monster.MaxHP;
                    CurrentMp = Monster.MaxMP;
                    MapX = firstX;
                    MapY = firstY;

                    Map.Broadcast(GenerateIn3());
                    Map.Broadcast(GenerateEff(7));
                }
                return;
            }
            else if (Target == -1)
            {
                //Normal Move Mode
                if (Alive == false)
                {
                    return;
                }
                Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                double time = (DateTime.Now - LastMove).TotalSeconds;
                int MoveFrequent = 5 - (int)Math.Round((double)(Monster.Speed / 5));
                if (MoveFrequent < 1)
                    MoveFrequent = 1;
                if (IsMoving)
                {
                    if (path.Where(s => s != null).ToList().Count > 0)//fix a path problem
                    {
                        if ((DateTime.Now - LastMove).TotalSeconds > 1.0 / Monster.Speed)
                        {
                            short MapX = path.ElementAt(0).X;
                            short MapY = path.ElementAt(0).Y;
                            path.RemoveAt(0);
                            LastMove = DateTime.Now;
                            Map.Broadcast($"mv 3 {this.MapMonsterId} {this.MapX} {this.MapY} {Monster.Speed}");

                            Task.Factory.StartNew(async () =>
                            {
                                await Task.Delay(500);
                                this.MapX = MapX;
                                this.MapY = MapY;
                            });
                            return;
                        }
                    }
                    else if (time > r.Next(1, MoveFrequent) + 1)
                    {
                        int MoveDistance = (int)Math.Round((double)Monster.Speed / 2);
                        byte xpoint = (byte)(r.Next(1, MoveDistance));
                        byte ypoint = (byte)(r.Next(1, MoveDistance));

                        short MapX = firstX;
                        short MapY = firstY;
                        if (ServerManager.GetMap(MapId).GetFreePosition(ref MapX, ref MapY, xpoint, ypoint))
                        {
                            LastMove = DateTime.Now;

                            string movepacket = $"mv 3 {this.MapMonsterId} {MapX} {MapY} {Monster.Speed}";
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
                if (Monster.IsHostile)
                {
                    Character character = ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.Hp > 0).OrderBy(s => Map.GetDistance(new MapCell() { X = MapX, Y = MapY }, new MapCell() { X = s.Character.MapX, Y = s.Character.MapY })).FirstOrDefault(s => s.Character != null && !s.Character.Invisible && s.Character.MapId == MapId)?.Character;
                    if (character != null)
                    {
                        if (Map.GetDistance(new MapCell() { X = character.MapX, Y = character.MapY }, new MapCell() { X = MapX, Y = MapY }) < 7)
                        {
                            Target = character.CharacterId;
                            if (!Monster.NoAggresiveIcon)
                                character.Session.Client.SendPacket(GenerateEff(5000));
                        }
                    }
                }
            }
            else
            {
                ClientSession targetSession = Map.Sessions.SingleOrDefault(s => s.Character.CharacterId == Target);

                if (targetSession == null || (bool)targetSession.Character.Invisible)
                {
                    Target = -1;
                    return;
                }

                Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                NpcMonsterSkill ski = Monster.Skills.Where(s => !s.Used && (DateTime.Now - s.LastUse).TotalMilliseconds >= 100 * ServerManager.GetSkill(s.SkillVNum).Cooldown).OrderBy(rnd => r.Next()).FirstOrDefault();
                Skill sk = null;

                if (ski != null)
                {
                    sk = ServerManager.GetSkill(ski.SkillVNum);
                }

                int damage = 100;
                if (targetSession != null && ((sk != null && Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) < sk.Range) || (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) <= Monster.BasicRange)))
                {
                    if ((sk != null && ((DateTime.Now - LastEffect).TotalMilliseconds >= sk.Cooldown * 100 + 1000)) || ((DateTime.Now - LastEffect).TotalMilliseconds >= (Monster.BasicCooldown < 4 ? 4 : Monster.BasicCooldown) * 300 + 100)) // Need more information about cooldown time of monster attack(In waiting)
                    {
                        if (ski != null)
                        {
                            ski.Used = true;
                            ski.LastUse = DateTime.Now;
                            Map.Broadcast($"ct 3 {MapMonsterId} 1 {Target} {sk.CastAnimation} -1 {sk.SkillVNum}");
                        }
                        LastMove = DateTime.Now;

                        // deal 0 damage to GM with GodMode
                        damage = targetSession.Character.HasGodMode ? 0 : 100;
                        if (targetSession.Character.IsSitting)
                        {
                            targetSession.Character.IsSitting = false;
                            Map.Broadcast(null, targetSession.Character.GenerateRest(), ReceiverType.OnlySomeone, "", targetSession.Character.CharacterId);
                        }
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
                            Map.Broadcast($"su 3 {MapMonsterId} 1 {Target} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(targetSession.Character.Hp > 0 ? 1 : 0)} {(int)((double)targetSession.Character.Hp / ServerManager.Instance.GetUserMethod<double>(Target, "HPLoad"))} {damage} 0 0");

                        if (ski != null)
                            ski.Used = false;
                        LastEffect = DateTime.Now;
                        if (targetSession.Character.Hp <= 0)
                        {
                            Thread.Sleep(1000);
                            ServerManager.Instance.AskRevive(targetSession.Character.CharacterId);
                            Target = -1;
                        }
                        if ((sk != null && (sk.Range > 0 || sk.TargetRange > 0)))
                        {
                            foreach (Character chara in ServerManager.GetMap(MapId).GetListPeopleInRange(sk.TargetRange == 0 ? this.MapX : targetSession.Character.MapX, sk.TargetRange == 0 ? this.MapY : targetSession.Character.MapY, (byte)(sk.TargetRange + sk.Range)).Where(s => s.CharacterId != Target))
                            {
                                if (chara.IsSitting)
                                {
                                    chara.IsSitting = false;
                                    Map.Broadcast(null, chara.GenerateRest(), ReceiverType.OnlySomeone, "", chara.CharacterId);
                                }
                                damage = chara.HasGodMode ? 0 : 100;
                                bool AlreadyDead2 = chara.Hp <= 0;
                                chara.GetDamage(damage);
                                chara.LastDefence = DateTime.Now;
                                Map.Broadcast(null, chara.GenerateStat(), ReceiverType.OnlySomeone, "", chara.CharacterId);
                                Map.Broadcast($"su 3 {MapMonsterId} 1 {chara.CharacterId} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(chara.Hp > 0 ? 1 : 0)} {(int)((double)chara.Hp / chara.HPLoad())} {damage} 0 0");
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
                    if (IsMoving == true)
                    {
                        short maxdistance = 22;

                        if (path.Count() == 0 && targetSession != null && (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) < maxdistance))
                        {
                            path = ServerManager.GetMap(MapId).AStar(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY, MapId = this.MapId });
                        }
                        if (path.Count > 0 && Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY, MapId = this.MapId }) > 1)
                        {
                            this.MapX = path.ElementAt(0).X;
                            this.MapY = path.ElementAt(0).Y;
                            path.RemoveAt(0);
                        }
                        if (targetSession == null || MapId != targetSession.Character.MapId || (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) > maxdistance))
                        {
                            path = ServerManager.GetMap(MapId).AStar(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = firstX, Y = firstY, MapId = this.MapId });
                            Target = -1;
                        }
                        else
                        {
                            if ((DateTime.Now - LastMove).TotalSeconds > 1.0 / Monster.Speed)
                            {
                                LastMove = DateTime.Now;
                                Map.Broadcast($"mv 3 {this.MapMonsterId} {this.MapX} {this.MapY} {Monster.Speed}");
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}