using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.Mock
{
    public class MailDAO : BaseDAO<MailDTO>, IMailDAO
    {
        #region Methods

        public DeleteResult DeleteById(long mailId)
        {
            MailDTO dtoToDelete = LoadById(mailId);
            Container.Remove(dtoToDelete);
            return DeleteResult.Deleted;
        }

        public SaveResult InsertOrUpdate(ref MailDTO mail)
        {
            MailDTO dto = LoadById(mail.MailId);
            if (dto != null)
            {
                dto = mail;
                return SaveResult.Updated;
            }
            else
            {
                Insert(mail);
                return SaveResult.Inserted;
            }
        }

        public MailDTO LoadById(long mailId)
        {
            return Container.SingleOrDefault(m => m.MailId == mailId);
        }

        public IEnumerable<MailDTO> LoadByReceiverId(long receiverId)
        {
            return Container.Where(m => m.ReceiverId == receiverId);
        }

        public IEnumerable<MailDTO> LoadBySenderId(long senderId)
        {
            return Container.Where(m => m.SenderId == senderId);
        }

        #endregion
    }
}