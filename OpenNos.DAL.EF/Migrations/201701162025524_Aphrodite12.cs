namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite12 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Family", "FamilyHeadGender", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Family", "FamilyHeadGender");
        }
    }
}
