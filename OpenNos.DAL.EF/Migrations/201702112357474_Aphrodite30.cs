using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite30 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            CreateTable(
                "dbo.WarehouseItem",
                c => new
                {
                    WarehouseItemId = c.Long(nullable: false, identity: true),
                    AccountId = c.Long(),
                    FamilyId = c.Long(),
                    ItemInstanceId = c.Guid(nullable: false)
                })
                .PrimaryKey(t => t.WarehouseItemId);

            CreateIndex("dbo.WarehouseItem", "ItemInstanceId");
            CreateIndex("dbo.WarehouseItem", "FamilyId");
            CreateIndex("dbo.WarehouseItem", "AccountId");
            AddForeignKey("dbo.WarehouseItem", "AccountId", "dbo.Account", "AccountId");
            AddForeignKey("dbo.WarehouseItem", "ItemInstanceId", "dbo.ItemInstance", "Id");
            AddForeignKey("dbo.WarehouseItem", "FamilyId", "dbo.Family", "FamilyId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.WarehouseItem", "FamilyId", "dbo.Family");
            DropForeignKey("dbo.WarehouseItem", "ItemInstanceId", "dbo.ItemInstance");
            DropForeignKey("dbo.WarehouseItem", "AccountId", "dbo.Account");
            DropIndex("dbo.WarehouseItem", new[] { "AccountId" });
            DropIndex("dbo.WarehouseItem", new[] { "FamilyId" });
            DropIndex("dbo.WarehouseItem", new[] { "ItemInstanceId" });
            DropTable("dbo.WarehouseItem");
        }

        #endregion
    }
}