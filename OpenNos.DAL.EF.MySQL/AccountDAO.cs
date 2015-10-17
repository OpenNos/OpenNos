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

namespace OpenNos.DAL.EF.MySQL
{
    public class AccountDAO : IAccountDAO
    {
        public bool CheckPasswordValiditiy(string name, string passwordHashed)
        {
            using (var context = DBHelper.CreateContext())
            {
                return context.Account.Any(a => a.Name.Equals(name) && a.Password.Equals(passwordHashed));
            }
        }

        public bool IsLoggedIn(string name)
        {
            using (var context = DBHelper.CreateContext())
            {
                return context.Account.Any(a => a.Name.Equals(name) && a.LoggedIn);
            }
        }

        public AuthorityType LoadAuthorityType(string name)
        {
            using (var context = DBHelper.CreateContext())
            {
                return (AuthorityType)context.Account.SingleOrDefault(a => a.Name.Equals(name)).Authority;
            }
        }

        public void LogIn(string name)
        {
            using (var context = DBHelper.CreateContext())
            {
                account account = context.Account.SingleOrDefault(a => a.Name.Equals(name));
                account.LoggedIn = true;
                account.LastConnect = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void UpdateLastSessionAndIp(string name, int session, string ip)
        {
            using (var context = DBHelper.CreateContext())
            {
                account account = context.Account.SingleOrDefault(a => a.Name.Equals(name));
                account.LastSession = session;
                account.LastIp = ip;
                account.LastConnect = DateTime.Now;
                context.SaveChanges();
            }
        }
    }
}
