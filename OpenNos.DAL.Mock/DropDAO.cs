using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class DropDAO : IDropDAO
    {
        #region Methods

        public void Insert(List<DropDTO> drops)
        {
            throw new NotImplementedException();
        }

        public DropDTO Insert(DropDTO drop)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DropDTO> LoadByMonster(short monsterVNum)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}