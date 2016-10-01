using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class ItemDAO : BaseDAO<ItemDTO>, IItemDAO
    {
        #region Methods

        public IEnumerable<ItemDTO> FindByName(string name)
        {
            return Container.Where(i => i.Name.Contains(name));
        }

        public ItemDTO LoadById(short vnum)
        {
            return Container.SingleOrDefault(i => i.VNum == vnum);
        }

        #endregion
    }
}