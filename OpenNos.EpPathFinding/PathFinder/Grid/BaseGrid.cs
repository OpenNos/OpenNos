/*!
@file BaseGrid.cs
@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
        <http://github.com/juhgiyo/eppathfinding.cs>
@date July 16, 2013
@brief BaseGrid Interface
@version 2.0

@section LICENSE

The MIT License (MIT)

Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

@section DESCRIPTION

An Interface for the BaseGrid Class.

*/

using System;
using System.Collections.Generic;

namespace EpPathFinding
{
    public abstract class BaseGrid
    {
        #region Members

        protected GridRect MGridRect;

        #endregion

        #region Instantiation

        public BaseGrid()
        {
            MGridRect = new GridRect();
        }

        public BaseGrid(BaseGrid b)
        {
            MGridRect = new GridRect(b.MGridRect);
            Width = b.Width;
            Height = b.Height;
        }

        #endregion

        #region Properties

        public GridRect GridRect
        {
            get { return MGridRect; }
        }

        public abstract int Height { get; protected set; }

        public abstract int Width { get; protected set; }

        #endregion

        #region Methods

        public abstract BaseGrid Clone();

        public List<Node> GetNeighbors(Node iNode, bool iCrossCorners, bool iCrossAdjacentPoint)
        {
            int tX = iNode.x;
            int tY = iNode.y;
            List<Node> neighbors = new List<Node>();
            bool tS0 = false, tD0,
                tS1 = false, tD1,
                tS2 = false, tD2,
                tS3 = false, tD3;

            GridPos pos = new GridPos();
            if (IsWalkableAt(pos.Set(tX, tY - 1)))
            {
                neighbors.Add(GetNodeAt(pos));
                tS0 = true;
            }
            if (IsWalkableAt(pos.Set(tX + 1, tY)))
            {
                neighbors.Add(GetNodeAt(pos));
                tS1 = true;
            }
            if (IsWalkableAt(pos.Set(tX, tY + 1)))
            {
                neighbors.Add(GetNodeAt(pos));
                tS2 = true;
            }
            if (IsWalkableAt(pos.Set(tX - 1, tY)))
            {
                neighbors.Add(GetNodeAt(pos));
                tS3 = true;
            }
            if (iCrossCorners && iCrossAdjacentPoint)
            {
                tD0 = true;
                tD1 = true;
                tD2 = true;
                tD3 = true;
            }
            else if (iCrossCorners)
            {
                tD0 = tS3 || tS0;
                tD1 = tS0 || tS1;
                tD2 = tS1 || tS2;
                tD3 = tS2 || tS3;
            }
            else
            {
                tD0 = tS3 && tS0;
                tD1 = tS0 && tS1;
                tD2 = tS1 && tS2;
                tD3 = tS2 && tS3;
            }

            if (tD0 && IsWalkableAt(pos.Set(tX - 1, tY - 1)))
            {
                neighbors.Add(GetNodeAt(pos));
            }
            if (tD1 && IsWalkableAt(pos.Set(tX + 1, tY - 1)))
            {
                neighbors.Add(GetNodeAt(pos));
            }
            if (tD2 && IsWalkableAt(pos.Set(tX + 1, tY + 1)))
            {
                neighbors.Add(GetNodeAt(pos));
            }
            if (tD3 && IsWalkableAt(pos.Set(tX - 1, tY + 1)))
            {
                neighbors.Add(GetNodeAt(pos));
            }
            return neighbors;
        }

        public abstract Node GetNodeAt(int iX, int iY);

        public abstract Node GetNodeAt(GridPos iPos);

        public abstract bool IsWalkableAt(int iX, int iY);

        public abstract bool IsWalkableAt(GridPos iPos);

        public abstract void Reset();

        public abstract bool SetWalkableAt(int iX, int iY, byte iWalkable);

        public abstract bool SetWalkableAt(GridPos iPos, byte iWalkable);

        #endregion
    }

    public class Node : IComparable
    {
        #region Members

        public float? heuristicCurNodeToEndLen;
        public float heuristicStartToEndLen;
        public bool isClosed;
        public bool isOpened;
        public object parent;
        public float startToCurNodeLen;
        public byte walkable;
        public int x;
        public int y;

        #endregion

        #region Instantiation

        public Node(int iX, int iY, byte? iWalkable = null)
        {
            x = iX;
            y = iY;
            walkable = iWalkable ?? 1;
            heuristicStartToEndLen = 0;
            startToCurNodeLen = 0;
            heuristicCurNodeToEndLen = null;
            isOpened = false;
            isClosed = false;
            parent = null;
        }

        public Node(Node b)
        {
            x = b.x;
            y = b.y;
            walkable = b.walkable;
            heuristicStartToEndLen = b.heuristicStartToEndLen;
            startToCurNodeLen = b.startToCurNodeLen;
            heuristicCurNodeToEndLen = b.heuristicCurNodeToEndLen;
            isOpened = b.isOpened;
            isClosed = b.isClosed;
            parent = b.parent;
        }

        #endregion

        #region Methods

        public static List<GridPos> Backtrace(Node iNode)
        {
            List<GridPos> path = new List<GridPos> { new GridPos(iNode.x, iNode.y) };
            while (iNode.parent != null)
            {
                iNode = (Node)iNode.parent;
                path.Add(new GridPos(iNode.x, iNode.y));
            }
            path.Reverse();
            return path;
        }

        public static bool operator !=(Node a, Node b)
        {
            return !(a == b);
        }

        public static bool operator ==(Node a, Node b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            // Return true if the fields match:
            return a.x == b.x && a.y == b.y;
        }

        public int CompareTo(object iObj)
        {
            Node tOtherNode = (Node)iObj;
            float result = heuristicStartToEndLen - tOtherNode.heuristicStartToEndLen;
            if (result > 0.0f)
                return -1;
            return result == 0.0f ? 0 : 1;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Point return false.
            Node p = obj as Node;
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return x == p.x && y == p.y;
        }

        public bool Equals(Node p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return x == p.x && y == p.y;
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }

        public void Reset(byte? iWalkable = null)
        {
            if (iWalkable.HasValue)
                walkable = iWalkable.Value;
            heuristicStartToEndLen = 0;
            startToCurNodeLen = 0;
            heuristicCurNodeToEndLen = null;
            isOpened = false;
            isClosed = false;
            parent = null;
        }

        #endregion
    }
}