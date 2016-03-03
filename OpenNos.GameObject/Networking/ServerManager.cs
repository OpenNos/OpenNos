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

using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace OpenNos.GameObject
{
    public static class ServerManager
    {
        #region Members

        private static List<Item> _items = new List<Item>();
        private static ConcurrentDictionary<Guid, Map> _maps = new ConcurrentDictionary<Guid, Map>();

        #endregion

        #region Properties

        public static EventHandler NotifyChildren { get; set; }

        #endregion

        #region Methods

        public static ConcurrentDictionary<Guid, Map> GetAllMap()
        {
            return _maps;
        }

        public static Item GetItem(short vnum)
        {
            return _items.SingleOrDefault(m => m.VNum.Equals(vnum));
        }

        public static Map GetMap(short id)
        {
            return _maps.SingleOrDefault(m => m.Value.MapId.Equals(id)).Value;
        }

        public static void Initialize()
        {
            foreach (ItemDTO item in DAOFactory.ItemDAO.LoadAll())
            {
                _items.Add(new UsableItem
                {
                    WaterResistance = item.WaterResistance,
                    PvpDefence = item.PvpDefence,
                    Price = item.Price,
                    Name = item.Name,
                    Class = item.Class,
                    IsBlocked = item.IsBlocked,
                    IsColored = item.IsColored,
                    CriticalLuckRate = item.CriticalLuckRate,
                    Concentrate = item.Concentrate,
                    CriticalRate = item.CriticalRate,
                    DamageMaximum = item.DamageMaximum,
                    DamageMinimum = item.DamageMinimum,
                    DarkElement = item.DarkElement,
                    DarkResistance = item.DarkResistance,
                    ReduceOposantResistance = item.ReduceOposantResistance,
                    DistanceDefence = item.DistanceDefence,
                    DistanceDefenceDodge = item.DistanceDefenceDodge,
                    DefenceDodge = item.DefenceDodge,
                    IsDroppable = item.IsDroppable,
                    Element = item.Element,
                    ElementRate = item.ElementRate,
                    FireElement = item.FireElement,
                    EquipmentSlot = item.EquipmentSlot,
                    FireResistance = item.FireResistance,
                    HitRate = item.HitRate,
                    Hp = item.Hp,
                    HpRegeneration = item.HpRegeneration,
                    IsConsumable = item.IsConsumable,
                    IsWarehouse = item.IsWarehouse,
                    ItemType = item.ItemType,
                    LevelJobMinimum = item.LevelJobMinimum,
                    ReputationMinimum = item.ReputationMinimum,
                    LevelMinimum = item.LevelMinimum,
                    LightElement = item.LightElement,
                    LightResistance = item.LightResistance,
                    MagicDefence = item.MagicDefence,
                    MaxCellon = item.MaxCellon,
                    MaxCellonLvl = item.MaxCellonLvl,
                    IsMinilandObject = item.IsMinilandObject,
                    MoreHp = item.MoreHp,
                    MoreMp = item.MoreMp,
                    Morph = item.Morph,
                    Mp = item.Mp,
                    MpRegeneration = item.MpRegeneration,
                    PvpStrength = item.PvpStrength,
                    CloseDefence = item.CloseDefence,
                    IsSoldable = item.IsSoldable,
                    IsTradable = item.IsTradable,
                    Speed = item.Speed,
                    Type = item.Type,
                    VNum = item.VNum,
                    WaterElement = item.WaterElement,
                    SpType = item.SpType,
                    Color = item.Color,
                    CellonLvl = item.CellonLvl,
                    BasicUpgrade = item.BasicUpgrade,
                    Effect = item.Effect,
                    EffectValue = item.EffectValue,
                    FairyMaximumLevel = item.FairyMaximumLevel,
                    IsPearl = item.IsPearl,
                    Sex = item.Sex,
                    SecondaryElement = item.SecondaryElement,
                    MaximumAmmo = item.MaximumAmmo,
                    ItemValidTime = item.ItemValidTime,
                    ItemSubType = item.ItemSubType
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
                    _maps.TryAdd(guid, newMap);
                    i++;
                }
                if (i != 0)
                    Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MAP_LOADED"), i));
                else
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("NO_MAP"));
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
        }

        public static void MemoryWatch(string type)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            while (true)
            {
                Console.Title = $"{type} v{fileVersionInfo.ProductVersion} - Memory: {GC.GetTotalMemory(true) / (1024 * 1024)}MB";
                Thread.Sleep(1000);
            }
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
    }
}