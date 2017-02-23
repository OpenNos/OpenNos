using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite4 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.FamilyCharacter", "CharacterId");
        }

        public override void Up()
        {
            AddColumn("dbo.FamilyCharacter", "CharacterId", c => c.Long(nullable: false));
        }

        #endregion
    }
}