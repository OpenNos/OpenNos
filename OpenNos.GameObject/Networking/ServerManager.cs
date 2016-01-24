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
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public static class ServerManager
    {
        #region Members

        private static ConcurrentDictionary<Guid, Map> _maps = new ConcurrentDictionary<Guid, Map>();
        private static List<Item> _items = new List<Item>();
        #endregion

        #region Event Handlers

        #endregion

        #region Methods

        public static void Initialize()
        {
            foreach (ItemDTO item in DAOFactory.ItemDAO.LoadAll())
            {
                _items.Add(new Item
                {
                    WaterResistance = item.WaterResistance,
                    PvpDefence = item.PvpDefence,
                    Price = item.Price,
                    Name = item.Name,
                    Classe = item.Classe,
                    Blocked = item.Blocked,
                    Colored = item.Colored,
                    CriticalLuckRate = item.CriticalLuckRate,
                    Concentrate = item.Concentrate,
                    CriticalRate = item.CriticalRate,
                    DamageMaximum = item.DamageMaximum,
                    DamageMinimum = item.DamageMinimum,
                    DarkElement = item.DarkElement,
                    DarkResistance = item.DarkResistance,
                    ReduceOposantResistance = item.ReduceOposantResistance,
                    DistanceDefence = item.DistanceDefence,
                    Dodge = item.Dodge,
                    Droppable = item.Droppable,
                    Element = item.Element,
                    ElementRate = item.ElementRate,
                    FireElement = item.FireElement,
                    EquipmentSlot = item.EquipmentSlot,
                    FireResistance = item.FireResistance,
                    HitRate = item.HitRate,
                    Hp = item.Hp,
                    HpRegeneration = item.HpRegeneration,
                    isConsumable = item.isConsumable,
                    isWareHouse = item.isWareHouse,
                    ItemType = item.ItemType,
                    LevelJobMinimum = item.LevelJobMinimum,
                    ReputationMinimum = item.ReputationMinimum,
                    LevelMinimum = item.LevelMinimum,
                    LightElement = item.LightElement,
                    LightResistance = item.LightResistance,
                    MagicDefence = item.MagicDefence,
                    MaxCellon = item.MaxCellon,
                    MaxCellonLvl = item.MaxCellonLvl,
                    MinilandObject = item.MinilandObject,
                    MoreHp = item.MoreHp,
                    MoreMp = item.MoreMp,
                    Morph = item.Morph,
                    Mp = item.Mp,
                    MpRegeneration = item.MpRegeneration,
                    PvpStrength = item.PvpStrength,
                    RangeDefence = item.RangeDefence,
                    Soldable = item.Soldable,
                    Transaction = item.Transaction,
                    Speed = item.Speed,
                    Type = item.Type,
                    VNum = item.VNum,
                    WaterElement = item.WaterElement
                });
            }

            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("ITEM_LOADED"), _items.Count()));

            try
            {
                int i = 0;
                foreach (MapDTO map in DAOFactory.MapDAO.LoadAll())
                {
                    Guid guid = Guid.NewGuid();
                    Map newMap = new Map(Convert.ToInt16(map.MapId), guid, map.Data);
                    newMap.Music = map.Music;
                    //register for broadcast
                    NotifyChildren += newMap.GetNotification;
                    _maps.TryAdd(guid, newMap);
                    i++;
                }
                if(i!=0)
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MAP_LOADED"), i));
                else
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("NO_MAP"));
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }

        }
        public static Item GetItem(short vnum)
        {
            return _items.SingleOrDefault(m => m.VNum.Equals(vnum));
        }
        public static Map GetMap(short id)
        {
            return _maps.SingleOrDefault(m => m.Value.MapId.Equals(id)).Value;
        }
        public static ConcurrentDictionary<Guid, Map> GetAllMap()
        {
            return _maps;
        }

        public static void OnBroadCast(MapPacket mapPacket)
        {
            var handler = NotifyChildren;
            if (handler != null)
            {
                handler(mapPacket, new EventArgs());
            }
        }

        #endregion

        public static EventHandler NotifyChildren { get; set; }
    }
}
