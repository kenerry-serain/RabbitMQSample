using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQSample.EventBus.Abstractions;
using RabbitMQSample.EventBus.RabbitMQ.Abstractions;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSample.EventBus.RabbitMQ.Implementation
{
    public class RabbitMqEventBus : IRabbitMqEventBus
    {
        private string _queueName;
        private IModel _consumerChannel;
        private readonly int _retryCount;
        private const string _exchangeType = "direct";
        private readonly IServiceProvider _serviceProvider;
        private const string _brokerName = "RabbitMQ Exchange";
        private readonly IEventBusSubscriptionManager _subscriptionManager;
        private readonly IRabbitMqPersistentConnection _persistentConnection;

        public RabbitMqEventBus
        (
            IServiceProvider serviceProvider,
            IEventBusSubscriptionManager subscriptionManager,
            IRabbitMqPersistentConnection persistentConnection,
            string queueName,
            int retryCount = 5
        )
        {
            _queueName = queueName;
            _retryCount = retryCount;
            _serviceProvider = serviceProvider;
            _subscriptionManager = subscriptionManager;
            _persistentConnection = persistentConnection;
            _subscriptionManager.OnEventRemoved += SubscriptionManager_OnEventRemoved;
        }

        public void StartConsumer()
        {
            if (string.IsNullOrWhiteSpace(_queueName))
                throw new InvalidOperationException("Queue name is missing for the consumer");

            DeclareConsumerQueue();
            _consumerChannel = StartConsumerChannel();
        }

        public Task Publish<TIntegrationEvent>(TIntegrationEvent @event)
            where TIntegrationEvent : IIntegrationEvent
        {
            var eventName = @event.GetType().Name;
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry
                (
                    _retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

            policy.Execute(() =>
            {
                using (var producerChannel = _persistentConnection.CreateModel())
                {
                    producerChannel.ExchangeDeclare
                    (
                        type: _exchangeType,
                        exchange: _brokerName,
                        autoDelete: false,
                        durable: true
                    );

                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = producerChannel.CreateBasicProperties();
                    properties.ContentType = "application/json";
                    properties.DeliveryMode = 2; // persistent
                    producerChannel.BasicPublish
                    (
                        routingKey: eventName,
                        exchange: _brokerName,
                        mandatory: true,
                        body: body,
                        basicProperties: properties
                    );
                }
            });

            return Task.CompletedTask;
        }

        public void Subscribe<TIntegrationEvent, TIntegrationEventHandler>()
            where TIntegrationEvent : IIntegrationEvent
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
        {
            var eventName = _subscriptionManager.GetEventName<TIntegrationEvent>();
            var containsKey = _subscriptionManager.HasSubscriptionsForEvent(eventName);
            if (containsKey)
                return;

            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueBind
                (
                    routingKey: eventName,
                    exchange: _brokerName,
                    queue: _queueName
                );
            }
            _subscriptionManager.AddSubscription<TIntegrationEvent, TIntegrationEventHandler>();
        }

        public void Unsubscribe<TIntegrationEvent, TIntegrationEventHandler>() where TIntegrationEvent : IIntegrationEvent where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
        {
            _subscriptionManager.RemoveSubscription<TIntegrationEvent, TIntegrationEventHandler>();
        }

        private IModel StartConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var channel = _persistentConnection.CreateModel();

            //Quality Of Service - Limiting the number of unacknowledged messages
            channel.BasicQos
            (
                prefetchCount: 5,
                prefetchSize: 0,
                global: false //Apply thi limit for the queue and not for the channel
            );

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var eventName = ea.RoutingKey;
                var message = Encoding.UTF8.GetString(ea.Body);

                // ATTENTION: If it throws any exception, the message won't receive the acknowledgement,
                // and will return to the Handler on the next chance;
                // The Application process may be terminated in case of an exception;
                // It's highly recommended using LoggerIntegrationEventHandler for Consumers.
                await ProcessEvent(eventName, message);

                //Releasing message from queue and processing the next one
                channel.BasicAck
                (
                    multiple: false,
                    deliveryTag: ea.DeliveryTag
                );
            };

            channel.BasicConsume
            (
                autoAck: false,
                queue: _queueName,
                consumer: consumer
            );

            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = StartConsumerChannel();
            };

            return channel;
        }


        private void SubscriptionManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind
                (
                    routingKey: eventName,
                    exchange: _brokerName,
                    queue: _queueName
                );

                if (!_subscriptionManager.HasNoHandlers)
                    return;

                _queueName = string.Empty;
                _consumerChannel.Close();
            }
        }

        private void DeclareConsumerQueue()
        {
            if (string.IsNullOrWhiteSpace(_queueName))
                return;

            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.ExchangeDeclare
                (
                    durable: true,
                    exchange: _brokerName,
                    autoDelete: false,
                    type: _exchangeType
                );

                channel.QueueDeclare
                (
                    autoDelete: false,
                    queue: _queueName,
                    durable: true,
                    exclusive: false
                );
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (!_subscriptionManager.HasSubscriptionsForEvent(eventName))
                throw new InvalidOperationException($"No Subscriptions for event {eventName}");

            using (var scope = _serviceProvider.CreateScope())
            {
                var eventType = _subscriptionManager.GetEventTypeByName(eventName);
                var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                var eventHandlerType = _subscriptionManager.GetEventHandlerType(eventName);
                var handler = scope.ServiceProvider.GetService(eventHandlerType);
                var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                await (Task)concreteType
                    .GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.Handle))
                    .Invoke(handler, new[] { integrationEvent });
            }
        }
    }
}
