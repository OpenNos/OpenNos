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
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.Interface;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using AutoMapper;
using OpenNos.Core;

namespace OpenNos.DAL.EF.MySQL
{
    public class ShopDAO : IShopDAO
    {
        public ShopDTO LoadById(int ShopId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map < ShopDTO > (context.shop.FirstOrDefault(s => s.ShopId.Equals(ShopId)));
         }
        }

        public IEnumerable<ShopDTO> LoadByNpc(short NpcId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Shop shopitem in context.shop.Where(i => i.NpcId.Equals(NpcId)))
                {
                    yield return Mapper.Map<ShopDTO>(shopitem);
                }
            }
        }
    }
}
