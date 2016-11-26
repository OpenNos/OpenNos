namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ares5 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Respawn", "RespawnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.MapType", "RespawnMapType_RespawnMapTypeId", "dbo.RespawnMapType");
            RenameColumn(table: "dbo.MapType", name: "RespawnMapType_RespawnMapTypeId", newName: "RespawnMapTypeId");
            RenameIndex(table: "dbo.MapType", name: "IX_RespawnMapType_RespawnMapTypeId", newName: "IX_RespawnMapTypeId");
            DropPrimaryKey("dbo.RespawnMapType");
            AlterColumn("dbo.RespawnMapType", "RespawnMapTypeId", c => c.Long(nullable: false));
            AddPrimaryKey("dbo.RespawnMapType", "RespawnMapTypeId");
            AddForeignKey("dbo.Respawn", "RespawnMapTypeId", "dbo.RespawnMapType", "RespawnMapTypeId");
            AddForeignKey("dbo.MapType", "RespawnMapTypeId", "dbo.RespawnMapType", "RespawnMapTypeId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MapType", "RespawnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.Respawn", "RespawnMapTypeId", "dbo.RespawnMapType");
            DropPrimaryKey("dbo.RespawnMapType");
            AlterColumn("dbo.RespawnMapType", "RespawnMapTypeId", c => c.Long(nullable: false, identity: false));
            AddPrimaryKey("dbo.RespawnMapType", "RespawnMapTypeId");
            RenameIndex(table: "dbo.MapType", name: "IX_RespawnMapTypeId", newName: "IX_RespawnMapType_RespawnMapTypeId");
            RenameColumn(table: "dbo.MapType", name: "RespawnMapTypeId", newName: "RespawnMapType_RespawnMapTypeId");
            AddForeignKey("dbo.MapType", "RespawnMapType_RespawnMapTypeId", "dbo.RespawnMapType", "RespawnMapTypeId");
            AddForeignKey("dbo.Respawn", "RespawnMapTypeId", "dbo.RespawnMapType", "RespawnMapTypeId");
        }
    }
}
