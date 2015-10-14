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

        //public static string ProviderConnectionString
        //{
        //    get
        //    {
        //        if (_providerConnectionString == null)
        //        {
        //            SqlConnectionStringBuilder sqlConnectionStringBuilder = null;
        //            AssemblyName callingAppName = Assembly.GetCallingAssembly().GetName();
        //            try
        //            {
        //                sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
        //                sqlConnectionStringBuilder.IntegratedSecurity = false;
        //                sqlConnectionStringBuilder.MultipleActiveResultSets = true;
        //                sqlConnectionStringBuilder.DataSource = "localhost";//TODO read from config
        //                sqlConnectionStringBuilder.InitialCatalog = "opennos"; //todo read from config
        //                sqlConnectionStringBuilder.UserID = "root";
        //                sqlConnectionStringBuilder.Password = "root";
        //                sqlConnectionStringBuilder.ApplicationName = "OpenNos";

        //                _providerConnectionString = sqlConnectionStringBuilder.ToString();
        //            }
        //            finally
        //            {
        //                if (sqlConnectionStringBuilder != null)
        //                {
        //                    sqlConnectionStringBuilder.Clear();
        //                    sqlConnectionStringBuilder = null;
        //                }
        //            }
        //        }

        //        return _providerConnectionString;
        //    }
        //}

        //#endregion

        //#region Private

        //private static string EntityConnectionString
        //{
        //    get
        //    {
        //        if (_entityConnectionString == null)
        //        {
        //            EntityConnectionStringBuilder entityConnectionStringBuilder = null;
        //            try
        //            {
        //                entityConnectionStringBuilder = new EntityConnectionStringBuilder();
        //                entityConnectionStringBuilder.Provider = "System.Data.MySQLClient";
        //                entityConnectionStringBuilder.ProviderConnectionString = ProviderConnectionString;
        //                entityConnectionStringBuilder.Metadata = @"res://*/opennos.csdl|res://*/opennos.ssdl|res://*/opennos.msl";

        //                _entityConnectionString = entityConnectionStringBuilder.ToString();
        //            }
        //            finally
        //            {
        //                if (entityConnectionStringBuilder != null)
        //                {
        //                    entityConnectionStringBuilder.Clear();
        //                    entityConnectionStringBuilder = null;
        //                }
        //            }
        //        }

        //        return _entityConnectionString;
        //    }
        //}

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
