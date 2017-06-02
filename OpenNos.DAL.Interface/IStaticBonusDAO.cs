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
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IStaticBuffDAO : IMappingBaseDAO
    {
        #region Methods

        /// <summary>
        /// Inserts new object to database context
        /// </summary>
        /// <param name="staticBonus"></param>
        /// <returns></returns>
        SaveResult InsertOrUpdate(ref StaticBuffDTO staticBuff);

        /// <summary>
        /// Loads staticBonus by characterid
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        IEnumerable<StaticBuffDTO> LoadByCharacterId(long characterId);

        IEnumerable<short> LoadByTypeCharacterId(long characterId);

        void Delete(short bonusToDelete, long characterId);

        #endregion
    }
}