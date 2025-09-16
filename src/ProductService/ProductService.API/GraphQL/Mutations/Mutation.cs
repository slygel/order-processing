using MediatR;
using ProductService.Application.UseCases.Commands;
using ProductService.Application.DTOs;
using ProductService.Application.Common;

namespace ProductService.API.GraphQL.Mutations;

[ExtendObjectType("Mutation")]
public class Mutation
{
    public async Task<Results<ProductDto>> CreateProduct([Service] IMediator mediator, CreateProductCommand command)
    {
        var result = await mediator.Send(command);
        return result;
    }
}
