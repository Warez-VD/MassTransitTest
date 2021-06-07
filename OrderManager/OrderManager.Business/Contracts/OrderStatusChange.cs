using System;
using OrderManager.Business.Enums;

namespace OrderManager.Business.Contracts
{
    public interface OrderStatusChange
    {
        Guid CorrelationId { get; }

        OrderStatusType State { get; }

        DateTime UpdateDate { get; }
    }
}
