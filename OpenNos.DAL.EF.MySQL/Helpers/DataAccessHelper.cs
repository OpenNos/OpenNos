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
using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.Data;
using System;
using System.Data.Common;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL.Helpers
{
    public static class DataAccessHelper
    {
        #region Members

        private static OpenNosContainer _context;

        #endregion

        #region Instantiation

        static DataAccessHelper()
        {
            //Mapper needs to Create Maps, this is a bit not cool, but the Framework itself is nice
            Mapper.CreateMap<Account, AccountDTO>();
            Mapper.CreateMap<AccountDTO, Account>();
            Mapper.CreateMap<Character, CharacterDTO>();
            Mapper.CreateMap<CharacterDTO, Character>();
            Mapper.CreateMap<Portal, PortalDTO>();
            Mapper.CreateMap<PortalDTO, Portal>();
            Mapper.CreateMap<NpcMonster, NpcMonsterDTO>();
            Mapper.CreateMap<NpcMonsterDTO, NpcMonster>();
            Mapper.CreateMap<Map, MapDTO>();
            Mapper.CreateMap<MapDTO, Map>();
            Mapper.CreateMap<Item, ItemDTO>();
            Mapper.CreateMap<ItemDTO, Item>();
            Mapper.CreateMap<Shop, ShopDTO>();
            Mapper.CreateMap<ShopDTO, Shop>();
            Mapper.CreateMap<ShopItem, ShopItemDTO>();
            Mapper.CreateMap<ShopItemDTO, ShopItem>();
            Mapper.CreateMap<MapNpcDTO, MapNpc>();
            Mapper.CreateMap<MapNpc, MapNpcDTO>();
            Mapper.CreateMap<MapMonsterDTO, MapMonster>();
            Mapper.CreateMap<MapMonster, MapMonsterDTO>();
            Mapper.CreateMap<Inventory, InventoryDTO>();
            Mapper.CreateMap<InventoryDTO, Inventory>();
            Mapper.CreateMap<InventoryItem, InventoryItemDTO>();
            Mapper.CreateMap<InventoryItemDTO, InventoryItem>();
            Mapper.CreateMap<GeneralLog, GeneralLogDTO>();
            Mapper.CreateMap<GeneralLogDTO, GeneralLog>();
            Mapper.CreateMap<Teleporter, TeleporterDTO>();
            Mapper.CreateMap<TeleporterDTO, Teleporter>();
            Mapper.CreateMap<Recipe, RecipeDTO>();
            Mapper.CreateMap<RecipeDTO, Recipe>();
            Mapper.CreateMap<RecipeItem, RecipeItemDTO>();
            Mapper.CreateMap<RecipeItemDTO, RecipeItem>();
            Mapper.CreateMap<Drop, DropDTO>();
            Mapper.CreateMap<DropDTO, Drop>();
            Mapper.CreateMap<Skill, SkillDTO>();
            Mapper.CreateMap<SkillDTO, Skill>();
            Mapper.CreateMap<ShopSkill, ShopSkillDTO>();
            Mapper.CreateMap<ShopSkillDTO, ShopSkill>();
            Mapper.CreateMap<CharacterSkill, CharacterSkillDTO>();
            Mapper.CreateMap<CharacterSkillDTO, CharacterSkill>();
            Mapper.CreateMap<NpcMonsterSkill, NpcMonsterSkillDTO>();
            Mapper.CreateMap<NpcMonsterSkillDTO, NpcMonsterSkill>();
        }

        #endregion

        #region Properties

        public static OpenNosContainer Context
        {
            get
            {
                if (_context == null)
                {
                    _context = CreateContext();
                }
                return _context;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins and returns a new transaction. Be sure to commit/rollback/dispose this transaction
        /// or use it in an using-clause.
        /// </summary>
        /// <returns>A new transaction.</returns>
        public static DbTransaction BeginTransaction()
        {
            // an open connection is needed for a transaction
            if (Context.Database.Connection.State == System.Data.ConnectionState.Broken ||
                Context.Database.Connection.State == System.Data.ConnectionState.Closed)
            {
                Context.Database.Connection.Open();
            }

            // begin and return new transaction
            return Context.Database.Connection.BeginTransaction();
        }

        /// <summary>
        /// Creates new instance of database context.
        /// </summary>
        public static OpenNosContainer CreateContext()
        {
            return new OpenNosContainer();
        }

        /// <summary>
        /// Disposes the current instance of database context.
        /// </summary>
        public static void DisposeContext()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }

        public static bool Initialize()
        {
            using (var context = CreateContext())
            {
                context.Database.Initialize(force: true);
                try
                {
                    context.Database.Connection.Open();
                    //add on line as above for each tableset(it will load the table at startup and will speedup the first query)
                    context.account.Any();
                    context.map.Any();
                    context.portal.Any();
                    context.generallog.Any();
                    context.character.Any();
                    context.npcmonster.Any();
                    context.mapnpc.Any();
                    context.mapmonster.Any();
                    context.inventory.Any();
                    context.inventoryitem.Any();
                    context.item.Any();
                    context.respawn.Any();
                    context.shop.Any();
                    context.shopitem.Any();
                    context.drop.Any();
                    context.skill.Any();
                    context.shopskill.Any();
                    context.characterskill.Any();
                    context.npcmonsterskill.Any();
                    Logger.Log.Info(Language.Instance.GetMessageFromKey("DATABASE_INITIALIZED"));
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                    return false;
                }
                return true;
            }
        }

        #endregion
    }
}