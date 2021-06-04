using System;

namespace OrderManagerHost.Models
{
    public class CreateOrderRequestModel
    {
        public string OrderNumber { get; set; }

        public DateTime? OrderDate { get; set; }

        public string CustomerName { get; set; }

        public string CustomerSurname { get; set; }

        public string Sku { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}
