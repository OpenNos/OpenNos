namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Item", "IsHolder", c => c.Boolean(nullable: false));
            DropColumn("dbo.Item", "IsWarehouse");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Item", "IsWarehouse", c => c.Boolean(nullable: false));
            DropColumn("dbo.Item", "IsHolder");
        }
    }
}
