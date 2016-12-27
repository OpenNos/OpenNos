namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FamilyCharacter", "CharacterId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FamilyCharacter", "CharacterId");
        }
    }
}
