using System;

namespace OpenNos.PathFinder
{
    public class Node : GridPos, IComparable<Node>
    {
        private Node node;

        public Node(GridPos node)
        {
            Value = node.Value;
            X = node.X;
            Y = node.Y;
        }

        public Node()
        {

        }

        #region Properties

        public double F { get; internal set; }

        public double N { get; internal set; }

        public bool Opened { get; internal set; }

        public Node Parent { get; internal set; }

        public bool Closed { get; internal set; }

        #endregion

        #region Methods

        public int CompareTo(Node other)
        {
            return F > other.F ? 1 : F < other.F ? -1 : 0;
        }

        #endregion
    }
}