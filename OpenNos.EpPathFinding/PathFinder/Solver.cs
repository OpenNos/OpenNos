using EpPathFinding.PathFinder;
using System;

namespace OpenNos.Pathfinding
{
    public class Solver<TPathNode, TUserContext> : SettlersEngine.SpatialAStar<TPathNode, TUserContext> where TPathNode : SettlersEngine.IPathNode<TUserContext>
    {
    
        protected override Double NeighborDistance(PathNode inStart, PathNode inEnd)
        {
            int iDx = Math.Abs(inStart.X - inEnd.X);
            int iDy = Math.Abs(inStart.Y - inEnd.Y);
            return HeuristicDistance.Octil(iDx,iDy);
        }

        public Solver(TPathNode[,] inGrid)
            : base(inGrid)
        {
        }
    }
}
