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

using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IShopDAO : IMappingBaseDAO
    {
        #region Methods

        ShopDTO Insert(ShopDTO shop);

        void Insert(List<ShopDTO> shops);

        IEnumerable<ShopDTO> LoadAll();

        ShopDTO LoadById(int shopId);

        ShopDTO LoadByNpc(int mapNpcId);

        #endregion
    }
}