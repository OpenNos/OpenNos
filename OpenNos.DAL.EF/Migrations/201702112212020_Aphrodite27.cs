using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite27 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Character", "LastLogin", c => c.DateTime(nullable: false));
            AddColumn("dbo.Account", "LastCompliment", c => c.DateTime(nullable: false));
        }

        public override void Up()
        {
            DropColumn("dbo.Account", "LastCompliment");
            DropColumn("dbo.Character", "LastLogin");
        }

        #endregion
    }
}