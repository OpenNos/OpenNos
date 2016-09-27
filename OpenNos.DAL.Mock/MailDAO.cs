using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public class MailDAO : IMailDAO
    {
        #region Methods

        public DeleteResult DeleteById(long mailId)
        {
            throw new NotImplementedException();
        }

        public SaveResult InsertOrUpdate(ref MailDTO mail)
        {
            throw new NotImplementedException();
        }

        public MailDTO LoadById(long mailId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MailDTO> LoadByReceiverId(long receiverId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MailDTO> LoadBySenderId(long senderId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}