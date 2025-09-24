using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.UseCases.Queries;
using ProductService.Application.UseCases.Queries.Handlers;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Test.Handlers.Queries;

public class GetProductByIdQueryTest
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryTest()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByIdQueryHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            ProductName = "Test Product",
            Price = 99.99m,
            CreatedAt = DateTime.UtcNow
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.ProductName, result.ProductName);
        Assert.Equal(product.Price, result.Price);
        Assert.Equal(product.CreatedAt, result.CreatedAt);

        // Verify repository call
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        var query = new GetProductByIdQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenIdIsEmpty()
    {
        // Arrange
        var query = new GetProductByIdQuery(Guid.Empty);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(Guid.Empty))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
        _productRepositoryMock.Verify(x => x.GetByIdAsync(Guid.Empty), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var expectedError = new Exception("Database connection error");

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ThrowsAsync(expectedError);

        var query = new GetProductByIdQuery(productId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(query, CancellationToken.None)
        );

        Assert.Equal(expectedError.Message, exception.Message);
        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId), Times.Once);
    }
}