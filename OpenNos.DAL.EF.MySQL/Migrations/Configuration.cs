namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<OpenNos.DAL.EF.MySQL.DB.OpenNosContext>
    {
        #region Instantiation

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
        }

        #endregion
    }
}