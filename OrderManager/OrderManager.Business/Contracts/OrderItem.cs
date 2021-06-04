namespace OrderManager.Business.Contracts
{
    public interface OrderItem
    {
        string Sku { get; }

        decimal Price { get; }

        int Quantity { get; }
    }
}
