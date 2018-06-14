using MediatR;

namespace RabbitMQSample.Product.Application.Commands
{
    public class RegisterProductCommand : IRequest
    {
        public RegisterProductCommand(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
    }
}
