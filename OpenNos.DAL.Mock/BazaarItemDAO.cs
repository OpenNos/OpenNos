using OpenNos.Data;
using OpenNos.DAL.Interface;
using OpenNos.Data.Enums;
using System;

namespace OpenNos.DAL.Mock
{
    public class BazaarItemDAO : BaseDAO<BazaarItemDTO>, IBazaarItemDAO
    {
        public DeleteResult Delete(long bazaarItemId)
        {
            throw new NotImplementedException();
        }

        public SaveResult InsertOrUpdate(ref BazaarItemDTO bazaarItem)
        {
            throw new NotImplementedException();
        }

        public BazaarItemDTO LoadById(long bazaarItemId)
        {
            throw new NotImplementedException();
        }

        public void RemoveOutDated()
        {
            throw new NotImplementedException();
        }
    }
}