namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite67 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType");
            DropForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType");
            DropPrimaryKey("dbo.MapType");
            AddColumn("dbo.MapType", "MapTypeId2", c => c.Short(nullable: false));
            AlterColumn("dbo.MapType", "MapTypeId", c => c.Short(nullable: false));
            AddPrimaryKey("dbo.MapType", "MapTypeId2");
            AddForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType", "MapTypeId2");
            AddForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType", "MapTypeId2");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType");
            DropForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType");
            DropPrimaryKey("dbo.MapType");
            AlterColumn("dbo.MapType", "MapTypeId", c => c.Short(nullable: false, identity: true));
            DropColumn("dbo.MapType", "MapTypeId2");
            AddPrimaryKey("dbo.MapType", "MapTypeId");
            AddForeignKey("dbo.MapTypeMap", "MapTypeId", "dbo.MapType", "MapTypeId");
            AddForeignKey("dbo.Drop", "MapTypeId", "dbo.MapType", "MapTypeId");
        }
    }
}
