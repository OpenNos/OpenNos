namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration3 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Item", "HeroLevelMinimum");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Item", "HeroLevelMinimum", c => c.Byte(nullable: false));
        }
    }
}
