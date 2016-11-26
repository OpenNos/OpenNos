namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ares6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RespawnMapType", "Name", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RespawnMapType", "Name");
        }
    }
}
