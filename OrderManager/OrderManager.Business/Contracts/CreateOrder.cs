using System;
using System.Collections.Generic;

namespace OrderManager.Business.Contracts
{
    public interface CreateOrder
    {
        Guid CorrelationId { get; }

        string OrderNumber { get; }

        DateTime? OrderDate { get; }

        string CustomerName { get; }

        string CustomerSurname { get; }

        IEnumerable<OrderItem> Items { get; }
    }
}
