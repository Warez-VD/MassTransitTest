using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OrderManager.Business.Contracts;
using OrderManager.Business.Enums;
using OrderManager.Business.Sagas;
using OrderManager.Tests.Helpers;

namespace OrderManager.Tests
{
    [TestFixture]
    public class Tests
    {
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            _serviceProvider = new ServiceCollection()
                .AddMassTransitInMemoryTestHarness(cfg =>
                {
                    cfg.AddSagaStateMachine<OrderStateMachine, OrderSagaState>()
                        .InMemoryRepository();
                    cfg.AddSagaStateMachineTestHarness<OrderStateMachine, OrderSagaState>();
                })
                .BuildServiceProvider(true);
        }

        [Test]
        public async Task OnCreateOrder_Saga_ShouldPersistOrder()
        {
            var harness = _serviceProvider.GetRequiredService<InMemoryTestHarness>();
            await harness.Start();

            try
            {
                var sagaId = NewId.NextGuid();
                var orderNumber = "123123";
                var orderDate = DateTime.Now;
                var customerName = "John";
                var customerSurname = "Doe";
                var items = new List<OrderItem>()
                {
                    new OrderItemImpl
                    {
                        Sku = "item-123",
                        Price = 100,
                        Quantity = 1
                    },
                    new OrderItemImpl
                    {
                        Sku = "item-323",
                        Price = 300,
                        Quantity = 2
                    }
                };

                await harness.Bus.Publish<CreateOrder>(new
                {
                    CorrelationId = sagaId,
                    OrderNumber = orderNumber,
                    OrderDate = orderDate,
                    CustomerName = customerName,
                    CustomerSurname = customerSurname,
                    Items = items
                });

                var sagaHarness = _serviceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderSagaState, OrderStateMachine>>();
                Assert.That(await sagaHarness.Consumed.Any<CreateOrder>());
                var instanceId = await sagaHarness.Exists(sagaId, x => x.AwaitingPacking);
                Assert.That(instanceId, Is.Not.Null);

                var instance = sagaHarness.Sagas.Contains(instanceId.Value);
                Assert.That(instance.CorrelationId, Is.EqualTo(sagaId));
                Assert.That(instance.OrderNumber, Is.EqualTo(orderNumber));
                Assert.That(instance.OrderDate, Is.EqualTo(orderDate));
                Assert.That(instance.CustomerName, Is.EqualTo(customerName));
                Assert.That(instance.CustomerSurname, Is.EqualTo(customerSurname));
                Assert.That(instance.Items, Is.Not.Empty);
                Assert.That(instance.Items.Count, Is.EqualTo(items.Count));
            }
            finally
            {
                await harness.Stop();
                _serviceProvider.Dispose();
            }
        }

        [Test]
        public async Task OnCreateOrder_Saga_ShouldTransitionToAwaitPackingState()
        {
            var harness = _serviceProvider.GetRequiredService<InMemoryTestHarness>();
            await harness.Start();

            try
            {
                var sagaId = NewId.NextGuid();
                var orderNumber = "123123";
                var orderDate = DateTime.Now;
                var customerName = "John";
                var customerSurname = "Doe";
                var items = new List<OrderItem>();

                await harness.Bus.Publish<CreateOrder>(new
                {
                    CorrelationId = sagaId,
                    OrderNumber = orderNumber,
                    OrderDate = orderDate,
                    CustomerName = customerName,
                    CustomerSurname = customerSurname,
                    Items = items
                });

                var sagaHarness = _serviceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderSagaState, OrderStateMachine>>();
                var instanceId = await sagaHarness.Exists(sagaId, x => x.AwaitingPacking);
                Assert.That(instanceId, Is.Not.Null);
            }
            finally
            {
                await harness.Stop();
                _serviceProvider.Dispose();
            }
        }

