using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQSample.EventBus.Abstractions;
using RabbitMQSample.IntegrationEvents.Events;

namespace RabbitMQSample.Stock.EventsHandlers
{
    public class RegisteredProductEventHandler : IIntegrationEventHandler<RegisteredProductEvent>
    {
        private readonly ILogger<RegisteredProductEventHandler> _logger;
        public RegisteredProductEventHandler(ILogger<RegisteredProductEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(RegisteredProductEvent @event)
        {
            _logger.LogInformation($"[Logger INFO] Received Id: {@event.ProductId}");
            return Task.CompletedTask;
        }
    }
}
