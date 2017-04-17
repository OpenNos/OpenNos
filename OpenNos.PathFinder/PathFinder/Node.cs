using System;

namespace OpenNos.PathFinder
{
    public class Node : GridPos, IComparable<Node>
    {
        #region Properties

        public Double F { get; internal set; }

        public Double N { get; internal set; }

        public bool Opened { get; internal set; }

        public Node Parent { get; internal set; }

        #endregion

        #region Methods

        public int CompareTo(Node other)
        {
            return F > other.F ? 1 : F < other.F ? -1 : 0;
        }

        #endregion
    }
}