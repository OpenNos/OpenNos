namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite14 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Character", "FamilyCharacterId", "dbo.FamilyCharacter");
            DropIndex("dbo.Character", new[] { "FamilyCharacterId" });
            DropColumn("dbo.Character", "FamilyCharacterId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Character", "FamilyCharacterId", c => c.Long());
            CreateIndex("dbo.Character", "FamilyCharacterId");
            AddForeignKey("dbo.Character", "FamilyCharacterId", "dbo.FamilyCharacter", "FamilyCharacterId");
        }
    }
}
