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

        public static float Octile(int iDx, int iDy)
        {
            int min = Math.Min(iDx, iDy);
            int max = Math.Max(iDx, iDy);
            return (float)(min * SQRT_2 + max - min);
        }
    }
}
