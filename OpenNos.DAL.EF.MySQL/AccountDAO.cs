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
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.Interface;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using AutoMapper;

namespace OpenNos.DAL.EF.MySQL
{
    public class AccountDAO : IAccountDAO
    {
        public bool IsLoggedIn(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return context.account.Any(a => a.Name.Equals(name) && a.LoggedIn);
            }
        }

        public AccountDTO LoadBySessionId(int sessionId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account account = context.account.FirstOrDefault(a => a.LastSession.Equals(sessionId));

                if(account != null)
                {
                    return Mapper.Map<AccountDTO>(account); 
                }
            }

            return null;
        }

        public AccountDTO LoadByName(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account account = context.account.FirstOrDefault(a => a.Name.Equals(name));

                if (account != null)
                {
                    return Mapper.Map<AccountDTO>(account);
                }
            }

            return null;
        }

        public void LogIn(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account account = context.account.SingleOrDefault(a => a.Name.Equals(name));
                account.LoggedIn = true;
                context.SaveChanges();
            }
        }

        public void UpdateLastSessionAndIp(string name, int session, string ip)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account account = context.account.SingleOrDefault(a => a.Name.Equals(name));
                account.LastSession = session;
                context.SaveChanges();
            }
        }

        public void WriteConnectionLog(long accountId, string ipAddress)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                ConnectionLog log = new ConnectionLog()
                {
                    AccountId = accountId,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.Now
                };

                context.connectionlog.Add(log);
                context.SaveChanges();
            }
        }
    }
}
