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
        /*private void ScriptGenerator()
        {
            var migrator = new DbMigrator(new Configuration());
            var scriptor = new MigratorScriptingDecorator(migrator);
            var sql = scriptor.ScriptUpdate("0", migrator.GetLocalMigrations().LastOrDefault());
            string fileName = "OpenNos.sql";
            File.WriteAllText(Path.Combine(@"C:\\OpenNos.sql"), sql);
        }*/

        #endregion

        #region Methods

        protected override void Seed(OpenNos.DAL.EF.MySQL.DB.OpenNosContext context)
        {
            IList<Account> accounts = new List<Account>();

            accounts.Add(new Account() { AccountId = 1, Authority = Domain.AuthorityType.Admin, Name = "admin", Password = "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08" });
            accounts.Add(new Account() { AccountId = 2, Authority = Domain.AuthorityType.User, Name = "test", Password = "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08" });

            context.Account.AddOrUpdate(a => a.AccountId, accounts.ToArray());
        }

        #endregion
    }
}