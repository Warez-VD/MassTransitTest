using FluentMigrator;

namespace OrderManager.Migrations.Runner.Migrations
{
    [Migration(20210604092700)]
    public class Migration_20210604092700 : Migration
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            Alter.Table("OrderSagaItems")
                .AddColumn("Price").AsCurrency()
                .AddColumn("Quantity").AsInt32();
        }
    }
}
