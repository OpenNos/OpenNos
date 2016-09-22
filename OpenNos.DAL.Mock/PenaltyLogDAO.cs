using OpenNos.DAL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Mock
{
    public class PenaltyLogDAO : IPenaltyLogDAO
    {
        public DeleteResult Delete(int penaltylogId)
        {
            throw new NotImplementedException();
        }

        public bool IdAlreadySet(long id)
        {
            throw new NotImplementedException();
        }

        public PenaltyLogDTO Insert(PenaltyLogDTO penaltylog)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PenaltyLogDTO> LoadByAccount(long accountId)
        {
            throw new NotImplementedException();
        }

        public PenaltyLogDTO LoadById(int penaltylogId)
        {
            throw new NotImplementedException();
        }

        public void Update(PenaltyLogDTO penaltylog)
        {
            throw new NotImplementedException();
        }
    }
}
