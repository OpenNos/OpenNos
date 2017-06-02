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
using OpenNos.Domain;
using System;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class AccountDAO : BaseDAO<AccountDTO>, IAccountDAO
    {
        #region Methods

        public DeleteResult Delete(long accountId)
        {
            throw new NotImplementedException();
        }

        public SaveResult InsertOrUpdate(ref AccountDTO account)
        {
            AccountDTO dto = LoadById(account.AccountId);
            if (dto != null)
            {
                dto = account;
                return SaveResult.Updated;
            }
            Insert(account);
            return SaveResult.Inserted;
        }

        public AccountDTO LoadById(long accountId)
        {
            return MapEntity(Container.SingleOrDefault(a => a.AccountId == accountId));
        }

        public AccountDTO LoadByName(string name)
        {
            return Container.SingleOrDefault(a => a.Name == name);
        }

        public void LogIn(string name)
        {
            throw new NotImplementedException();
        }

        public void UpdateLastSessionAndIp(string name, int session, string ip)
        {
            AccountDTO account = Container.SingleOrDefault(a => a.Name == name);
        }

        public void WriteGeneralLog(long accountId, string ipAddress, long? characterId, GeneralLogType logType, string logData)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}