using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite22 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Item", "Width");
            DropColumn("dbo.Item", "Length");
        }

        public override void Up()
        {
            AddColumn("dbo.Item", "Length", c => c.Byte(nullable: false));
            AddColumn("dbo.Item", "Width", c => c.Byte(nullable: false));
        }

        #endregion
    }
}