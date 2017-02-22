using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite16 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.FamilyCharacter", "JoinDate", c => c.DateTime(nullable: false));
        }

        public override void Up()
        {
            DropColumn("dbo.FamilyCharacter", "JoinDate");
        }

        #endregion
    }
}