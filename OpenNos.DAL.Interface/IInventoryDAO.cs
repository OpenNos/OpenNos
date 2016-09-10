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
using OpenNos.Domain;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IInventoryDAO : ISynchronizableBaseDAO<InventoryDTO>
    {
        #region Methods

        void InitializeMapper(Type baseType);

        IEnumerable<InventoryDTO> LoadByCharacterId(long characterId);

        InventoryDTO LoadBySlotAndType(long characterId, short slot, InventoryType type);

        IEnumerable<InventoryDTO> LoadByType(long characterId, InventoryType type);

        IEnumerable<Guid> LoadKeysByCharacterId(long characterId);

        void RegisterMapping(Type gameObjectType);

        #endregion
    }
}