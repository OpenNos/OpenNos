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

using System;

namespace OpenNos.Data
{
    public class ItemInstanceDTO
    {
        #region Properties

        public bool IsUsed { get; set; }
        public int Amount { get; set; }
        public DateTime? ItemDeleteTime { get; set; }
        public long ItemInstanceId { get; set; }
        public short ItemVNum { get; set; }

        public byte Rare { get; set; }
        public byte Upgrade { get; set; }
        public byte Design { get; set; }

        #endregion
    }
}