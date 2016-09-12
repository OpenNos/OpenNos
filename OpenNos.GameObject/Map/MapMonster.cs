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
            Path = new List<MapCell>();
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
        public NpcMonster Monster { get; set; }
        public List<MapCell> Path { get; set; }
        public long Target { get; set; }

        #endregion

        #region Methods

        public static int GenerateMapMonsterId()
        {
            Random rnd = new Random();
            List<int> monsterIds = new List<int>();

            for (int i = ServerManager.Monsters.Count - 1; i >= 0; i--)
            {
                monsterIds.Add(ServerManager.Monsters[i].MapMonsterId);
            }

            for (int i = 20000; i < int.MaxValue; i++)
                if (!monsterIds.Contains(i))
                    return i;
            return -1;
        }

        public string GenerateEff(int effect)
        {
            return $"eff 3 {MapMonsterId} {effect}";
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
                    Path = new List<MapCell>();
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
                Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                double time = (DateTime.Now - LastMove).TotalSeconds;
                int MoveFrequent = 5 - (int)Math.Round((double)(Monster.Speed / 5));
                if (MoveFrequent < 1)
                    MoveFrequent = 1;
                if (IsMoving)
                {
                    if (Path.Where(s => s != null).ToList().Count > 0)//fix a path problem
                    {
                        if ((DateTime.Now - LastMove).TotalSeconds > 1.0 / Monster.Speed)
                        {
                            short mapX = Path.ElementAt(0).X;
                            short mapY = Path.ElementAt(0).Y;
                            Path.RemoveAt(0);
                            LastMove = DateTime.Now;
                            Map.Broadcast($"mv 3 {this.MapMonsterId} {this.MapX} {this.MapY} {Monster.Speed}");

                            Task.Factory.StartNew(async () =>
                            {
                                await Task.Delay(500);
                                this.MapX = mapX;
                                this.MapY = mapY;
                            });
                            return;
                        }
                    }
                    else if (time > random.Next(1, MoveFrequent) + 1)
                    {
                        int moveDistance = (int)Math.Round((double)Monster.Speed / 2);
                        byte xpoint = (byte)(random.Next(1, moveDistance + 1));
                        byte ypoint = (byte)(random.Next(1, moveDistance + 1));

                        short mapX = firstX;
                        short mapY = firstY;
                        if (ServerManager.GetMap(MapId).GetFreePosition(ref mapX, ref mapY, xpoint, ypoint))
                        {
                            LastMove = DateTime.Now;

                            string movePacket = $"mv 3 {this.MapMonsterId} {mapX} {mapY} {Monster.Speed}";
                            Map.Broadcast(movePacket);

                            Task.Factory.StartNew(async () =>
                            {
                                await Task.Delay(500);
                                this.MapX = mapX;
                                this.MapY = mapY;
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

                if (targetSession == null || targetSession.Character.Invisible)
                {
                    Target = -1;
                    return;
                }

                Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                NpcMonsterSkill npcMonsterSkill = Monster.Skills.Where(s => !s.Used && (DateTime.Now - s.LastUse).TotalMilliseconds >= 100 * ServerManager.GetSkill(s.SkillVNum).Cooldown).OrderBy(rnd => random.Next()).FirstOrDefault();
                Skill skill = null;

                if (npcMonsterSkill != null)
                {
                    skill = ServerManager.GetSkill(npcMonsterSkill.SkillVNum);
                }

                int damage = 100;
                if (targetSession != null && ((skill != null && Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) < skill.Range) || (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) <= Monster.BasicRange)))
                {
                    if ((skill != null && ((DateTime.Now - LastEffect).TotalMilliseconds >= skill.Cooldown * 100 + 1000)) || ((DateTime.Now - LastEffect).TotalMilliseconds >= (Monster.BasicCooldown < 4 ? 4 : Monster.BasicCooldown) * 300 + 100)) // Need more information about cooldown time of monster attack(In waiting)
                    {
                        if (npcMonsterSkill != null)
                        {
                            npcMonsterSkill.Used = true;
                            npcMonsterSkill.LastUse = DateTime.Now;
                            Map.Broadcast($"ct 3 {MapMonsterId} 1 {Target} {skill.CastAnimation} -1 {skill.SkillVNum}");
                        }
                        LastMove = DateTime.Now;

                        // deal 0 damage to GM with GodMode
                        damage = targetSession.Character.HasGodMode ? 0 : 100;
                        if (targetSession.Character.IsSitting)
                        {
                            targetSession.Character.IsSitting = false;
                            Map.Broadcast(null, targetSession.Character.GenerateRest(), ReceiverType.OnlySomeone, "", targetSession.Character.CharacterId);
                        }
                        if (skill != null && skill.CastEffect != 0)
                        {
                            Map.Broadcast(GenerateEff(skill.CastEffect));
                            Thread.Sleep(skill.CastTime * 100);
                        }
                        Path = new List<MapCell>();
                        targetSession.Character.LastDefence = DateTime.Now;
                        targetSession.Character.GetDamage(damage);

                        Map.Broadcast(null, ServerManager.Instance.GetUserMethod<string>(Target, "GenerateStat"), ReceiverType.OnlySomeone, "", Target);

                        if (skill != null)
                            Map.Broadcast($"su 3 {MapMonsterId} 1 {Target} {npcMonsterSkill.SkillVNum} {skill.Cooldown} {skill.AttackAnimation} {skill.Effect} {this.MapX} {this.MapY} {(targetSession.Character.Hp > 0 ? 1 : 0)} {(int)((double)targetSession.Character.Hp / ServerManager.Instance.GetUserMethod<double>(Target, "HPLoad"))} {damage} 0 0");
                        else
                            Map.Broadcast($"su 3 {MapMonsterId} 1 {Target} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(targetSession.Character.Hp > 0 ? 1 : 0)} {(int)((double)targetSession.Character.Hp / ServerManager.Instance.GetUserMethod<double>(Target, "HPLoad"))} {damage} 0 0");

                        if (npcMonsterSkill != null)
                            npcMonsterSkill.Used = false;
                        LastEffect = DateTime.Now;
                        if (targetSession.Character.Hp <= 0)
                        {
                            Thread.Sleep(1000);
                            ServerManager.Instance.AskRevive(targetSession.Character.CharacterId);
                            Target = -1;
                        }
                        if ((skill != null && (skill.Range > 0 || skill.TargetRange > 0)))
                        {
                            foreach (Character chara in ServerManager.GetMap(MapId).GetListPeopleInRange(skill.TargetRange == 0 ? this.MapX : targetSession.Character.MapX, skill.TargetRange == 0 ? this.MapY : targetSession.Character.MapY, (byte)(skill.TargetRange + skill.Range)).Where(s => s.CharacterId != Target))
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
                        short maxDistance = 22;

                        if (Path.Count() == 0 && targetSession != null && (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) < maxDistance))
                        {
                            Path = ServerManager.GetMap(MapId).AStar(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY, MapId = this.MapId });
                        }
                        if (Path.Count > 0 && Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY, MapId = this.MapId }) > 1)
                        {
                            this.MapX = Path.ElementAt(0).X;
                            this.MapY = Path.ElementAt(0).Y;
                            Path.RemoveAt(0);
                        }
                        if (targetSession == null || MapId != targetSession.Character.MapId || (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = targetSession.Character.MapX, Y = targetSession.Character.MapY }) > maxDistance))
                        {
                            Path = ServerManager.GetMap(MapId).AStar(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = firstX, Y = firstY, MapId = this.MapId });
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