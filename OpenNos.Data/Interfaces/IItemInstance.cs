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
    public interface IItemInstance
    {
        #region Public Properties

        int Amount { get; set; }
        byte Design { get; set; }
        bool IsUsed { get; set; }
        DateTime? ItemDeleteTime { get; set; }
        long ItemInstanceId { get; set; }
        short ItemVNum { get; set; }
        byte Rare { get; set; }
        byte Upgrade { get; set; }

        #endregion
    }
}