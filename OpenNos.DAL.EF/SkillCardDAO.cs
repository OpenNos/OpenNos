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
    public class SkillCardDAO : MappingBaseDAO<SkillCard, SkillCardDTO>, ISkillCardDAO
    {
        #region Methods

        public void Insert(List<SkillCardDTO> skillCards)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (SkillCardDTO skillCard in skillCards)
                    {
                        SkillCard entity = _mapper.Map<SkillCard>(skillCard);
                        context.SkillCard.Add(entity);
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

        public IEnumerable<SkillCardDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (SkillCard skillCard in context.SkillCard)
                {
                    yield return _mapper.Map<SkillCardDTO>(skillCard);
                }
            }
        }

        public IEnumerable<SkillCardDTO> LoadByCardId(short cardId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (SkillCard skillCard in context.SkillCard.Where(c => c.CardId.Equals(cardId)))
                {
                    yield return _mapper.Map<SkillCardDTO>(skillCard);
                }
            }
        }

        public SkillCardDTO LoadByCardIdAndSkillVNum(short cardId, short skillVNum)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<SkillCardDTO>(context.SkillCard.FirstOrDefault(i => i.CardId.Equals(cardId) && i.SkillVNum.Equals(skillVNum)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<SkillCardDTO> LoadBySkillVNum(short skillVNum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (SkillCard skillCard in context.SkillCard.Where(c => c.SkillVNum.Equals(skillVNum)))
                {
                    yield return _mapper.Map<SkillCardDTO>(skillCard);
                }
            }
        }

        #endregion
    }
}