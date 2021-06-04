using System;
using System.Collections.Generic;

namespace OrderManagerHost.Models
{
    public class CreateOrderRequestModel
    {
        public string OrderNumber { get; set; }

        public DateTime? OrderDate { get; set; }

        public string CustomerName { get; set; }

        public string CustomerSurname { get; set; }

        public IEnumerable<OrderItemModel> Items { get; set; }
    }
}
