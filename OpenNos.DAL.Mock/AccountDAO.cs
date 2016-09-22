using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;

namespace OpenNos.DAL.Mock
{
    public class AccountDAO : IAccountDAO
    {
        #region Methods

        public DeleteResult Delete(long accountId)
        {
            throw new NotImplementedException();
        }

        public SaveResult InsertOrUpdate(ref AccountDTO account)
        {
            throw new NotImplementedException();
        }

        public AccountDTO LoadById(long accountId)
        {
            throw new NotImplementedException();
        }

        public AccountDTO LoadByName(string Name)
        {
            throw new NotImplementedException();
        }

        public AccountDTO LoadBySessionId(int sessionId)
        {
            throw new NotImplementedException();
        }

        public void LogIn(string name)
        {
            throw new NotImplementedException();
        }

        public void UpdateLastSessionAndIp(string name, int session, string ip)
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