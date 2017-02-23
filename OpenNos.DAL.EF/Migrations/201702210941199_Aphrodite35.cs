using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite35 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.ItemCard", "ItemVNum", "dbo.Item");
            DropForeignKey("dbo.ItemCard", "CardId", "dbo.Card");
            DropIndex("dbo.ItemCard", new[] { "CardId" });
            DropIndex("dbo.ItemCard", new[] { "ItemVNum" });
            DropTable("dbo.ItemCard");
        }

        public override void Up()
        {
            CreateTable(
                "dbo.ItemCard",
                c => new
                {
                    ItemVNum = c.Short(nullable: false),
                    CardId = c.Short(nullable: false),
                    CardChance = c.Short(nullable: false)
                })
                .PrimaryKey(t => new { t.ItemVNum, t.CardId })
                .ForeignKey("dbo.Card", t => t.CardId)
                .ForeignKey("dbo.Item", t => t.ItemVNum)
                .Index(t => t.ItemVNum)
                .Index(t => t.CardId);
        }

        #endregion
    }
}