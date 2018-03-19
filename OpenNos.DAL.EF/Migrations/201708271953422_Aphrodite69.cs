namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite69 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType");
            DropForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType");
            DropPrimaryKey("dbo.MapType");
            AddColumn("dbo.MapType", "MapTypeId", c => c.Short(nullable: false));
            AddPrimaryKey("dbo.MapType", "MapTypeId");
            AddForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType", "MapTypeId");
            AddForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType", "MapTypeId");
            DropColumn("dbo.MapType", "MapTypeId2");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MapType", "MapTypeId2", c => c.Short(nullable: false));
            DropForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType");
            DropForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType");
            DropPrimaryKey("dbo.MapType");
            DropColumn("dbo.MapType", "MapTypeId");
            AddPrimaryKey("dbo.MapType", "MapTypeId2");
            AddForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType", "MapTypeId2");
            AddForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType", "MapTypeId2");
        }
    }
}
