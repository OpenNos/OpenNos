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

using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.PathFinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Map : IMapDTO
    {
        #region Members

        private readonly Random _random;

        #endregion

        #region Instantiation

        public Map(short mapId, byte[] data)
        {
            _random = new Random();
            MapId = mapId;
            Data = data;
            LoadZone();
            MapTypes = new List<MapTypeDTO>();
            foreach (MapTypeMapDTO maptypemap in DAOFactory.MapTypeMapDAO.LoadByMapId(mapId).ToList())
            {
                MapTypeDTO maptype = DAOFactory.MapTypeDAO.LoadById(maptypemap.MapTypeId);
                MapTypes.Add(maptype);
            }

            if (MapTypes.Any())
            {
                if (MapTypes.ElementAt(0).RespawnMapTypeId != null)
                {
                    long? respawnMapTypeId = MapTypes.ElementAt(0).RespawnMapTypeId;
                    long? returnMapTypeId = MapTypes.ElementAt(0).ReturnMapTypeId;
                    if (respawnMapTypeId != null)
                    {
                        DefaultRespawn = DAOFactory.RespawnMapTypeDAO.LoadById((long)respawnMapTypeId);
                    }
                    if (returnMapTypeId != null)
                    {
                        DefaultReturn = DAOFactory.RespawnMapTypeDAO.LoadById((long)returnMapTypeId);
                    }
                }
            }
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        public RespawnMapTypeDTO DefaultRespawn { get; private set; }

        public RespawnMapTypeDTO DefaultReturn { get; private set; }

        public GridPos[,] Grid { get; set; }

        public short MapId { get; set; }

        public List<MapTypeDTO> MapTypes { get; }

        public int Music { get; set; }

        /// <summary>
        /// This list ONLY for READ access to MapMonster, you CANNOT MODIFY them here. Use
        /// Add/RemoveMonster instead.
        /// </summary>
        public string Name { get; set; }

        public bool ShopAllowed { get; set; }

        private int XLength { get; set; }

        private int YLength { get; set; }

        #endregion

        #region Methods

        public static int GetDistance(Character character1, Character character2)
        {
            return GetDistance(new MapCell { X = character1.PositionX, Y = character1.PositionY }, new MapCell { X = character2.PositionX, Y = character2.PositionY });
        }

        public static int GetDistance(MapCell p, MapCell q)
        {
            return (int)Heuristic.Octile(Math.Abs(p.X - q.X), Math.Abs(p.Y - q.Y));
        }

        public IEnumerable<MonsterToSummon> GenerateMonsters(short vnum, short amount, bool move, List<EventContainer> deathEvents, bool isBonus = false, bool isHostile = true, bool isBoss = false)
        {
            List<MonsterToSummon> SummonParameters = new List<MonsterToSummon>();
            for (int i = 0; i < amount; i++)
            {
                MapCell cell = GetRandomPosition();
                SummonParameters.Add(new MonsterToSummon(vnum, cell, -1, move, isBonus: isBonus, isHostile: isHostile, isBoss: isBoss) {DeathEvents = deathEvents });
            }
            return SummonParameters;
        }

        public List<NpcToSummon> GenerateNpcs(short vnum, short amount, List<EventContainer> deathEvents, bool isMate, bool isProtected)
        {
            List<NpcToSummon> SummonParameters = new List<NpcToSummon>();
            for (int i = 0; i < amount; i++)
            {
                MapCell cell = GetRandomPosition();
                SummonParameters.Add(new NpcToSummon(vnum, cell, -1, deathEvents, isMate: isMate, isProtected: isProtected));
            }
            return SummonParameters;
        }

        public MapCell GetRandomPosition()
        {
            List<MapCell> cells = new List<MapCell>();
            for (short y = 0; y <= YLength; y++)
            {
                for (short x = 0; x <= XLength; x++)
                {
                    if (!IsBlockedZone(x, y))
                    {
                        cells.Add(new MapCell { X = x, Y = y });
                    }
                }
            }
            return cells.OrderBy(s => _random.Next(int.MaxValue)).FirstOrDefault();
        }

        public bool IsBlockedZone(int x, int y)
        {
            try
            {
                if (Grid != null)
                {
                    if (!Grid[x, y].IsWalkable())
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return true;
            }
        }

        internal bool GetFreePosition(ref short firstX, ref short firstY, byte xpoint, byte ypoint)
        {
            short MinX = (short)(-xpoint + firstX);
            short MaxX = (short)(xpoint + firstX);

            short MinY = (short)(-ypoint + firstY);
            short MaxY = (short)(ypoint + firstY);

            List<MapCell> cells = new List<MapCell>();
            for (short y = MinY; y <= MaxY; y++)
            {
                for (short x = MinX; x <= MaxX; x++)
                {
                    if (x != firstX || y != firstY)
                    {
                        cells.Add(new MapCell { X = x, Y = y });
                    }
                }
            }
            foreach (MapCell cell in cells.OrderBy(s => _random.Next(int.MaxValue)))
            {
                if (!IsBlockedZone(firstX, firstY, cell.X, cell.Y))
                {
                    firstX = cell.X;
                    firstY = cell.Y;
                    return true;
                }
            }
            return false;
        }

        private bool IsBlockedZone(int firstX, int firstY, int mapX, int mapY)
        {
            for (int i = 1; i <= Math.Abs(mapX - firstX); i++)
            {
                if (IsBlockedZone(firstX + Math.Sign(mapX - firstX) * i, firstY))
                {
                    return true;
                }
            }

            for (int i = 1; i <= Math.Abs(mapY - firstY); i++)
            {
                if (IsBlockedZone(firstX, firstY + Math.Sign(mapY - firstY) * i))
                {
                    return true;
                }
            }
            return false;
        }

        private void LoadZone()
        {
            // TODO: Optimize
            using (Stream stream = new MemoryStream(Data))
            {
                const int numBytesToRead = 1;
                const int numBytesRead = 0;
                byte[] bytes = new byte[numBytesToRead];

                byte[] xlength = new byte[2];
                byte[] ylength = new byte[2];
                stream.Read(bytes, numBytesRead, numBytesToRead);
                xlength[0] = bytes[0];
                stream.Read(bytes, numBytesRead, numBytesToRead);
                xlength[1] = bytes[0];
                stream.Read(bytes, numBytesRead, numBytesToRead);
                ylength[0] = bytes[0];
                stream.Read(bytes, numBytesRead, numBytesToRead);
                ylength[1] = bytes[0];
                YLength = BitConverter.ToInt16(ylength, 0);
                XLength = BitConverter.ToInt16(xlength, 0);

                Grid = new GridPos[XLength, YLength];
                for (short i = 0; i < YLength; ++i)
                {
                    for (short t = 0; t < XLength; ++t)
                    {
                        stream.Read(bytes, numBytesRead, numBytesToRead);
                        Grid[t, i] = new GridPos()
                        {
                            Value = bytes[0],
                            X = t,
                            Y = i,
                        };
                    }
                }
            }
        }

        #endregion
    }
}