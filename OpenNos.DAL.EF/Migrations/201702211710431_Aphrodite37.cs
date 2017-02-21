namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite37 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Mate", "IsTeamMember", c => c.Boolean(nullable: false));
            AddColumn("dbo.Mate", "Hp", c => c.Int(nullable: false));
            AddColumn("dbo.Mate", "Mp", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Mate", "Mp");
            DropColumn("dbo.Mate", "Hp");
            DropColumn("dbo.Mate", "IsTeamMember");
        }
    }
}
