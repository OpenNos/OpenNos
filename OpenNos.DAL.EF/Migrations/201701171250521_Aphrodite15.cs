using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite15 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.FamilyCharacter", "CharacterId", "dbo.Character");
            DropIndex("dbo.FamilyCharacter", new[] { "CharacterId" });
        }

        public override void Up()
        {
            CreateIndex("dbo.FamilyCharacter", "CharacterId");
            AddForeignKey("dbo.FamilyCharacter", "CharacterId", "dbo.Character", "CharacterId");
        }

        #endregion
    }
}