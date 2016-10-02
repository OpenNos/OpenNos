using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
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
            else
            {
                Insert(account);
                return SaveResult.Inserted;
            }
        }

        public AccountDTO LoadById(long accountId)
        {
            return Container.SingleOrDefault(a => a.AccountId == accountId);
        }

        public AccountDTO LoadByName(string name)
        {
            return Container.SingleOrDefault(a => a.Name == name);
        }

        public AccountDTO LoadBySessionId(int sessionId)
        {
            return Container.SingleOrDefault(a => a.LastSession == sessionId);
        }

        public void LogIn(string name)
        {
            throw new NotImplementedException();
        }

        public void UpdateLastSessionAndIp(string name, int session, string ip)
        {
            AccountDTO account = Container.SingleOrDefault(a => a.Name == name);
            account.LastSession = session;
        }

        public void WriteGeneralLog(long accountId, string ipAddress, long? characterId, string logType, string logData)
        {
            // we dont care about general log in test now
        }

        #endregion
    }
}