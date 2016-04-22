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
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class AccountDAO : IAccountDAO
    {
        #region Methods

        public DeleteResult Delete(long AccountId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Account Account = context.Account.FirstOrDefault(c => c.AccountId.Equals(AccountId));

                    if (Account != null)
                    {
                        context.Account.Remove(Account);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("DELETE_Account_ERROR"), AccountId, e.Message);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref AccountDTO Account)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long AccountId = Account.AccountId;
                    Account entity = context.Account.FirstOrDefault(c => c.AccountId.Equals(AccountId));

                    if (entity == null) //new entity
                    {
                        Account = Insert(Account, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        Account = Update(entity, Account, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_Account_ERROR"), Account.AccountId, e.Message);
                return SaveResult.Error;
            }
        }

        public AccountDTO LoadById(long AccountId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account Account = context.Account.FirstOrDefault(a => a.AccountId.Equals(AccountId));

                if (Account != null)
                {
                    return Mapper.DynamicMap<AccountDTO>(Account);
                }
            }

            return null;
        }

        public AccountDTO LoadByName(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account Account = context.Account.FirstOrDefault(a => a.Name.Equals(name));

                if (Account != null)
                {
                    return Mapper.DynamicMap<AccountDTO>(Account);
                }
            }

            return null;
        }

        public AccountDTO LoadBySessionId(int sessionId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account Account = context.Account.FirstOrDefault(a => a.LastSession.Equals(sessionId));

                if (Account != null)
                {
                    return Mapper.DynamicMap<AccountDTO>(Account);
                }
            }

            return null;
        }

        public void LogIn(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account Account = context.Account.FirstOrDefault(a => a.Name.Equals(name));
                context.SaveChanges();
            }
        }

        public void ToggleBan(long id)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account Account = context.Account.FirstOrDefault(a => a.AccountId.Equals(id));
                Account.Authority = Account.Authority >= 1 ? (byte)0 : (byte)1;
                context.SaveChanges();
            }
        }

        public void UpdateLastSessionAndIp(string name, int session, string ip)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account Account = context.Account.FirstOrDefault(a => a.Name.Equals(name));
                Account.LastSession = session;
                context.SaveChanges();
            }
        }

        public void WriteGeneralLog(long AccountId, string ipAddress, long? CharacterId, string logType, string logData)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                GeneralLog log = new GeneralLog()
                {
                    AccountId = AccountId,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.Now,
                    LogType = logType,
                    LogData = logData,
                    CharacterId = CharacterId
                };

                context.GeneralLog.Add(log);
                context.SaveChanges();
            }
        }

        private AccountDTO Insert(AccountDTO Account, OpenNosContext context)
        {
            Account entity = Mapper.DynamicMap<Account>(Account);
            context.Account.Add(entity);
            context.SaveChanges();
            return Mapper.DynamicMap<AccountDTO>(entity);
        }

        private AccountDTO Update(Account entity, AccountDTO Account, OpenNosContext context)
        {
            entity = Mapper.DynamicMap<Account>(Account);
            context.SaveChanges();
            return Mapper.DynamicMap<AccountDTO>(entity);
        }

        #endregion
    }
}