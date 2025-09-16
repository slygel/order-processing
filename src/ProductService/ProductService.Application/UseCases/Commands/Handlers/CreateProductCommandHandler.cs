using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Application.UseCases.Commands.Handlers;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Results<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public CreateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Results<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if product with same name already exists
            var existingProduct = await _productRepository.GetByNameAsync(request.ProductName.Trim().ToLower());
            if (existingProduct != null)
            {
                return Results<ProductDto>.Failure($"Product with name '{request.ProductName}' already exists",400);
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = request.ProductName,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            var productDto = new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Price = product.Price,
                CreatedAt = product.CreatedAt
            };

            return Results<ProductDto>.Success(productDto, 200);
        }
        catch (Exception ex)
        {
            return Results<ProductDto>.Failure($"An error occurred while creating the product: {ex.Message}", 500);
        }
    }
}
