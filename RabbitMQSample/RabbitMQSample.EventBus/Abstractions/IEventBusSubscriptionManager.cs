using System;

namespace RabbitMQSample.EventBus.Abstractions
{
    public interface IEventBusSubscriptionManager
    {
        bool HasNoHandlers { get; }
        void ClearAllHandlers();
        event EventHandler<string> OnEventRemoved;
        Type GetEventTypeByName(string eventName);
        string GetEventName<TIntegrationEvent>();

        void AddSubscription<TIntegrationEvent, TIntegrationEventHandler>()
            where TIntegrationEvent : IIntegrationEvent
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>;

        void RemoveSubscription<TIntegrationEvent, TIntegrationEventHandler>()
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
            where TIntegrationEvent : IIntegrationEvent;

        bool HasSubscriptionsForEvent(string eventName);
        bool HasSubscriptionsForEvent<TIntegrationEvent>() where TIntegrationEvent : IIntegrationEvent;

        Type GetEventHandlerType(string eventName);
        Type GetEventHandlerType<TIntegrationEvent>() where TIntegrationEvent : IIntegrationEvent;
    }
}
