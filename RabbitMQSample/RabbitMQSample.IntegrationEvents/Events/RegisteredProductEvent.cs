using RabbitMQSample.EventBus.Abstractions;

namespace RabbitMQSample.IntegrationEvents.Events
{
    public class RegisteredProductEvent : IIntegrationEvent
    {
        public RegisteredProductEvent(string productId)
        {
            ProductId = productId;
        }

        public string ProductId { get; set; }
    }
}
