using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite10 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Family", "Size", c => c.Byte(nullable: false));
        }

        public override void Up()
        {
            DropColumn("dbo.Family", "Size");
        }

        #endregion
    }
}