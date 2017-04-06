/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using System;

namespace OpenNos.PathFinder
{
    public class Heuristic
    {
        #region Members

        public static readonly Double SQRT_2 = Math.Sqrt(2);

        #endregion

        #region Methods

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

        #endregion
    }
}