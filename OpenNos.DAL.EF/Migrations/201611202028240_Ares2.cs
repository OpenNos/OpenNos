namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ares2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NpcMonster", "CriticalChance", c => c.Byte(nullable: false));
            DropColumn("dbo.NpcMonster", "CriticalLuckRate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NpcMonster", "CriticalLuckRate", c => c.Byte(nullable: false));
            DropColumn("dbo.NpcMonster", "CriticalChance");
        }
    }
}
