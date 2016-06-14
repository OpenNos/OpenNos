namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<OpenNos.DAL.EF.MySQL.DB.OpenNosContext>
    {
        #region Instantiation

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
        }

        #endregion

        /*private void ScriptGenerator()
        {
            var migrator = new DbMigrator(new Configuration());
            var scriptor = new MigratorScriptingDecorator(migrator);
            var sql = scriptor.ScriptUpdate("0", migrator.GetLocalMigrations().LastOrDefault());
            string fileName = "OpenNos.sql";
            File.WriteAllText(Path.Combine(@"C:\\OpenNos.sql"), sql);
        }*/

        #region Methods

        protected override void Seed(OpenNos.DAL.EF.MySQL.DB.OpenNosContext context)
        {
            IList<Account> accounts = new List<Account>();

            accounts.Add(new Account() { AccountId = 1, Authority = Domain.AuthorityType.Admin, Name = "admin", Password = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff" });
            accounts.Add(new Account() { AccountId = 2, Authority = Domain.AuthorityType.User, Name = "test", Password = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff" });

            context.Account.AddOrUpdate(a => a.AccountId, accounts.ToArray());
        }

        #endregion
    }
}