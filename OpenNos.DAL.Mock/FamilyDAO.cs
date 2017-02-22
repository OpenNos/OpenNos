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

namespace OpenNos.DAL.Mock
{
    public class FamilyDAO : BaseDAO<FamilyDTO>, IFamilyDAO
    {
        #region Methods

        public DeleteResult Delete(long familyId)
        {
            return DeleteResult.Deleted;
        }

        public SaveResult InsertOrUpdate(ref FamilyDTO family)
        {
            return SaveResult.Inserted;
        }

        public FamilyDTO LoadByCharacterId(long characterId)
        {
            return null;
        }

        public FamilyDTO LoadById(long familyId)
        {
            return null;
        }

        public FamilyDTO LoadByName(string name)
        {
            return null;
        }

        #endregion
    }
}