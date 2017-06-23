using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OpenNos.PathFinder
{
    public class BestFirstSearch
    {
        #region Methods

        public static List<Node> FindPath(GridPos start, GridPos end, GridPos[,] Grid)
        {
            Node node = new Node();
            Node[,] grid = new Node[Grid.GetLength(0), Grid.GetLength(1)];
            if (grid[start.X, start.Y] == null)
            {
                grid[start.X, start.Y] = new Node(Grid[start.X, start.Y]);
            }
            Node Start = grid[start.X, start.Y];
            MinHeap path = new MinHeap();

            // push the start node into the open list
            path.Push(Start);
            Start.Opened = true;

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                node = path.Pop();
                if (grid[node.X, node.Y] == null)
                {
                    grid[node.X, node.Y] = new Node(Grid[node.X, node.Y]);
                }
                grid[node.X, node.Y].Closed = true;

                //if reached the end position, construct the path and return it
                if (node.X == end.X && node.Y == end.Y)
                {
                    return Backtrace(node);
                }

                // get neigbours of the current node
                List<Node> neighbors = GetNeighbors(grid, node,Grid);

                for (int i = 0, l = neighbors.Count(); i < l; ++i)
                {
                    Node neighbor = neighbors[i];

                    if (neighbor.Closed)
                    {
                        continue;
                    }

                    // check if the neighbor has not been inspected yet, or can be reached with
                    // smaller cost from the current node
                    if (!neighbor.Opened)
                    {
                        if (neighbor.F == 0)
                        {
                            neighbor.F = Heuristic.Octile(Math.Abs(neighbor.X - end.X), Math.Abs(neighbor.Y - end.Y));
                        }

                        neighbor.Parent = node;

                        if (!neighbor.Opened)
                        {
                            path.Push(neighbor);
                            neighbor.Opened = true;
                        }
                        else
                        {
                            neighbor.Parent = node;
                        }
                    }
                }
            }
            return new List<Node>();
        }

        public static Node[,] LoadBrushFire(GridPos user, GridPos[,] mapGrid, short MaxDistance = 22)
        {
            Node[,] grid = new Node[mapGrid.GetLength(0), mapGrid.GetLength(1)]; 

            Node node = new Node();
            if (grid[user.X, user.Y] == null)
            {
                grid[user.X, user.Y] = new Node(mapGrid[user.X, user.Y]);
            }
            Node Start = grid[user.X, user.Y];
            MinHeap path = new MinHeap();

            // push the start node into the open list
            path.Push(Start);
            Start.Opened = true;

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                node = path.Pop();
                if (grid[node.X, node.Y] == null)
                {
                    grid[node.X, node.Y] = new Node(mapGrid[node.X, node.Y]);
                }

                grid[node.X, node.Y].Closed = true;

                // get neigbours of the current node
                List<Node> neighbors = GetNeighbors(grid, node, mapGrid);

                for (int i = 0, l = neighbors.Count(); i < l; ++i)
                {
                    Node neighbor = neighbors[i];

                    if (neighbor.Closed)
                    {
                        continue;
                    }

                    // check if the neighbor has not been inspected yet, or can be reached with
                    // smaller cost from the current node
                    if (!neighbor.Opened)
                    {
                        if (neighbor.F == 0)
                        {
                            double distance = Heuristic.Octile(Math.Abs(neighbor.X - node.X), Math.Abs(neighbor.Y - node.Y)) + node.F;
                            if (distance > MaxDistance)
                            {
                                neighbor.Value = 1;
                                continue;
                            }
                            else
                            {
                                neighbor.F = distance;
                            }
                            grid[neighbor.X, neighbor.Y].F = neighbor.F;
                        }

                        neighbor.Parent = node;

                        if (!neighbor.Opened)
                        {
                            path.Push(neighbor);
                            neighbor.Opened = true;
                        }
                        else
                        {
                            neighbor.Parent = node;
                        }
                    }
                }
            }
            return grid;
        }

        public static List<Node> GetNeighbors(Node[,] Grid, Node node, GridPos[,] MapGrid)
        {
            short x = node.X,
                y = node.Y;
            List<Node> neighbors = new List<Node>();
            bool s0 = false, d0 = false,
             s1 = false, d1 = false,
             s2 = false, d2 = false,
             s3 = false, d3 = false;
            int IndexX;
            int IndexY;


            // ↑
            IndexX = x;
            IndexY = y - 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && MapGrid[IndexX, IndexY].IsWalkable())
            {
                if (Grid[IndexX, IndexY] == null)
                {
                    Grid[IndexX, IndexY] = new Node(MapGrid[IndexX, IndexY]);
                }
                neighbors.Add(Grid[IndexX, IndexY]);
                s0 = true;
            }

            // →
            IndexX = x + 1;
            IndexY = y;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && MapGrid[IndexX, IndexY].IsWalkable())
            {
                if (Grid[IndexX, IndexY] == null)
                {
                    Grid[IndexX, IndexY] = new Node(MapGrid[IndexX, IndexY]);
                }
                neighbors.Add(Grid[IndexX, IndexY]);
                s1 = true;
            }


            // ↓
            IndexX = x;
            IndexY = y + 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && MapGrid[IndexX, IndexY].IsWalkable())
            {
                if (Grid[IndexX, IndexY] == null)
                {
                    Grid[IndexX, IndexY] = new Node(MapGrid[IndexX, IndexY]);
                }
                neighbors.Add(Grid[IndexX, IndexY]);
                s2 = true;
            }

            // ←
            IndexX = x - 1;
            IndexY = y;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && MapGrid[IndexX, IndexY].IsWalkable())
            {
                if (Grid[IndexX, IndexY] == null)
                {
                    Grid[IndexX, IndexY] = new Node(MapGrid[IndexX, IndexY]);
                }
                neighbors.Add(Grid[IndexX, IndexY]);
                s3 = true;
            }

            d0 = s3 || s0;
            d1 = s0 || s1;
            d2 = s1 || s2;
            d3 = s2 || s3;

            // ↖
            IndexX = x - 1;
            IndexY = y - 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && d0 && MapGrid[IndexX, IndexY].IsWalkable() == true)
            {
                if (Grid[IndexX, IndexY] == null)
                {
                    Grid[IndexX, IndexY] = new Node(MapGrid[IndexX, IndexY]);
                }
                neighbors.Add(Grid[IndexX, IndexY]);
            }

            // ↗
            IndexX = x + 1;
            IndexY = y - 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && d1 && MapGrid[IndexX, IndexY].IsWalkable() == true)
            {
                if (Grid[IndexX, IndexY] == null)
                {
                    Grid[IndexX, IndexY] = new Node(MapGrid[IndexX, IndexY]);
                }
                neighbors.Add(Grid[IndexX, IndexY]);
            }

            // ↘
            IndexX = x + 1;
            IndexY = y + 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && d2 && MapGrid[IndexX, IndexY].IsWalkable() == true)
            {
                if (Grid[IndexX, IndexY] == null)
                {
                    Grid[IndexX, IndexY] = new Node(MapGrid[IndexX, IndexY]);
                }
                neighbors.Add(Grid[IndexX, IndexY]);
            }

            // ↙
            IndexX = x - 1;
            IndexY = y + 1;
            if (Grid.GetLength(0) > IndexX && Grid.GetLength(1) > IndexY && IndexX >= 0 && IndexY >= 0 && d3 && MapGrid[IndexX, IndexY].IsWalkable() == true)
            {
                if (Grid[IndexX, IndexY] == null)
                {
                    Grid[IndexX, IndexY] = new Node(MapGrid[IndexX, IndexY]);
                }
                neighbors.Add(Grid[IndexX, IndexY]);
            }

            return neighbors;
        }

        public static List<Node> Backtrace(Node end)
        {
            List<Node> path = new List<Node>();
            while (end.Parent != null)
            {
                end = end.Parent;
                path.Add(end);
            }
            path.Reverse();
            return path;
        }

        public static List<Node> TracePath(Node node, Node[,] Grid, GridPos[,] MapGrid)
        {
            List<Node> list = new List<Node>();
            if (MapGrid == null || Grid == null || node.X >= Grid.GetLength(0) || node.Y >= Grid.GetLength(1) || Grid[node.X, node.Y] == null)
            {
                node.F = 100;
                list.Add(node);
                return list;
            }          
            Node currentnode = Grid[node.X, node.Y];
            while (currentnode.F != 1 && currentnode.F != 0)
            {
                Node newnode = null;
                newnode = BestFirstSearch.GetNeighbors(Grid, currentnode, MapGrid)?.OrderBy(s => s.F).FirstOrDefault();
                if (newnode != null)
                {
                    list.Add(newnode);
                    currentnode = newnode;
                }
            }
            return list;
        }
        #endregion
    }
}