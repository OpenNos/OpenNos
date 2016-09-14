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
using OpenNos.Core;
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class ShopItemDAO : IShopItemDAO
    {
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public ShopItemDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ShopItem, ShopItemDTO>();
                cfg.CreateMap<ShopItemDTO, ShopItem>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Methods

        public DeleteResult DeleteById(int itemId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    ShopItem Item = context.ShopItem.FirstOrDefault(i => i.ShopItemId.Equals(itemId));

                    if (Item != null)
                    {
                        context.ShopItem.Remove(Item);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return DeleteResult.Error;
            }
        }

        public ShopItemDTO Insert(ShopItemDTO item)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    ShopItem entity = _mapper.Map<ShopItem>(item);
                    context.ShopItem.Add(entity);
                    context.SaveChanges();
                    return _mapper.Map<ShopItemDTO>(entity);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public void Insert(List<ShopItemDTO> items)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ShopItemDTO Item in items)
                    {
                        ShopItem entity = _mapper.Map<ShopItem>(Item);
                        context.ShopItem.Add(entity);
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

        public ShopItemDTO LoadById(int itemId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<ShopItemDTO>(context.ShopItem.FirstOrDefault(i => i.ShopItemId.Equals(itemId)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ShopItemDTO> LoadByShopId(int shopId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ShopItem ShopItem in context.ShopItem.Where(i => i.ShopId.Equals(shopId)))
                {
                    yield return _mapper.Map<ShopItemDTO>(ShopItem);
                }
            }
        }

        private ShopItemDTO Update(ShopItem entity, ShopItemDTO shopItem, OpenNosContext context)
        {
            if (entity != null)
            {
                _mapper.Map(shopItem, entity);
                context.SaveChanges();
            }
            return _mapper.Map<ShopItemDTO>(entity);
        }

        #endregion
    }
}