        [Test]
        public async Task OnAwaitingPacking_Saga_ShouldTransitionToPackedState_When_StatePacked_IsPassed_AndUpdateUpdateDate()
        {
            var harness = _serviceProvider.GetRequiredService<InMemoryTestHarness>();
            await harness.Start();

            try
            {
                var sagaId = NewId.NextGuid();
                var orderNumber = "123123";
                var orderDate = DateTime.Now;
                var customerName = "John";
                var customerSurname = "Doe";
                var items = new List<OrderItem>();

                await harness.Bus.Publish<CreateOrder>(new
                {
                    CorrelationId = sagaId,
                    OrderNumber = orderNumber,
                    OrderDate = orderDate,
                    CustomerName = customerName,
                    CustomerSurname = customerSurname,
                    Items = items
                });

                var state = OrderStatusType.Packed;
                var updateDate = DateTime.Now;

                await harness.Bus.Publish<OrderStatusChange>(new
                {
                    CorrelationId = sagaId,
                    State = state,
                    UpdateDate = updateDate
                });

                var sagaHarness = _serviceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderSagaState, OrderStateMachine>>();
                var instanceId = await sagaHarness.Exists(sagaId, x => x.Packed);
                Assert.That(instanceId, Is.Not.Null);

                var instance = sagaHarness.Sagas.Contains(instanceId.Value);
                Assert.That(instance.UpdateDate, Is.EqualTo(updateDate));
            }
            finally
            {
                await harness.Stop();
                _serviceProvider.Dispose();
            }
        }

        [Test]
        public async Task OnAwaitingPacking_Saga_Should_StayIn_AwaitingPackingState_When_StateDifferent_ThanPacked_IsPassed()
        {
            var harness = _serviceProvider.GetRequiredService<InMemoryTestHarness>();
            await harness.Start();

            try
            {
                var sagaId = NewId.NextGuid();
                var orderNumber = "123123";
                var orderDate = DateTime.Now;
                var customerName = "John";
                var customerSurname = "Doe";
                var items = new List<OrderItem>();

                await harness.Bus.Publish<CreateOrder>(new
                {
                    CorrelationId = sagaId,
                    OrderNumber = orderNumber,
                    OrderDate = orderDate,
                    CustomerName = customerName,
                    CustomerSurname = customerSurname,
                    Items = items
                });

                var state = OrderStatusType.Shipped;
                var updateDate = DateTime.Now;

                await harness.Bus.Publish<OrderStatusChange>(new
                {
                    CorrelationId = sagaId,
                    State = state,
                    UpdateDate = updateDate
                });

                var sagaHarness = _serviceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderSagaState, OrderStateMachine>>();
                var instanceId = await sagaHarness.Exists(sagaId, x => x.AwaitingPacking);
                Assert.That(instanceId, Is.Not.Null);
            }
            finally
            {
                await harness.Stop();
                _serviceProvider.Dispose();
            }
        }

        [Test]
        public async Task OnPacked_Saga_ShouldTransitionToShippedState_AndUpdate_UpdateDate_AndShippedDate()
        {
            var harness = _serviceProvider.GetRequiredService<InMemoryTestHarness>();
            await harness.Start();

            try
            {
                var sagaId = NewId.NextGuid();
                var orderNumber = "123123";
                var orderDate = DateTime.Now;
                var customerName = "John";
                var customerSurname = "Doe";
                var items = new List<OrderItem>();

                await harness.Bus.Publish<CreateOrder>(new
                {
                    CorrelationId = sagaId,
                    OrderNumber = orderNumber,
                    OrderDate = orderDate,
                    CustomerName = customerName,
                    CustomerSurname = customerSurname,
                    Items = items
                });

                var state = OrderStatusType.Packed;
                var updateAwaitingPackingDate = DateTime.Now;

                await harness.Bus.Publish<OrderStatusChange>(new
                {
                    CorrelationId = sagaId,
                    State = state,
                    UpdateDate = updateAwaitingPackingDate
                });

                var updatePackedDate = DateTime.Now;
                var shipDate = DateTime.Now;

                await harness.Bus.Publish<PackOrder>(new
                {
                    CorrelationId = sagaId,
                    ShipDate = shipDate,
                    UpdateDate = updatePackedDate
                });

                var sagaHarness = _serviceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderSagaState, OrderStateMachine>>();
                var instanceId = await sagaHarness.Exists(sagaId, x => x.Shipped);
                Assert.That(instanceId, Is.Not.Null);

                var instance = sagaHarness.Sagas.Contains(instanceId.Value);
                Assert.That(instance.UpdateDate, Is.EqualTo(updatePackedDate));
                Assert.That(instance.ShippedDate, Is.EqualTo(shipDate));
            }
            finally
            {
                await harness.Stop();
                _serviceProvider.Dispose();
            }
        }

