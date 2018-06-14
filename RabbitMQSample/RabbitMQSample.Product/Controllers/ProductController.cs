using MediatR;
using Microsoft.AspNetCore.Mvc;
using RabbitMQSample.Product.Application.Commands;
using System.Threading.Tasks;

namespace RabbitMQSample.Product.Controllers
{
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> RegisterProduct([FromBody] RegisterProductCommand productCommand, [FromServices]IMediator mediator)
        {
            await mediator.Send(productCommand);
            return Ok();
        }
    }
}
