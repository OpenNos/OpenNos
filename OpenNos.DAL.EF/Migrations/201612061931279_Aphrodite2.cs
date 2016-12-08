namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite2 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Account", "VerificationToken");
        }

        public override void Up()
        {
            AddColumn("dbo.Account", "VerificationToken", c => c.String(maxLength: 32));
        }

        #endregion
    }
}