using System.Linq;
using OpenNos.Data;
using OpenNos.DAL.Interface;

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