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
            switch (Effect)
            {
                case 0:
                    switch (VNum)
                    {
                        case 1218:
                            Session.SendPacket(Session.Character.GenerateGuri(12, 1, 26));
                            break;

                        case 1363:
                            Session.SendPacket(Session.Character.GenerateGuri(12, 1, 27));
                            break;

                        case 1364:
                            Session.SendPacket(Session.Character.GenerateGuri(12, 1, 28));
                            break;

                        case 5107:
                            Session.SendPacket(Session.Character.GenerateGuri(12, 1, 47));
                            break;

                        case 5207:
                            Session.SendPacket(Session.Character.GenerateGuri(12, 1, 50));
                            break;

                        case 5519:
                            Session.SendPacket(Session.Character.GenerateGuri(12, 1, 60));
                            break;

                        case 5369:
                            Session.SendPacket(Session.Character.GenerateGuri(12, 1, 61));
                            break;
                    }
                    break;

                default:
                    Logger.Log.Warn(String.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), this.GetType().ToString()));
                    break;
            }
        }

        #endregion
    }
}