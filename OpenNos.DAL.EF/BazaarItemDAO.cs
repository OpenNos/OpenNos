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
using OpenNos.DAL.EF.DB;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class BazaarItemDAO : MappingBaseDAO<BazaarItem, BazaarItemDTO>, IBazaarItemDAO
    {
        #region Methods

        public DeleteResult Delete(long bazaarItemId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    BazaarItem BazaarItem = context.BazaarItem.FirstOrDefault(c => c.BazaarItemId.Equals(bazaarItemId));

                    if (BazaarItem != null)
                    {
                        context.BazaarItem.Remove(BazaarItem);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_ERROR"), bazaarItemId, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref BazaarItemDTO bazaarItem)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long bazaarItemId = bazaarItem.BazaarItemId;
                    BazaarItem entity = context.BazaarItem.FirstOrDefault(c => c.BazaarItemId.Equals(bazaarItemId));

                    if (entity == null)
                    {
                        bazaarItem = Insert(bazaarItem, context);
                        return SaveResult.Inserted;
                    }

                    bazaarItem = Update(entity, bazaarItem, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), bazaarItem.BazaarItemId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<BazaarItemDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (BazaarItem bazaarItem in context.BazaarItem)
                {
                    yield return _mapper.Map<BazaarItemDTO>(bazaarItem);
                }
            }
        }

        public BazaarItemDTO LoadById(long bazaarItemId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<BazaarItemDTO>(context.BazaarItem.FirstOrDefault(i => i.BazaarItemId.Equals(bazaarItemId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public void RemoveOutDated()
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    foreach (BazaarItem entity in context.BazaarItem.Where(e => DbFunctions.AddDays(DbFunctions.AddHours(e.DateStart, e.Duration), e.MedalUsed ? 30 : 7) < DateTime.Now))
                    {
                        context.BazaarItem.Remove(entity);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        private BazaarItemDTO Insert(BazaarItemDTO bazaarItem, OpenNosContext context)
        {
            BazaarItem entity = _mapper.Map<BazaarItem>(bazaarItem);
            context.BazaarItem.Add(entity);
            context.SaveChanges();
            return _mapper.Map<BazaarItemDTO>(entity);
        }

        private BazaarItemDTO Update(BazaarItem entity, BazaarItemDTO bazaarItem, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(bazaarItem, entity);
                context.SaveChanges();
            }

            return _mapper.Map<BazaarItemDTO>(entity);
        }

        #endregion
    }
}