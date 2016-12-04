using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class GeneralLogDAO : BaseDAO<GeneralLogDTO>, IGeneralLogDAO
    {
        #region Methods

        public bool IdAlreadySet(long id)
        {
            return Container.Any(gl => gl.LogId == id);
        }

        public IEnumerable<GeneralLogDTO> LoadByAccount(long accountId)
        {
            return Container.Where(c => c.AccountId == accountId).Select(e => MapEntity(e));
        }

        public IEnumerable<GeneralLogDTO> LoadByLogType(string LogType, long? CharacterId)
        {
            return Enumerable.Empty<GeneralLogDTO>().Select(e => MapEntity(e));
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