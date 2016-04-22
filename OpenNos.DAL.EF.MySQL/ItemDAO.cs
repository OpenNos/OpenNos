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

using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class ItemDAO : IItemDAO
    {
        #region Methods

        public void Insert(List<ItemDTO> Items)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (ItemDTO Item in Items)
                {
                    Item entity = Mapper.DynamicMap<Item>(Item);
                    context.Item.Add(entity);
                }
                context.SaveChanges();
            }
        }

        public ItemDTO Insert(ItemDTO Item)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Item entity = Mapper.DynamicMap<Item>(Item);
                context.Item.Add(entity);
                context.SaveChanges();
                return Mapper.DynamicMap<ItemDTO>(entity);
            }
        }

        public IEnumerable<ItemDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Item Item in context.Item)
                {
                    yield return Mapper.DynamicMap<ItemDTO>(Item);
                }
            }
        }

        public ItemDTO LoadById(short ItemVnum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<ItemDTO>(context.Item.FirstOrDefault(i => i.VNum.Equals(ItemVnum)));
            }
        }

        #endregion
    }
}