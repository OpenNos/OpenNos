using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite32 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AlterColumn("dbo.Card", "Type", c => c.Byte(nullable: false));
            AlterColumn("dbo.Card", "SecondData", c => c.Short(nullable: false));
            AlterColumn("dbo.Card", "FirstData", c => c.Short(nullable: false));
        }

        public override void Up()
        {
            AlterColumn("dbo.Card", "FirstData", c => c.Int(nullable: false));
            AlterColumn("dbo.Card", "SecondData", c => c.Int(nullable: false));
            AlterColumn("dbo.Card", "Type", c => c.Short(nullable: false));
        }

        #endregion
    }
}