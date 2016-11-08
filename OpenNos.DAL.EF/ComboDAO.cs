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
    public class ComboDAO : MappingBaseDAO<Combo, ComboDTO>, IComboDAO
    {
        #region Methods

        public void Insert(List<ComboDTO> combos)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ComboDTO combo in combos)
                    {
                        Combo entity = _mapper.Map<Combo>(combo);
                        context.Combo.Add(entity);
                    }
                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public ComboDTO Insert(ComboDTO combo)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Combo entity = _mapper.Map<Combo>(combo);
                    context.Combo.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<ComboDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ComboDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Combo combo in context.Combo)
                {
                    yield return _mapper.Map<ComboDTO>(combo);
                }
            }
        }

        public ComboDTO LoadById(short comboId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<ComboDTO>(context.Combo.FirstOrDefault(s => s.SkillVNum.Equals(comboId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ComboDTO> LoadBySkillVnum(short skillVNum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Combo combo in context.Combo.Where(c => c.SkillVNum == skillVNum))
                {
                    yield return _mapper.Map<ComboDTO>(combo);
                }
            }
        }

        public IEnumerable<ComboDTO> LoadByVNumHitAndEffect(short skillVNum, short hit, short effect)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Combo combo in context.Combo.Where(s => s.SkillVNum == skillVNum && s.Hit == hit && s.Effect == effect))
                {
                    yield return _mapper.Map<ComboDTO>(combo);
                }
            }
        }

        #endregion
    }
}