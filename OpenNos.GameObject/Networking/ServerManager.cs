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
        private static List<NpcMonster> _npcs = new List<NpcMonster>();
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
         
                foreach (ItemDTO itemDTO in DAOFactory.ItemDAO.LoadAll())
            {
                Item ItemGO = null;

                switch (itemDTO.ItemType)
                {
                    case (byte)Domain.ItemType.Ammo:
                        ItemGO = Mapper.DynamicMap<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Armor:
                        ItemGO = Mapper.DynamicMap<WearableItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Box:
                        ItemGO = Mapper.DynamicMap<BoxItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Event:
                        ItemGO = Mapper.DynamicMap<MagicalItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Fashion:
                        ItemGO = Mapper.DynamicMap<WearableItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Food:
                        ItemGO = Mapper.DynamicMap<FoodItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Jewelery:
                        ItemGO = Mapper.DynamicMap<WearableItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Magical:
                        ItemGO = Mapper.DynamicMap<MagicalItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Main:
                        ItemGO = Mapper.DynamicMap<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Map:
                        ItemGO = Mapper.DynamicMap<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Part:
                        ItemGO = Mapper.DynamicMap<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Potion:
                        ItemGO = Mapper.DynamicMap<PotionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Production:
                        ItemGO = Mapper.DynamicMap<ProduceItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Quest1:
                        ItemGO = Mapper.DynamicMap<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Quest2:
                        ItemGO = Mapper.DynamicMap<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Sell:
                        ItemGO = Mapper.DynamicMap<NoFunctionItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Shell:
                        ItemGO = Mapper.DynamicMap<MagicalItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Snack:
                        ItemGO = Mapper.DynamicMap<SnackItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Special:
                        ItemGO = Mapper.DynamicMap<SpecialItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Specialist:
                        ItemGO = Mapper.DynamicMap<WearableItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Teacher:
                        ItemGO = Mapper.DynamicMap<TeacherItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Upgrade:
                        ItemGO = Mapper.DynamicMap<UpgradeItem>(itemDTO);
                        break;

                    case (byte)Domain.ItemType.Weapon:
                        ItemGO = Mapper.DynamicMap<WearableItem>(itemDTO);
                        break;

                    default:
                        ItemGO = Mapper.DynamicMap<NoFunctionItem>(itemDTO);
                        break;
                }
                _items.Add(ItemGO);
            }

            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("ITEM_LOADED"), _items.Count()));

            foreach (NpcMonsterDTO npcmonsterDTO in DAOFactory.NpcMonsterDAO.LoadAll())
            {
                NpcMonster npcmonster = new NpcMonster()
                {
                    AttackClass = npcmonsterDTO.AttackClass,
                    DistanceDefenceDodge = npcmonsterDTO.DistanceDefenceDodge,
                    Level = npcmonsterDTO.Level,
                    LightResistance = npcmonsterDTO.LightResistance,
                    NpcMonsterVNum = npcmonsterDTO.NpcMonsterVNum,
                    AttackUpgrade = npcmonsterDTO.AttackUpgrade,
                    DistanceDefence = npcmonsterDTO.DistanceDefence,
                    WaterResistance = npcmonsterDTO.WaterResistance,
                    MagicDefence = npcmonsterDTO.MagicDefence,
                    ElementRate = npcmonsterDTO.ElementRate,
                    CloseDefence = npcmonsterDTO.CloseDefence,
                    Concentrate = npcmonsterDTO.Concentrate,
                    CriticalLuckRate = npcmonsterDTO.CriticalLuckRate,
                    CriticalRate = npcmonsterDTO.CriticalRate,
                    DamageMaximum = npcmonsterDTO.DamageMaximum,
                    DamageMinimum = npcmonsterDTO.DamageMinimum,
                    DarkResistance = npcmonsterDTO.DarkResistance,
                    DefenceDodge = npcmonsterDTO.DefenceDodge,
                    DefenceUpgrade = npcmonsterDTO.DefenceUpgrade,
                    Speed = npcmonsterDTO.Speed,
                    FireResistance = npcmonsterDTO.FireResistance,
                    Element = npcmonsterDTO.Element,
                    Name = npcmonsterDTO.Name

                };
                _npcs.Add(npcmonster);
            }
            Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("NPCSMONSTERS_LOADED"), _npcs.Count()));
            try
            {
                int i = 0;
                int npccount = 0;
                int shopcount = 0;
                foreach (MapDTO map in DAOFactory.MapDAO.LoadAll())
                {
                    Guid guid = Guid.NewGuid();
                    Map newMap = new Map(Convert.ToInt16(map.MapId), guid, map.Data);
                    newMap.Music = map.Music;
                    //register for broadcast
                    _maps.TryAdd(guid, newMap);
                    i++;
                    npccount += newMap.Npcs.Count();
                    foreach (MapNpc n in newMap.Npcs.Where(n => n.Shop != null))
                            shopcount++;
                }
                if (i != 0)
                    Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("MAP_LOADED"), i));
                else
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("NO_MAP"));

                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("NPCS_LOADED"), npccount));
                Logger.Log.Info(String.Format(Language.Instance.GetMessageFromKey("SHOPS_LOADED"), shopcount));
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
        }

        public static NpcMonster GetNpc(short npcVNum)
        {
            return _npcs.SingleOrDefault(m => m.NpcMonsterVNum.Equals(npcVNum));
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