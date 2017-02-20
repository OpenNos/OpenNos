namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite33 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Skill", "BuffId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Skill", "BuffId", c => c.Short(nullable: false));
        }
    }
}
