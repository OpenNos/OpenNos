namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite48 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BCard",
                c => new
                    {
                        BCardId = c.Short(nullable: false, identity: true),
                        SubType = c.Byte(nullable: false),
                        Type = c.Byte(nullable: false),
                        FirstData = c.Int(nullable: false),
                        SecondData = c.Int(nullable: false),
                        CardId = c.Short(nullable: false),
                        Delayed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BCardId)
                .ForeignKey("dbo.Card", t => t.CardId)
                .Index(t => t.CardId);
            
            DropColumn("dbo.Card", "FirstData");
            DropColumn("dbo.Card", "SecondData");
            DropColumn("dbo.Card", "SubType");
            DropColumn("dbo.Card", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Card", "Type", c => c.Short(nullable: false));
            AddColumn("dbo.Card", "SubType", c => c.Byte(nullable: false));
            AddColumn("dbo.Card", "SecondData", c => c.Int(nullable: false));
            AddColumn("dbo.Card", "FirstData", c => c.Int(nullable: false));
            DropForeignKey("dbo.BCard", "CardId", "dbo.Card");
            DropIndex("dbo.BCard", new[] { "CardId" });
            DropTable("dbo.BCard");
        }
    }
}
