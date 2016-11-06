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

using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class Shop : ShopDTO
    {
        #region Instantiation

        public Shop()
        {
        }

        public override void Initialize()
        {
            ShopItems = new List<ShopItemDTO>();
            foreach (ShopItemDTO item in DAOFactory.ShopItemDAO.LoadByShopId(ShopId))
            {
                ShopItems.Add(item as ShopItemDTO);
            }
            ShopSkills = new List<ShopSkillDTO>();
            foreach (ShopSkillDTO skill in DAOFactory.ShopSkillDAO.LoadByShopId(ShopId))
            {
                ShopSkills.Add(skill as ShopSkillDTO);
            }
        }

        #endregion

        #region Properties

        public List<ShopItemDTO> ShopItems { get; set; }

        public List<ShopSkillDTO> ShopSkills { get; set; }

        #endregion

        #region Methods

        public void Save()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}