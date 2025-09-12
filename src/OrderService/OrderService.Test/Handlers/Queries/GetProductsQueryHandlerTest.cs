using Moq;
using OrderService.Application.UseCases.Queries.GetProducts;
using OrderService.Application.UseCases.Queries.Handlers;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Test.Handlers.Queries;

public class GetProductsQueryHandlerTest
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTest()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductsQueryHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllProducts_WhenProductsExist()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Product 1",
                Price = 100,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Product 2",
                Price = 200,
                CreatedAt = DateTime.UtcNow
            }
        };

        _productRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        var productDtos = result.ToList();
        Assert.Equal(2, productDtos.Count);

        // Verify first product
        var product1 = products[0];
        var dto1 = productDtos[0];
        Assert.Equal(product1.Id, dto1.Id);
        Assert.Equal(product1.ProductName, dto1.ProductName);
        Assert.Equal(product1.Price, dto1.Price);
        Assert.Equal(product1.CreatedAt, dto1.CreatedAt);

        // Verify second product
        var product2 = products[1];
        var dto2 = productDtos[1];
        Assert.Equal(product2.Id, dto2.Id);
        Assert.Equal(product2.ProductName, dto2.ProductName);
        Assert.Equal(product2.Price, dto2.Price);
        Assert.Equal(product2.CreatedAt, dto2.CreatedAt);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoProductsExist()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Product>());

        // Act
        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
