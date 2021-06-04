namespace OrderManager.Business.Enums
{
    public enum OrderStatusType
    {
        Initial = 0,
        AwaitingPacking = 1,
        Packed = 2,
        Shipped = 3,
        Cancelled = 4
    }
}
