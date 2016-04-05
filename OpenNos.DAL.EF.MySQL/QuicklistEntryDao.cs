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
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Core;

namespace OpenNos.DAL.EF.MySQL
{
    public class QuicklistEntryDAO : IQuicklistEntryDAO
    {

        public SaveResult InsertOrUpdate(ref QuicklistEntryDTO quicklist)
        {
            try
            {
                using (OpenNosContainer context = DataAccessHelper.CreateContext())
                {
                    long entryId = quicklist.EntryId;
                    QuicklistEntry dbentry = context.quicklist.FirstOrDefault(c => c.EntryId == entryId);
                    if (dbentry == null)
                    {
                        // new entity
                        QuicklistEntry entry = Mapper.Map<QuicklistEntry>(quicklist);
                        context.quicklist.Add(entry);
                        context.SaveChanges();
                        Mapper.Map(entry, quicklist);
                        return SaveResult.Inserted;
                    }
                    else
                    {
                        //existing entity
                        Mapper.Map(quicklist, dbentry);
                        context.SaveChanges();
                        quicklist = Mapper.Map<QuicklistEntryDTO>(quicklist); // does this line anything?
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

        public IEnumerable<QuicklistEntryDTO> Load(long characterId)
        {
            using (OpenNosContainer context = DataAccessHelper.CreateContext())
            {
                foreach (QuicklistEntry quicklistobject in context.quicklist.Where(i => i.CharacterId == characterId))
                {
                    yield return Mapper.Map<QuicklistEntryDTO>(quicklistobject);
                }
            }
        }

        public DeleteResult Delete(long characterId, long entryId)
        {
            using (OpenNosContainer context = DataAccessHelper.CreateContext())
            {
                QuicklistEntry quicklistitem = context.quicklist.FirstOrDefault(i => i.CharacterId == characterId && i.EntryId == entryId);
                if (quicklistitem != null)
                {
                    context.quicklist.Remove(quicklistitem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
    }
}

