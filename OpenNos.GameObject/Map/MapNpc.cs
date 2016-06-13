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
    public class MapNpc : MapNpcDTO
    {
        #region Public Instantiation

        public MapNpc(int npcId, Map parent)
        {
            LifeTaskIsRunning = false;
            MapNpcId = npcId;
            LastEffect = LastMove = DateTime.Now;
            IEnumerable<RecipeDTO> Recipe = DAOFactory.RecipeDAO.LoadByNpc(MapNpcId);
            Recipes = new List<Recipe>();
            foreach (RecipeDTO rec in Recipe)
            {
                Recipes.Add(new GameObject.Recipe(rec.RecipeId) { ItemVNum = rec.ItemVNum, MapNpcId = rec.MapNpcId, RecipeId = rec.RecipeId, Amount = rec.Amount });
            }

            IEnumerable<TeleporterDTO> Teleporter = DAOFactory.TeleporterDAO.LoadFromNpc(MapNpcId);
            Teleporters = new List<Teleporter>();
            foreach (TeleporterDTO telep in Teleporter)
            {
                Teleporters.Add(new GameObject.Teleporter() { MapId = telep.MapId, Index = telep.Index, MapNpcId = telep.MapNpcId, MapX = telep.MapX, MapY = telep.MapY, TeleporterId = telep.TeleporterId });
            }
            ShopDTO shop = DAOFactory.ShopDAO.LoadByNpc(MapNpcId);
            if (shop != null)
                Shop = new Shop(shop.ShopId) { Name = shop.Name, MapNpcId = MapNpcId, MenuType = shop.MenuType, ShopType = shop.ShopType };

            Map = parent;
        }

        #endregion

        #region Public Properties

        public short FirstX { get; set; }
        public short FirstY { get; set; }
        public DateTime LastEffect { get; private set; }
        public DateTime LastMove { get; private set; }
        public bool LifeTaskIsRunning { get; internal set; }
        public Map Map { get; set; }
        public List<Recipe> Recipes { get; set; }
        public Shop Shop { get; set; }
        public List<Teleporter> Teleporters { get; set; }

        #endregion

        #region Public Methods

        public string GenerateEff()
        {
            NpcMonster npc = ServerManager.GetNpc(this.NpcVNum);
            if (npc != null)
                return $"eff 2 {MapNpcId} {Effect}";
            else
                return "";
        }

        public string GetNpcDialog()
        {
            return $"npc_req 2 {MapNpcId} {Dialog}";
        }

        #endregion

        #region Internal Methods

        internal void NpcLife()
        {
            LifeTaskIsRunning = true;
            NpcMonster npc = ServerManager.GetNpc(this.NpcVNum);
            if (npc == null)
            {
                LifeTaskIsRunning = false;
                return;
            }
            double time = (DateTime.Now - LastEffect).TotalMilliseconds;
            if (Effect > 0 && time > EffectDelay)
            {
                Map.Broadcast(GenerateEff());
                LastEffect = DateTime.Now;
            }

            Random r = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            time = (DateTime.Now - LastMove).TotalSeconds;
            if (IsMoving && time > r.Next(1, 3) * (0.5 + r.NextDouble()))
            {
                byte point = (byte)r.Next(2, 5);
                byte fpoint = (byte)r.Next(0, 2);

                byte xpoint = (byte)r.Next(fpoint, point);
                byte ypoint = (byte)(point - xpoint);

                short mapX = FirstX;
                short mapY = FirstY;
                if (ServerManager.GetMap(MapId).GetFreePosition(ref mapX, ref mapY, xpoint, ypoint))
                {
                    this.MapX = mapX;
                    this.MapY = mapY;
                    LastMove = DateTime.Now;

                    string movepacket = $"mv 2 {this.MapNpcId} {this.MapX} {this.MapY} {npc.Speed}";
                    Map.Broadcast(movepacket);
                }
            }
            LifeTaskIsRunning = false;
        }

        #endregion
    }
}