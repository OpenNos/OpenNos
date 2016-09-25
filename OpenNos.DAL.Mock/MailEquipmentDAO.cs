using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using OpenNos.Data.Enums;

namespace OpenNos.DAL.Mock
{
    public class MailEquipmentDAO : IMailEquipmentDAO
    {

        #region Methods

        public DeleteResult DeleteByMailId(long mailId)
        {
            throw new NotImplementedException();
        }

        public MailEquipmentDTO Insert(MailEquipmentDTO mail)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MailEquipmentDTO> LoadByMailId(long mailId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}