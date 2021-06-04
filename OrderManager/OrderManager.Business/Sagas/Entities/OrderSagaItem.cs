using System;

namespace OrderManager.Business.Sagas.Entities
{
    public class OrderSagaItem
    {
        public virtual int Id { get; set; }

        public virtual string Sku { get; set; }

        public virtual decimal Price { get; set; }

        public virtual int Quantity { get; set; }

        public virtual Guid OrderSagaStateId { get; set; }

        public virtual OrderSagaState OrderSagaState { get; set; }
    }
}
