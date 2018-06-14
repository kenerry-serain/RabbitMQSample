using System.Threading.Tasks;

namespace RabbitMQSample.EventBus.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent>
        where TIntegrationEvent : IIntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }
}
