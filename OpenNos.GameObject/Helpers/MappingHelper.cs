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
        #region Members

        private static Dictionary<int, int> _guriItemEffects;

        #endregion

        #region Instantiation

        static MappingHelper()
        {
            // intialize
            // hardcode in waiting for better solution
            _guriItemEffects = new Dictionary<int, int>();

            // effect items aka. fireworks
            _guriItemEffects.Add(859, 1343);
            _guriItemEffects.Add(860, 1344);
            _guriItemEffects.Add(861, 1344);
            _guriItemEffects.Add(875, 1558);
            _guriItemEffects.Add(876, 1559);
            _guriItemEffects.Add(877, 1560);
            _guriItemEffects.Add(878, 1560);
            _guriItemEffects.Add(879, 1561);
            _guriItemEffects.Add(880, 1561);
        }

        #endregion

        #region Properties

        public static Dictionary<int, int> GuriItemEffects
        {
            get
            {
                return _guriItemEffects;
            }
        }

        #endregion
    }
}