using FluentMigrator;

namespace OrderManager.Migrations.Runner.Migrations
{
    [Migration(20210603112800)]
    public class Migration_20210603112800 : Migration
    {
        public override void Down()
        {
            Delete.Table("OrderSagaItems");
        }

        public override void Up()
        {
            Create.Table("OrderSagaItems")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Sku").AsString().NotNullable()
                .WithColumn("OrderSagaStateId").AsGuid().ForeignKey("OrderSagaState", "CorrelationId");
        }
    }
}
