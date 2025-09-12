using HotChocolate;
using MediatR;
using ProductService.Application.UseCases.Queries;
using ProductService.Application.DTOs;

namespace ProductService.API.GraphQL.Queries;

[ExtendObjectType("Query")]
public class Query
{
    public async Task<IEnumerable<ProductDto>> GetProducts([Service] IMediator mediator)
    {
        return await mediator.Send(new GetProductsQuery());
    }

    public async Task<ProductDto?> GetProductById([Service] IMediator mediator, Guid id)
    {
        return await mediator.Send(new GetProductByIdQuery(id));
    }
}
