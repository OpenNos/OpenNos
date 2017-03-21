using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpPathFinding.PathFinder
{
   public class HeuristicDistance
    {
        private static readonly Double SQRT_2 = Math.Sqrt(2);

        public static float Chebyshev(int iDx, int iDy)
        {
            return Math.Max(iDx, iDy);
        }

        public static float Euclidean(int iDx, int iDy)
        {
            float tFdx = iDx;
            float tFdy = iDy;
            return (float)Math.Sqrt(tFdx * tFdx + tFdy * tFdy);
        }

        public static float Manhattan(int iDx, int iDy)
        {
            return (float)iDx + iDy;
        }

        public static float Octil(int iDx, int iDy)
        {
            return (float)(Math.Min(iDx, iDy) * Math.Sqrt(2) + Math.Max(iDx, iDy) - Math.Min(iDx, iDy));

        }
    }
}
