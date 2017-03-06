namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite41 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NpcMonster", "HeroXP", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NpcMonster", "HeroXP");
        }
    }
}
