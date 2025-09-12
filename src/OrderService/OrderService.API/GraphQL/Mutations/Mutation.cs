using MediatR;
using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Commands.CreateOrder;

namespace OrderService.API.GraphQL.Mutations;

public class Mutation
{
    public async Task<OrderDto> CreateOrder([Service] IMediator mediator, CreateOrderCommand command)
    {
        return await mediator.Send(command);
    }
}
