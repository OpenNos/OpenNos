namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class XENA : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Mail", "ItemVnum", c => c.Short());
            CreateIndex("dbo.Mail", "ItemVnum");
            AddForeignKey("dbo.Mail", "ItemVnum", "dbo.Item", "VNum");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Mail", "ItemVnum", "dbo.Item");
            DropIndex("dbo.Mail", new[] { "ItemVnum" });
            DropColumn("dbo.Mail", "ItemVnum");
        }
    }
}
