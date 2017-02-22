using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite14 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Character", "FamilyCharacterId", c => c.Long());
            CreateIndex("dbo.Character", "FamilyCharacterId");
            AddForeignKey("dbo.Character", "FamilyCharacterId", "dbo.FamilyCharacter", "FamilyCharacterId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.Character", "FamilyCharacterId", "dbo.FamilyCharacter");
            DropIndex("dbo.Character", new[] { "FamilyCharacterId" });
            DropColumn("dbo.Character", "FamilyCharacterId");
        }

        #endregion
    }
}