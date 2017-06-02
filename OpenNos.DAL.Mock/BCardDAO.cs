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
    public class BCardDAO : BaseDAO<BCardDTO>, IBCardDAO
    {
        #region Methods

        public void Insert(List<BCardDTO> card)
        {
            throw new NotImplementedException();
        }

        public BCardDTO Insert(ref BCardDTO cardObject)
        {
            throw new NotImplementedException();
        }

        public BCardDTO LoadById(short cardId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BCardDTO> LoadByCardId(short cardId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BCardDTO> LoadByItemVNum(short vNum)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BCardDTO> LoadByNpcMonsterVNum(short vNum)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BCardDTO> LoadBySkillVNum(short vNum)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}