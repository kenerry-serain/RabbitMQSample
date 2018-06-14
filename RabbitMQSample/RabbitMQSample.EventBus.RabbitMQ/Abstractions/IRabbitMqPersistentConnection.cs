using RabbitMQ.Client;
using System;

namespace RabbitMQSample.EventBus.RabbitMQ.Abstractions
{
    public interface IRabbitMqPersistentConnection : IDisposable
    {
        bool TryConnect();
        IModel CreateModel();
        bool IsConnected { get; }
    }
}
