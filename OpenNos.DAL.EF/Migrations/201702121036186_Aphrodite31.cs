using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite31 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Family", "WarehouseSize");
        }

        public override void Up()
        {
            AddColumn("dbo.Family", "WarehouseSize", c => c.Byte(nullable: false));
        }

        #endregion
    }
}