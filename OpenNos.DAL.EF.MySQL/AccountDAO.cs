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
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class AccountDAO : IAccountDAO
    {
        #region Methods

        public DeleteResult Delete(long accountId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Account account = context.account.SingleOrDefault(c => c.AccountId.Equals(accountId));

                    if (account != null)
                    {
                        context.account.Remove(account);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("DELETE_ACCOUNT_ERROR"), accountId, e.Message);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref AccountDTO account)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long accountId = account.AccountId;
                    Account entity = context.account.SingleOrDefault(c => c.AccountId.Equals(accountId));

                    if (entity == null) //new entity
                    {
                        account = Insert(account, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        account = Update(entity, account, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_ACCOUNT_ERROR"), account.AccountId, e.Message);
                return SaveResult.Error;
            }
        }

        public AccountDTO LoadById(long accountId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account account = context.account.FirstOrDefault(a => a.AccountId.Equals(accountId));

                if (account != null)
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

        public AccountDTO LoadBySessionId(int sessionId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account account = context.account.FirstOrDefault(a => a.LastSession.Equals(sessionId));

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
                context.SaveChanges();
            }
        }

        public void ToggleBan(long id)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Account account = context.account.SingleOrDefault(a => a.AccountId.Equals(id));
                account.Authority = account.Authority >= 1 ? (short)0 : (short)1;
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

        private AccountDTO Insert(AccountDTO account, OpenNosContainer context)
        {
            Account entity = Mapper.Map<Account>(account);
            context.account.Add(entity);
            context.SaveChanges();
            return Mapper.Map<AccountDTO>(entity);
        }

        private AccountDTO Update(Account entity, AccountDTO account, OpenNosContainer context)
        {
            entity = Mapper.Map<Account>(account);
            context.SaveChanges();
            return Mapper.Map<AccountDTO>(entity);
        }

        #endregion
    }
}