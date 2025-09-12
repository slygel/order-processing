using MediatR;
using Microsoft.AspNetCore.Mvc; 
using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Commands.CreateOrder;
using OrderService.Application.UseCases.Queries.GetOrderById;
using OrderService.Application.UseCases.Queries.GetOrders;

namespace OrderService.API.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderCommand command)
        {
            var order = await _mediator.Send(command);
            return Ok(order);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var query = new GetOrdersQuery();
            var orders = await _mediator.Send(query);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto?>> GetOrderById(Guid id)
        {
            var order = await _mediator.Send(new GetOrderByIdQuery { OrderId = id });
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
    }
}
