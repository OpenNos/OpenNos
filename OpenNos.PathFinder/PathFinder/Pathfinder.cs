using OpenNos.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNos.PathFinder
{
    public class BestFirstSearch
    {
        public static List<GridPos> GetNeighbors(GridPos[,] Grid, Node node)
        {
            short x = node.X,
                y = node.Y;
            List<GridPos> neighbors = new List<GridPos>();
            bool s0 = false, d0 = false,
             s1 = false, d1 = false,
             s2 = false, d2 = false,
             s3 = false, d3 = false;

            // ↑
            if (Grid[x, y - 1].IsWalkable())
            {
                neighbors.Add(Grid[x, y - 1]);
                s0 = true;
            }
            // →
            if (Grid[x + 1, y].IsWalkable())
            {
                neighbors.Add(Grid[x + 1, y]);
                s1 = true;
            }
            // ↓
            if (Grid[x, y + 1].IsWalkable())
            {
                neighbors.Add(Grid[x, y + 1]);
                s2 = true;
            }
            // ←
            if (Grid[x - 1, y].IsWalkable())
            {
                neighbors.Add(Grid[x - 1, y]);
                s3 = true;
            }


            d0 = s3 || s0;
            d1 = s0 || s1;
            d2 = s1 || s2;
            d3 = s2 || s3;


            // ↖
            if (d0 && Grid[x - 1, y - 1].IsWalkable())
            {
                neighbors.Add(Grid[x - 1, y - 1]);
            }
            // ↗
            if (d1 && Grid[x + 1, y - 1].IsWalkable())
            {
                neighbors.Add(Grid[x + 1, y - 1]);
            }
            // ↘
            if (d2 && Grid[x + 1, y + 1].IsWalkable())
            {
                neighbors.Add(Grid[x + 1, y + 1]);
            }
            // ↙
            if (d3 && Grid[x - 1, y + 1].IsWalkable())
            {
                neighbors.Add(Grid[x - 1, y + 1]);
            }

            return neighbors;
        }
        public static List<GridPos> findPath(GridPos start, GridPos end, GridPos[,] Grid)
        {
            Node node = new Node();
            Node Start = new Node(Grid[start.X, start.Y]);
            MinHeap path = new MinHeap();
            short X,Y;
            path.Push(Start);
            Start.Opened = true;
            
            while (path.Count > 0)
            {
                node = path.Pop();
                node.Closed = true;
                
                if (node.X == end.X && node.Y == end.Y)
                {
                    return Backtrace(node);
                }
                
                List<GridPos> neighbors = GetNeighbors(Grid, node);

                for (int i = 0, l = neighbors.Count(); i < l; ++i)
                {
                    Node neighbor = new Node(neighbors[i]);

                    if (neighbor.Closed)
                    {
                        continue;
                    }
                    X = neighbor.X;
                    Y = neighbor.Y;
                    double ng = node.G + ((X - node.X == 0 || Y - node.Y == 0) ? 1 : Heuristic.SQRT_2);
                    if (!neighbor.Opened || ng < neighbor.G)
                    {
                        neighbor.G = ng;
                        if (neighbor.H == 0)
                        {
                            neighbor.H = Heuristic.Octile(Math.Abs(X - end.X), Math.Abs(Y - end.Y));
                        }
                        if (neighbor.N == 0)
                        {
                            neighbor.N = GetNeighbors(Grid, neighbor).Count() / 9d;
                        }
                        neighbor.F = (neighbor.G * neighbor.N) + neighbor.H;
                        neighbor.Parent = node;

                        if (!neighbor.Opened)
                        {
                            path.Push(neighbor);
                            neighbor.Opened = true;
                        }                       
                    }
                }
            }
            return new List<GridPos>();
        }

        private static List<GridPos> Backtrace(Node end)
        {
            List<GridPos> path = new List<GridPos>();
            while (end.Parent != null)
            {
                end = end.Parent;
                path.Add(end);
            }
            path.Reverse();
            return path;
        }
    }
}
