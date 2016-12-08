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

using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IItemInstanceDAO : ISynchronizableBaseDAO<ItemInstanceDTO>
    {
        #region Methods

        DeleteResult DeleteFromSlotAndType(long characterId, short slot, InventoryType type);

        void InitializeMapper(Type baseType);

        IEnumerable<ItemInstanceDTO> LoadByCharacterId(long characterId);

        ItemInstanceDTO LoadBySlotAndType(long characterId, short slot, InventoryType type);

        IEnumerable<ItemInstanceDTO> LoadByType(long characterId, InventoryType type);

        IList<Guid> LoadSlotAndTypeByCharacterId(long characterId);

        #endregion
    }
}