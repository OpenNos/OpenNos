using System;

namespace OpenNos.Pathfinding
{
    public class Node :GridPos , IComparable<Node>
    {

        public Node(GridPos gridPos)
        {
            this.X = gridPos.X;
            this.Value = gridPos.Value;
            this.Y = gridPos.Y;
        }

        public Node()
        {
        }

        public Double G { get; internal set; }
        public Double N { get; internal set; }
        public Double H { get; internal set; }
        public Double F { get; internal set; }
        public bool Opened { get; internal set; }
        public bool Closed { get; internal set; }
        public Node Parent { get; internal set; }


        public int CompareTo(Node other)
        {
            return F > other.F ? 1 : F < other.F ? -1 : 0;
        }
    }
}