namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite23 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Item", "Height", c => c.Byte(nullable: false));
            DropColumn("dbo.Item", "Length");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Item", "Length", c => c.Byte(nullable: false));
            DropColumn("dbo.Item", "Height");
        }
    }
}
