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
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class PenaltyLogDAO : MappingBaseDAO<PenaltyLog, PenaltyLogDTO>, IPenaltyLogDAO
    {
        #region Methods

        public DeleteResult Delete(int penaltylogid)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    PenaltyLog PenaltyLog = context.PenaltyLog.FirstOrDefault(c => c.PenaltyLogId.Equals(penaltylogid));

                    if (PenaltyLog != null)
                    {
                        context.PenaltyLog.Remove(PenaltyLog);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("DELETE_PENALTYLOG_ERROR"), penaltylogid, e.Message), e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref PenaltyLogDTO log)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    int id = log.PenaltyLogId;
                    PenaltyLog entity = context.PenaltyLog.FirstOrDefault(c => c.PenaltyLogId.Equals(id));

                    if (entity == null)
                    {
                        log = Insert(log, context);
                        return SaveResult.Inserted;
                    }

                    log = Update(entity, log, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("UPDATE_PENALTYLOG_ERROR"), log.PenaltyLogId, e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<PenaltyLogDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (PenaltyLog entity in context.PenaltyLog)
                {
                    yield return _mapper.Map<PenaltyLogDTO>(entity);
                }
            }
        }

        public IEnumerable<PenaltyLogDTO> LoadByAccount(long accountId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (PenaltyLog PenaltyLog in context.PenaltyLog.Where(s => s.AccountId.Equals(accountId)))
                {
                    yield return _mapper.Map<PenaltyLogDTO>(PenaltyLog);
                }
            }
        }

        public PenaltyLogDTO LoadById(int relId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<PenaltyLogDTO>(context.PenaltyLog.FirstOrDefault(s => s.PenaltyLogId.Equals(relId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private PenaltyLogDTO Insert(PenaltyLogDTO penaltylog, OpenNosContext context)
        {
            PenaltyLog entity = _mapper.Map<PenaltyLog>(penaltylog);
            context.PenaltyLog.Add(entity);
            context.SaveChanges();
            return _mapper.Map<PenaltyLogDTO>(entity);
        }

        private PenaltyLogDTO Update(PenaltyLog entity, PenaltyLogDTO penaltylog, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(penaltylog, entity);
                context.SaveChanges();
            }
            return _mapper.Map<PenaltyLogDTO>(entity);
        }

        #endregion
    }
}