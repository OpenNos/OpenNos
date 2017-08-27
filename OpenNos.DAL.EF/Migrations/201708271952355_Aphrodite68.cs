namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite68 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.MapType", "MapTypeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MapType", "MapTypeId", c => c.Short(nullable: false));
        }
    }
}
