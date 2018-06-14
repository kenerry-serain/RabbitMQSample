using RabbitMQSample.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitMQSample.EventBus.Implementations
{
    public class EventBusSubscriptionManager : IEventBusSubscriptionManager
    {
        private readonly List<Type> _eventTypes;
        private readonly Dictionary<string, Type> _eventHandlers;
        public EventBusSubscriptionManager()
        {
            _eventTypes = new List<Type>();
            _eventHandlers = new Dictionary<string, Type>();
        }

        public void ClearAllHandlers()
        {
            _eventTypes.Clear();
        }

        public event EventHandler<string> OnEventRemoved;
        public Type GetEventTypeByName(string eventName)
        {
            return _eventTypes.SingleOrDefault(evt => evt.Name == eventName);
        }

        public string GetEventName<TIntegrationEvent>()
        {
            return typeof(TIntegrationEvent).Name;
        }

        public void AddSubscription<TIntegrationEvent, TIntegrationEventHandler>()
            where TIntegrationEvent : IIntegrationEvent
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
        {
            var eventName = GetEventName<TIntegrationEvent>();
            if (HasSubscriptionsForEvent(eventName))
                throw new ArgumentException($"Event {eventName} already registered!");

            _eventTypes.Add(typeof(TIntegrationEvent));
            _eventHandlers[eventName] = typeof(TIntegrationEventHandler);
        }

        public void RemoveSubscription<TIntegrationEvent, TIntegrationEventHandler>()
            where TIntegrationEvent : IIntegrationEvent
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
        {
            var handlerToRemove = FindSubscriptionToRemove<TIntegrationEvent>();

            if (handlerToRemove == null) return;
            _eventTypes.Remove(typeof(TIntegrationEventHandler));

            var eventName = GetEventName<TIntegrationEvent>();
            var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);

            if (eventType != null)
                _eventTypes.Remove(eventType);

            RaiseOnEventRemoved(eventName);
        }

        public bool HasSubscriptionsForEvent(string eventName)
        {
            return _eventHandlers.ContainsKey(eventName);
        }

        public bool HasSubscriptionsForEvent<TIntegrationEvent>()
            where TIntegrationEvent : IIntegrationEvent
        {
            var eventName = GetEventName<TIntegrationEvent>();
            return _eventHandlers.ContainsKey(eventName);
        }

        public Type GetEventHandlerType(string eventName)
        {
            return _eventHandlers[eventName];
        }

        public Type GetEventHandlerType<TIntegrationEvent>() where TIntegrationEvent : IIntegrationEvent
        {
            var key = GetEventName<TIntegrationEvent>();
            return GetEventHandlerType(key);
        }

        private Type FindSubscriptionToRemove<TIntegrationEvent>()
            where TIntegrationEvent : IIntegrationEvent
        {
            var eventName = GetEventName<TIntegrationEvent>();
            return HasSubscriptionsForEvent(eventName)
                ? _eventHandlers[eventName]
                : null;
        }

        private void RaiseOnEventRemoved(string eventName)
        {
            OnEventRemoved?.Invoke(this, eventName);
        }

        public bool HasNoHandlers => !_eventHandlers.Keys.Any();
    }
}







































//private readonly List<Type> _eventTypes;
//private readonly Dictionary<string, List<Type>> _handlers;
//public EventBusSubscriptionManager()
//{
//    _eventTypes = new List<Type>();
//    _handlers = new Dictionary<string, List<Type>>();
//}

//public void ClearAllHandlers()
//{
//    _handlers.Clear();
//}

//public string GetEventName<TIntegrationEvent>()
//{
//    return typeof(TIntegrationEvent).Name;
//}

//public Type GetEventTypeByName(string eventName)
//{
//    return _eventTypes.SingleOrDefault(t => t.Name == eventName);
//}

//public void AddSubscription<TIntegrationEvent, TIntegrationEventHandler>()
//    where TIntegrationEvent : IIntegrationEvent
//    where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
//{
//    var eventName = GetEventName<TIntegrationEvent>();
//    var handlerType = typeof(TIntegrationEventHandler);

//    if (!HasSubscriptionsForEvent(eventName))
//        _handlers.Add(eventName, new List<Type>());

//    if (_handlers[eventName].Any(s => s == handlerType))
//        throw new ArgumentException(
//            $"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));

//    _handlers[eventName].Add(handlerType);
//    _eventTypes.Add(typeof(TIntegrationEvent));
//}

//public void RemoveSubscription<TIntegrationEvent, TIntegrationEventHandler>()
//    where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
//    where TIntegrationEvent : IIntegrationEvent
//{
//    var eventName = GetEventName<TIntegrationEvent>();
//    var handlerToRemove = FindSubscriptionToRemove<TIntegrationEvent, TIntegrationEventHandler>();

//    if (handlerToRemove == null)
//        return;

//    _handlers[eventName].Remove(handlerToRemove);

//    if (_handlers[eventName].Any())
//        return;

//    _handlers.Remove(eventName);

//    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
//    if (eventType != null)
//        _eventTypes.Remove(eventType);

//    RaiseOnEventRemoved(eventName);
//}

//public IEnumerable<Type> GetEventHandlerType<TIntegrationEvent>()
//    where TIntegrationEvent : IIntegrationEvent
//{
//    var key = GetEventName<TIntegrationEvent>();
//    return GetEventHandlerType(key);
//}

//public IEnumerable<Type> GetEventHandlerType(string eventName)
//{
//    return _handlers[eventName];
//}

//public bool HasSubscriptionsForEvent<TIntegrationEvent>()
//    where TIntegrationEvent : IIntegrationEvent
//{
//    var key = GetEventName<TIntegrationEvent>();
//    return HasSubscriptionsForEvent(key);
//}

//public bool HasSubscriptionsForEvent(string eventName)
//{
//    return _handlers.ContainsKey(eventName);
//}


//public bool HasNoHandlers => !_handlers.Keys.Any();
//public event EventHandler<string> OnEventRemoved;

//private void RaiseOnEventRemoved(string eventName)
//{
//    OnEventRemoved?.Invoke(this, eventName);
//}


//private Type FindSubscriptionToRemove<TIntegrationEvent, TIntegrationEventHandler>()
//    where TIntegrationEvent : IIntegrationEvent
//    where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
//{
//    var eventName = GetEventName<TIntegrationEvent>();
//    return HasSubscriptionsForEvent(eventName)
//        ? _handlers[eventName].SingleOrDefault(s => s == typeof(TIntegrationEventHandler))
//        : null;
//}