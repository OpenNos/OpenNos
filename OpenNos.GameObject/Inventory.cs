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
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Inventory : InventoryDTO, IGameObject
    {
        public InventoryItem InventoryItem
        {
            get; set; }

        #region Instantiation
        

        public Inventory()
        {
            Mapper.CreateMap<InventoryDTO, Inventory>();
            Mapper.CreateMap<Inventory, InventoryDTO>();
        }


        #endregion

        #region Methods
     
        public void Save()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
