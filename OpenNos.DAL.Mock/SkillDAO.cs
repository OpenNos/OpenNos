using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class SkillDAO : BaseDAO<SkillDTO>, ISkillDAO
    {
        #region Methods

        public void Insert(List<SkillDTO> skills)
        {
            throw new NotImplementedException();
        }

        public SkillDTO Insert(SkillDTO skill)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SkillDTO> LoadAll()
        {
            return Container;
        }

        public SkillDTO LoadById(short SkillId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}