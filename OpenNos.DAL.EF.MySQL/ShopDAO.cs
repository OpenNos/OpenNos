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
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Linq;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.EF.MySQL
{
    public class ShopDAO : IShopDAO
    {
        public void Insert(List<ShopDTO> shops)
        {
            using (var context = DataAccessHelper.CreateContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                foreach (ShopDTO item in shops)
                {
                    Shop entity = Mapper.Map<Shop>(item);
                    context.shop.Add(entity);
                }
                context.SaveChanges();

            }
        }
        #region Methods

        public ShopDTO Insert(ShopDTO shop)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                if (context.shop.FirstOrDefault(c => c.MapNpcId.Equals(shop.MapNpcId)) == null)
                {
                    Shop entity = Mapper.Map<Shop>(shop);
                    context.shop.Add(entity);
                    context.SaveChanges();
                    return Mapper.Map<ShopDTO>(entity);
                }
                else return new ShopDTO();
            }
        }

        public ShopDTO LoadById(int ShopId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<ShopDTO>(context.shop.FirstOrDefault(s => s.ShopId.Equals(ShopId)));
            }
        }

        public ShopDTO LoadByNpc(int npcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<ShopDTO>(context.shop.FirstOrDefault(s => s.MapNpcId.Equals(npcId)));
            }
        }

        #endregion
    }
}