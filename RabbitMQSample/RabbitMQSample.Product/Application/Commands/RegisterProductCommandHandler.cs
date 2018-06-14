using MediatR;
using Microsoft.Extensions.Logging;
using RabbitMQSample.EventBus.Abstractions;
using RabbitMQSample.IntegrationEvents.Events;
using RabbitMQSample.Product.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQSample.Product.Application.Commands
{
    public class RegisterProductCommandHandler : IRequestHandler<RegisterProductCommand>
    {
        private readonly IRabbitMqEventBus _rabbitMqEventBus;
        private readonly ILogger<RegisterProductCommandHandler> _logger;
        private static readonly List<ProductEntity> _productCollection = new List<ProductEntity>();
        public RegisterProductCommandHandler
        (
            IRabbitMqEventBus rabbitMqEventBus,
            ILogger<RegisterProductCommandHandler> logger
        )
        {
            _rabbitMqEventBus = rabbitMqEventBus;
            _logger = logger;
        }
        public async Task<Unit> Handle(RegisterProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogWarning("[LogWarning INFO] Registering ProductEntity");

            var productObject = new ProductEntity(request.Name, request.Email);
            _productCollection.Add(productObject);

            _logger.LogWarning("[Logger INFO] ProductEntity Registered");
            _logger.LogWarning("[Logger INFO] Publishing RegisteredProductEvent");
            _logger.LogWarning($"[Logger INFO] Sending Id: {productObject.Id}");

            await _rabbitMqEventBus.Publish(new RegisteredProductEvent(productObject.Id));

            _logger.LogWarning("[Logger INFO] RegisteredProductEvent Published With Success");
            return new Unit();
        }
    }
}
