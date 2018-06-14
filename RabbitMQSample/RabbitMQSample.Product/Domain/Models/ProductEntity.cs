using System;

namespace RabbitMQSample.Product.Domain.Models
{
    public class ProductEntity
    {
        public ProductEntity(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string Name { get; private set; }
        public string Email { get; private set; }
    }
}
