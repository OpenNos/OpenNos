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
using OpenNos.DAL.Interface;
using System.Collections.Generic;
using System;
using OpenNos.DAL.EF.Helpers;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class FamilyLogDAO : MappingBaseDAO<FamilyLog, FamilyLogDTO>, IFamilyLogDAO
    {
        public IEnumerable<FamilyLogDTO> LoadByFamilyId(long familyId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (FamilyLog familylog in context.FamilyLog.Where(fc => fc.FamilyId.Equals(familyId)))
                {
                    yield return _mapper.Map<FamilyLogDTO>(familylog);
                }
            }
        }
    }
}