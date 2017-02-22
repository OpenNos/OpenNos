using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite12 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Family", "FamilyHeadGender");
        }

        public override void Up()
        {
            AddColumn("dbo.Family", "FamilyHeadGender", c => c.Byte(nullable: false));
        }

        #endregion
    }
}