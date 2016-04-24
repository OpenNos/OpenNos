using AutoMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MapCellAStar : MapCell
    {
        public int totalCost
        {
            get
            {
                return g + h;
            }
            set
            {
                totalCost = value;
            }
        }
        public int g;
        public int h;
 

        private MapCellAStar _goalcell;
        public MapCellAStar parentcell;


        public MapCellAStar(MapCellAStar parentcell, MapCellAStar goalcell, short x, short y,short MapId)
        {
            this.parentcell = parentcell;
            this._goalcell = goalcell;
            this.X = x;
            this.Y = y;
            this.MapId = MapId;
            Initcell();
        }

        private void Initcell()
        {
            this.g = (parentcell != null) ? this.parentcell.g + 1 : 1;
            this.h = (_goalcell != null) ? (int)Euclidean_H() : 0;
        }

        private double Euclidean_H()
        {
            double xd = this.X - this._goalcell.X;
            double yd = this.Y - this._goalcell.Y;
            return Math.Sqrt((xd * xd) + (yd * yd));
        }

        public int CompareTo(object obj)
        {

            MapCellAStar n = (MapCellAStar)obj;
            int cFactor = this.totalCost - n.totalCost;
            return cFactor;
        }

        public bool isMatch(MapCellAStar n)
        {
            if (n != null)
                return (X == n.X && Y == n.Y);
            else
                return false;
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
                        MapCellAStar n = new MapCellAStar(this, this._goalcell, (short)(X + xd), (short)(Y + yd),MapId);
                        if (!n.isMatch(this.parentcell) && !n.isMatch(this))
                            successors.Add(n);

                    }
                }
            }
            return successors;
        }
    }
}
