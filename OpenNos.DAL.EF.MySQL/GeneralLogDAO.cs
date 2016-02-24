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

using AutoMapper;
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class GeneralLogDAO : IGeneralLogDAO
    {
        #region Methods

        public IEnumerable<GeneralLogDTO> LoadByLogType(string LogType, Nullable<long> CharacterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (GeneralLog log in context.generallog.Where(c => c.LogType.Equals(LogType) && c.CharacterId == CharacterId))
                {
                    yield return Mapper.Map<GeneralLogDTO>(log);
                }
            }
        }

        public void SetCharIdNull(long? CharacterId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (GeneralLog log in context.generallog.Where(c => c.CharacterId == CharacterId))
                {
                    log.CharacterId = null;
                }
                context.SaveChanges();
            }
        }

        public void WriteGeneralLog(long accountId, string ipAddress, Nullable<long> characterId, string logType, string logData)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                GeneralLog log = new GeneralLog()
                {
                    AccountId = accountId,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.Now,
                    LogType = logType,
                    LogData = logData,
                    CharacterId = characterId
                };

                context.generallog.Add(log);
                context.SaveChanges();
            }
        }

        #endregion
    }
}