using System;

namespace OrderManager.Business.Contracts
{
    public interface CreateOrder
    {
        Guid CorrelationId { get; }

        string OrderNumber { get; }

        DateTime? OrderDate { get; }

        string CustomerName { get; }

        string CustomerSurname { get; }

        string Sku { get; }

        decimal Price { get; }

        int Quantity { get; }
    }
}
