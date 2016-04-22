namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<OpenNos.DAL.EF.MySQL.DB.OpenNosContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
        }

        protected override void Seed(OpenNos.DAL.EF.MySQL.DB.OpenNosContext context)
        {

        }
    }
}
