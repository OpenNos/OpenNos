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

using OpenNos.Core;
using System;

namespace OpenNos.GameObject
{
    public class BoxItemHandler
    {
        #region Methods

        internal void UseItemHandler(ref Inventory inv, ClientSession session, short effect, int effectValue)
        {
            switch (effect)
            {
                default:
                    Logger.Log.Warn(Language.Instance.GetMessageFromKey(String.Format("NO_HANDLER_ITEM", this.GetType().ToString())));
                    break;
            }
        }

        #endregion
    }
}