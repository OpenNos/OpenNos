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

using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class CharacterSkillDAO : SynchronizableBaseDAO<CharacterSkillDTO>, ICharacterSkillDAO
    {
        #region Methods

        public DeleteResult Delete(long characterId, short skillVNum)
        {
            CharacterSkillDTO characterSkill = Container.SingleOrDefault(c => c.CharacterId == characterId && c.SkillVNum == skillVNum);
            Container.Remove(characterSkill);
            return DeleteResult.Deleted;
        }

        public IEnumerable<CharacterSkillDTO> LoadByCharacterId(long characterId)
        {
            return Container.Where(c => c.CharacterId == characterId).Select(e => MapEntity(e));
        }

        public IEnumerable<Guid> LoadKeysByCharacterId(long characterId)
        {
            return Container.Where(c => c.CharacterId == characterId).Select(c => c.Id);
        }

        #endregion
    }
}