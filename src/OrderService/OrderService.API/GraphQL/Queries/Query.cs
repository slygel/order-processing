using MediatR;
using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Queries.GetOrders;
using OrderService.Application.UseCases.Queries.GetOrderById;

namespace OrderService.API.GraphQL.Queries;

public class Query
{
    public async Task<IEnumerable<OrderDto>> GetOrders([Service] IMediator mediator)
    {
        return await mediator.Send(new GetOrdersQuery());
    }

    public async Task<OrderDto?> GetOrderById([Service] IMediator mediator, Guid id)
    {
        return await mediator.Send(new GetOrderByIdQuery { OrderId = id });
    }
}
