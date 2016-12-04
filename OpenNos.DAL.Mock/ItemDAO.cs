using System.Collections.Generic;
using System.Linq;
using OpenNos.Data;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.Mock
{
    public class ItemDAO : BaseDAO<ItemDTO>, IItemDAO
    {
        #region Methods

        public IEnumerable<ItemDTO> FindByName(string name)
        {
            return Container.Where(i => i.Name.Contains(name)).Select(e => MapEntity(e));
        }

        public ItemDTO LoadById(short vnum)
        {
            return Container.SingleOrDefault(i => i.VNum == vnum);
        }

        #endregion
    }
}