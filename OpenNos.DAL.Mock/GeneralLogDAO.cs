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
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class GeneralLogDAO : BaseDAO<GeneralLogDTO>, IGeneralLogDAO
    {
        #region Methods

        public bool IdAlreadySet(long id)
        {
            return Container.Any(gl => gl.LogId == id);
        }

        public IEnumerable<GeneralLogDTO> LoadByAccount(long? accountId)
        {
            return Container.Where(c => c.AccountId == accountId).Select(e => MapEntity(e));
        }

        public IEnumerable<GeneralLogDTO> LoadByLogType(string LogType, long? CharacterId)
        {
            return Enumerable.Empty<GeneralLogDTO>().Select(e => MapEntity(e));
        }

        public void SetCharIdNull(long? CharacterId)
        {
            throw new NotImplementedException();
        }

        public void WriteGeneralLog(long accountId, string ipAddress, long? characterId, string logType, string logData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}