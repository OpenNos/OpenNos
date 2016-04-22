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
    public class ShopDAO : IShopDAO
    {
        #region Methods

        public void Insert(List<ShopDTO> Shops)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (ShopDTO Item in Shops)
                {
                    Shop entity = Mapper.DynamicMap<Shop>(Item);
                    context.Shop.Add(entity);
                }
                context.SaveChanges();
            }
        }

        public ShopDTO Insert(ShopDTO Shop)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                if (context.Shop.FirstOrDefault(c => c.MapNpcId.Equals(Shop.MapNpcId)) == null)
                {
                    Shop entity = Mapper.DynamicMap<Shop>(Shop);
                    context.Shop.Add(entity);
                    context.SaveChanges();
                    return Mapper.DynamicMap<ShopDTO>(entity);
                }
                else return new ShopDTO();
            }
        }

        public ShopDTO LoadById(int ShopId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<ShopDTO>(context.Shop.FirstOrDefault(s => s.ShopId.Equals(ShopId)));
            }
        }

        public ShopDTO LoadByNpc(int npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.DynamicMap<ShopDTO>(context.Shop.FirstOrDefault(s => s.MapNpcId.Equals(npcId)));
            }
        }

        #endregion
    }
}