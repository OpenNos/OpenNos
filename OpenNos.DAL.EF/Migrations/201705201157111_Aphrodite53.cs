namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite53 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StaticBuff", "CardId", c => c.Short(nullable: false));
            CreateIndex("dbo.StaticBuff", "CardId");
            AddForeignKey("dbo.StaticBuff", "CardId", "dbo.Card", "CardId");
            DropColumn("dbo.StaticBuff", "EffectId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StaticBuff", "EffectId", c => c.Int(nullable: false));
            DropForeignKey("dbo.StaticBuff", "CardId", "dbo.Card");
            DropIndex("dbo.StaticBuff", new[] { "CardId" });
            DropColumn("dbo.StaticBuff", "CardId");
        }
    }
}
