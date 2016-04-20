using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MapCell
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



        public short X {
            get; set;
        }
        public short Y {
            get; set; }
        public short MapId
        {
            get; set;
        }

        private MapCell _goalcell;
        public MapCell parentcell;


        public MapCell(MapCell parentcell, MapCell goalcell, short x, short y,short MapId)
        {

            this.parentcell = parentcell;
            this._goalcell = goalcell;
            this.X = x;
            this.Y = y;
            this.MapId = MapId;
            Initcell();
        }

        public MapCell()
        {
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

            MapCell n = (MapCell)obj;
            int cFactor = this.totalCost - n.totalCost;
            return cFactor;
        }

        public bool isMatch(MapCell n)
        {
            if (n != null)
                return (X == n.X && Y == n.Y);
            else
                return false;
        }

        public ArrayList GetSuccessors()
        {
            ArrayList successors = new ArrayList();

            for (short xd = -1; xd <= 1; xd++)
            {
                for (short yd = -1; yd <= 1; yd++)
                {
                    if (!ServerManager.GetMap(MapId).IsBlockedZone(X + xd, Y + yd))
                    {
                        MapCell n = new MapCell(this, this._goalcell, (short)(X + xd), (short)(Y + yd),MapId);
                        if (!n.isMatch(this.parentcell) && !n.isMatch(this))
                            successors.Add(n);

                    }
                }
            }
            return successors;
        }
    }
}
