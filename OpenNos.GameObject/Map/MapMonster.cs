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

using AutoMapper;
using OpenNos.Core;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MapMonster : MapMonsterDTO
    {
        #region Instantiation

        public MapMonster()
        {
            Mapper.CreateMap<MapMonsterDTO, MapMonster>();
            Mapper.CreateMap<MapMonster, MapMonsterDTO>();
            LastEffect = LastMove = DateTime.Now;
            Target = -1;
            inBattle = false;
            path = new List<MapCell>();
        }

        public bool Alive { get; set; }
        public DateTime Death { get; set; }
        public bool inBattle { get; set; }
        public int CurrentHp { get; set; }
        public int CurrentMp { get; set; }
        public List<MapCell> path { get; set; }
        #endregion

        #region Properties

        public short firstX { get; set; }
        public short firstY { get; set; }
        public DateTime LastEffect { get; private set; }
        public DateTime LastMove { get; private set; }
        public long Target { get; set; }

        #endregion

        #region Methods
        public string GenerateEff(int Effect)
        {
            return $"eff 3 {MapMonsterId} {Effect}";
        }

        public static int generateMapMonsterId()
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

        public string GenerateIn3()
        {
            NpcMonster monsterinfo = ServerManager.GetNpc(this.MonsterVNum);
            if (monsterinfo != null && Alive)
                return $"in 3 {MonsterVNum} {MapMonsterId} {MapX} {MapY} {Position} {(int)(((float)CurrentHp / (float)monsterinfo.MaxHP) * 100)} {(int)(((float)CurrentMp / (float)monsterinfo.MaxMP) * 100)} 0 0 0 -1 1 0 -1 - 0 -1 0 0 0 0 0 0 0 0";
            else return "";
        }

        internal void MonsterLife()
        {
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

                    ClientLinkManager.Instance.BroadcastToMap(MapId, GenerateIn3());
                    ClientLinkManager.Instance.BroadcastToMap(MapId, GenerateEff(7));
                }
                return;
            }
            else if (Target == -1)
            {
                //Normal Move Mode
                if (monster == null || Alive == false)
                    return;
                Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                double time = (DateTime.Now - LastMove).TotalSeconds;
                if (IsMoving && time > r.Next(1, 3) * (0.5 + r.NextDouble()))
                {
                    byte point = (byte)r.Next(2, 5);
                    byte fpoint = (byte)r.Next(0, 2);

                    byte xpoint = (byte)r.Next(fpoint, point);
                    byte ypoint = (byte)(point - xpoint);

                    short MapX = firstX;
                    short MapY = firstY;
                    if (ServerManager.GetMap(MapId).GetFreePosition(ref MapX, ref MapY, xpoint, ypoint))
                    {


                        LastMove = DateTime.Now;

                        string movepacket = $"mv 3 {this.MapMonsterId} {MapX} {MapY} {monster.Speed}";
                        ClientLinkManager.Instance.BroadcastToMap(MapId, movepacket);

                        Task.Factory.StartNew(async () =>
                        {
                            await Task.Delay(500);
                            this.MapX = MapX;
                            this.MapY = MapY;
                        });


                    }
                }
                if (monster.IsHostile)
                {
                    Character character = ClientLinkManager.Instance.Sessions.Where(s => s.Character != null && s.Character.Hp > 0).OrderBy(s => Map.GetDistance(new MapCell() { X = MapX, Y = MapY }, new MapCell() { X = s.Character.MapX, Y = s.Character.MapY })).FirstOrDefault(s => s.Character != null && s.Character.MapId == MapId)?.Character;
                    if (character != null)
                    {
                        if (Map.GetDistance(new MapCell() { X = character.MapX, Y = character.MapY }, new MapCell() { X = MapX, Y = MapY }) < 7)
                        {
                            Target = character.CharacterId;

                            ClientLinkManager.Instance.Sessions.FirstOrDefault(s => s != null && s.Client != null && s.Character != null && s.Character.CharacterId.Equals(Target)).Client.SendPacket(GenerateEff(5000));
                        }
                    }
                }
            }
            else
            {
                short? MapX = ClientLinkManager.Instance.GetProperty<short?>(Target, "MapX");
                short? MapY = ClientLinkManager.Instance.GetProperty<short?>(Target, "MapY");
                short? mapId = ClientLinkManager.Instance.GetProperty<short?>(Target, "MapId");

                if (MapX == null || MapY == null) { Target = -1; return; }
                short mapX = this.MapX;
                short mapY = this.MapY;

                Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                NpcMonsterSkill ski = monster.Skills.Where(s => !s.Used && (DateTime.Now - s.LastUse).TotalMilliseconds >= 100 * ServerManager.GetSkill(s.SkillVNum).Cooldown).OrderBy(rnd => r.Next()).FirstOrDefault();
                if (ski != null)
                {
                    Skill sk = ServerManager.GetSkill(ski.SkillVNum);
                    if (MapId == mapId && Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = (short)MapX, Y = (short)MapY }) < sk.Range + 1)
                    {
                        ski.Used = true;
                        ski.LastUse = DateTime.Now;
                        LastMove = DateTime.Now;


                        ClientLinkManager.Instance.BroadcastToMap(MapId, $"ct 3 {MapMonsterId} 1 {Target} {sk.CastAnimation} -1 {sk.SkillVNum}");

                        if (sk.CastEffect != 0)
                        {
                            ClientLinkManager.Instance.BroadcastToMap(MapId, GenerateEff(sk.CastEffect));
                            Thread.Sleep(sk.CastTime * 100);
                        }

                        ClientLinkManager.Instance.BroadcastToMap(MapId, $"su 3 {MapMonsterId} 1 {Target} {ski.SkillVNum} {sk.Cooldown} {sk.AttackAnimation} {sk.Effect} {this.MapX} {this.MapY} 1 100 0 1 0");
                        ski.Used = false;
                    }
                }
                else
                {
                    if (!inBattle)
                    {
                        if ((DateTime.Now - LastEffect).TotalMilliseconds >= monster.BasicCooldown * 100 && (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = (short)MapX, Y = (short)MapY }) <= monster.BasicRange + 1))
                        {
                            LastEffect = DateTime.Now;
                            inBattle = true;
                            ClientLinkManager.Instance.BroadcastToMap(MapId, $"ct 3 {MapMonsterId} 1 {Target} -1 -1 0");
                            int? Hp = ClientLinkManager.Instance.GetProperty<int?>(Target, "Hp");

                            int damage = 100;
                            int HP = ((int)Hp - damage);
                            ClientLinkManager.Instance.SetProperty(Target, "Hp", (int)((HP) <= 0 ? 0 : HP));

                            ClientLinkManager.Instance.SetProperty(Target, "LastDefence", DateTime.Now);

                            ClientLinkManager.Instance.BroadcastToMap(MapId, $"su 3 {MapMonsterId} 1 {Target} 0 {monster.BasicCooldown} 11 {monster.BasicSkill} 0 0 {((HP) > 0 ? 1 : 0)} {(int)((double)(HP) / ClientLinkManager.Instance.GetUserMethod<double>(Target, "HPLoad"))} {damage} 0 0");
                            ClientLinkManager.Instance.Broadcast(null, ClientLinkManager.Instance.GetUserMethod<string>(Target, "GenerateStat"), ReceiverType.OnlySomeone, "", Target);

                            foreach (Character chara in ServerManager.GetMap(MapId).GetListPeopleInRange((short)MapX, (short)MapY, monster.BasicArea).Where(s => s.CharacterId != Target))
                            {
                                damage = 100;
                                chara.Hp -= damage;
                                chara.LastDefence = DateTime.Now;
                                ClientLinkManager.Instance.Broadcast(null, ClientLinkManager.Instance.GetUserMethod<string>(chara.CharacterId, "GenerateStat"), ReceiverType.OnlySomeone, "", Target);
                                ClientLinkManager.Instance.BroadcastToMap(MapId, $"su 3 {MapMonsterId} 1 {chara.CharacterId} 0 {monster.BasicCooldown} 11 {monster.BasicSkill} 0 0 1 {(int)((double)Hp / chara.HPLoad())} {damage} 0 0");

                                if (HP <= 0)
                                {
                                    ClientLinkManager.Instance.AskRevive(chara.CharacterId);
                                }
                            }
                            if (HP <= 0)
                            {
                                ClientLinkManager.Instance.AskRevive(Target);
                                Target = -1;
                            }
                            inBattle = false;
                        }
                    }
                }
                if (IsMoving == true)
                {

                    short maxdistance = 20;
                    if (path.Count() < 1)
                        path = ServerManager.GetMap(MapId).AStar(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = (short)MapX, Y = (short)MapY, MapId = this.MapId });
                    if (path.Count > 0 && Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY, MapId = this.MapId }, new MapCell() { X = (short)MapX, Y = (short)MapY, MapId = this.MapId }) > 1)
                    {
                        mapX = path.ElementAt(0) == null ? mapX : path.ElementAt(0).X;
                        mapY = path.ElementAt(0) == null ? mapY : path.ElementAt(0).Y;
                        path.RemoveAt(0);
                    }
                    if (MapId != mapId || (Map.GetDistance(new MapCell() { X = this.MapX, Y = this.MapY }, new MapCell() { X = (short)MapX, Y = (short)MapY }) > maxdistance + 1))
                    {
                        //TODO add return to origin
                        Target = -1;
                    }
                    else
                    {

                        if ((DateTime.Now - LastMove).TotalSeconds > 1.0 / monster.Speed)
                        {
                            this.MapX = mapX;
                            this.MapY = mapY;
                            LastMove = DateTime.Now;
                            ClientLinkManager.Instance.BroadcastToMap(MapId, $"mv 3 {this.MapMonsterId} {this.MapX} {this.MapY} {monster.Speed}");
                        }
                    }
                }

            }

        }

        #endregion
    }
}