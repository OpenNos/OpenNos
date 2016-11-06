using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class NpcMonsterSkillDAO : BaseDAO<NpcMonsterSkillDTO>, INpcMonsterSkillDAO
    {
        #region Methods

        private IList<NpcMonsterSkillDTO> _mockContainer = new List<NpcMonsterSkillDTO>();

        public void Insert(List<NpcMonsterSkillDTO> skills)
        {
            throw new NotImplementedException();
        }

        public NpcMonsterSkillDTO Insert(ref NpcMonsterSkillDTO npcmonsterskill)
        {
            throw new NotImplementedException();
        }

        public List<NpcMonsterSkillDTO> LoadAll()
        {
            return _mockContainer.ToList();
        }

        public IEnumerable<NpcMonsterSkillDTO> LoadByNpcMonster(short npcId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}