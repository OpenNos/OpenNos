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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
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
        public bool ShopAllowed { get; set; }
        public short MapId { get; set; }
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

        public StaticGrid Grid { get; set; }

        public List<MapTypeDTO> MapTypes { get; }

        /// <summary>
        /// This list ONLY for READ access to MapMonster, you CANNOT MODIFY them here. Use
        /// Add/RemoveMonster instead.
        /// </summary>

        public int Music { get; set; }

        public string Name { get; set; }

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
            return Math.Max(Math.Abs(p.X - q.X), Math.Abs(p.Y - q.Y));
        }



        public bool IsBlockedZone(int x, int y)
        {
            if (Grid != null)
            {
                if (!Grid.IsWalkableAt(new GridPos(x, y)))
                {
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

        public static List<GridPos> JPSPlus(JumpPointParam JumpPointParameters, GridPos cell1, GridPos cell2)
        {
            if (JumpPointParameters != null)
            {
                JumpPointParameters.Reset(cell1, cell2);
                return JumpPointFinder.GetFullPath(JumpPointFinder.FindPath(JumpPointParameters));
            }
            return new List<GridPos>();
        }

        private void LoadZone()
        {
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

                Grid = new StaticGrid(XLength, YLength);
                for (int i = 0; i < YLength; ++i)
                {
                    for (int t = 0; t < XLength; ++t)
                    {
                        stream.Read(bytes, numBytesRead, numBytesToRead);
                        Grid.SetWalkableAt(new GridPos(t, i), bytes[0]);
                    }
                }
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


        internal List<GridPos> StraightPath(GridPos mapCell1, GridPos mapCell2)
        {
            List<GridPos> Path = new List<GridPos> { mapCell1 };
            do
            {
                if (Path.Last().x < mapCell2.x && Path.Last().y < mapCell2.y)
                {
                    Path.Add(new GridPos { x = (short)(Path.Last().x + 1), y = (short)(Path.Last().y + 1) });
                }
                else if (Path.Last().x > mapCell2.x && Path.Last().y > mapCell2.y)
                {
                    Path.Add(new GridPos { x = (short)(Path.Last().x - 1), y = (short)(Path.Last().y - 1) });
                }
                else if (Path.Last().x < mapCell2.x && Path.Last().y > mapCell2.y)
                {
                    Path.Add(new GridPos { x = (short)(Path.Last().x + 1), y = (short)(Path.Last().y - 1) });
                }
                else if (Path.Last().x > mapCell2.x && Path.Last().y < mapCell2.y)
                {
                    Path.Add(new GridPos { x = (short)(Path.Last().x - 1), y = (short)(Path.Last().y + 1) });
                }
                else if (Path.Last().x > mapCell2.x)
                {
                    Path.Add(new GridPos { x = (short)(Path.Last().x - 1), y = (short)Path.Last().y });
                }
                else if (Path.Last().x < mapCell2.x)
                {
                    Path.Add(new GridPos { x = (short)(Path.Last().x + 1), y = (short)Path.Last().y });
                }
                else if (Path.Last().y > mapCell2.y)
                {
                    Path.Add(new GridPos { x = (short)Path.Last().x, y = (short)(Path.Last().y - 1) });
                }
                else if (Path.Last().y < mapCell2.y)
                {
                    Path.Add(new GridPos { x = (short)Path.Last().x, y = (short)(Path.Last().y + 1) });
                }
            }
            while ((Path.Last().x != mapCell2.x || Path.Last().y != mapCell2.y) && !IsBlockedZone(Path.Last().x, Path.Last().y));
            if (IsBlockedZone(Path.Last().x, Path.Last().y))
            {
                if (Path.Any())
                {
                    Path.Remove(Path.Last());
                }
            }
            if (Path.Count > 0)
            {
                Path.RemoveAt(0);
            }
            return Path;
        }


        #endregion
    }
}