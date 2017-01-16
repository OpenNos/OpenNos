namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite10 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Family", "Size");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Family", "Size", c => c.Byte(nullable: false));
        }
    }
}
