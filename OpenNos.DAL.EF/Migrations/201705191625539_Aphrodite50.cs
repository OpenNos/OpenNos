namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite50 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItemCard", "CardId", "dbo.Card");
            DropForeignKey("dbo.ItemCard", "ItemVNum", "dbo.Item");
            DropIndex("dbo.BCard", new[] { "CardId" });
            DropIndex("dbo.ItemCard", new[] { "ItemVNum" });
            DropIndex("dbo.ItemCard", new[] { "CardId" });
            AddColumn("dbo.BCard", "ItemVnum", c => c.Short());
            AlterColumn("dbo.BCard", "CardId", c => c.Short());
            CreateIndex("dbo.BCard", "CardId");
            CreateIndex("dbo.BCard", "ItemVnum");
            AddForeignKey("dbo.BCard", "ItemVnum", "dbo.Item", "VNum");
            DropTable("dbo.ItemCard");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItemCard",
                c => new
                    {
                        ItemVNum = c.Short(nullable: false),
                        CardId = c.Short(nullable: false),
                        CardChance = c.Short(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItemVNum, t.CardId });
            
            DropForeignKey("dbo.BCard", "ItemVnum", "dbo.Item");
            DropIndex("dbo.BCard", new[] { "ItemVnum" });
            DropIndex("dbo.BCard", new[] { "CardId" });
            AlterColumn("dbo.BCard", "CardId", c => c.Short(nullable: false));
            DropColumn("dbo.BCard", "ItemVnum");
            CreateIndex("dbo.ItemCard", "CardId");
            CreateIndex("dbo.ItemCard", "ItemVNum");
            CreateIndex("dbo.BCard", "CardId");
            AddForeignKey("dbo.ItemCard", "ItemVNum", "dbo.Item", "VNum");
            AddForeignKey("dbo.ItemCard", "CardId", "dbo.Card", "CardId");
        }
    }
}
