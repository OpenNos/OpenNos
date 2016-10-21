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

namespace OpenNos.DAL.EF
{
    public class Drop
    {
        #region Properties

        public int Amount { get; set; }

        public int DropChance { get; set; }

        public short DropId { get; set; }

        public virtual Item Item { get; set; }

        public short ItemVNum { get; set; }

        public virtual MapType MapType { get; set; }

        public short? MapTypeId { get; set; }

        public short? MonsterVNum { get; set; }

        public virtual NpcMonster NpcMonster { get; set; }

        #endregion
    }
}