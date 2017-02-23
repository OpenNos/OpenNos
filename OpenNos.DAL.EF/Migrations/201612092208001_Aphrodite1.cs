using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite1 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.CharacterRelation", "CharacterId", "dbo.Character");
            DropIndex("dbo.CharacterRelation", new[] { "CharacterId" });
            DropTable("dbo.CharacterRelation");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.CharacterRelation",
                c => new
                {
                    CharacterRelationId = c.Long(nullable: false, identity: true),
                    CharacterId = c.Long(nullable: false),
                    RelatedCharacterId = c.Long(nullable: false),
                    RelationType = c.Short(nullable: false)
                })
                .PrimaryKey(t => t.CharacterRelationId)
                .ForeignKey("dbo.Character", t => t.CharacterId, cascadeDelete: true)
                .Index(t => t.CharacterId);
        }

        #endregion
    }
}