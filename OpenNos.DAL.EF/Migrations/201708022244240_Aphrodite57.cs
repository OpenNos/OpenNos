namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite57 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BCard", "IsLevelScaled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BCard", "IsLevelScaled");
        }
    }
}
