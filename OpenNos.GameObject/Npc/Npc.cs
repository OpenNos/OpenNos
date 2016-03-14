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

namespace OpenNos.GameObject
{
    public class Npc : NpcDTO
    {
        #region Instantiation
        public short firstX
        {
            get; set;
        }
        public short firstY
        {
            get; set;
        }
        public Npc(short npcId)
        {
            Mapper.CreateMap<NpcDTO, Npc>();
            Mapper.CreateMap<Npc, NpcDTO>();
            NpcId = npcId;
            firstX = MapX;
            firstY = MapY;
            IEnumerable<TeleporterDTO> Teleporters = DAOFactory.TeleporterDAO.LoadFromNpc(NpcId);
            ShopDTO shop = DAOFactory.ShopDAO.LoadByNpc(NpcId);
            if (shop != null)
                Shop = new Shop(shop.ShopId) { Name = shop.Name, NpcId = NpcId, MenuType = shop.MenuType, ShopType = shop.ShopType };
        }

        #endregion

        #region Properties
        public IEnumerable<TeleporterDTO> Teleporters { get; set; }
        public Shop Shop { get; set; }

        #endregion

        #region Methods

        public string GetNpcDialog()
        {
            string dialog = String.Empty;

            dialog = $"npc_req 2 {NpcId} {Dialog}";

            return dialog;
        }

        public string GenerateEInfo()
        {
            return $"e_info 10 {Vnum} {Level} {Element} {AttackClass} {ElementRate} {AttackUpgrade} {DamageMinimum} {DamageMaximum} {Concentrate} {CriticalLuckRate} {CriticalRate} {DefenceUpgrade} {CloseDefence} {DefenceDodge} {DistanceDefence} {DistanceDefenceDodge} {MagicDefence} {FireResistance} {WaterResistance} {LightResistance} {DarkResistance} 0 0 -1 {Name.Replace(' ', '^')}"; // {Hp} {Mp} in 0 0 
        }

        #endregion
    }
}