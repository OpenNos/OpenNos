using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class DropDAO : BaseDAO<DropDTO>, IDropDAO
    {
        #region Members

        private IList<DropDTO> _mockContainer = new List<DropDTO>();

        #endregion

        #region Methods

        public void Insert(List<DropDTO> drops)
        {
            throw new NotImplementedException();
        }

        public DropDTO Insert(DropDTO drop)
        {
            throw new NotImplementedException();
        }

        public List<DropDTO> LoadAll()
        {
            return _mockContainer.ToList();
        }

        public IEnumerable<DropDTO> LoadByMonster(short monsterVNum)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}