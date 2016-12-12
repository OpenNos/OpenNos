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

        public bool IdAlreadySet(long id)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return context.PenaltyLog.Any(gl => gl.PenaltyLogId == id);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return false;
            }
        }

        public PenaltyLogDTO Insert(PenaltyLogDTO penaltylog)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    PenaltyLog entity = _mapper.Map<PenaltyLog>(penaltylog);
                    context.PenaltyLog.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<PenaltyLogDTO>(penaltylog);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
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

        public PenaltyLogDTO LoadById(int penaltylogId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<PenaltyLogDTO>(context.PenaltyLog.FirstOrDefault(s => s.PenaltyLogId.Equals(penaltylogId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public void Update(PenaltyLogDTO penaltylog)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    PenaltyLog result = context.PenaltyLog.FirstOrDefault(c => c.AccountId == penaltylog.AccountId && c.PenaltyLogId == penaltylog.PenaltyLogId);
                    if (result != null)
                    {
                        penaltylog.PenaltyLogId = result.PenaltyLogId;
                        _mapper.Map(penaltylog, result);
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion
    }
}