        [Test]
        public async Task OnCancelled_Saga_ShouldTransitionToCancelledState()
        {
            var harness = _serviceProvider.GetRequiredService<InMemoryTestHarness>();
            await harness.Start();

            try
            {
                var sagaId = NewId.NextGuid();
                var orderNumber = "123123";
                var orderDate = DateTime.Now;
                var customerName = "John";
                var customerSurname = "Doe";
                var items = new List<OrderItem>();

                await harness.Bus.Publish<CreateOrder>(new
                {
                    CorrelationId = sagaId,
                    OrderNumber = orderNumber,
                    OrderDate = orderDate,
                    CustomerName = customerName,
                    CustomerSurname = customerSurname,
                    Items = items
                });

                var state = OrderStatusType.Packed;
                var updateAwaitingPackingDate = DateTime.Now;

                await harness.Bus.Publish<OrderStatusChange>(new
                {
                    CorrelationId = sagaId,
                    State = state,
                    UpdateDate = updateAwaitingPackingDate
                });

                await harness.Bus.Publish<CancelOrder>(new
                {
                    CorrelationId = sagaId
                });

                var sagaHarness = _serviceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderSagaState, OrderStateMachine>>();
                var instanceId = await sagaHarness.Exists(sagaId, x => x.Cancelled);
                Assert.That(instanceId, Is.Not.Null);
            }
            finally
            {
                await harness.Stop();
                _serviceProvider.Dispose();
            }
        }

        [Test]
        public async Task When_SagaIsCancelled_ShouldIgnoreOtherEvents_And_StayCancelled()
        {
            var harness = _serviceProvider.GetRequiredService<InMemoryTestHarness>();
            await harness.Start();

            try
            {
                var sagaId = NewId.NextGuid();
                var orderNumber = "123123";
                var orderDate = DateTime.Now;
                var customerName = "John";
                var customerSurname = "Doe";
                var items = new List<OrderItem>();

                await harness.Bus.Publish<CreateOrder>(new
                {
                    CorrelationId = sagaId,
                    OrderNumber = orderNumber,
                    OrderDate = orderDate,
                    CustomerName = customerName,
                    CustomerSurname = customerSurname,
                    Items = items
                });

                var state = OrderStatusType.Packed;
                var updateAwaitingPackingDate = DateTime.Now;

                await harness.Bus.Publish<OrderStatusChange>(new
                {
                    CorrelationId = sagaId,
                    State = state,
                    UpdateDate = updateAwaitingPackingDate
                });

                await harness.Bus.Publish<CancelOrder>(new
                {
                    CorrelationId = sagaId
                });

                await harness.Bus.Publish<OrderStatusChange>(new
                {
                    CorrelationId = sagaId,
                    State = state,
                    UpdateDate = updateAwaitingPackingDate
                });

                var sagaHarness = _serviceProvider.GetRequiredService<IStateMachineSagaTestHarness<OrderSagaState, OrderStateMachine>>();
                var instanceId = await sagaHarness.Exists(sagaId, x => x.Cancelled);
                Assert.That(instanceId, Is.Not.Null);
            }
            finally
            {
                await harness.Stop();
                _serviceProvider.Dispose();
            }
        }
    }
}