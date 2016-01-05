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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Npc : NpcDTO, IGameObject
    {
        #region Instantiation
        public List<Shop> Shops { get; set; }

        public Npc()
        {

            Mapper.CreateMap<NpcDTO, Npc>();
            Mapper.CreateMap<Npc, NpcDTO>();
            Shops = new List<Shop>();
            foreach (ShopDTO shop in DAOFactory.ShopDAO.LoadByNpc(NpcId))
                Shops.Add(new Shop() { Name = shop.Name, NpcId = NpcId, ShopId = shop.ShopId,Type=shop.Type });
        }

        #endregion

        #region Methods

        public string GetNpcDialog()
        {
            string dialog = String.Empty;
            if (false)// shop == true)
            {
                //open npcshop
            }
            else
            {
                dialog = String.Format("npc_req 2 {0} {1}", NpcId, Dialog);
            }
            return dialog;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
