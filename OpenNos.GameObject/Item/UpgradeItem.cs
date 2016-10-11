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
    public class UpgradeItem : Item
    {
        #region Methods

        public override void Use(ClientSession Session, ref Inventory Inv, bool DelayUsed = false)
        {
            if (MappingHelper.GuriItemEffects.ContainsKey(VNum))
            {
                Session.SendPacket(Session.Character.GenerateGuri(12, 1, MappingHelper.GuriItemEffects[VNum]));
            }
            else
            {
                Logger.Log.Warn(String.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), this.GetType().ToString()));
            }
        }
    }

    #endregion
}