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

using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class MapItem
    {
        #region Public Instantiation

        public MapItem(short x, short y, bool isNew)
        {
            PositionX = x;
            PositionY = y;
            IsNew = isNew;
        }

        #endregion

        #region Public Properties

        public bool IsNew { get; set; }
        public ItemInstance ItemInstance { get; set; }
        public short PositionX { get; set; }
        public short PositionY { get; set; }

        #endregion

        #region Public Methods

        public void Rarify(ClientSession session)
        {
            if (IsNew && ItemInstance is WearableInstance)
            {
                ((WearableInstance)ItemInstance).RarifyItem(session, RarifyMode.Drop, RarifyProtection.None);
            }
        }

        #endregion
    }
}