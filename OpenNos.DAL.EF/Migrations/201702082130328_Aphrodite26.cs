namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite26 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Item", "MinilandObjectPoint", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Item", "MinilandObjectPoint");
        }
    }
}
