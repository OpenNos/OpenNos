namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite13 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FamilyLog", "FamilyLogType", c => c.Byte(nullable: false));
            AddColumn("dbo.FamilyLog", "FamilyLogData", c => c.String(maxLength: 255));
            AddColumn("dbo.FamilyLog", "Timestamp", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FamilyLog", "Timestamp");
            DropColumn("dbo.FamilyLog", "FamilyLogData");
            DropColumn("dbo.FamilyLog", "FamilyLogType");
        }
    }
}
