using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class ItemDAO : IItemDAO
    {
        #region Methods

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

        #endregion
    }
}