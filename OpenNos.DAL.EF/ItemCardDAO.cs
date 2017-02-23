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
    public class ItemCardDAO : MappingBaseDAO<ItemCard, ItemCardDTO>, IItemCardDAO
    {
        #region Methods

        public void Insert(List<ItemCardDTO> itemCards)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (ItemCardDTO itemCard in itemCards)
                    {
                        ItemCard entity = _mapper.Map<ItemCard>(itemCard);
                        context.ItemCard.Add(entity);
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

        public IEnumerable<ItemCardDTO> LoadAll()
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ItemCard itemCard in context.ItemCard)
                {
                    yield return _mapper.Map<ItemCardDTO>(itemCard);
                }
            }
        }

        public IEnumerable<ItemCardDTO> LoadByCardId(short cardId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ItemCard itemCard in context.ItemCard.Where(c => c.CardId.Equals(cardId)))
                {
                    yield return _mapper.Map<ItemCardDTO>(itemCard);
                }
            }
        }

        public ItemCardDTO LoadByCardIdAndItemVNum(short cardId, short itemVNum)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    return _mapper.Map<ItemCardDTO>(context.ItemCard.FirstOrDefault(i => i.CardId.Equals(cardId) && i.ItemVNum.Equals(itemVNum)));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public IEnumerable<ItemCardDTO> LoadByItemVNum(short itemVNum)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (ItemCard itemCard in context.ItemCard.Where(c => c.ItemVNum.Equals(itemVNum)))
                {
                    yield return _mapper.Map<ItemCardDTO>(itemCard);
                }
            }
        }

        #endregion
    }
}