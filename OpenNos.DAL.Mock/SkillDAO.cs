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

using System.Collections.Generic;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class SkillDAO : BaseDAO<SkillDTO>, ISkillDAO
    {
        #region Methods

        public SkillDTO LoadById(short skillId)
        {
            return Container.SingleOrDefault(s => s.SkillVNum == skillId);
        }

        public void Insert(List<SkillDTO> skills)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}