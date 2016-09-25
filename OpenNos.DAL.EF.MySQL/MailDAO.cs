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
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
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

        public MailDTO Insert(MailDTO mail)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Mail entity = _mapper.Map<Mail>(mail);
                    context.Mail.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<MailDTO>(mail);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
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

        public IEnumerable<MailDTO> LoadByReceiverId(long receiverId)
        {

            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Mail mail in context.Mail.Where(i => i.ReceiverId.Equals(receiverId)))
                {
                    yield return _mapper.Map<MailDTO>(mail);
                }
            }

        }

        public IEnumerable<MailDTO> LoadBySenderId(long senderId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Mail mail in context.Mail.Where(i => i.SenderId.Equals(senderId)))
                {
                    yield return _mapper.Map<MailDTO>(mail);
                }
            }
        }

        #endregion
    }
}