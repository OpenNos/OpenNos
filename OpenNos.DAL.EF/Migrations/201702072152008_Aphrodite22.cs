namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite22 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Item", "Length", c => c.Byte(nullable: false));
            AddColumn("dbo.Item", "Width", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Item", "Width");
            DropColumn("dbo.Item", "Length");
        }
    }
}
