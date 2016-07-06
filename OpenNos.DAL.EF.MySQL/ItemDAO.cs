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
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public ItemDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Item, ItemDTO>();
                cfg.CreateMap<ItemDTO, Item>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Methods

        public void Insert(List<ItemDTO> items)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (ItemDTO Item in items)
                {
                    Item entity = _mapper.Map<Item>(Item);
                    context.Item.Add(entity);
                }
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
            }
        }

        public ItemDTO Insert(ItemDTO item)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                Item entity = _mapper.Map<Item>(item);
                context.Item.Add(entity);
                context.SaveChanges();
                return _mapper.Map<ItemDTO>(entity);
            }
        }

        public IEnumerable<ItemDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Item Item in context.Item)
                {
                    yield return _mapper.Map<ItemDTO>(Item);
                }
            }
        }

        public ItemDTO LoadById(short ItemVnum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return _mapper.Map<ItemDTO>(context.Item.FirstOrDefault(i => i.VNum.Equals(ItemVnum)));
            }
        }

        #endregion
    }
}