using HotChocolate;
using MediatR;
using ProductService.Application.UseCases.Commands;
using ProductService.Application.DTOs;

namespace ProductService.API.GraphQL.Mutations;

[ExtendObjectType("Mutation")]
public class Mutation
{
    public async Task<ProductDto> CreateProduct([Service] IMediator mediator, CreateProductCommand command)
    {
        return await mediator.Send(command);
    }
}
