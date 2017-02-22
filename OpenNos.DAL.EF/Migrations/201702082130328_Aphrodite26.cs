using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite26 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Item", "MinilandObjectPoint");
        }

        public override void Up()
        {
            AddColumn("dbo.Item", "MinilandObjectPoint", c => c.Int(nullable: false));
        }

        #endregion
    }
}