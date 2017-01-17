namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite15 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.FamilyCharacter", "CharacterId");
            AddForeignKey("dbo.FamilyCharacter", "CharacterId", "dbo.Character", "CharacterId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FamilyCharacter", "CharacterId", "dbo.Character");
            DropIndex("dbo.FamilyCharacter", new[] { "CharacterId" });
        }
    }
}
