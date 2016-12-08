/*!
@file JumpPointFinder.cs
@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
        <http://github.com/juhgiyo/eppathfinding.cs>
@date July 16, 2013
@brief Jump Point Search Algorithm Interface
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

An Interface for the Jump Point Search Algorithm Class.

*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace EpPathFinding
{
    public delegate float HeuristicDelegate(int iDx, int iDy);

    public class JumpPointParam
    {
        public JumpPointParam(BaseGrid iGrid, GridPos iStartPos, GridPos iEndPos, bool iAllowEndNodeUnWalkable = true, bool iCrossCorner = true, bool iCrossAdjacentPoint = true, HeuristicMode iMode = HeuristicMode.Euclidean)
        {
            switch (iMode)
            {
                case HeuristicMode.Manhattan:
                    MHeuristic = Heuristic.Manhattan;
                    break;

                case HeuristicMode.Euclidean:
                    MHeuristic = Heuristic.Euclidean;
                    break;

                case HeuristicMode.Chebyshev:
                    MHeuristic = Heuristic.Chebyshev;
                    break;

                default:
                    MHeuristic = Heuristic.Euclidean;
                    break;
            }
            MAllowEndNodeUnWalkable = iAllowEndNodeUnWalkable;
            MCrossAdjacentPoint = iCrossAdjacentPoint;
            MCrossCorner = iCrossCorner;
            OpenList = new List<Node>();

            MSearchGrid = iGrid;
            MStartNode = MSearchGrid.GetNodeAt(iStartPos.X, iStartPos.Y);
            MEndNode = MSearchGrid.GetNodeAt(iEndPos.X, iEndPos.Y);
            if (MStartNode == null)
                MStartNode = new Node(iStartPos.X, iStartPos.Y, 0);
            if (MEndNode == null)
                MEndNode = new Node(iEndPos.X, iEndPos.Y, 0);
            MUseRecursive = false;
        }

        public JumpPointParam(BaseGrid iGrid, bool iAllowEndNodeUnWalkable = true, bool iCrossCorner = true, bool iCrossAdjacentPoint = true, HeuristicMode iMode = HeuristicMode.Euclidean)
        {
            switch (iMode)
            {
                case HeuristicMode.Manhattan:
                    MHeuristic = Heuristic.Manhattan;
                    break;

                case HeuristicMode.Euclidean:
                    MHeuristic = Heuristic.Euclidean;
                    break;

                case HeuristicMode.Chebyshev:
                    MHeuristic = Heuristic.Chebyshev;
                    break;

                default:
                    MHeuristic = Heuristic.Euclidean;
                    break;
            }
            MAllowEndNodeUnWalkable = iAllowEndNodeUnWalkable;
            MCrossAdjacentPoint = iCrossAdjacentPoint;
            MCrossCorner = iCrossCorner;

            OpenList = new List<Node>();

            MSearchGrid = iGrid;
            MStartNode = null;
            MEndNode = null;
            MUseRecursive = false;
        }

        public JumpPointParam(JumpPointParam b)
        {
            MHeuristic = b.MHeuristic;
            MAllowEndNodeUnWalkable = b.MAllowEndNodeUnWalkable;
            MCrossAdjacentPoint = b.MCrossAdjacentPoint;
            MCrossCorner = b.MCrossCorner;

            OpenList = new List<Node>(b.OpenList);

            MSearchGrid = b.MSearchGrid;
            MStartNode = b.MStartNode;
            MEndNode = b.MEndNode;
            MUseRecursive = b.MUseRecursive;
        }

        public void SetHeuristic(HeuristicMode iMode)
        {
            MHeuristic = null;
            switch (iMode)
            {
                case HeuristicMode.Manhattan:
                    MHeuristic = Heuristic.Manhattan;
                    break;

                case HeuristicMode.Euclidean:
                    MHeuristic = Heuristic.Euclidean;
                    break;

                case HeuristicMode.Chebyshev:
                    MHeuristic = Heuristic.Chebyshev;
                    break;

                default:
                    MHeuristic = Heuristic.Euclidean;
                    break;
            }
        }

        public void Reset(GridPos iStartPos, GridPos iEndPos, BaseGrid iSearchGrid = null)
        {
            OpenList.Clear();
            MStartNode = null;
            MEndNode = null;

            if (iSearchGrid != null)
                MSearchGrid = iSearchGrid;
            MSearchGrid.Reset();
            MStartNode = MSearchGrid.GetNodeAt(iStartPos.X, iStartPos.Y);
            MEndNode = MSearchGrid.GetNodeAt(iEndPos.X, iEndPos.Y);
            if (MStartNode == null)
                MStartNode = new Node(iStartPos.X, iStartPos.Y, 0);
            if (MEndNode == null)
                MEndNode = new Node(iEndPos.X, iEndPos.Y, 0);
        }

        public bool CrossAdjacentPoint
        {
            get
            {
                return MCrossCorner && MCrossAdjacentPoint;
            }
            set
            {
                MCrossAdjacentPoint = value;
            }
        }

        public bool CrossCorner
        {
            get
            {
                return MCrossCorner;
            }
            set
            {
                MCrossCorner = value;
            }
        }

        public bool AllowEndNodeUnWalkable
        {
            get
            {
                return MAllowEndNodeUnWalkable;
            }
            set
            {
                MAllowEndNodeUnWalkable = value;
            }
        }

        public HeuristicDelegate HeuristicFunc
        {
            get
            {
                return MHeuristic;
            }
        }

        public BaseGrid SearchGrid
        {
            get
            {
                return MSearchGrid;
            }
        }

        public Node StartNode
        {
            get
            {
                return MStartNode;
            }
        }

        public Node EndNode
        {
            get
            {
                return MEndNode;
            }
        }

        public bool UseRecursive
        {
            get
            {
                return MUseRecursive;
            }
            set
            {
                MUseRecursive = value;
            }
        }

        protected HeuristicDelegate MHeuristic;
        protected bool MCrossAdjacentPoint;
        protected bool MCrossCorner;
        protected bool MAllowEndNodeUnWalkable;

        protected bool MUseRecursive;

        protected BaseGrid MSearchGrid;
        protected Node MStartNode;
        protected Node MEndNode;

        public List<Node> OpenList;
    }

    public class JumpPointFinder
    {
        public static List<GridPos> GetFullPath(List<GridPos> routeFound)
        {

            List<GridPos> consecutiveGridList = new List<GridPos>();
            if (routeFound == null)
            { return consecutiveGridList; }
            if (routeFound.Count > 1)
                consecutiveGridList.Add(routeFound[0]);
            for (int routeTrav = 0; routeTrav < routeFound.Count - 1; routeTrav++)
            {
                GridPos fromGrid = routeFound[routeTrav];
                GridPos toGrid = routeFound[routeTrav + 1];
                int dX = toGrid.X - fromGrid.X;
                int dY = toGrid.Y - fromGrid.Y;
                if (dX != 0 && dY != 0) // diagonal move
                {
                    while (fromGrid != toGrid)
                    {
                        fromGrid.X += (dX / Math.Abs(dX));
                        fromGrid.Y += (dY / Math.Abs(dY));
                        consecutiveGridList.Add(new GridPos(fromGrid.X, fromGrid.Y));
                    }
                }
                else if (dX == 0)  // horizontal move
                {
                    while (fromGrid != toGrid)
                    {
                        fromGrid.Y += (dY / Math.Abs(dY));
                        consecutiveGridList.Add(new GridPos(fromGrid.X, fromGrid.Y));
                    }
                }
                else // vertical move
                {
                    while (fromGrid != toGrid)
                    {
                        fromGrid.X += dX / Math.Abs(dX);
                        consecutiveGridList.Add(new GridPos(fromGrid.X, fromGrid.Y));
                    }
                }
            }
            return consecutiveGridList;
        }

        public static List<GridPos> FindPath(JumpPointParam iParam)
        {
            List<Node> tOpenList = iParam.OpenList;
            Node tStartNode = iParam.StartNode;
            Node tEndNode = iParam.EndNode;
            bool revertEndNodeWalkable = false;

            // set the `g` and `f` value of the start node to be 0
            tStartNode.StartToCurNodeLen = 0;
            tStartNode.HeuristicStartToEndLen = 0;

            // push the start node into the open list
            tOpenList.Add(tStartNode);
            tStartNode.IsOpened = true;

            if (iParam.AllowEndNodeUnWalkable && !iParam.SearchGrid.IsWalkableAt(tEndNode.X, tEndNode.Y))
            {
                iParam.SearchGrid.SetWalkableAt(tEndNode.X, tEndNode.Y, 0);
                revertEndNodeWalkable = true;
            }

            // while the open list is not empty
            while (tOpenList.Any())
            {
                // pop the position of node which has the minimum `f` value.
                Node tNode = tOpenList.Last();
                tOpenList.RemoveAt(tOpenList.Count - 1);
                tNode.IsClosed = true;

                if (tNode.Equals(tEndNode))
                {
                    if (revertEndNodeWalkable)
                    {
                        iParam.SearchGrid.SetWalkableAt(tEndNode.X, tEndNode.Y, 1);
                    }
                    return Node.Backtrace(tNode); // rebuilding path
                }

                IdentifySuccessors(iParam, tNode);
            }

            if (revertEndNodeWalkable)
            {
                iParam.SearchGrid.SetWalkableAt(tEndNode.X, tEndNode.Y, 1);
            }

            // fail to find the path
            return new List<GridPos>();
        }

        private static void IdentifySuccessors(JumpPointParam iParam, Node iNode)
        {
            HeuristicDelegate tHeuristic = iParam.HeuristicFunc;
            List<Node> tOpenList = iParam.OpenList;
            if (iParam.EndNode != null)
            {
                int tEndX = iParam.EndNode.X;
                int tEndY = iParam.EndNode.Y;

                IEnumerable<GridPos> tNeighbors = FindNeighbors(iParam, iNode);
                foreach (GridPos tNeighbor in tNeighbors)
                {
                    GridPos tJumpPoint = iParam.UseRecursive ? Jump(iParam, tNeighbor.X, tNeighbor.Y, iNode.X, iNode.Y) : JumpLoop(iParam, tNeighbor.X, tNeighbor.Y, iNode.X, iNode.Y);
                    if (tJumpPoint != null)
                    {
                        Node tJumpNode = iParam.SearchGrid.GetNodeAt(tJumpPoint.X, tJumpPoint.Y);
                        if (tJumpNode == null)
                        {
                            if (iParam.EndNode.X == tJumpPoint.X && iParam.EndNode.Y == tJumpPoint.Y)
                                tJumpNode = iParam.SearchGrid.GetNodeAt(tJumpPoint);
                        }
                        if (tJumpNode != null && tJumpNode.IsClosed)
                        {
                            continue;
                        }
                        // include distance, as parent may not be immediately adjacent:
                        float tCurNodeToJumpNodeLen = tHeuristic(Math.Abs(tJumpPoint.X - iNode.X), Math.Abs(tJumpPoint.Y - iNode.Y));
                        float tStartToJumpNodeLen = iNode.StartToCurNodeLen + tCurNodeToJumpNodeLen; // next `startToCurNodeLen` value

                        if (tJumpNode != null && (!tJumpNode.IsOpened || tStartToJumpNodeLen < tJumpNode.StartToCurNodeLen))
                        {
                            tJumpNode.StartToCurNodeLen = tStartToJumpNodeLen;
                            tJumpNode.HeuristicCurNodeToEndLen = tJumpNode.HeuristicCurNodeToEndLen ?? tHeuristic(Math.Abs(tJumpPoint.X - tEndX), Math.Abs(tJumpPoint.Y - tEndY));
                            tJumpNode.HeuristicStartToEndLen = tJumpNode.StartToCurNodeLen + tJumpNode.HeuristicCurNodeToEndLen.Value;
                            tJumpNode.Parent = iNode;

                            if (!tJumpNode.IsOpened)
                            {
                                tOpenList.Add(tJumpNode);
                                tJumpNode.IsOpened = true;
                            }
                        }
                    }
                }
            }
        }

        private class JumpSnapshot
        {
            public int Ix;
            public int Iy;
            public int Px;
            public int Py;
            public int Dx;
            public int Dy;
            public GridPos Jx;
            public GridPos Jy;
            public int Stage;

            public JumpSnapshot()
            {
                Ix = 0;
                Iy = 0;
                Px = 0;
                Py = 0;
                Dx = 0;
                Dy = 0;
                Jx = null;
                Jy = null;
                Stage = 0;
            }
        }

        private static GridPos JumpLoop(JumpPointParam iParam, int iX, int iY, int iPx, int iPy)
        {
            GridPos retVal = null;
            Stack<JumpSnapshot> stack = new Stack<JumpSnapshot>();

            JumpSnapshot currentSnapshot = new JumpSnapshot
            {
                Ix = iX,
                Iy = iY,
                Px = iPx,
                Py = iPy,
                Stage = 0
            };

            stack.Push(currentSnapshot);
            while (stack.Count != 0)
            {
                currentSnapshot = stack.Pop();
                JumpSnapshot newSnapshot;
                switch (currentSnapshot.Stage)
                {
                    case 0:
                        if (!iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy))
                        {
                            retVal = null;
                            continue;
                        }
                        if (iParam.SearchGrid.GetNodeAt(currentSnapshot.Ix, currentSnapshot.Iy).Equals(iParam.EndNode))
                        {
                            retVal = new GridPos(currentSnapshot.Ix, currentSnapshot.Iy);
                            continue;
                        }

                        currentSnapshot.Dx = currentSnapshot.Ix - currentSnapshot.Px;
                        currentSnapshot.Dy = currentSnapshot.Iy - currentSnapshot.Py;
                        currentSnapshot.Jx = null;
                        currentSnapshot.Jy = null;
                        if (iParam.CrossCorner)
                        {
                            // check for forced neighbors
                            // along the diagonal
                            if (currentSnapshot.Dx != 0 && currentSnapshot.Dy != 0)
                            {
                                if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix - currentSnapshot.Dx, currentSnapshot.Iy + currentSnapshot.Dy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix - currentSnapshot.Dx, currentSnapshot.Iy)) ||
                                    (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy - currentSnapshot.Dy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy - currentSnapshot.Dy)))
                                {
                                    retVal = new GridPos(currentSnapshot.Ix, currentSnapshot.Iy);
                                    continue;
                                }
                            }
                            // horizontally/vertically
                            else
                            {
                                if (currentSnapshot.Dx != 0)
                                {
                                    // moving along x
                                    if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy + 1) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy + 1)) ||
                                        (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy - 1) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy - 1)))
                                    {
                                        retVal = new GridPos(currentSnapshot.Ix, currentSnapshot.Iy);
                                        continue;
                                    }
                                }
                                else
                                {
                                    if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + 1, currentSnapshot.Iy + currentSnapshot.Dy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + 1, currentSnapshot.Iy)) ||
                                        (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix - 1, currentSnapshot.Iy + currentSnapshot.Dy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix - 1, currentSnapshot.Iy)))
                                    {
                                        retVal = new GridPos(currentSnapshot.Ix, currentSnapshot.Iy);
                                        continue;
                                    }
                                }
                            }
                            // when moving diagonally, must check for vertical/horizontal jump points
                            if (currentSnapshot.Dx != 0 && currentSnapshot.Dy != 0)
                            {
                                currentSnapshot.Stage = 1;
                                stack.Push(currentSnapshot);

                                newSnapshot = new JumpSnapshot
                                {
                                    Ix = currentSnapshot.Ix + currentSnapshot.Dx,
                                    Iy = currentSnapshot.Iy,
                                    Px = currentSnapshot.Ix,
                                    Py = currentSnapshot.Iy,
                                    Stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }

                            // moving diagonally, must make sure one of the vertical/horizontal
                            // neighbors is open to allow the path

                            // moving diagonally, must make sure one of the vertical/horizontal
                            // neighbors is open to allow the path
                            if (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy) || iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy + currentSnapshot.Dy))
                            {
                                newSnapshot = new JumpSnapshot
                                {
                                    Ix = currentSnapshot.Ix + currentSnapshot.Dx,
                                    Iy = currentSnapshot.Iy + currentSnapshot.Dy,
                                    Px = currentSnapshot.Ix,
                                    Py = currentSnapshot.Iy,
                                    Stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }
                            if (iParam.CrossAdjacentPoint)
                            {
                                newSnapshot = new JumpSnapshot
                                {
                                    Ix = currentSnapshot.Ix + currentSnapshot.Dx,
                                    Iy = currentSnapshot.Iy + currentSnapshot.Dy,
                                    Px = currentSnapshot.Ix,
                                    Py = currentSnapshot.Iy,
                                    Stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }
                        }
                        else //if (!iParam.CrossCorner)
                        {
                            // check for forced neighbors
                            // along the diagonal
                            if (currentSnapshot.Dx != 0 && currentSnapshot.Dy != 0)
                            {
                                if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy + currentSnapshot.Dy) && iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy + currentSnapshot.Dy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy)) ||
                                    (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy + currentSnapshot.Dy) && iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy + currentSnapshot.Dy)))
                                {
                                    retVal = new GridPos(currentSnapshot.Ix, currentSnapshot.Iy);
                                    continue;
                                }
                            }
                            // horizontally/vertically
                            else
                            {
                                if (currentSnapshot.Dx != 0)
                                {
                                    // moving along x
                                    if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy + 1) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix - currentSnapshot.Dx, currentSnapshot.Iy + 1)) ||
                                        (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy - 1) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix - currentSnapshot.Dx, currentSnapshot.Iy - 1)))
                                    {
                                        retVal = new GridPos(currentSnapshot.Ix, currentSnapshot.Iy);
                                        continue;
                                    }
                                }
                                else
                                {
                                    if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + 1, currentSnapshot.Iy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + 1, currentSnapshot.Iy - currentSnapshot.Dy)) ||
                                        (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix - 1, currentSnapshot.Iy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix - 1, currentSnapshot.Iy - currentSnapshot.Dy)))
                                    {
                                        retVal = new GridPos(currentSnapshot.Ix, currentSnapshot.Iy);
                                        continue;
                                    }
                                }
                            }

                            // when moving diagonally, must check for vertical/horizontal jump points
                            if (currentSnapshot.Dx != 0 && currentSnapshot.Dy != 0)
                            {
                                currentSnapshot.Stage = 3;
                                stack.Push(currentSnapshot);

                                newSnapshot = new JumpSnapshot
                                {
                                    Ix = currentSnapshot.Ix + currentSnapshot.Dx,
                                    Iy = currentSnapshot.Iy,
                                    Px = currentSnapshot.Ix,
                                    Py = currentSnapshot.Iy,
                                    Stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }

                            // moving diagonally, must make sure both of the vertical/horizontal
                            // neighbors is open to allow the path
                            if (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy) && iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy + currentSnapshot.Dy))
                            {
                                newSnapshot = new JumpSnapshot
                                {
                                    Ix = currentSnapshot.Ix + currentSnapshot.Dx,
                                    Iy = currentSnapshot.Iy + currentSnapshot.Dy,
                                    Px = currentSnapshot.Ix,
                                    Py = currentSnapshot.Iy,
                                    Stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }
                        }
                        retVal = null;
                        break;

                    case 1:
                        currentSnapshot.Jx = retVal;

                        currentSnapshot.Stage = 2;
                        stack.Push(currentSnapshot);

                        newSnapshot = new JumpSnapshot
                        {
                            Ix = currentSnapshot.Ix,
                            Iy = currentSnapshot.Iy + currentSnapshot.Dy,
                            Px = currentSnapshot.Ix,
                            Py = currentSnapshot.Iy,
                            Stage = 0
                        };
                        stack.Push(newSnapshot);
                        break;

                    case 2:
                        currentSnapshot.Jy = retVal;
                        if (currentSnapshot.Jx != null || currentSnapshot.Jy != null)
                        {
                            retVal = new GridPos(currentSnapshot.Ix, currentSnapshot.Iy);
                            continue;
                        }

                        // moving diagonally, must make sure one of the vertical/horizontal
                        // neighbors is open to allow the path
                        if (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy) || iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy + currentSnapshot.Dy))
                        {
                            newSnapshot = new JumpSnapshot
                            {
                                Ix = currentSnapshot.Ix + currentSnapshot.Dx,
                                Iy = currentSnapshot.Iy + currentSnapshot.Dy,
                                Px = currentSnapshot.Ix,
                                Py = currentSnapshot.Iy,
                                Stage = 0
                            };
                            stack.Push(newSnapshot);
                            continue;
                        }
                        if (iParam.CrossAdjacentPoint)
                        {
                            newSnapshot = new JumpSnapshot
                            {
                                Ix = currentSnapshot.Ix + currentSnapshot.Dx,
                                Iy = currentSnapshot.Iy + currentSnapshot.Dy,
                                Px = currentSnapshot.Ix,
                                Py = currentSnapshot.Iy,
                                Stage = 0
                            };
                            stack.Push(newSnapshot);
                            continue;
                        }
                        retVal = null;
                        break;

                    case 3:
                        currentSnapshot.Jx = retVal;

                        currentSnapshot.Stage = 4;
                        stack.Push(currentSnapshot);

                        newSnapshot = new JumpSnapshot
                        {
                            Ix = currentSnapshot.Ix,
                            Iy = currentSnapshot.Iy + currentSnapshot.Dy,
                            Px = currentSnapshot.Ix,
                            Py = currentSnapshot.Iy,
                            Stage = 0
                        };
                        stack.Push(newSnapshot);
                        break;

                    case 4:
                        currentSnapshot.Jy = retVal;
                        if (currentSnapshot.Jx != null || currentSnapshot.Jy != null)
                        {
                            retVal = new GridPos(currentSnapshot.Ix, currentSnapshot.Iy);
                            continue;
                        }

                        // moving diagonally, must make sure both of the vertical/horizontal
                        // neighbors is open to allow the path
                        if (iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix + currentSnapshot.Dx, currentSnapshot.Iy) && iParam.SearchGrid.IsWalkableAt(currentSnapshot.Ix, currentSnapshot.Iy + currentSnapshot.Dy))
                        {
                            newSnapshot = new JumpSnapshot
                            {
                                Ix = currentSnapshot.Ix + currentSnapshot.Dx,
                                Iy = currentSnapshot.Iy + currentSnapshot.Dy,
                                Px = currentSnapshot.Ix,
                                Py = currentSnapshot.Iy,
                                Stage = 0
                            };
                            stack.Push(newSnapshot);
                            continue;
                        }
                        retVal = null;
                        break;
                }
            }

            return retVal;
        }

        private static GridPos Jump(JumpPointParam iParam, int iX, int iY, int iPx, int iPy)
        {
            while (true)
            {
                if (!iParam.SearchGrid.IsWalkableAt(iX, iY))
                {
                    return null;
                }
                if (iParam.SearchGrid.GetNodeAt(iX, iY).Equals(iParam.EndNode))
                {
                    return new GridPos(iX, iY);
                }

                int tDx = iX - iPx;
                int tDy = iY - iPy;
                GridPos jx;
                GridPos jy;
                if (iParam.CrossCorner)
                {
                    // check for forced neighbors
                    // along the diagonal
                    if (tDx != 0 && tDy != 0)
                    {
                        if ((iParam.SearchGrid.IsWalkableAt(iX - tDx, iY + tDy) && !iParam.SearchGrid.IsWalkableAt(iX - tDx, iY)) || (iParam.SearchGrid.IsWalkableAt(iX + tDx, iY - tDy) && !iParam.SearchGrid.IsWalkableAt(iX, iY - tDy)))
                        {
                            return new GridPos(iX, iY);
                        }
                    }
                    // horizontally/vertically
                    else
                    {
                        if (tDx != 0)
                        {
                            // moving along x
                            if ((iParam.SearchGrid.IsWalkableAt(iX + tDx, iY + 1) && !iParam.SearchGrid.IsWalkableAt(iX, iY + 1)) || (iParam.SearchGrid.IsWalkableAt(iX + tDx, iY - 1) && !iParam.SearchGrid.IsWalkableAt(iX, iY - 1)))
                            {
                                return new GridPos(iX, iY);
                            }
                        }
                        else
                        {
                            if ((iParam.SearchGrid.IsWalkableAt(iX + 1, iY + tDy) && !iParam.SearchGrid.IsWalkableAt(iX + 1, iY)) || (iParam.SearchGrid.IsWalkableAt(iX - 1, iY + tDy) && !iParam.SearchGrid.IsWalkableAt(iX - 1, iY)))
                            {
                                return new GridPos(iX, iY);
                            }
                        }
                    }
                    // when moving diagonally, must check for vertical/horizontal jump points
                    if (tDx != 0 && tDy != 0)
                    {
                        jx = Jump(iParam, iX + tDx, iY, iX, iY);
                        jy = Jump(iParam, iX, iY + tDy, iX, iY);
                        if (jx != null || jy != null)
                        {
                            return new GridPos(iX, iY);
                        }
                    }

                    // moving diagonally, must make sure one of the vertical/horizontal
                    // neighbors is open to allow the path
                    if (iParam.SearchGrid.IsWalkableAt(iX + tDx, iY) || iParam.SearchGrid.IsWalkableAt(iX, iY + tDy))
                    {
                        var iX1 = iX;
                        var iY1 = iY;
                        iX = iX + tDx;
                        iY = iY + tDy;
                        iPx = iX1;
                        iPy = iY1;
                        continue;
                    }
                    if (iParam.CrossAdjacentPoint)
                    {
                        var iX1 = iX;
                        var iY1 = iY;
                        iX = iX + tDx;
                        iY = iY + tDy;
                        iPx = iX1;
                        iPy = iY1;
                        continue;
                    }
                    return null;
                }
                // check for forced neighbors
                // along the diagonal
                if (tDx != 0 && tDy != 0)
                {
                    if ((iParam.SearchGrid.IsWalkableAt(iX + tDx, iY + tDy) && iParam.SearchGrid.IsWalkableAt(iX, iY + tDy) && !iParam.SearchGrid.IsWalkableAt(iX + tDx, iY)) || (iParam.SearchGrid.IsWalkableAt(iX + tDx, iY + tDy) && iParam.SearchGrid.IsWalkableAt(iX + tDx, iY) && !iParam.SearchGrid.IsWalkableAt(iX, iY + tDy)))
                    {
                        return new GridPos(iX, iY);
                    }
                }
                // horizontally/vertically
                else
                {
                    if (tDx != 0)
                    {
                        // moving along x
                        if ((iParam.SearchGrid.IsWalkableAt(iX, iY + 1) && !iParam.SearchGrid.IsWalkableAt(iX - tDx, iY + 1)) || (iParam.SearchGrid.IsWalkableAt(iX, iY - 1) && !iParam.SearchGrid.IsWalkableAt(iX - tDx, iY - 1)))
                        {
                            return new GridPos(iX, iY);
                        }
                    }
                    else
                    {
                        if ((iParam.SearchGrid.IsWalkableAt(iX + 1, iY) && !iParam.SearchGrid.IsWalkableAt(iX + 1, iY - tDy)) || (iParam.SearchGrid.IsWalkableAt(iX - 1, iY) && !iParam.SearchGrid.IsWalkableAt(iX - 1, iY - tDy)))
                        {
                            return new GridPos(iX, iY);
                        }
                    }
                }

                // when moving diagonally, must check for vertical/horizontal jump points
                if (tDx != 0 && tDy != 0)
                {
                    jx = Jump(iParam, iX + tDx, iY, iX, iY);
                    jy = Jump(iParam, iX, iY + tDy, iX, iY);
                    if (jx != null || jy != null)
                    {
                        return new GridPos(iX, iY);
                    }
                }

                // moving diagonally, must make sure both of the vertical/horizontal
                // neighbors is open to allow the path
                if (iParam.SearchGrid.IsWalkableAt(iX + tDx, iY) && iParam.SearchGrid.IsWalkableAt(iX, iY + tDy))
                {
                    var iX1 = iX;
                    var iY1 = iY;
                    iX = iX + tDx;
                    iY = iY + tDy;
                    iPx = iX1;
                    iPy = iY1;
                    continue;
                }
                return null;
            }
        }

        private static IEnumerable<GridPos> FindNeighbors(JumpPointParam iParam, Node iNode)
        {
            Node tParent = (Node)iNode.Parent;
            int tX = iNode.X;
            int tY = iNode.Y;
            List<GridPos> tNeighbors = new List<GridPos>();

            // directed pruning: can ignore most neighbors, unless forced.
            if (tParent != null)
            {
                int tPx = tParent.X;
                int tPy = tParent.Y;
                // get the normalized direction of travel
                int tDx = (tX - tPx) / Math.Max(Math.Abs(tX - tPx), 1);
                int tDy = (tY - tPy) / Math.Max(Math.Abs(tY - tPy), 1);

                if (iParam.CrossCorner)
                {
                    // search diagonally
                    if (tDx != 0 && tDy != 0)
                    {
                        if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy))
                        {
                            tNeighbors.Add(new GridPos(tX, tY + tDy));
                        }
                        if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY))
                        {
                            tNeighbors.Add(new GridPos(tX + tDx, tY));
                        }

                        if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY + tDy))
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy) || iParam.SearchGrid.IsWalkableAt(tX + tDx, tY))
                            {
                                tNeighbors.Add(new GridPos(tX + tDx, tY + tDy));
                            }
                            else if (iParam.CrossAdjacentPoint)
                            {
                                tNeighbors.Add(new GridPos(tX + tDx, tY + tDy));
                            }
                        }

                        if (iParam.SearchGrid.IsWalkableAt(tX - tDx, tY + tDy))
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy) && !iParam.SearchGrid.IsWalkableAt(tX - tDx, tY))
                            {
                                tNeighbors.Add(new GridPos(tX - tDx, tY + tDy));
                            }
                            else if (iParam.CrossAdjacentPoint)
                            {
                                tNeighbors.Add(new GridPos(tX - tDx, tY + tDy));
                            }
                        }

                        if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY - tDy))
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY) && !iParam.SearchGrid.IsWalkableAt(tX, tY - tDy))
                            {
                                tNeighbors.Add(new GridPos(tX + tDx, tY - tDy));
                            }
                            else if (iParam.CrossAdjacentPoint)
                            {
                                tNeighbors.Add(new GridPos(tX + tDx, tY - tDy));
                            }
                        }
                    }
                    // search horizontally/vertically
                    else
                    {
                        if (tDx == 0)
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy))
                            {
                                tNeighbors.Add(new GridPos(tX, tY + tDy));

                                if (iParam.SearchGrid.IsWalkableAt(tX + 1, tY + tDy) && !iParam.SearchGrid.IsWalkableAt(tX + 1, tY))
                                {
                                    tNeighbors.Add(new GridPos(tX + 1, tY + tDy));
                                }
                                if (iParam.SearchGrid.IsWalkableAt(tX - 1, tY + tDy) && !iParam.SearchGrid.IsWalkableAt(tX - 1, tY))
                                {
                                    tNeighbors.Add(new GridPos(tX - 1, tY + tDy));
                                }
                            }
                            else if (iParam.CrossAdjacentPoint)
                            {
                                if (iParam.SearchGrid.IsWalkableAt(tX + 1, tY + tDy) && !iParam.SearchGrid.IsWalkableAt(tX + 1, tY))
                                {
                                    tNeighbors.Add(new GridPos(tX + 1, tY + tDy));
                                }
                                if (iParam.SearchGrid.IsWalkableAt(tX - 1, tY + tDy) && !iParam.SearchGrid.IsWalkableAt(tX - 1, tY))
                                {
                                    tNeighbors.Add(new GridPos(tX - 1, tY + tDy));
                                }
                            }
                        }
                        else
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY))
                            {
                                tNeighbors.Add(new GridPos(tX + tDx, tY));

                                if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY + 1) && !iParam.SearchGrid.IsWalkableAt(tX, tY + 1))
                                {
                                    tNeighbors.Add(new GridPos(tX + tDx, tY + 1));
                                }
                                if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY - 1) && !iParam.SearchGrid.IsWalkableAt(tX, tY - 1))
                                {
                                    tNeighbors.Add(new GridPos(tX + tDx, tY - 1));
                                }
                            }
                            else if (iParam.CrossAdjacentPoint)
                            {
                                if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY + 1) && !iParam.SearchGrid.IsWalkableAt(tX, tY + 1))
                                {
                                    tNeighbors.Add(new GridPos(tX + tDx, tY + 1));
                                }
                                if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY - 1) && !iParam.SearchGrid.IsWalkableAt(tX, tY - 1))
                                {
                                    tNeighbors.Add(new GridPos(tX + tDx, tY - 1));
                                }
                            }
                        }
                    }
                }
                else // if(!iParam.CrossCorner)
                {
                    // search diagonally
                    if (tDx != 0 && tDy != 0)
                    {
                        if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy))
                        {
                            tNeighbors.Add(new GridPos(tX, tY + tDy));
                        }
                        if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY))
                        {
                            tNeighbors.Add(new GridPos(tX + tDx, tY));
                        }

                        if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY + tDy))
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy) && iParam.SearchGrid.IsWalkableAt(tX + tDx, tY))
                                tNeighbors.Add(new GridPos(tX + tDx, tY + tDy));
                        }

                        if (iParam.SearchGrid.IsWalkableAt(tX - tDx, tY + tDy))
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy) && iParam.SearchGrid.IsWalkableAt(tX - tDx, tY))
                                tNeighbors.Add(new GridPos(tX - tDx, tY + tDy));
                        }

                        if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY - tDy))
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY - tDy) && iParam.SearchGrid.IsWalkableAt(tX + tDx, tY))
                                tNeighbors.Add(new GridPos(tX + tDx, tY - tDy));
                        }
                    }
                    // search horizontally/vertically
                    else
                    {
                        if (tDx == 0)
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy))
                            {
                                tNeighbors.Add(new GridPos(tX, tY + tDy));

                                if (iParam.SearchGrid.IsWalkableAt(tX + 1, tY + tDy) && iParam.SearchGrid.IsWalkableAt(tX + 1, tY))
                                {
                                    tNeighbors.Add(new GridPos(tX + 1, tY + tDy));
                                }
                                if (iParam.SearchGrid.IsWalkableAt(tX - 1, tY + tDy) && iParam.SearchGrid.IsWalkableAt(tX - 1, tY))
                                {
                                    tNeighbors.Add(new GridPos(tX - 1, tY + tDy));
                                }
                            }
                            if (iParam.SearchGrid.IsWalkableAt(tX + 1, tY))
                                tNeighbors.Add(new GridPos(tX + 1, tY));
                            if (iParam.SearchGrid.IsWalkableAt(tX - 1, tY))
                                tNeighbors.Add(new GridPos(tX - 1, tY));
                        }
                        else
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY))
                            {
                                tNeighbors.Add(new GridPos(tX + tDx, tY));

                                if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY + 1) && iParam.SearchGrid.IsWalkableAt(tX, tY + 1))
                                {
                                    tNeighbors.Add(new GridPos(tX + tDx, tY + 1));
                                }
                                if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY - 1) && iParam.SearchGrid.IsWalkableAt(tX, tY - 1))
                                {
                                    tNeighbors.Add(new GridPos(tX + tDx, tY - 1));
                                }
                            }
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY + 1))
                                tNeighbors.Add(new GridPos(tX, tY + 1));
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY - 1))
                                tNeighbors.Add(new GridPos(tX, tY - 1));
                        }
                    }
                }
            }
            // return all neighbors
            else
            {
                List<Node> tNeighborNodes = iParam.SearchGrid.GetNeighbors(iNode, iParam.CrossCorner, iParam.CrossAdjacentPoint);
                tNeighbors.AddRange(tNeighborNodes.Select(tNeighborNode => new GridPos(tNeighborNode.X, tNeighborNode.Y)));
            }

            return tNeighbors;
        }
    }
}