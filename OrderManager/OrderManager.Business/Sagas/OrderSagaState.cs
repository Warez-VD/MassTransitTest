using System;
using System.Collections.Generic;
using Automatonymous;
using OrderManager.Business.Sagas.Entities;

namespace OrderManager.Business.Sagas
{
    public class OrderSagaState : SagaStateMachineInstance
    {
        private IList<OrderSagaItem> _items;

        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }

        public int Version { get; set; }

        public string OrderNumber { get; set; }

        public DateTime? OrderDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public string CustomerName { get; set; }

        public string CustomerSurname { get; set; }

        public DateTime? ShippedDate { get; set; }

        public virtual IList<OrderSagaItem> Items 
        {
            get 
            {
                return _items ?? (_items = new List<OrderSagaItem>());
            }
            set 
            {
                _items = value;
            }
        }
    }
}
