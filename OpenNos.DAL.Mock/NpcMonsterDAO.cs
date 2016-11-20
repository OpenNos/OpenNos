using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Mock
{
    public class NpcMonsterDAO : BaseDAO<NpcMonsterDTO>, INpcMonsterDAO
    {
        #region Methods

        public IEnumerable<NpcMonsterDTO> FindByName(string name)
        {
            throw new NotImplementedException();
        }

        public SaveResult InsertOrUpdate(ref NpcMonsterDTO npcMonster)
        {
            throw new NotImplementedException();
        }

        public NpcMonsterDTO LoadByVNum(short MapId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}