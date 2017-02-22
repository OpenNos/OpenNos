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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF
{
    public class Shop
    {
        #region Instantiation

        public Shop()
        {
            ShopItem = new HashSet<ShopItem>();
            ShopSkill = new HashSet<ShopSkill>();
        }

        #endregion

        #region Properties

        public virtual MapNpc MapNpc { get; set; }

        public int MapNpcId { get; set; }

        public byte MenuType { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public int ShopId { get; set; }

        public virtual ICollection<ShopItem> ShopItem { get; set; }

        public virtual ICollection<ShopSkill> ShopSkill { get; set; }

        public byte ShopType { get; set; }

        #endregion
    }
}