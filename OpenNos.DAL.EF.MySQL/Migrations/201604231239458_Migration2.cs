namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migration2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Item", "IsHeroic", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Item", "IsHeroic");
        }
    }
}
