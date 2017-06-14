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

using OpenNos.Core;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class RollGeneratedItemDAO : MappingBaseDAO<RollGeneratedItem, RollGeneratedItemDTO>, IRollGeneratedItemDAO
    {
        #region Methods
        public RollGeneratedItemDTO Insert(RollGeneratedItemDTO item)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    RollGeneratedItem entity = _mapper.Map<RollGeneratedItem>(item);
                    context.RollGeneratedItem.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<RollGeneratedItemDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<RollGeneratedItemDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (RollGeneratedItem item in context.RollGeneratedItem)
                {
                    yield return _mapper.Map<RollGeneratedItemDTO>(item);
                }
            }
        }

        public RollGeneratedItemDTO LoadById(short id)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<RollGeneratedItemDTO>(context.RollGeneratedItem.FirstOrDefault(i => i.RollGeneratedItemId.Equals(id)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<RollGeneratedItemDTO> LoadByItemVNum(short vnum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (RollGeneratedItem item in context.RollGeneratedItem.Where(s => s.OriginalItemVNum == vnum))
                {
                    yield return _mapper.Map<RollGeneratedItemDTO>(item);
                }
            }
        }


        #endregion
    }
}