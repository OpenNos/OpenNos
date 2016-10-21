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

using AutoMapper;
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class MailDAO : IMailDAO
    {
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public MailDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Mail, MailDTO>();
                cfg.CreateMap<MailDTO, Mail>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Methods

        public SaveResult InsertOrUpdate(ref MailDTO mail)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    long mailId = mail.MailId;
                    Mail entity = context.Mail.FirstOrDefault(c => c.MailId.Equals(mailId));

                    if (entity == null) //new entity
                    {
                        mail = Insert(mail, context);
                        return SaveResult.Inserted;
                    }
                    else //existing entity
                    {
                        mail.MailId = entity.MailId;
                        mail = Update(entity, mail, context);
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SaveResult.Error;
            }
        }

        private MailDTO Insert(MailDTO mail, OpenNosContext context)
        {
            try
            {
                Mail entity = _mapper.Map<Mail>(mail);
                context.Mail.Add(entity);
                context.SaveChanges();
                return _mapper.Map<MailDTO>(mail);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private MailDTO Update(Mail entity, MailDTO respawn, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(respawn, entity);
                context.SaveChanges();
            }
            return _mapper.Map<MailDTO>(entity);
        }

        public MailDTO LoadById(long mailId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<MailDTO>(context.Mail.FirstOrDefault(i => i.MailId.Equals(mailId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
        public DeleteResult DeleteById(long mailId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Mail mail = context.Mail.First(i => i.MailId.Equals(mailId));

                    if (mail != null)
                    {
                        context.Mail.Remove(mail);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }
        public IEnumerable<MailDTO> LoadByReceiverId(long receiverId)
        {

            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Mail mail in context.Mail.Where(i => i.ReceiverId.Equals(receiverId) && i.OwnerId.Equals(receiverId)))
                {
                    yield return _mapper.Map<MailDTO>(mail);
                }
            }

        }

        public IEnumerable<MailDTO> LoadBySenderId(long senderId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Mail mail in context.Mail.Where(i => i.SenderId.Equals(senderId) && i.OwnerId.Equals(senderId)))
                {
                    yield return _mapper.Map<MailDTO>(mail);
                }
            }
        }

        #endregion
    }
}