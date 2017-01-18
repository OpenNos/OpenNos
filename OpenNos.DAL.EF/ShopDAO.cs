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

using OpenNos.Core;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class ShopDAO : MappingBaseDAO<Shop, ShopDTO>, IShopDAO
    {
        #region Methods

        public void Insert(List<ShopDTO> shops)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ShopDTO Item in shops)
                    {
                        Shop entity = _mapper.Map<Shop>(Item);
                        context.Shop.Add(entity);
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

        public ShopDTO Insert(ShopDTO shop)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    if (context.Shop.FirstOrDefault(c => c.MapNpcId.Equals(shop.MapNpcId)) == null)
                    {
                        Shop entity = _mapper.Map<Shop>(shop);
                        context.Shop.Add(entity);
                        context.SaveChanges();
                        return _mapper.Map<ShopDTO>(entity);
                    }
                    return new ShopDTO();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ShopDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (Shop entity in context.Shop)
                {
                    yield return _mapper.Map<ShopDTO>(entity);
                }
            }
        }

        public ShopDTO LoadById(int shopId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<ShopDTO>(context.Shop.FirstOrDefault(s => s.ShopId.Equals(shopId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public ShopDTO LoadByNpc(int mapNpcId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<ShopDTO>(context.Shop.FirstOrDefault(s => s.MapNpcId.Equals(mapNpcId)));
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