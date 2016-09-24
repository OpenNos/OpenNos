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
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MapNpc : MapNpcDTO
    {
        #region Instantiation

        private int _movetime;
        public MapNpc(MapNpcDTO npc, Map parent)
        {
            //Replace by MAPPING
            MapId = npc.MapId;
            MapX = npc.MapX;
            MapY = npc.MapY;
            Position = npc.Position;
            NpcVNum = npc.NpcVNum;
            IsSitting = npc.IsSitting;
            IsMoving = npc.IsMoving;
            Effect = npc.Effect;
            EffectDelay = npc.EffectDelay;
            Dialog = npc.Dialog;
            FirstX = npc.MapX;
            FirstY = npc.MapY;
            MapNpcId = npc.MapNpcId;
           ////////////////////////

            Npc = ServerManager.GetNpc(this.NpcVNum);
            LastEffect = LastMove = DateTime.Now;
            Map = parent;
            _movetime = new Random().Next(300, 3000);
            IEnumerable<RecipeDTO> recipe = DAOFactory.RecipeDAO.LoadByNpc(MapNpcId);
            if (recipe != null)
            {
                Recipes = new List<Recipe>();
                foreach (RecipeDTO rec in recipe)
                {

                    //Replace by MAPPING
                    Recipes.Add(new Recipe(rec.RecipeId) { ItemVNum = rec.ItemVNum, MapNpcId = rec.MapNpcId, RecipeId = rec.RecipeId, Amount = rec.Amount });
                    ///////////////////
                }
            }

            IEnumerable<TeleporterDTO> teleporters = DAOFactory.TeleporterDAO.LoadFromNpc(MapNpcId);
            if (teleporters != null)
            {
                Teleporters = new List<Teleporter>();
                foreach (TeleporterDTO teleporter in teleporters)
                {
                    //Replace by MAPPING
                    Teleporters.Add(new Teleporter() { MapId = teleporter.MapId, Index = teleporter.Index, MapNpcId = teleporter.MapNpcId, MapX = teleporter.MapX, MapY = teleporter.MapY, TeleporterId = teleporter.TeleporterId });
                    ///////////////////
                }
            }

            ShopDTO shop = DAOFactory.ShopDAO.LoadByNpc(MapNpcId);
            if (shop != null)
            {
                //Replace by MAPPING
                Shop = new Shop(shop.ShopId) { Name = shop.Name, MapNpcId = MapNpcId, MenuType = shop.MenuType, ShopType = shop.ShopType };
                ///////////////////
            }


        }

        #endregion

        #region Properties

        public short FirstX { get; set; }
        public short FirstY { get; set; }
        public DateTime LastEffect { get; private set; }
        public DateTime LastMove { get; private set; }
        public Map Map { get; set; }
        public List<Recipe> Recipes { get; set; }
        public Shop Shop { get; set; }
        public List<Teleporter> Teleporters { get; set; }
        public NpcMonster Npc;


        #endregion

        #region Methods

        public string GenerateEff()
        {
            NpcMonster npc = ServerManager.GetNpc(this.NpcVNum);
            if (npc != null)
                return $"eff 2 {MapNpcId} {Effect}";
            else return String.Empty;
        }

        public string GenerateIn2()
        {
            NpcMonster npcinfo = ServerManager.GetNpc(this.NpcVNum);
            if (npcinfo != null && !IsDisabled)
                return $"in 2 {NpcVNum} {MapNpcId} {MapX} {MapY} {Position} 100 100 {Dialog} 0 0 -1 1 {(IsSitting ? 1 : 0)} -1 - 0 -1 0 0 0 0 0 0 0 0";
            else return String.Empty;
        }

        public string GetNpcDialog()
        {
            return $"npc_req 2 {MapNpcId} {Dialog}";
        }

        internal void NpcLife()
        {
            double time = (DateTime.Now - LastEffect).TotalMilliseconds;
            if (Effect > 0 && time > EffectDelay)
            {
                Map.Broadcast(GenerateEff());
                LastEffect = DateTime.Now;
            }

            Random random = new Random((int)DateTime.Now.Ticks * MapNpcId & 0x0000FFFF);
            time = (DateTime.Now - LastMove).TotalMilliseconds;
            if (IsMoving && Npc.Speed > 0 && time > _movetime)
            {

                _movetime = random.Next(500, 3000);
                byte point = (byte)random.Next(2, 4);
                byte fpoint = (byte)random.Next(0, 2);

                byte xpoint = (byte)random.Next(fpoint, point);
                byte ypoint = (byte)(point - xpoint);

                short mapX = FirstX;
                short mapY = FirstY;

                if (ServerManager.GetMap(MapId).GetFreePosition(ref mapX, ref mapY, xpoint, ypoint))
                {
                    Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(1000 * (xpoint + ypoint) / (2 * Npc.Speed));
                        this.MapX = mapX;
                        this.MapY = mapY;
                    });
                    LastMove = DateTime.Now.AddSeconds((xpoint + ypoint) / (2 * Npc.Speed));

                    string movePacket = $"mv 2 {this.MapNpcId} {this.MapX} {this.MapY} {Npc.Speed}";
                    Map.Broadcast(movePacket);
                }

               
            }
        }

        #endregion
    }
}