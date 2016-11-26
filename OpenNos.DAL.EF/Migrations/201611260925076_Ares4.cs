namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ares4 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RespawnMapType",
                c => new
                    {
                        RespawnMapTypeId = c.Long(nullable: false, identity: true),
                        DefaultMapId = c.Short(nullable: false),
                        DefaultX = c.Short(nullable: false),
                        DefaultY = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.RespawnMapTypeId)
                .ForeignKey("dbo.Map", t => t.DefaultMapId)
                .Index(t => t.DefaultMapId);
            
            AddColumn("dbo.MapType", "RespawnMapType_RespawnMapTypeId", c => c.Long());
            AddColumn("dbo.Respawn", "RespawnMapTypeId", c => c.Long(nullable: false));
            CreateIndex("dbo.MapType", "RespawnMapType_RespawnMapTypeId");
            CreateIndex("dbo.Respawn", "MapId");
            CreateIndex("dbo.Respawn", "RespawnMapTypeId");
            AddForeignKey("dbo.Respawn", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Respawn", "RespawnMapTypeId", "dbo.RespawnMapType", "RespawnMapTypeId");
            AddForeignKey("dbo.MapType", "RespawnMapType_RespawnMapTypeId", "dbo.RespawnMapType", "RespawnMapTypeId");
            DropColumn("dbo.Respawn", "RespawnType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Respawn", "RespawnType", c => c.Byte(nullable: false));
            DropForeignKey("dbo.MapType", "RespawnMapType_RespawnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.Respawn", "RespawnMapTypeId", "dbo.RespawnMapType");
            DropForeignKey("dbo.Respawn", "MapId", "dbo.Map");
            DropForeignKey("dbo.RespawnMapType", "DefaultMapId", "dbo.Map");
            DropIndex("dbo.Respawn", new[] { "RespawnMapTypeId" });
            DropIndex("dbo.Respawn", new[] { "MapId" });
            DropIndex("dbo.RespawnMapType", new[] { "DefaultMapId" });
            DropIndex("dbo.MapType", new[] { "RespawnMapType_RespawnMapTypeId" });
            DropColumn("dbo.Respawn", "RespawnMapTypeId");
            DropColumn("dbo.MapType", "RespawnMapType_RespawnMapTypeId");
            DropTable("dbo.RespawnMapType");
        }
    }
}
