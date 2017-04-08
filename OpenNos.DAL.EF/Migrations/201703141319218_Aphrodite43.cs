namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite43 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.TimeSpace", "Label", c => c.String());
            AddColumn("dbo.TimeSpace", "SpecialItemGift", c => c.String());
            AddColumn("dbo.TimeSpace", "BonusItemGift", c => c.String());
            AddColumn("dbo.TimeSpace", "DrawItemGift", c => c.String());
            AddColumn("dbo.TimeSpace", "LevelMaximum", c => c.Int(nullable: false));
            AddColumn("dbo.TimeSpace", "LevelMinimum", c => c.Int(nullable: false));
            DropColumn("dbo.TimeSpace", "XMLName");
        }

        public override void Up()
        {
            AddColumn("dbo.TimeSpace", "XMLName", c => c.String());
            DropColumn("dbo.TimeSpace", "LevelMinimum");
            DropColumn("dbo.TimeSpace", "LevelMaximum");
            DropColumn("dbo.TimeSpace", "DrawItemGift");
            DropColumn("dbo.TimeSpace", "BonusItemGift");
            DropColumn("dbo.TimeSpace", "SpecialItemGift");
            DropColumn("dbo.TimeSpace", "Label");
        }

        #endregion
    }
}