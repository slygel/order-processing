using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.UseCases.Queries;
using ProductService.Application.UseCases.Queries.Handlers;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Test.Handlers.Queries;

public class GetProductsQueryTest
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryTest()
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
                Price = 99.99m,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Product 2",
                Price = 149.99m,
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

        // Verify repository call
        _productRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
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
        _productRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPreserveOrder_WhenReturningProducts()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var products = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "Z Product", // Alphabetically last
                Price = 99.99m,
                CreatedAt = now.AddDays(-2)
            },
            new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "A Product", // Alphabetically first
                Price = 149.99m,
                CreatedAt = now.AddDays(-1)
            },
            new Product
            {
                Id = Guid.NewGuid(),
                ProductName = "M Product", // Alphabetically middle
                Price = 199.99m,
                CreatedAt = now
            }
        };

        _productRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        var productDtos = result.ToList();
        Assert.Equal(3, productDtos.Count);

        // Verify order is preserved
        Assert.Collection(productDtos,
            dto => Assert.Equal("Z Product", dto.ProductName),
            dto => Assert.Equal("A Product", dto.ProductName),
            dto => Assert.Equal("M Product", dto.ProductName)
        );

        _productRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var expectedError = new Exception("Database connection error");

        _productRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ThrowsAsync(expectedError);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(new GetProductsQuery(), CancellationToken.None)
        );

        Assert.Equal(expectedError.Message, exception.Message);
        _productRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllProperties_ForEachProduct()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var testProducts = Enumerable.Range(1, 5).Select(i => new Product
        {
            Id = Guid.NewGuid(),
            ProductName = $"Product {i}",
            Price = i * 100m,
            CreatedAt = now.AddDays(-i)
        }).ToList();

        _productRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(testProducts);

        // Act
        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        var productDtos = result.ToList();
        Assert.Equal(testProducts.Count, productDtos.Count);

        // Verify each product's properties
        for (int i = 0; i < testProducts.Count; i++)
        {
            var product = testProducts[i];
            var dto = productDtos[i];

            Assert.Equal(product.Id, dto.Id);
            Assert.Equal(product.ProductName, dto.ProductName);
            Assert.Equal(product.Price, dto.Price);
            Assert.Equal(product.CreatedAt, dto.CreatedAt);
        }

        _productRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }
}
