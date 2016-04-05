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
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        }

        public bool Alive { get; set; }
        public DateTime Death { get; set; }
        public int CurrentHp { get; set; }
        public int CurrentMp { get; set; }

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
            return $"in 3 {MonsterVNum} {MapMonsterId} {MapX} {MapY} {Position} 100 100 0 0 0 -1 1 0 -1 - 0 -1 0 0 0 0 0 0 0 0";// 100 100 hp/mp
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
                    ClientLinkManager.Instance.RequiereBroadcastFromMap(MapId, GenerateIn3());
                }
                return;
            }
            if (Target == -1)
            {
                //Normal Move Mode
                if (monster == null || Alive == false)
                    return;
                Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                double time = (DateTime.Now - LastMove).TotalSeconds;
                if (Move && time > r.Next(1, 3) * (0.5 + r.NextDouble()))
                {
                    byte point = (byte)r.Next(2, 5);
                    byte fpoint = (byte)r.Next(0, 2);

                    byte xpoint = (byte)r.Next(fpoint, point);
                    byte ypoint = (byte)(point - xpoint);

                    short MapX = (short)r.Next(-xpoint + firstX, xpoint + firstX);
                    short MapY = (short)r.Next(-ypoint + firstY, ypoint + firstY);
                    if (!ServerManager.GetMap(MapId).IsBlockedZone(firstX, firstY, MapX, MapY))
                    {
                        this.MapX = MapX;
                        this.MapY = MapY;
                        LastMove = DateTime.Now;

                        string movepacket = $"mv 3 {this.MapMonsterId} {this.MapX} {this.MapY} {monster.Speed}";
                        ClientLinkManager.Instance.RequiereBroadcastFromMap(MapId, movepacket);
                    }

                    if (monster.IsHostile && Target ==-1)
                    {
                        Character character = ClientLinkManager.Instance.ClosestUser(MapId, MapX, MapY);
                        if (character != null)
                        {
                            if ((Math.Pow(character.MapX - MapX, 2) + Math.Pow(character.MapY - MapY, 2)) < (Math.Pow(7, 2)))
                            {
                                Target = character.CharacterId;
                                ClientLinkManager.Instance.RequiereBroadcastFromMap(MapId, GenerateEff(5000));
                            }
                        }
                    }
                }
            }
            else if(Move == true)
            {
                short? MapX = ClientLinkManager.Instance.GetProperty<short?>(Target, "MapX");
                short? MapY = ClientLinkManager.Instance.GetProperty<short?>(Target, "MapY");
                short? mapId = ClientLinkManager.Instance.GetProperty<short?>(Target, "MapId");
                short mapX = this.MapX;
                short mapY = this.MapY;
                short maxdistance = 30;
                if (MapX == null || MapY == null) { Target = -1; }
                else
                {
                    NextPositionByDistance((short)MapX, (short)MapY, ref mapX, ref mapY);

                    if (MapId != mapId || (Math.Pow(this.MapY - (short)MapY, 2) + Math.Pow(this.MapY - (short)MapY, 2) > (Math.Pow(maxdistance, 2))))
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
                            ClientLinkManager.Instance.RequiereBroadcastFromMap(MapId, $"mv 3 {this.MapMonsterId} {this.MapX} {this.MapY} {monster.Speed}");
                        }
                    }
                }
            }

        }

        private void NextPositionByDistance(short MapX, short MapY, ref short mapX, ref short mapY)
        {
            //TODO add pathfinding
            NpcMonster monster = ServerManager.GetNpc(this.MonsterVNum);
            if (MapX > this.MapX+1)
            {
                mapX++;
            }
            else if (MapX < this.MapX-1)
            {
                mapX--;
            }
            if (MapY > this.MapY+1)
            {
                mapY++;
            }
            else if (MapY < this.MapY-1)
            {
                mapY--;
            }
        }
        #endregion
    }
}