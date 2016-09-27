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
using OpenNos.DAL.EF.MySQL.DB;
using System;
using System.Data.Common;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL.Helpers
{
    public static class DataAccessHelper
    {
        #region Members

        private static OpenNosContext _context;

        #endregion

        #region Instantiation

        static DataAccessHelper()
        {
        }

        #endregion

        #region Properties

        public static OpenNosContext Context
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
        public static OpenNosContext CreateContext()
        {
            return new OpenNosContext();
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
                try
                {
                    context.Database.Initialize(force: true);
                    context.Database.Connection.Open();

                    // add on line as above for each tableset(it will load the table at startup and will speedup the first query)
                    context.Account.Any();
                    context.Map.Any();
                    context.MapTypeMap.Any();
                    context.MapType.Any();
                    context.Portal.Any();
                    context.GeneralLog.Any();
                    context.PenaltyLog.Any();
                    context.Character.Any();
                    context.NpcMonster.Any();
                    context.MapNpc.Any();
                    context.MapMonster.Any();
                    context.Inventory.Any();
                    context.ItemInstance.Any();
                    context.Teleporter.Any();
                    context.Mail.Any();
                    context.Item.Any();
                    context.Respawn.Any();
                    context.Recipe.Any();
                    context.RecipeItem.Any();
                    context.QuicklistEntry.Any();
                    context.CellonOption.Any();
                    context.Shop.Any();
                    context.ShopItem.Any();
                    context.Drop.Any();
                    context.Skill.Any();
                    context.ShopSkill.Any();
                    context.CharacterSkill.Any();
                    context.NpcMonsterSkill.Any();
                    context.Combo.Any();

                    Logger.Log.Info(Language.Instance.GetMessageFromKey("DATABASE_INITIALIZED"));
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("Database Error", ex);
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("DATABASE_NOT_UPTODATE"));
                    return false;
                }
                return true;
            }
        }

        #endregion
    }
}