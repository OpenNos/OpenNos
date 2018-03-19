namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite70 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BCard", "IsLevelDivided", c => c.Boolean(nullable: false));
            AddColumn("dbo.RollGeneratedItem", "IsSuperReward", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RollGeneratedItem", "IsSuperReward");
            DropColumn("dbo.BCard", "IsLevelDivided");
        }
    }
}
