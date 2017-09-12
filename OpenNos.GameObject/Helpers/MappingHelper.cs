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
    public class MappingHelper
    {
        #region Properties

        public Dictionary<int, int> GuriItemEffects = new Dictionary<int, int>
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

        #endregion

        #region Singleton

        private static MappingHelper _instance;

        public static MappingHelper Instance
        {
            get { return _instance ?? (_instance = new MappingHelper()); }
        }

        #endregion
    }
}