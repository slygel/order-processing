using MediatR;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Application.UseCases.Commands.Handlers;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;

    public CreateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Check if product with same name already exists
        var existingProduct = await _productRepository.GetByNameAsync(request.ProductName.Trim().ToLower());
        if (existingProduct != null)
        {
            throw new InvalidOperationException($"Product with name '{request.ProductName}' already exists");
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

        return new ProductDto
        {
            Id = product.Id,
            ProductName = product.ProductName,
            Price = product.Price,
            CreatedAt = product.CreatedAt
        };
    }
}
