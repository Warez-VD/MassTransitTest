using System;

namespace OrderManager.Business.Contracts
{
    public interface OrderStatusChange
    {
        Guid CorrelationId { get; }

        string State { get; }
    }
}
