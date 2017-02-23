using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite33 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Skill", "BuffId", c => c.Short(nullable: false));
        }

        public override void Up()
        {
            DropColumn("dbo.Skill", "BuffId");
        }

        #endregion
    }
}