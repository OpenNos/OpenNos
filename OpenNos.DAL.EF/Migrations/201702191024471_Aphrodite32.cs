namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite32 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Card", "FirstData", c => c.Int(nullable: false));
            AlterColumn("dbo.Card", "SecondData", c => c.Int(nullable: false));
            AlterColumn("dbo.Card", "Type", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Card", "Type", c => c.Byte(nullable: false));
            AlterColumn("dbo.Card", "SecondData", c => c.Short(nullable: false));
            AlterColumn("dbo.Card", "FirstData", c => c.Short(nullable: false));
        }
    }
}
