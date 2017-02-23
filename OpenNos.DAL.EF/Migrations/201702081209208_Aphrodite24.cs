using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite24 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Character", "MinilandPoint");
        }

        public override void Up()
        {
            AddColumn("dbo.Character", "MinilandPoint", c => c.Short(nullable: false));
        }

        #endregion
    }
}