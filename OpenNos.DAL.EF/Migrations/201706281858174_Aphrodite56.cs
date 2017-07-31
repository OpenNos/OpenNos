namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite56 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BCard", "CastType", c => c.Byte(nullable: false));
            AddColumn("dbo.BCard", "ThirdData", c => c.Int(nullable: false));
            DropColumn("dbo.BCard", "IsDelayed");
            DropColumn("dbo.BCard", "Delay");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BCard", "Delay", c => c.Short(nullable: false));
            AddColumn("dbo.BCard", "IsDelayed", c => c.Boolean(nullable: false));
            DropColumn("dbo.BCard", "ThirdData");
            DropColumn("dbo.BCard", "CastType");
        }
    }
}
