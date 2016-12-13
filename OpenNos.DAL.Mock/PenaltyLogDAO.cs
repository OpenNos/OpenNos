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
using OpenNos.Data.Enums;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class PenaltyLogDAO : BaseDAO<PenaltyLogDTO>, IPenaltyLogDAO
    {
        #region Methods

        public DeleteResult Delete(int penaltylogId)
        {
            PenaltyLogDTO dto = LoadById(penaltylogId);
            Container.Remove(dto);
            return DeleteResult.Deleted;
        }

        public bool IdAlreadySet(long id)
        {
            return Container.Any(pl => pl.PenaltyLogId == id);
        }

        public IEnumerable<PenaltyLogDTO> LoadByAccount(long accountId)
        {
            return Container.Where(pl => pl.AccountId == accountId);
        }

        public PenaltyLogDTO LoadById(int penaltylogId)
        {
            return Container.SingleOrDefault(p => p.PenaltyLogId == penaltylogId);
        }

        public void Update(PenaltyLogDTO penaltylog)
        {
            PenaltyLogDTO dto = LoadById(penaltylog.PenaltyLogId);
            dto = penaltylog;
        }

        #endregion
    }
}