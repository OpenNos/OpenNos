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
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL.EF
{
    public class ItemDao : MappingBaseDao<Item, ItemDTO>, IItemDAO
    {
        #region Methods

        public IEnumerable<ItemDTO> FindByName(string name)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Item item in context.Item.Where(s => s.Name.Contains(name)))
                {
                    yield return Mapper.Map<ItemDTO>(item);
                }
            }
        }

        public void Insert(List<ItemDTO> items)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ItemDTO item in items)
                    {
                        Item entity = Mapper.Map<Item>(item);
                        context.Item.Add(entity);
                    }
                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public ItemDTO Insert(ItemDTO item)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    Item entity = Mapper.Map<Item>(item);
                    context.Item.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<ItemDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ItemDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Item item in context.Item)
                {
                    yield return Mapper.Map<ItemDTO>(item);
                }
            }
        }

        public ItemDTO LoadById(short itemVnum)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return Mapper.Map<ItemDTO>(context.Item.FirstOrDefault(i => i.VNum.Equals(itemVnum)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        #endregion
    }
}