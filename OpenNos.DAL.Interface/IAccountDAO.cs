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
using OpenNos.Data;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.Interface
{
    public interface IAccountDAO
    {
        #region Methods

        AccountDTO LoadById(long accountId);

        AccountDTO LoadBySessionId(int sessionId);

        AccountDTO LoadByName(string Name);

        void UpdateLastSessionAndIp(string name, int session, string ip);

        bool IsLoggedIn(string name);

        void LogIn(string name);

        void WriteConnectionLog(long accountId, string ipAddress, Nullable<long> characterId, string logType, string logData);

        SaveResult InsertOrUpdate(ref AccountDTO account);

        DeleteResult Delete(long accountId);

        void BanUnBan(long id);

        #endregion
    }
}
