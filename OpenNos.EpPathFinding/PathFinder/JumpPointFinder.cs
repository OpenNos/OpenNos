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
        public JumpPointParam(BaseGrid iGrid, GridPos iStartPos, GridPos iEndPos, bool iAllowEndNodeUnWalkable = true, bool iCrossCorner = true, bool iCrossAdjacentPoint = true, HeuristicMode iMode = HeuristicMode.EUCLIDEAN)
        {
            switch (iMode)
            {
                case HeuristicMode.MANHATTAN:
                    m_heuristic = Heuristic.Manhattan;
                    break;

                case HeuristicMode.EUCLIDEAN:
                    m_heuristic = Heuristic.Euclidean;
                    break;

                case HeuristicMode.CHEBYSHEV:
                    m_heuristic = Heuristic.Chebyshev;
                    break;

                default:
                    m_heuristic = Heuristic.Euclidean;
                    break;
            }
            m_allowEndNodeUnWalkable = iAllowEndNodeUnWalkable;
            m_crossAdjacentPoint = iCrossAdjacentPoint;
            m_crossCorner = iCrossCorner;
            openList = new List<Node>();

            m_searchGrid = iGrid;
            m_startNode = m_searchGrid.GetNodeAt(iStartPos.x, iStartPos.y);
            m_endNode = m_searchGrid.GetNodeAt(iEndPos.x, iEndPos.y);
            if (m_startNode == null)
            {
                m_startNode = new Node(iStartPos.x, iStartPos.y, 0);
            }
            if (m_endNode == null)
            {
                m_endNode = new Node(iEndPos.x, iEndPos.y, 0);
            }
            m_useRecursive = false;
        }

        public JumpPointParam(BaseGrid iGrid, bool iAllowEndNodeUnWalkable = true, bool iCrossCorner = true, bool iCrossAdjacentPoint = true, HeuristicMode iMode = HeuristicMode.EUCLIDEAN)
        {
            switch (iMode)
            {
                case HeuristicMode.MANHATTAN:
                    m_heuristic = Heuristic.Manhattan;
                    break;

                case HeuristicMode.EUCLIDEAN:
                    m_heuristic = Heuristic.Euclidean;
                    break;

                case HeuristicMode.CHEBYSHEV:
                    m_heuristic = Heuristic.Chebyshev;
                    break;

                default:
                    m_heuristic = Heuristic.Euclidean;
                    break;
            }
            m_allowEndNodeUnWalkable = iAllowEndNodeUnWalkable;
            m_crossAdjacentPoint = iCrossAdjacentPoint;
            m_crossCorner = iCrossCorner;

            openList = new List<Node>();

            m_searchGrid = iGrid;
            m_startNode = null;
            m_endNode = null;
            m_useRecursive = false;
        }

        public JumpPointParam(JumpPointParam b)
        {
            m_heuristic = b.m_heuristic;
            m_allowEndNodeUnWalkable = b.m_allowEndNodeUnWalkable;
            m_crossAdjacentPoint = b.m_crossAdjacentPoint;
            m_crossCorner = b.m_crossCorner;

            openList = new List<Node>(b.openList);

            m_searchGrid = b.m_searchGrid;
            m_startNode = b.m_startNode;
            m_endNode = b.m_endNode;
            m_useRecursive = b.m_useRecursive;
        }

        public void SetHeuristic(HeuristicMode iMode)
        {
            m_heuristic = null;
            switch (iMode)
            {
                case HeuristicMode.MANHATTAN:
                    m_heuristic = Heuristic.Manhattan;
                    break;

                case HeuristicMode.EUCLIDEAN:
                    m_heuristic = Heuristic.Euclidean;
                    break;

                case HeuristicMode.CHEBYSHEV:
                    m_heuristic = Heuristic.Chebyshev;
                    break;

                default:
                    m_heuristic = Heuristic.Euclidean;
                    break;
            }
        }

        public void Reset(GridPos iStartPos, GridPos iEndPos, BaseGrid iSearchGrid = null)
        {
            openList.Clear();
            m_startNode = null;
            m_endNode = null;

            if (iSearchGrid != null)
            {
                m_searchGrid = iSearchGrid;
            }
            m_searchGrid.Reset();
            m_startNode = m_searchGrid.GetNodeAt(iStartPos.x, iStartPos.y);
            m_endNode = m_searchGrid.GetNodeAt(iEndPos.x, iEndPos.y);
            if (m_startNode == null)
            {
                m_startNode = new Node(iStartPos.x, iStartPos.y, 0);
            }
            if (m_endNode == null)
            {
                m_endNode = new Node(iEndPos.x, iEndPos.y, 0);
            }
        }

        public bool CrossAdjacentPoint
        {
            get
            {
                return m_crossCorner && m_crossAdjacentPoint;
            }
            set
            {
                m_crossAdjacentPoint = value;
            }
        }

        public bool CrossCorner
        {
            get
            {
                return m_crossCorner;
            }
            set
            {
                m_crossCorner = value;
            }
        }

        public bool AllowEndNodeUnWalkable
        {
            get
            {
                return m_allowEndNodeUnWalkable;
            }
            set
            {
                m_allowEndNodeUnWalkable = value;
            }
        }

        public HeuristicDelegate HeuristicFunc
        {
            get
            {
                return m_heuristic;
            }
        }

        public BaseGrid SearchGrid
        {
            get
            {
                return m_searchGrid;
            }
        }

        public Node StartNode
        {
            get
            {
                return m_startNode;
            }
        }

        public Node EndNode
        {
            get
            {
                return m_endNode;
            }
        }

        public bool UseRecursive
        {
            get
            {
                return m_useRecursive;
            }
            set
            {
                m_useRecursive = value;
            }
        }

        protected HeuristicDelegate m_heuristic;
        protected bool m_crossAdjacentPoint;
        protected bool m_crossCorner;
        protected bool m_allowEndNodeUnWalkable;

        protected bool m_useRecursive;

        protected BaseGrid m_searchGrid;
        protected Node m_startNode;
        protected Node m_endNode;

        public List<Node> openList;
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
                int dX = toGrid.x - fromGrid.x;
                int dY = toGrid.y - fromGrid.y;
                if (dX != 0 && dY != 0) // diagonal move
                {
                    while (fromGrid != toGrid)
                    {
                        fromGrid.x += (dX / Math.Abs(dX));
                        fromGrid.y += (dY / Math.Abs(dY));
                        consecutiveGridList.Add(new GridPos(fromGrid.x, fromGrid.y));
                    }
                }
                else if (dX == 0)  // horizontal move
                {
                    while (fromGrid != toGrid)
                    {
                        fromGrid.y += (dY / Math.Abs(dY));
                        consecutiveGridList.Add(new GridPos(fromGrid.x, fromGrid.y));
                    }
                }
                else // vertical move
                {
                    while (fromGrid != toGrid)
                    {
                        fromGrid.x += (dX / Math.Abs(dX));
                        consecutiveGridList.Add(new GridPos(fromGrid.x, fromGrid.y));
                    }
                }
            }
            return consecutiveGridList;
        }

        public static List<GridPos> FindPath(JumpPointParam iParam)
        {
            List<Node> tOpenList = iParam.openList;
            Node tStartNode = iParam.StartNode;
            Node tEndNode = iParam.EndNode;
            bool revertEndNodeWalkable = false;

            // set the `g` and `f` value of the start node to be 0
            tStartNode.startToCurNodeLen = 0;
            tStartNode.heuristicStartToEndLen = 0;

            // push the start node into the open list
            tOpenList.Add(tStartNode);
            tStartNode.isOpened = true;

            if (iParam.AllowEndNodeUnWalkable && !iParam.SearchGrid.IsWalkableAt(tEndNode.x, tEndNode.y))
            {
                iParam.SearchGrid.SetWalkableAt(tEndNode.x, tEndNode.y, 0);
                revertEndNodeWalkable = true;
            }

            // while the open list is not empty
            while (tOpenList.Any())
            {
                // pop the position of node which has the minimum `f` value.
                Node tNode = tOpenList.Last();
                tOpenList.RemoveAt(tOpenList.Count - 1);
                tNode.isClosed = true;

                if (tNode.Equals(tEndNode))
                {
                    if (revertEndNodeWalkable)
                    {
                        iParam.SearchGrid.SetWalkableAt(tEndNode.x, tEndNode.y, 1);
                    }
                    return Node.Backtrace(tNode); // rebuilding path
                }

                identifySuccessors(iParam, tNode);
            }

            if (revertEndNodeWalkable)
            {
                iParam.SearchGrid.SetWalkableAt(tEndNode.x, tEndNode.y, 1);
            }

            // fail to find the path
            return new List<GridPos>();
        }

        private static void identifySuccessors(JumpPointParam iParam, Node iNode)
        {
            HeuristicDelegate tHeuristic = iParam.HeuristicFunc;
            List<Node> tOpenList = iParam.openList;
            if (iParam.EndNode != null)
            {
                int tEndX = iParam.EndNode.x;
                int tEndY = iParam.EndNode.y;

                IEnumerable<GridPos> tNeighbors = findNeighbors(iParam, iNode);
                foreach (GridPos gridPos in tNeighbors)
                {
                    GridPos tNeighbor = gridPos;
                    GridPos tJumpPoint = iParam.UseRecursive ? jump(iParam, tNeighbor.x, tNeighbor.y, iNode.x, iNode.y) : jumpLoop(iParam, tNeighbor.x, tNeighbor.y, iNode.x, iNode.y);
                    if (tJumpPoint != null)
                    {
                        Node tJumpNode = iParam.SearchGrid.GetNodeAt(tJumpPoint.x, tJumpPoint.y);
                        if (tJumpNode == null)
                        {
                            if (iParam.EndNode.x == tJumpPoint.x && iParam.EndNode.y == tJumpPoint.y)
                            {
                                tJumpNode = iParam.SearchGrid.GetNodeAt(tJumpPoint);
                            }
                        }
                        if (tJumpNode != null && tJumpNode.isClosed)
                        {
                            continue;
                        }
                        // include distance, as parent may not be immediately adjacent:
                        float tCurNodeToJumpNodeLen = tHeuristic(Math.Abs(tJumpPoint.x - iNode.x), Math.Abs(tJumpPoint.y - iNode.y));
                        float tStartToJumpNodeLen = iNode.startToCurNodeLen + tCurNodeToJumpNodeLen; // next `startToCurNodeLen` value

                        if (tJumpNode != null && (!tJumpNode.isOpened || tStartToJumpNodeLen < tJumpNode.startToCurNodeLen))
                        {
                            tJumpNode.startToCurNodeLen = tStartToJumpNodeLen;
                            tJumpNode.heuristicCurNodeToEndLen = tJumpNode.heuristicCurNodeToEndLen ?? tHeuristic(Math.Abs(tJumpPoint.x - tEndX), Math.Abs(tJumpPoint.y - tEndY));
                            tJumpNode.heuristicStartToEndLen = tJumpNode.startToCurNodeLen + tJumpNode.heuristicCurNodeToEndLen.Value;
                            tJumpNode.parent = iNode;

                            if (!tJumpNode.isOpened)
                            {
                                tOpenList.Add(tJumpNode);
                                tJumpNode.isOpened = true;
                            }
                        }
                    }
                }
            }
        }

        private class JumpSnapshot
        {
            public int iX;
            public int iY;
            public int iPx;
            public int iPy;
            public int tDx;
            public int tDy;
            public GridPos jx;
            public GridPos jy;
            public int stage;

            public JumpSnapshot()
            {
                iX = 0;
                iY = 0;
                iPx = 0;
                iPy = 0;
                tDx = 0;
                tDy = 0;
                jx = null;
                jy = null;
                stage = 0;
            }
        }

        private static GridPos jumpLoop(JumpPointParam iParam, int iX, int iY, int iPx, int iPy)
        {
            GridPos retVal = null;
            Stack<JumpSnapshot> stack = new Stack<JumpSnapshot>();

            JumpSnapshot currentSnapshot = new JumpSnapshot
            {
                iX = iX,
                iY = iY,
                iPx = iPx,
                iPy = iPy,
                stage = 0
            };

            stack.Push(currentSnapshot);
            while (stack.Count != 0)
            {
                currentSnapshot = stack.Pop();
                JumpSnapshot newSnapshot;
                switch (currentSnapshot.stage)
                {
                    case 0:
                        if (!iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY))
                        {
                            retVal = null;
                            continue;
                        }
                        if (iParam.SearchGrid.GetNodeAt(currentSnapshot.iX, currentSnapshot.iY).Equals(iParam.EndNode))
                        {
                            retVal = new GridPos(currentSnapshot.iX, currentSnapshot.iY);
                            continue;
                        }

                        currentSnapshot.tDx = currentSnapshot.iX - currentSnapshot.iPx;
                        currentSnapshot.tDy = currentSnapshot.iY - currentSnapshot.iPy;
                        currentSnapshot.jx = null;
                        currentSnapshot.jy = null;
                        if (iParam.CrossCorner)
                        {
                            // check for forced neighbors
                            // along the diagonal
                            if (currentSnapshot.tDx != 0 && currentSnapshot.tDy != 0)
                            {
                                if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX - currentSnapshot.tDx, currentSnapshot.iY + currentSnapshot.tDy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX - currentSnapshot.tDx, currentSnapshot.iY)) ||
                                    (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY - currentSnapshot.tDy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY - currentSnapshot.tDy)))
                                {
                                    retVal = new GridPos(currentSnapshot.iX, currentSnapshot.iY);
                                    continue;
                                }
                            }
                            // horizontally/vertically
                            else
                            {
                                if (currentSnapshot.tDx != 0)
                                {
                                    // moving along x
                                    if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY + 1) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + 1)) ||
                                        (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY - 1) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY - 1)))
                                    {
                                        retVal = new GridPos(currentSnapshot.iX, currentSnapshot.iY);
                                        continue;
                                    }
                                }
                                else
                                {
                                    if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + 1, currentSnapshot.iY + currentSnapshot.tDy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + 1, currentSnapshot.iY)) ||
                                        (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX - 1, currentSnapshot.iY + currentSnapshot.tDy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX - 1, currentSnapshot.iY)))
                                    {
                                        retVal = new GridPos(currentSnapshot.iX, currentSnapshot.iY);
                                        continue;
                                    }
                                }
                            }
                            // when moving diagonally, must check for vertical/horizontal jump points
                            if (currentSnapshot.tDx != 0 && currentSnapshot.tDy != 0)
                            {
                                currentSnapshot.stage = 1;
                                stack.Push(currentSnapshot);

                                newSnapshot = new JumpSnapshot
                                {
                                    iX = currentSnapshot.iX + currentSnapshot.tDx,
                                    iY = currentSnapshot.iY,
                                    iPx = currentSnapshot.iX,
                                    iPy = currentSnapshot.iY,
                                    stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }

                            // moving diagonally, must make sure one of the vertical/horizontal
                            // neighbors is open to allow the path

                            // moving diagonally, must make sure one of the vertical/horizontal
                            // neighbors is open to allow the path
                            if (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) || iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy))
                            {
                                newSnapshot = new JumpSnapshot
                                {
                                    iX = currentSnapshot.iX + currentSnapshot.tDx,
                                    iY = currentSnapshot.iY + currentSnapshot.tDy,
                                    iPx = currentSnapshot.iX,
                                    iPy = currentSnapshot.iY,
                                    stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }
                            else if (iParam.CrossAdjacentPoint)
                            {
                                newSnapshot = new JumpSnapshot
                                {
                                    iX = currentSnapshot.iX + currentSnapshot.tDx,
                                    iY = currentSnapshot.iY + currentSnapshot.tDy,
                                    iPx = currentSnapshot.iX,
                                    iPy = currentSnapshot.iY,
                                    stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }
                        }
                        else //if (!iParam.CrossCorner)
                        {
                            // check for forced neighbors
                            // along the diagonal
                            if (currentSnapshot.tDx != 0 && currentSnapshot.tDy != 0)
                            {
                                if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY + currentSnapshot.tDy) && iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY)) ||
                                    (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY + currentSnapshot.tDy) && iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy)))
                                {
                                    retVal = new GridPos(currentSnapshot.iX, currentSnapshot.iY);
                                    continue;
                                }
                            }
                            // horizontally/vertically
                            else
                            {
                                if (currentSnapshot.tDx != 0)
                                {
                                    // moving along x
                                    if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + 1) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX - currentSnapshot.tDx, currentSnapshot.iY + 1)) ||
                                        (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY - 1) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX - currentSnapshot.tDx, currentSnapshot.iY - 1)))
                                    {
                                        retVal = new GridPos(currentSnapshot.iX, currentSnapshot.iY);
                                        continue;
                                    }
                                }
                                else
                                {
                                    if ((iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + 1, currentSnapshot.iY) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + 1, currentSnapshot.iY - currentSnapshot.tDy)) ||
                                        (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX - 1, currentSnapshot.iY) && !iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX - 1, currentSnapshot.iY - currentSnapshot.tDy)))
                                    {
                                        retVal = new GridPos(currentSnapshot.iX, currentSnapshot.iY);
                                        continue;
                                    }
                                }
                            }

                            // when moving diagonally, must check for vertical/horizontal jump points
                            if (currentSnapshot.tDx != 0 && currentSnapshot.tDy != 0)
                            {
                                currentSnapshot.stage = 3;
                                stack.Push(currentSnapshot);

                                newSnapshot = new JumpSnapshot
                                {
                                    iX = currentSnapshot.iX + currentSnapshot.tDx,
                                    iY = currentSnapshot.iY,
                                    iPx = currentSnapshot.iX,
                                    iPy = currentSnapshot.iY,
                                    stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }

                            // moving diagonally, must make sure both of the vertical/horizontal
                            // neighbors is open to allow the path
                            if (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) && iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy))
                            {
                                newSnapshot = new JumpSnapshot
                                {
                                    iX = currentSnapshot.iX + currentSnapshot.tDx,
                                    iY = currentSnapshot.iY + currentSnapshot.tDy,
                                    iPx = currentSnapshot.iX,
                                    iPy = currentSnapshot.iY,
                                    stage = 0
                                };
                                stack.Push(newSnapshot);
                                continue;
                            }
                        }
                        retVal = null;
                        break;

                    case 1:
                        currentSnapshot.jx = retVal;

                        currentSnapshot.stage = 2;
                        stack.Push(currentSnapshot);

                        newSnapshot = new JumpSnapshot
                        {
                            iX = currentSnapshot.iX,
                            iY = currentSnapshot.iY + currentSnapshot.tDy,
                            iPx = currentSnapshot.iX,
                            iPy = currentSnapshot.iY,
                            stage = 0
                        };
                        stack.Push(newSnapshot);
                        break;

                    case 2:
                        currentSnapshot.jy = retVal;
                        if (currentSnapshot.jx != null || currentSnapshot.jy != null)
                        {
                            retVal = new GridPos(currentSnapshot.iX, currentSnapshot.iY);
                            continue;
                        }

                        // moving diagonally, must make sure one of the vertical/horizontal
                        // neighbors is open to allow the path
                        if (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) || iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy))
                        {
                            newSnapshot = new JumpSnapshot
                            {
                                iX = currentSnapshot.iX + currentSnapshot.tDx,
                                iY = currentSnapshot.iY + currentSnapshot.tDy,
                                iPx = currentSnapshot.iX,
                                iPy = currentSnapshot.iY,
                                stage = 0
                            };
                            stack.Push(newSnapshot);
                            continue;
                        }
                        if (iParam.CrossAdjacentPoint)
                        {
                            newSnapshot = new JumpSnapshot
                            {
                                iX = currentSnapshot.iX + currentSnapshot.tDx,
                                iY = currentSnapshot.iY + currentSnapshot.tDy,
                                iPx = currentSnapshot.iX,
                                iPy = currentSnapshot.iY,
                                stage = 0
                            };
                            stack.Push(newSnapshot);
                            continue;
                        }
                        retVal = null;
                        break;

                    case 3:
                        currentSnapshot.jx = retVal;

                        currentSnapshot.stage = 4;
                        stack.Push(currentSnapshot);

                        newSnapshot = new JumpSnapshot
                        {
                            iX = currentSnapshot.iX,
                            iY = currentSnapshot.iY + currentSnapshot.tDy,
                            iPx = currentSnapshot.iX,
                            iPy = currentSnapshot.iY,
                            stage = 0
                        };
                        stack.Push(newSnapshot);
                        break;

                    case 4:
                        currentSnapshot.jy = retVal;
                        if (currentSnapshot.jx != null || currentSnapshot.jy != null)
                        {
                            retVal = new GridPos(currentSnapshot.iX, currentSnapshot.iY);
                            continue;
                        }

                        // moving diagonally, must make sure both of the vertical/horizontal
                        // neighbors is open to allow the path
                        if (iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX + currentSnapshot.tDx, currentSnapshot.iY) && iParam.SearchGrid.IsWalkableAt(currentSnapshot.iX, currentSnapshot.iY + currentSnapshot.tDy))
                        {
                            newSnapshot = new JumpSnapshot
                            {
                                iX = currentSnapshot.iX + currentSnapshot.tDx,
                                iY = currentSnapshot.iY + currentSnapshot.tDy,
                                iPx = currentSnapshot.iX,
                                iPy = currentSnapshot.iY,
                                stage = 0
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

        private static GridPos jump(JumpPointParam iParam, int iX, int iY, int iPx, int iPy)
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
                        jx = jump(iParam, iX + tDx, iY, iX, iY);
                        jy = jump(iParam, iX, iY + tDy, iX, iY);
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
                    }
                    else if (iParam.CrossAdjacentPoint)
                    {
                        var iX1 = iX;
                        var iY1 = iY;
                        iX = iX + tDx;
                        iY = iY + tDy;
                        iPx = iX1;
                        iPy = iY1;
                    }
                    else
                    {
                        return null;
                    }
                }
                else //if (!iParam.CrossCorner)
                {
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
                        jx = jump(iParam, iX + tDx, iY, iX, iY);
                        jy = jump(iParam, iX, iY + tDy, iX, iY);
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
        }

        private static IEnumerable<GridPos> findNeighbors(JumpPointParam iParam, Node iNode)
        {
            Node tParent = (Node)iNode.parent;
            int tX = iNode.x;
            int tY = iNode.y;
            List<GridPos> tNeighbors = new List<GridPos>();

            // directed pruning: can ignore most neighbors, unless forced.
            if (tParent != null)
            {
                int tPx = tParent.x;
                int tPy = tParent.y;

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
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy) &&
                                iParam.SearchGrid.IsWalkableAt(tX + tDx, tY))
                            {
                                tNeighbors.Add(new GridPos(tX + tDx, tY + tDy));
                            }
                        }

                        if (iParam.SearchGrid.IsWalkableAt(tX - tDx, tY + tDy))
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY + tDy) &&
                                iParam.SearchGrid.IsWalkableAt(tX - tDx, tY))
                            {
                                tNeighbors.Add(new GridPos(tX - tDx, tY + tDy));
                            }
                        }

                        if (iParam.SearchGrid.IsWalkableAt(tX + tDx, tY - tDy))
                        {
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY - tDy) &&
                                iParam.SearchGrid.IsWalkableAt(tX + tDx, tY))
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
                            {
                                tNeighbors.Add(new GridPos(tX + 1, tY));
                            }
                            if (iParam.SearchGrid.IsWalkableAt(tX - 1, tY))
                            {
                                tNeighbors.Add(new GridPos(tX - 1, tY));
                            }
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
                            {
                                tNeighbors.Add(new GridPos(tX, tY + 1));
                            }
                            if (iParam.SearchGrid.IsWalkableAt(tX, tY - 1))
                            {
                                tNeighbors.Add(new GridPos(tX, tY - 1));
                            }
                        }
                    }
                }
            }
            // return all neighbors
            else
            {
                List<Node> tNeighborNodes = iParam.SearchGrid.GetNeighbors(iNode, iParam.CrossCorner, iParam.CrossAdjacentPoint);
                foreach (Node tNeighborNode in tNeighborNodes)
                {
                    tNeighbors.Add(new GridPos(tNeighborNode.x, tNeighborNode.y));
                }
            }

            return tNeighbors;
        }
    }
}