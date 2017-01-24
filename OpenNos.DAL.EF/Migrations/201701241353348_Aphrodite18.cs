namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite18 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CharacterRelation", "CharacterId", "dbo.Character");
            DropColumn("dbo.CharacterRelation", "RelatedCharacterId");
            RenameColumn(table: "dbo.CharacterRelation", name: "CharacterId", newName: "RelatedCharacterId");
            RenameIndex(table: "dbo.CharacterRelation", name: "IX_CharacterId", newName: "IX_RelatedCharacterId");
            AddForeignKey("dbo.CharacterRelation", "RelatedCharacterId", "dbo.Character", "CharacterId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CharacterRelation", "RelatedCharacterId", "dbo.Character");
            RenameIndex(table: "dbo.CharacterRelation", name: "IX_RelatedCharacterId", newName: "IX_CharacterId");
            RenameColumn(table: "dbo.CharacterRelation", name: "RelatedCharacterId", newName: "CharacterId");
            AddColumn("dbo.CharacterRelation", "RelatedCharacterId", c => c.Long(nullable: false));
            AddForeignKey("dbo.CharacterRelation", "CharacterId", "dbo.Character", "CharacterId", cascadeDelete: true);
        }
    }
}
