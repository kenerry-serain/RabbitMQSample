using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQSample.EventBus.RabbitMQ.Abstractions;
using System;
using System.Net.Sockets;

namespace RabbitMQSample.EventBus.RabbitMQ.Implementation
{
    public class RabbitMqPersistentConnection : IRabbitMqPersistentConnection
    {
        private bool _disposed;
        private IConnection _connection;
        private readonly int _retryCount;
        private readonly IConnectionFactory _connectionFactory;

        public RabbitMqPersistentConnection
        (
            IConnectionFactory connectionFactory,
            int retryCount = 5
        )
        {
            _connectionFactory = connectionFactory;
            _retryCount = retryCount;
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
                throw new InvalidOperationException("No RabbitMQ connection is available to perform this action");
            return _connection.CreateModel();
        }

        public bool TryConnect()
        {
            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)));

            policy.Execute(() => _connection = _connectionFactory.CreateConnection());

            if (IsConnected)
                return true;

            return false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _connection.Dispose();
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;
    }
}
