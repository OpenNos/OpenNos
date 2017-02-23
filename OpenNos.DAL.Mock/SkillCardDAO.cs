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
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class SkillCardDAO : BaseDAO<SkillCardDTO>, ISkillCardDAO
    {
        #region Methods

        public void Insert(List<SkillCardDTO> skillCards)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SkillCardDTO> LoadByCardId(short cardId)
        {
            throw new NotImplementedException();
        }

        public SkillCardDTO LoadByCardIdAndSkillVNum(short cardId, short skillVNum)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SkillCardDTO> LoadBySkillVNum(short skillVNum)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}