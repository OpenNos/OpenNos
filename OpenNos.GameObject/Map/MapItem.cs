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
using System;

namespace OpenNos.GameObject
{
    public class MapItem
    {
        #region Instantiation

        public MapItem(short x, short y)
        {
            PositionX = x;
            PositionY = y;
            CreateDate = DateTime.Now;
        }

        #endregion

        #region Properties

        public DateTime CreateDate { get; set; }

        public ItemInstance ItemInstance { get; set; }

        public long? Owner { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        #endregion

        #region Methods

        public string GenerateOut(long id)
        {
            return $"out 9 {id}";
        }

        public void Rarify(ClientSession session)
        {
            if (ItemInstance is WearableInstance)
            {
                ((WearableInstance)ItemInstance).RarifyItem(session, RarifyMode.Drop, RarifyProtection.None);
            }
        }

        #endregion
    }
}