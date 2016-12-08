namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite1 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Account", "Email");
        }

        public override void Up()
        {
            AddColumn("dbo.Account", "Email", c => c.String(maxLength: 255));
        }

        #endregion
    }
}