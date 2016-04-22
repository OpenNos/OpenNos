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

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OpenNos.Data;
using OpenNos.Data.Enums;

using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL.DB;

namespace OpenNos.DAL.EF.MySQL
{
    public class QuicklistEntryDAO : IQuicklistEntryDAO
    {

        public SaveResult InsertOrUpdate(ref QuicklistEntryDTO QuicklistEntry)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    long entryId = QuicklistEntry.EntryId;
                    QuicklistEntry dbentry = context.QuicklistEntry.FirstOrDefault(c => c.EntryId == entryId);
                    if (dbentry == null)
                    {
                        // new entity
                        QuicklistEntry entry = Mapper.Map<QuicklistEntry>(QuicklistEntry);
                        context.QuicklistEntry.Add(entry);
                        context.SaveChanges();
                        Mapper.Map(entry, QuicklistEntry);
                        return SaveResult.Inserted;
                    }
                    else
                    {
                        //existing entity
                        Mapper.Map(QuicklistEntry, dbentry);
                        context.SaveChanges();
                        QuicklistEntry = Mapper.Map<QuicklistEntryDTO>(QuicklistEntry); // does this line anything?
                        return SaveResult.Updated;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.ErrorFormat(Language.Instance.GetMessageFromKey("UPDATE_ERROR"), e.Message);
                return SaveResult.Error;
            }
        }

        public IEnumerable<QuicklistEntryDTO> Load(long CharacterId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (QuicklistEntry QuicklistEntryobject in context.QuicklistEntry.Where(i => i.CharacterId == CharacterId))
                {
                    yield return Mapper.Map<QuicklistEntryDTO>(QuicklistEntryobject);
                }
            }
        }

        public DeleteResult Delete(long CharacterId, long entryId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                QuicklistEntry QuicklistEntryItem = context.QuicklistEntry.FirstOrDefault(i => i.CharacterId == CharacterId && i.EntryId == entryId);
                if (QuicklistEntryItem != null)
                {
                    context.QuicklistEntry.Remove(QuicklistEntryItem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
    }
}

