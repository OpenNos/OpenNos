namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Hestia : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Character", "MapId", "dbo.Map");
            DropForeignKey("dbo.MapMonster", "MapId", "dbo.Map");
            DropForeignKey("dbo.MapNpc", "MapId", "dbo.Map");
            DropForeignKey("dbo.Portal", "DestinationMapId", "dbo.Map");
            DropForeignKey("dbo.Portal", "SourceMapId", "dbo.Map");
            DropForeignKey("dbo.Respawn", "MapId", "dbo.Map");
            DropForeignKey("dbo.RespawnMapType", "DefaultMapId", "dbo.Map");
            DropForeignKey("dbo.ScriptedInstance", "MapId", "dbo.Map");
            DropForeignKey("dbo.Teleporter", "MapId", "dbo.Map");
            DropForeignKey("dbo.MapTypeMap", "MapId", "dbo.Map");
            DropIndex("dbo.Character", new[] { "MapId" });
            DropIndex("dbo.MapTypeMap", new[] { "MapId" });
            DropIndex("dbo.MapMonster", new[] { "MapId" });
            DropIndex("dbo.MapNpc", new[] { "MapId" });
            DropIndex("dbo.Teleporter", new[] { "MapId" });
            DropIndex("dbo.Portal", new[] { "DestinationMapId" });
            DropIndex("dbo.Portal", new[] { "SourceMapId" });
            DropIndex("dbo.Respawn", new[] { "MapId" });
            DropIndex("dbo.RespawnMapType", new[] { "DefaultMapId" });
            DropIndex("dbo.ScriptedInstance", new[] { "MapId" });
            DropPrimaryKey("dbo.MapTypeMap");
            DropPrimaryKey("dbo.Map");
            AlterColumn("dbo.Character", "MapId", c => c.Int(nullable: false));
            AlterColumn("dbo.MapTypeMap", "MapId", c => c.Int(nullable: false));
            AlterColumn("dbo.Map", "MapId", c => c.Int(nullable: false));
            AlterColumn("dbo.MapMonster", "MapId", c => c.Int(nullable: false));
            AlterColumn("dbo.MapNpc", "MapId", c => c.Int(nullable: false));
            AlterColumn("dbo.Teleporter", "MapId", c => c.Int(nullable: false));
            AlterColumn("dbo.Portal", "DestinationMapId", c => c.Int(nullable: false));
            AlterColumn("dbo.Portal", "SourceMapId", c => c.Int(nullable: false));
            AlterColumn("dbo.Respawn", "MapId", c => c.Int(nullable: false));
            AlterColumn("dbo.RespawnMapType", "DefaultMapId", c => c.Int(nullable: false));
            AlterColumn("dbo.ScriptedInstance", "MapId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.MapTypeMap", new[] { "MapId", "MapTypeId" });
            AddPrimaryKey("dbo.Map", "MapId");
            CreateIndex("dbo.Character", "MapId");
            CreateIndex("dbo.MapTypeMap", "MapId");
            CreateIndex("dbo.MapMonster", "MapId");
            CreateIndex("dbo.MapNpc", "MapId");
            CreateIndex("dbo.Teleporter", "MapId");
            CreateIndex("dbo.Portal", "DestinationMapId");
            CreateIndex("dbo.Portal", "SourceMapId");
            CreateIndex("dbo.Respawn", "MapId");
            CreateIndex("dbo.RespawnMapType", "DefaultMapId");
            CreateIndex("dbo.ScriptedInstance", "MapId");
            AddForeignKey("dbo.Character", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.MapMonster", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.MapNpc", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Portal", "DestinationMapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Portal", "SourceMapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Respawn", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.RespawnMapType", "DefaultMapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.ScriptedInstance", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Teleporter", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.MapTypeMap", "MapId", "dbo.Map", "MapId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MapTypeMap", "MapId", "dbo.Map");
            DropForeignKey("dbo.Teleporter", "MapId", "dbo.Map");
            DropForeignKey("dbo.ScriptedInstance", "MapId", "dbo.Map");
            DropForeignKey("dbo.RespawnMapType", "DefaultMapId", "dbo.Map");
            DropForeignKey("dbo.Respawn", "MapId", "dbo.Map");
            DropForeignKey("dbo.Portal", "SourceMapId", "dbo.Map");
            DropForeignKey("dbo.Portal", "DestinationMapId", "dbo.Map");
            DropForeignKey("dbo.MapNpc", "MapId", "dbo.Map");
            DropForeignKey("dbo.MapMonster", "MapId", "dbo.Map");
            DropForeignKey("dbo.Character", "MapId", "dbo.Map");
            DropIndex("dbo.ScriptedInstance", new[] { "MapId" });
            DropIndex("dbo.RespawnMapType", new[] { "DefaultMapId" });
            DropIndex("dbo.Respawn", new[] { "MapId" });
            DropIndex("dbo.Portal", new[] { "SourceMapId" });
            DropIndex("dbo.Portal", new[] { "DestinationMapId" });
            DropIndex("dbo.Teleporter", new[] { "MapId" });
            DropIndex("dbo.MapNpc", new[] { "MapId" });
            DropIndex("dbo.MapMonster", new[] { "MapId" });
            DropIndex("dbo.MapTypeMap", new[] { "MapId" });
            DropIndex("dbo.Character", new[] { "MapId" });
            DropPrimaryKey("dbo.Map");
            DropPrimaryKey("dbo.MapTypeMap");
            AlterColumn("dbo.ScriptedInstance", "MapId", c => c.Short(nullable: false));
            AlterColumn("dbo.RespawnMapType", "DefaultMapId", c => c.Short(nullable: false));
            AlterColumn("dbo.Respawn", "MapId", c => c.Short(nullable: false));
            AlterColumn("dbo.Portal", "SourceMapId", c => c.Short(nullable: false));
            AlterColumn("dbo.Portal", "DestinationMapId", c => c.Short(nullable: false));
            AlterColumn("dbo.Teleporter", "MapId", c => c.Short(nullable: false));
            AlterColumn("dbo.MapNpc", "MapId", c => c.Short(nullable: false));
            AlterColumn("dbo.MapMonster", "MapId", c => c.Short(nullable: false));
            AlterColumn("dbo.Map", "MapId", c => c.Short(nullable: false));
            AlterColumn("dbo.MapTypeMap", "MapId", c => c.Short(nullable: false));
            AlterColumn("dbo.Character", "MapId", c => c.Short(nullable: false));
            AddPrimaryKey("dbo.Map", "MapId");
            AddPrimaryKey("dbo.MapTypeMap", new[] { "MapId", "MapTypeId" });
            CreateIndex("dbo.ScriptedInstance", "MapId");
            CreateIndex("dbo.RespawnMapType", "DefaultMapId");
            CreateIndex("dbo.Respawn", "MapId");
            CreateIndex("dbo.Portal", "SourceMapId");
            CreateIndex("dbo.Portal", "DestinationMapId");
            CreateIndex("dbo.Teleporter", "MapId");
            CreateIndex("dbo.MapNpc", "MapId");
            CreateIndex("dbo.MapMonster", "MapId");
            CreateIndex("dbo.MapTypeMap", "MapId");
            CreateIndex("dbo.Character", "MapId");
            AddForeignKey("dbo.MapTypeMap", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Teleporter", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.ScriptedInstance", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.RespawnMapType", "DefaultMapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Respawn", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Portal", "SourceMapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Portal", "DestinationMapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.MapNpc", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.MapMonster", "MapId", "dbo.Map", "MapId");
            AddForeignKey("dbo.Character", "MapId", "dbo.Map", "MapId");
        }
    }
}
