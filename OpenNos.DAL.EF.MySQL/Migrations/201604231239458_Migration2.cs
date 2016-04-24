namespace OpenNos.DAL.EF.MySQL.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Migration2 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.Item", "IsHeroic");
        }

        public override void Up()
        {
            AddColumn("dbo.Item", "IsHeroic", c => c.Boolean(nullable: false));
        }

        #endregion
    }
}