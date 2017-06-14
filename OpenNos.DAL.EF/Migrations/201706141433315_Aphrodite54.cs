namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite54 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RollGeneratedItem",
                c => new
                    {
                        RollGeneratedItemId = c.Short(nullable: false, identity: true),
                        OriginalItemRare = c.Byte(nullable: false),
                        OriginalItemDesign = c.Short(nullable: false),
                        OriginalItemVNum = c.Short(nullable: false),
                        Probability = c.Short(nullable: false),
                        ItemGeneratedAmount = c.Byte(nullable: false),
                        ItemGeneratedVNum = c.Short(nullable: false),
                        IsRareRandom = c.Boolean(nullable: false),
                        MinimumOriginalItemRare = c.Byte(nullable: false),
                        MaximumOriginalItemRare = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.RollGeneratedItemId)
                .ForeignKey("dbo.Item", t => t.ItemGeneratedVNum)
                .ForeignKey("dbo.Item", t => t.OriginalItemVNum)
                .Index(t => t.OriginalItemVNum)
                .Index(t => t.ItemGeneratedVNum);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RollGeneratedItem", "OriginalItemVNum", "dbo.Item");
            DropForeignKey("dbo.RollGeneratedItem", "ItemGeneratedVNum", "dbo.Item");
            DropIndex("dbo.RollGeneratedItem", new[] { "ItemGeneratedVNum" });
            DropIndex("dbo.RollGeneratedItem", new[] { "OriginalItemVNum" });
            DropTable("dbo.RollGeneratedItem");
        }
    }
}
