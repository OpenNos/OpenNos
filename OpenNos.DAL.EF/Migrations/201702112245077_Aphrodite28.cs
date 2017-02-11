namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite28 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            AddForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account", "AccountId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            AddForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account", "AccountId");
        }
    }
}
