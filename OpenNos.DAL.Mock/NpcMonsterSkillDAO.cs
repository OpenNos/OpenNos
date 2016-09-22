using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class NpcMonsterSkillDAO : INpcMonsterSkillDAO
    {
        #region Methods

        public void Insert(List<NpcMonsterSkillDTO> skills)
        {
            throw new NotImplementedException();
        }

        public NpcMonsterSkillDTO Insert(ref NpcMonsterSkillDTO npcmonsterskill)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NpcMonsterSkillDTO> LoadByNpcMonster(short npcId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}