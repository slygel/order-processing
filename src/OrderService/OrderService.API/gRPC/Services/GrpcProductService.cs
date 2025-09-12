using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using SharedEvent.Protos;

namespace OrderService.API.gRPC.Services;

public class GrpcProductService : IProductService
{
    private readonly ProductGrpc.ProductGrpcClient _client;

    public GrpcProductService(IOptions<GrpcSettings> settings)
    {
        var channel = GrpcChannel.ForAddress(settings.Value.ProductServiceUrl);
        _client = new ProductGrpc.ProductGrpcClient(channel);
    }

    public async Task<ProductDto?> GetProductAsync(Guid productId)
    {
        try
        {
            var response = await _client.GetProductAsync(new GetProductRequest 
            { 
                ProductId = productId.ToString() 
            });

            return new ProductDto
            {
                Id = Guid.Parse(response.Product.Id),
                ProductName = response.Product.ProductName,
                Price = (decimal)response.Product.Price,
                CreatedAt = DateTime.Parse(response.Product.CreatedAt)
            };
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
    }
}
