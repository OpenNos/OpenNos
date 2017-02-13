namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite29 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            DropIndex("dbo.GeneralLog", new[] { "AccountId" });
            AlterColumn("dbo.GeneralLog", "AccountId", c => c.Long());
            CreateIndex("dbo.GeneralLog", "AccountId");
            AddForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account", "AccountId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            DropIndex("dbo.GeneralLog", new[] { "AccountId" });
            AlterColumn("dbo.GeneralLog", "AccountId", c => c.Long(nullable: false));
            CreateIndex("dbo.GeneralLog", "AccountId");
            AddForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account", "AccountId", cascadeDelete: true);
        }
    }
}
