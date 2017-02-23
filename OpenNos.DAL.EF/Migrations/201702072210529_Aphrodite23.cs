using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite23 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Item", "Length", c => c.Byte(nullable: false));
            DropColumn("dbo.Item", "Height");
        }

        public override void Up()
        {
            AddColumn("dbo.Item", "Height", c => c.Byte(nullable: false));
            DropColumn("dbo.Item", "Length");
        }

        #endregion
    }
}