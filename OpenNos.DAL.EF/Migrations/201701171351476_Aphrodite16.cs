namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite16 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.FamilyCharacter", "JoinDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FamilyCharacter", "JoinDate", c => c.DateTime(nullable: false));
        }
    }
}
