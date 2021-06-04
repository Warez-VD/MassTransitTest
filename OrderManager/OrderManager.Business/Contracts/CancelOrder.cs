using System;

namespace OrderManager.Business.Contracts
{
    public interface CancelOrder
    {
        Guid CorrelationId { get; }
    }
}
