using System;
using Automatonymous;
using OrderManager.Business.Contracts;
using OrderManager.Business.Enums;
using OrderManager.Business.Sagas.Entities;

namespace OrderManager.Business.Sagas
{
    public class OrderStateMachine : MassTransitStateMachine<OrderSagaState>
    {
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => CreateOrder, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => OrderStatusChange, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => PackOrder, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => CancelOrder, x => x.CorrelateById(context => context.Message.CorrelationId));

            Initially(
                When(CreateOrder)
                    .Then(context =>
                    {
                        context.Instance.CorrelationId = context.Data.CorrelationId;
                        context.Instance.OrderNumber = context.Data.OrderNumber;
                        context.Instance.OrderDate = context.Data.OrderDate;
                        context.Instance.CustomerName = context.Data.CustomerName;
                        context.Instance.CustomerSurname = context.Data.CustomerSurname;

                        foreach (var item in context.Data.Items)
                        {
                            context.Instance.Items.Add(new OrderSagaItem
                            {
                                Sku = item.Sku,
                                Price = item.Price,
                                Quantity = item.Quantity,
                                OrderSagaStateId = context.Data.CorrelationId,
                                OrderSagaState = context.Instance
                            });
                        }
                    })
                    .TransitionTo(AwaitingPacking));

            During(AwaitingPacking,
                When(OrderStatusChange, context => context.Data.State == OrderStatusType.Packed)
                    .Then(context =>
                    {
                        context.Instance.UpdateDate = DateTime.Now;
                    })
                    .TransitionTo(Packed),
                Ignore(PackOrder));

            During(Packed,
                When(PackOrder)
                    .Then(context =>
                    {
                        context.Instance.UpdateDate = DateTime.Now;
                        context.Instance.ShippedDate = context.Data.ShipDate;
                    })
                    .TransitionTo(Shipped),
                Ignore(OrderStatusChange));

            During(Cancelled,
                Ignore(OrderStatusChange),
                Ignore(PackOrder));

            DuringAny(
                When(CancelOrder)
                .TransitionTo(Cancelled));
        }

        public Event<CreateOrder> CreateOrder { get; set; }

        public Event<OrderStatusChange> OrderStatusChange { get; set; }

        public Event<PackOrder> PackOrder { get; set; }

        public Event<CancelOrder> CancelOrder { get; set; }

        public State AwaitingPacking { get; }

        public State Packed { get; set; }

        public State Shipped { get; set; }

        public State Cancelled { get; set; }
    }
}
