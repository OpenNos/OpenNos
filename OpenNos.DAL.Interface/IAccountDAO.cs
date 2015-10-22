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

        AccountDTO LoadBySessionId(int sessionId);

        bool CheckPasswordValiditiy(string name, string passwordHashed);

        AuthorityType LoadAuthorityType(string name);

        void UpdateLastSessionAndIp(string name, int session, string ip);

        bool IsLoggedIn(string name);

        void LogIn(string name);
        AccountDTO LoadByName(string Name);


        #endregion
    }
}
