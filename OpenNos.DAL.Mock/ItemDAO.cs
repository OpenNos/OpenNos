using OpenNos.DAL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;

namespace OpenNos.DAL.Mock
{
    public class ItemDAO : IItemDAO
    {
        public IEnumerable<ItemDTO> FindByName(string name)
        {
            throw new NotImplementedException();
        }

        public void Insert(List<ItemDTO> items)
        {
            throw new NotImplementedException();
        }

        public ItemDTO Insert(ItemDTO item)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ItemDTO> LoadAll()
        {
            return new List<ItemDTO>()
            {
                new ItemDTO() { }
            };
        }

        public ItemDTO LoadById(short Vnum)
        {
            throw new NotImplementedException();
        }
    }
}
