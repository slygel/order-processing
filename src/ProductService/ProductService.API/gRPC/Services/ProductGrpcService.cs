using Grpc.Core;
using MediatR;
using ProductService.Application.UseCases.Queries;
using SharedEvent.Protos;

namespace ProductService.API.gRPC.Services;

public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IMediator _mediator;

    public ProductGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<GetProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        var productId = Guid.Parse(request.ProductId);
        var product = await _mediator.Send(new GetProductByIdQuery(productId));

        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.ProductId} not found"));
        }

        return new GetProductResponse
        {
            Product = new ProductInfo
            {
                Id = product.Id.ToString(),
                ProductName = product.ProductName,
                Price = (double)product.Price,
                CreatedAt = product.CreatedAt.ToString("O")
            }
        };
    }

    public override async Task<GetProductsResponse> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        var products = await _mediator.Send(new GetProductsQuery());
        var response = new GetProductsResponse();

        response.Products.AddRange(products.Select(p => new ProductInfo
        {
            Id = p.Id.ToString(),
            ProductName = p.ProductName,
            Price = (double)p.Price,
            CreatedAt = p.CreatedAt.ToString("O")
        }));

        return response;
    }
}
