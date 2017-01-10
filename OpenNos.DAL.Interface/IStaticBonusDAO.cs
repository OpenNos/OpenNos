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

namespace OpenNos.DAL.Interface
{
    public interface IStaticBonusDAO : IMappingBaseDAO
    {
        #region Methods

        /// <summary>
        /// Deletes already existing object from database
        /// </summary>
        /// <param name="staticBonusId"></param>
        /// <returns></returns>
        DeleteResult Delete(long staticBonusId);

        /// <summary>
        /// Inserts new object to database context
        /// </summary>
        /// <param name="staticBonus"></param>
        /// <returns></returns>
        StaticBonusDTO Insert(StaticBonusDTO staticBonus);

        /// <summary>
        /// Loads staticbonus by characterid
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        StaticBonusDTO LoadByCharacterId(long characterId);

        #endregion
    }
}