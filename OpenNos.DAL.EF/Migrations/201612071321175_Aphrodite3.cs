namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite3 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Account", "RegistrationIP");
        }

        public override void Up()
        {
            AddColumn("dbo.Account", "RegistrationIP", c => c.String(maxLength: 45));
        }

        #endregion
    }
}