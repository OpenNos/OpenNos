using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite28 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            AddForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account", "AccountId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            AddForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account", "AccountId", cascadeDelete: true);
        }

        #endregion
    }
}