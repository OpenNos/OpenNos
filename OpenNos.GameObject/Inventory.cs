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
using OpenNos.DAL;
using OpenNos.Data;

namespace OpenNos.GameObject
{
    public class Inventory : InventoryDTO, IGameObject
    {
        #region Instantiation

        public Inventory()
        {
            Mapper.CreateMap<InventoryDTO, Inventory>();
            Mapper.CreateMap<Inventory, InventoryDTO>();
        }

        #endregion

        #region Properties

        public InventoryItem InventoryItem
        {
            get; set;
        }

        #endregion

        #region Methods

        public void Save()
        {
            InventoryDTO tempsave = this;
            this.InventoryItem.Save();
            tempsave.InventoryItemId = InventoryItem.InventoryItemId;
            SaveResult insertResult = DAOFactory.InventoryDAO.InsertOrUpdate(ref tempsave);
        }

        #endregion
    }
}