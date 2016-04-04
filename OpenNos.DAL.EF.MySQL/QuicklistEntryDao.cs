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

namespace OpenNos.DAL.EF.MySQL
{
    public class QuicklistEntryDAO : IQuicklistEntryDAO
    {
        public QuicklistEntryDTO Insert(ref QuicklistEntryDTO characterquicklist)
        {
            using (OpenNosContainer context = DataAccessHelper.CreateContext())
            {
                CharacterQuicklist entity = Mapper.Map<CharacterQuicklist>(characterquicklist);
                context.characterquicklist.Add(entity);
                context.SaveChanges();
                return Mapper.Map<QuicklistEntryDTO>(entity);
            }
        }

        public IEnumerable<QuicklistEntryDTO> Load(long characterId)
        {
            using (OpenNosContainer context = DataAccessHelper.CreateContext())
            {
                foreach (CharacterQuicklist quicklistobject in context.characterquicklist.Where(i => i.CharacterId == characterId))
                {
                    yield return Mapper.Map<QuicklistEntryDTO>(quicklistobject);
                }
            }
        }

        public DeleteResult Delete(long characterId, long entryId)
        {
            using (OpenNosContainer context = DataAccessHelper.CreateContext())
            {
                CharacterQuicklist quicklistitem = context.characterquicklist.FirstOrDefault(i => i.CharacterId == characterId && i.EntryId == entryId);
                if (quicklistitem != null)
                {
                    context.characterquicklist.Remove(quicklistitem);
                    context.SaveChanges();
                }

                return DeleteResult.Deleted;
            }
        }
    }
}
