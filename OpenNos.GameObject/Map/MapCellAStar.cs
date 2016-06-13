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

using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class MapCellAStar : MapCell
    {
        #region Public Members

        public int g;

        public int h;

        public MapCellAStar parentcell;

        #endregion

        #region Private Members

        private MapCellAStar _goalcell;

        #endregion

        #region Public Instantiation

        public MapCellAStar(MapCellAStar parentcell, MapCellAStar goalcell, short x, short y, short MapId)
        {
            this.parentcell = parentcell;
            this._goalcell = goalcell;
            this.X = x;
            this.Y = y;
            this.MapId = MapId;
            InitCell();
        }

        #endregion

        #region Public Properties

        public int TotalCost
        {
            get
            {
                return g + h;
            }
            set
            {
                TotalCost = value;
            }
        }

        #endregion

        #region Public Methods

        public int CompareTo(object obj)
        {
            MapCellAStar n = (MapCellAStar)obj;
            int cFactor = this.TotalCost - n.TotalCost;
            return cFactor;
        }

        public List<MapCellAStar> GetSuccessors()
        {
            List<MapCellAStar> successors = new List<MapCellAStar>();

            for (short xd = -1; xd <= 1; xd++)
            {
                for (short yd = -1; yd <= 1; yd++)
                {
                    if (!ServerManager.GetMap(MapId).IsBlockedZone(X + xd, Y + yd))
                    {
                        MapCellAStar n = new MapCellAStar(this, this._goalcell, (short)(X + xd), (short)(Y + yd), MapId);
                        if (!n.IsMatch(this.parentcell) && !n.IsMatch(this))
                            successors.Add(n);
                    }
                }
            }
            return successors;
        }

        public bool IsMatch(MapCellAStar n)
        {
            if (n != null)
                return (X == n.X && Y == n.Y);
            else
                return false;
        }

        #endregion

        #region Private Methods

        private double Euclidean_H()
        {
            double xd = this.X - this._goalcell.X;
            double yd = this.Y - this._goalcell.Y;
            return Math.Sqrt((xd * xd) + (yd * yd));
        }

        private void InitCell()
        {
            this.g = (parentcell != null) ? this.parentcell.g + 1 : 1;
            this.h = (_goalcell != null) ? (int)Euclidean_H() : 0;
        }

        #endregion
    }
}