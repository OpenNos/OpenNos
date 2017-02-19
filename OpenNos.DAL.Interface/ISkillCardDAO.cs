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
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface ISkillCardDAO : IMappingBaseDAO
    {
        #region Methods

        void Insert(List<SkillCardDTO> skillCards);

        IEnumerable<SkillCardDTO> LoadAll();

        SkillCardDTO LoadByCardIdAndSkillVNum(short cardId, short skillVNum);

        IEnumerable<SkillCardDTO> LoadByCardId(short cardId);

        IEnumerable<SkillCardDTO> LoadBySkillVNum(short skillVNum);

        #endregion
    }
}