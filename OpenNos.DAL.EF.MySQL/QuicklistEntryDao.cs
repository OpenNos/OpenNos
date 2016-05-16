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
    public class QuicklistEntryDAO : IQuicklistEntryDAO
    {
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public QuicklistEntryDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<QuicklistEntry, QuicklistEntryDTO>();
                cfg.CreateMap<QuicklistEntryDTO, QuicklistEntry>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Methods

        public DeleteResult Delete(long characterId, long entryId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                QuicklistEntry QuicklistEntryItem = context.QuicklistEntry.FirstOrDefault(i => i.CharacterId == characterId && i.EntryId == entryId);
                if (QuicklistEntryItem != null)
                {
                    context.QuicklistEntry.Remove(QuicklistEntryItem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }

        public SaveResult InsertOrUpdate(ref QuicklistEntryDTO quickListEntry)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long entryId = quickListEntry.EntryId;
                    QuicklistEntry dbentry = context.QuicklistEntry.FirstOrDefault(c => c.EntryId == entryId);
                    if (dbentry == null)
                    {
                        // new entity
                        QuicklistEntry entry = _mapper.Map<QuicklistEntry>(quickListEntry);
                        context.QuicklistEntry.Add(entry);
                        context.SaveChanges();
                        _mapper.Map(entry, quickListEntry);
                        return SaveResult.Inserted;
                    }
                    else
                    {
                        //existing entity
                        _mapper.Map(quickListEntry, dbentry);
                        context.SaveChanges();
                        quickListEntry = _mapper.Map<QuicklistEntryDTO>(quickListEntry); // does this line anything?
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(String.Format(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message), e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<QuicklistEntryDTO> Load(long characterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (QuicklistEntry QuicklistEntryobject in context.QuicklistEntry.Where(i => i.CharacterId == characterId))
                {
                    yield return _mapper.Map<QuicklistEntryDTO>(QuicklistEntryobject);
                }
            }
        }

        #endregion
    }
}