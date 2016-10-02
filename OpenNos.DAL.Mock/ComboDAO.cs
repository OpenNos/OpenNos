using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class ComboDAO : BaseDAO<ComboDTO>, IComboDAO
    {
        #region Methods

        public ComboDTO LoadById(short comboId)
        {
            return Container.SingleOrDefault(c => c.ComboId == comboId);
        }

        public IEnumerable<ComboDTO> LoadBySkillVnum(short skillVNum)
        {
            return Container.Where(c => c.SkillVNum == skillVNum);
        }

        public IEnumerable<ComboDTO> LoadByVNumHitAndEffect(short skillVNum, short hit, short effect)
        {
            return Container.Where(c => c.SkillVNum == skillVNum && c.Hit == hit && c.Effect == effect);
        }

        #endregion
    }
}