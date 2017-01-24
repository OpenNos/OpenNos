namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite18 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CharacterRelation", "CharacterId", "dbo.Character");
            CreateIndex("dbo.CharacterRelation", "RelatedCharacterId");
            AddForeignKey("dbo.CharacterRelation", "RelatedCharacterId", "dbo.Character", "CharacterId");
            AddForeignKey("dbo.CharacterRelation", "CharacterId", "dbo.Character", "CharacterId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CharacterRelation", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.CharacterRelation", "RelatedCharacterId", "dbo.Character");
            DropIndex("dbo.CharacterRelation", new[] { "RelatedCharacterId" });
            AddForeignKey("dbo.CharacterRelation", "CharacterId", "dbo.Character", "CharacterId", cascadeDelete: true);
        }
    }
}
