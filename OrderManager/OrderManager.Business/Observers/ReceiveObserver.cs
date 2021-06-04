using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager.Business.Observers
{
    public class ReceiveObserver : IReceiveObserver
    {
        public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) where T : class
        {
            Console.Out.WriteLineAsync($"MessageId: {context.MessageId}; Duration: {duration}; ConsumerType: {consumerType}; Exception: {exception.Message}");
            return Task.CompletedTask;
        }

        public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class
        {
            return Task.CompletedTask;
        }

        public Task PostReceive(ReceiveContext context)
        {
            return Task.CompletedTask;
        }

        public Task PreReceive(ReceiveContext context)
        {
            return Task.CompletedTask;
        }

        public Task ReceiveFault(ReceiveContext context, Exception exception)
        {
            Console.Out.WriteLineAsync($"MessageId: {context.GetMessageId()}; Exception: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
