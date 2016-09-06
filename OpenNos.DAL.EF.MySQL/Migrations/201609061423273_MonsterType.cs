namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MonsterType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NpcMonster", "MonsterType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NpcMonster", "MonsterType");
        }
    }
}
