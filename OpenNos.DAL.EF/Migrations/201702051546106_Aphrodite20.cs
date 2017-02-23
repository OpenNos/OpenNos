using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite20 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Character", "Backpack", c => c.Int(nullable: false));
            DropForeignKey("dbo.WarehouseItem", "AccountId", "dbo.Account");
            DropForeignKey("dbo.MinilandObject", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.WarehouseItem", "ItemInstanceId", "dbo.ItemInstance");
            DropForeignKey("dbo.WarehouseItem", "FamilyId", "dbo.Family");
            DropForeignKey("dbo.MinilandObject", "MinilandObjectVNum", "dbo.Item");
            DropIndex("dbo.WarehouseItem", new[] { "ItemInstanceId" });
            DropIndex("dbo.WarehouseItem", new[] { "FamilyId" });
            DropIndex("dbo.WarehouseItem", new[] { "AccountId" });
            DropIndex("dbo.MinilandObject", new[] { "MinilandObjectVNum" });
            DropIndex("dbo.MinilandObject", new[] { "CharacterId" });
            DropTable("dbo.WarehouseItem");
            DropTable("dbo.MinilandObject");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.MinilandObject",
                c => new
                {
                    MinilandObjectId = c.Long(nullable: false, identity: true),
                    CharacterId = c.Long(nullable: false),
                    Durability = c.Int(nullable: false),
                    Level1BoxAmount = c.Byte(nullable: false),
                    Level2BoxAmount = c.Byte(nullable: false),
                    Level3BoxAmount = c.Byte(nullable: false),
                    Level4BoxAmount = c.Byte(nullable: false),
                    Level5BoxAmount = c.Byte(nullable: false),
                    MapX = c.Short(nullable: false),
                    MapY = c.Short(nullable: false),
                    MinilandObjectVNum = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.MinilandObjectId)
                .ForeignKey("dbo.Item", t => t.MinilandObjectVNum)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId)
                .Index(t => t.MinilandObjectVNum);

            CreateTable(
                "dbo.WarehouseItem",
                c => new
                {
                    WarehouseItemId = c.Long(nullable: false, identity: true),
                    AccountId = c.Long(),
                    FamilyId = c.Long(),
                    ItemInstanceId = c.Guid(nullable: false)
                })
                .PrimaryKey(t => t.WarehouseItemId)
                .ForeignKey("dbo.Family", t => t.FamilyId)
                .ForeignKey("dbo.ItemInstance", t => t.ItemInstanceId)
                .ForeignKey("dbo.Account", t => t.AccountId)
                .Index(t => t.AccountId)
                .Index(t => t.FamilyId)
                .Index(t => t.ItemInstanceId);

            DropColumn("dbo.Character", "Backpack");
        }

        #endregion
    }
}