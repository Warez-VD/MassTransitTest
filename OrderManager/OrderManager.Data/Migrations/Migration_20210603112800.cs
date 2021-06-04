using FluentMigrator;

namespace OrderManager.Data.Migrations
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
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Sku").AsString().NotNullable()
                .WithColumn("OrderSagaStateId").AsGuid();

            Create.ForeignKey() // You can give the FK a name or just let Fluent Migrator default to one
                .FromTable("OrderSagaItems").ForeignColumn("OrderSagaStateId")
                .ToTable("OrderSagaState").PrimaryColumn("CorrelationId");
        }
    }
}
