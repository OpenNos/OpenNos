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
using OpenNos.Data.Enums;

namespace OpenNos.DAL.EF.MySQL
{
    public class MailEquipmentDAO : IMailEquipmentDAO
    {
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public MailEquipmentDAO()
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
        public DeleteResult DeleteByMailId(long mailId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    foreach (Mail mail in context.Mail.Where(s => s.MailId.Equals(mailId)))
                    {
                        context.Mail.Remove(mail);
                    }
                    context.SaveChanges();

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public MailEquipmentDTO Insert(MailEquipmentDTO mail)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    MailEquipment entity = _mapper.Map<MailEquipment>(mail);
                    context.MailEquipment.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<MailEquipmentDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<MailEquipmentDTO> LoadByMailId(long mailId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (MailEquipment mail in context.MailEquipment.Where(s => s.Mail.MailId.Equals(mailId)))
                {
                    yield return _mapper.Map<MailEquipmentDTO>(mail);
                }
            }
        }

        #endregion
    }
}