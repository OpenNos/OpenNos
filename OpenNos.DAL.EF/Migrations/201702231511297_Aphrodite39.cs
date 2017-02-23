namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite39 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "MaxMateCount", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Character", "MaxMateCount");
        }
    }
}
