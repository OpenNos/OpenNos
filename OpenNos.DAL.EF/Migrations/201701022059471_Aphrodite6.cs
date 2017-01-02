namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite6 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Character", "BlockExp");
            DropColumn("dbo.Character", "BlockRep");
            DropColumn("dbo.Character", "BlockFXp");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Character", "BlockFXp", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "BlockRep", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "BlockExp", c => c.Boolean(nullable: false));
        }
    }
}
