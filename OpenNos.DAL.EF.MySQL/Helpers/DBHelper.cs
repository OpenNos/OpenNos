using OpenNos.DAL.EF.MySQL.DB;
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
    public static class DBHelper
    {

        #region Members

        private static string _entityConnectionString;
        private static string _providerConnectionString;
        private static OpenNosContainer _context;

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
            if (DBHelper.Context.Database.Connection.State == System.Data.ConnectionState.Broken ||
                DBHelper.Context.Database.Connection.State == System.Data.ConnectionState.Closed)
            {
                DBHelper.Context.Database.Connection.Open();
            }

            // begin and return new transaction
            return DBHelper.Context.Database.Connection.BeginTransaction();
        }

        #endregion

        #endregion
    }
}
