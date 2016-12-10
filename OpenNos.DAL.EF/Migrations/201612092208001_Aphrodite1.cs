namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CharacterRelation",
                c => new
                    {
                        CharacterRelationId = c.Long(nullable: false, identity: true),
                        CharacterId = c.Long(nullable: false),
                        RelatedCharacterId = c.Long(nullable: false),
                        RelationType = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.CharacterRelationId)
                .ForeignKey("dbo.Character", t => t.CharacterId, cascadeDelete: true)
                .Index(t => t.CharacterId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CharacterRelation", "CharacterId", "dbo.Character");
            DropIndex("dbo.CharacterRelation", new[] { "CharacterId" });
            DropTable("dbo.CharacterRelation");
        }
    }
}
