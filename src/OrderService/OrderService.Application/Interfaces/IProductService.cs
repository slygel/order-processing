using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetProductAsync(Guid productId);
}
