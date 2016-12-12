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

using OpenNos.DAL.EF.Migrations;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using System.Linq;

namespace OpenNos.DAL.EF.Helpers
{
    public static class MigrationHelper
    {
        #region Methods

        public static void GenerateSQLScript()
        {
#if DEBUG
            var migrator = new DbMigrator(new Configuration());
            var scriptor = new MigratorScriptingDecorator(migrator);
            var migration = migrator.GetLocalMigrations().LastOrDefault();
            var sql = scriptor.ScriptUpdate("0", migration);
            string info = $"-- ========================================== --\r\n-- Current Migration: {migration}\r\n-- ========================================== --\r\n\r\n";
            const string fileName = "OpenNos.sql";
            File.WriteAllText(Path.Combine(@"../../../OpenNos.DAL.EF/DB/", fileName), info + sql);
#endif
        }

        #endregion
    }
}