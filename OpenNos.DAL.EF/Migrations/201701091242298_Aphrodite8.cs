using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite8 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.BazaarItem", "Amount");
            DropColumn("dbo.BazaarItem", "IsPackage");
            DropColumn("dbo.BazaarItem", "MedalUsed");
        }

        public override void Up()
        {
            AddColumn("dbo.BazaarItem", "MedalUsed", c => c.Boolean(nullable: false));
            AddColumn("dbo.BazaarItem", "IsPackage", c => c.Boolean(nullable: false));
            AddColumn("dbo.BazaarItem", "Amount", c => c.Byte(nullable: false));
        }

        #endregion
    }
}