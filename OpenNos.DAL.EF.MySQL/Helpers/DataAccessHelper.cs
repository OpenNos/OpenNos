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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace OpenNos.DAL.EF.MySQL
{
    public static class DataAccessHelper
    {

        #region Members

        private static OpenNosContainer _context;

        #endregion

        #region Instantiation

        static DataAccessHelper()
        {
            //Mapper needs to Create Maps, this is a bit uncool, but the Framework itself is nice
            Mapper.CreateMap<Account, AccountDTO>();
            Mapper.CreateMap<AccountDTO, Account>();
            Mapper.CreateMap<Character, CharacterDTO>();
            Mapper.CreateMap<CharacterDTO, Character>();
            Mapper.CreateMap<Portal, PortalDTO>();
            Mapper.CreateMap<PortalDTO, Portal>();
            Mapper.CreateMap<Map, MapDTO>();
            Mapper.CreateMap<MapDTO, Map>();
        }

        #endregion

        #region Properties

        #region Public

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

        public static bool Initialize()
        {
            using (var context = CreateContext())
            {
                context.Database.Initialize(force: true);
                try
                {
                    context.Database.Connection.Open();
                    /*add on line as above for each tableset
                   (it will load the table at startup and will speedup the first query)
                   */
                    context.account.Any();
                    context.map.Any();
                    context.portal.Any();
                    context.connectionlog.Any();
                    context.character.Any();
                   
                    Logger.Log.Info(Language.Instance.GetMessageFromKey("DATABASE_HAS_BEEN_INITIALISE"));
                }
                catch(Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                    return false;
                }
                return true;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public

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

        /// <summary>
        /// Begins and returns a new transaction. Be sure to commit/rollback/dispose this transaction
        /// or use it in an using-clause.
        /// </summary>
        /// <returns>A new transaction.</returns>
        public static DbTransaction BeginTransaction()
        {
            // an open connection is needed for a transaction
            if (DataAccessHelper.Context.Database.Connection.State == System.Data.ConnectionState.Broken ||
                DataAccessHelper.Context.Database.Connection.State == System.Data.ConnectionState.Closed)
            {
                DataAccessHelper.Context.Database.Connection.Open();
            }

            // begin and return new transaction
            return DataAccessHelper.Context.Database.Connection.BeginTransaction();
        }

        #endregion

        #endregion
    }
}
