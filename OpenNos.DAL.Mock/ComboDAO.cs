using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class ComboDAO : IComboDAO
    {
        #region Methods

        public void Insert(List<ComboDTO> combos)
        {
            throw new NotImplementedException();
        }

        public ComboDTO Insert(ComboDTO combo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ComboDTO> LoadAll()
        {
            throw new NotImplementedException();
        }

        public ComboDTO LoadById(short ComboId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ComboDTO> LoadBySkillVnum(short skillVNum)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ComboDTO> LoadByVNumHitAndEffect(short skillVNum, short hit, short effect)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}