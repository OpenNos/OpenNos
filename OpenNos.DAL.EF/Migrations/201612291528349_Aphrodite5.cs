namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "BlockExp", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "BlockRep", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "BlockFXp", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Character", "BlockFXp");
            DropColumn("dbo.Character", "BlockRep");
            DropColumn("dbo.Character", "BlockExp");
        }
    }
}
