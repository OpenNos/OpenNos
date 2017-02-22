using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite7 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.BazaarItem", "ItemInstanceId", "dbo.ItemInstance");
            DropForeignKey("dbo.BazaarItem", "SellerId", "dbo.Character");
            DropIndex("dbo.BazaarItem", new[] { "SellerId" });
            DropIndex("dbo.BazaarItem", new[] { "ItemInstanceId" });
            DropColumn("dbo.ItemInstance", "BazaarItemId");
            DropTable("dbo.BazaarItem");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.BazaarItem",
                c => new
                {
                    BazaarItemId = c.Long(nullable: false, identity: true),
                    DateStart = c.DateTime(nullable: false),
                    Duration = c.Short(nullable: false),
                    ItemInstanceId = c.Guid(nullable: false),
                    Price = c.Long(nullable: false),
                    SellerId = c.Long(nullable: false)
                })
                .PrimaryKey(t => t.BazaarItemId)
                .ForeignKey("dbo.Character", t => t.SellerId)
                .ForeignKey("dbo.ItemInstance", t => t.ItemInstanceId)
                .Index(t => t.ItemInstanceId)
                .Index(t => t.SellerId);

            AddColumn("dbo.ItemInstance", "BazaarItemId", c => c.Long());
        }

        #endregion
    }
}