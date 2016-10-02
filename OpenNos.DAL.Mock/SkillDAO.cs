using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class SkillDAO : BaseDAO<SkillDTO>, ISkillDAO
    {
        #region Methods

        public SkillDTO LoadById(short skillId)
        {
            return Container.SingleOrDefault(s => s.SkillVNum == skillId);
        }

        #endregion
    }
}