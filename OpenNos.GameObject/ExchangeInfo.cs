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
    public class ExchangeInfo
    {
        #region Instantiation

        public ExchangeInfo()
        {
            Confirm = false;
            Gold = 0;
            TargetCharacterId = -1;
            ExchangeList = new List<ItemInstance>();
            Validate = false;
        }

        #endregion

        #region Properties

        public bool Confirm { get; set; }

        public List<ItemInstance> ExchangeList { get; set; }

        public long Gold { get; set; }

        public long TargetCharacterId { get; set; }

        public bool Validate { get; set; }

        #endregion
    }
}