using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNos.PathFinder
{
   public class Heuristic
    {
       public static readonly Double SQRT_2 = Math.Sqrt(2);

        public static double Chebyshev(int iDx, int iDy)
        {
            return Math.Max(iDx, iDy);
        }

        public static double Euclidean(int iDx, int iDy)
        {
            float tFdx = iDx;
            float tFdy = iDy;
            return Math.Sqrt(tFdx * tFdx + tFdy * tFdy);
        }

        public static double Manhattan(int iDx, int iDy)
        {
            return iDx + iDy;
        }

        public static double Octile(int iDx, int iDy)
        {
            int min = Math.Min(iDx, iDy);
            int max = Math.Max(iDx, iDy);
            return min * SQRT_2 + max - min;
        }
    }
}
