namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite31 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Family", "WarehouseSize", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Family", "WarehouseSize");
        }
    }
}
