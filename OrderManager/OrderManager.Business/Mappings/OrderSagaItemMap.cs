using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using OrderManager.Business.Sagas.Entities;

namespace OrderManager.Business.Mappings
{
    public class OrderSagaItemMap : ClassMapping<OrderSagaItem>
    {
        public OrderSagaItemMap()
        {
            Id(x => x.Id, map => map.Generator(Generators.Native));
            Property(x => x.Sku);
            Property(x => x.Price);
            Property(x => x.Quantity);
            ManyToOne(x => x.OrderSagaState,
                c =>
                {
                    c.Column("OrderSagaStateId");
                });
        }
    }
}
