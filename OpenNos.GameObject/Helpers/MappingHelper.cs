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

using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public static class MappingHelper
    {
        #region Instantiation

        static MappingHelper()
        {
            // intialize hardcode in waiting for better solution
            GuriItemEffects = new Dictionary<int, int>
            {
                {859, 1343},
                {860, 1344},
                {861, 1344},
                {875, 1558},
                {876, 1559},
                {877, 1560},
                {878, 1560},
                {879, 1561},
                {880, 1561}
            };

            // effect items aka. fireworks
        }

        #endregion

        #region Properties

        public static Dictionary<int, int> GuriItemEffects { get; }

        #endregion
    }
}