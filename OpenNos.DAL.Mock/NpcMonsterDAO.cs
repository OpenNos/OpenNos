using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class NpcMonsterDAO : BaseDAO<NpcMonsterDTO>, INpcMonsterDAO
    {
        #region Methods

        public IEnumerable<NpcMonsterDTO> FindByName(string name)
        {
            throw new NotImplementedException();
        }

        public NpcMonsterDTO LoadByVnum(short MapId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}