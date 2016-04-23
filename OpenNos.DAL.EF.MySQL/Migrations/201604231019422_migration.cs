namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Item", "HeroLevelMinimum", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Item", "HeroLevelMinimum");
        }
    }
}
