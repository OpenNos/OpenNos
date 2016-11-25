namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Ares3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "LastLogin", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Character", "LastLogin");
        }
    }
}
