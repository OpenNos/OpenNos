/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
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
            Insert(mail);
            return SaveResult.Inserted;
        }

        public MailDTO LoadById(long mailId)
        {
            return Container.SingleOrDefault(m => m.MailId == mailId);
        }

        #endregion
    }
}