using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrderManager.Business.Contracts;
using OrderManager.Business.Enums;
using OrderManagerHost.Models;

namespace OrderManagerHost.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HomeController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public HomeController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync(CreateOrderRequestModel model)
        {
            await _publishEndpoint.Publish<CreateOrder>(new 
            {
                CorrelationId = NewId.NextGuid(),
                OrderNumber = model.OrderNumber,
                OrderDate = model.OrderDate,
                CustomerName = model.CustomerName,
                CustomerSurname = model.CustomerSurname,
                Items = model.Items
            });
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> OrderStatusChange(Guid correlationId, OrderStatusType state)
        {
            await _publishEndpoint.Publish<OrderStatusChange>(new
            {
                CorrelationId = correlationId,
                State = state
            });

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> PackOrder(Guid correlationId, DateTime shipDate)
        {
            await _publishEndpoint.Publish<PackOrder>(new
            {
                CorrelationId = correlationId,
                ShipDate = shipDate
            });

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid correlationId)
        {
            await _publishEndpoint.Publish<CancelOrder>(new
            {
                CorrelationId = correlationId
            });

            return Ok();
        }
    }
}
