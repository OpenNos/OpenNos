using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class PenaltyLogDAO : BaseDAO<PenaltyLogDTO>, IPenaltyLogDAO
    {
        #region Methods

        public DeleteResult Delete(int penaltylogId)
        {
            PenaltyLogDTO dto = LoadById(penaltylogId);
            Container.Remove(dto);
            return DeleteResult.Deleted;
        }

        public bool IdAlreadySet(long id)
        {
            return Container.Any(pl => pl.PenaltyLogId == id);
        }

        public IEnumerable<PenaltyLogDTO> LoadByAccount(long accountId)
        {
            return Container.Where(pl => pl.AccountId == accountId);
        }

        public PenaltyLogDTO LoadById(int penaltylogId)
        {
            return Container.SingleOrDefault(p => p.PenaltyLogId == penaltylogId);
        }

        public void Update(PenaltyLogDTO penaltylog)
        {
            PenaltyLogDTO dto = LoadById(penaltylog.PenaltyLogId);
            dto = penaltylog;
        }

        #endregion
    }
}