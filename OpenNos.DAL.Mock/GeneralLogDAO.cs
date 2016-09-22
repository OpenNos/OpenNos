using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class GeneralLogDAO : IGeneralLogDAO
    {
        #region Methods

        public bool IdAlreadySet(long id)
        {
            throw new NotImplementedException();
        }

        public GeneralLogDTO Insert(GeneralLogDTO generallog)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GeneralLogDTO> LoadByAccount(long accountId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GeneralLogDTO> LoadByLogType(string LogType, long? CharacterId)
        {
            throw new NotImplementedException();
        }

        public void SetCharIdNull(long? CharacterId)
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