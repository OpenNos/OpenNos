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
    public interface INpcMonsterDAO : IMappingBaseDAO
    {
        #region Methods

        /// <summary>
        /// Used for searching monster by what it contains in name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEnumerable<NpcMonsterDTO> FindByName(string name);

        /// <summary>
        /// Used for inserting single object into entity
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        NpcMonsterDTO Insert(NpcMonsterDTO npc);

        /// <summary>
        /// Used for inserting list of data to entity
        /// </summary>
        /// <param name="npc"></param>
        void Insert(List<NpcMonsterDTO> npc);

        /// <summary>
        /// Inser or Update data in entity
        /// </summary>
        /// <param name="npcMonster"></param>
        /// <returns></returns>
        SaveResult InsertOrUpdate(ref NpcMonsterDTO npcMonster);

        /// <summary>
        /// Used for loading all monsters from entity
        /// </summary>
        /// <returns></returns>
        IEnumerable<NpcMonsterDTO> LoadAll();

        /// <summary>
        /// Used for loading monsters with specified VNum
        /// </summary>
        /// <param name="npcMonsterVNum"></param>
        /// <returns></returns>
        NpcMonsterDTO LoadByVNum(short npcMonsterVNum);

        #endregion
    }
}