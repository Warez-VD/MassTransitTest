using System;

namespace OrderManager.Business.Contracts
{
    public interface PackOrder
    {
        Guid CorrelationId { get; }

        DateTime? ShipDate { get; }
    }
}
