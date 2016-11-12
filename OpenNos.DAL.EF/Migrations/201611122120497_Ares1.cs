namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ares1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NpcMonster", "NoticeRange", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NpcMonster", "NoticeRange");
        }
    }
}
