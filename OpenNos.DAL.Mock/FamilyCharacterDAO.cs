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
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class FamilyCharacterDAO : BaseDAO<FamilyCharacterDTO>, IFamilyCharacterDAO
    {
        #region Methods

        public DeleteResult Delete(string characterName)
        {
            return DeleteResult.Deleted;
        }

        public SaveResult InsertOrUpdate(ref FamilyCharacterDTO character)
        {
            return SaveResult.Inserted;
        }

        public FamilyCharacterDTO LoadByCharacterId(long characterId)
        {
            return null;
        }

        public IList<FamilyCharacterDTO> LoadByFamilyId(long familyId)
        {
            return null;
        }

        public FamilyCharacterDTO LoadById(long familyCharacterId)
        {
            return null;
        }

        #endregion
    }
}