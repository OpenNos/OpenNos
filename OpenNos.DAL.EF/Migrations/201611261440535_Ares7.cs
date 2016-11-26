namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ares7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MapType", "ReturnMapTypeId", c => c.Long());
            CreateIndex("dbo.MapType", "ReturnMapTypeId");
            AddForeignKey("dbo.MapType", "ReturnMapTypeId", "dbo.RespawnMapType", "RespawnMapTypeId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MapType", "ReturnMapTypeId", "dbo.RespawnMapType");
            DropIndex("dbo.MapType", new[] { "ReturnMapTypeId" });
            DropColumn("dbo.MapType", "ReturnMapTypeId");
        }
    }
}
