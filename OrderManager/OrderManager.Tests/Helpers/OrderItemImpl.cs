using OrderManager.Business.Contracts;

namespace OrderManager.Tests.Helpers
{
    public class OrderItemImpl : OrderItem
    {
        public string Sku { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}
