using Moq;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.UseCases.Commands;
using ProductService.Application.UseCases.Commands.Handlers;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;

namespace ProductService.Test.Handlers.Commands;

public class CreateProductCommandHandlerTest
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTest()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new CreateProductCommandHandler(_productRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenInputIsValid()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", 99.99m);

        Product? savedProduct = null;
        _productRepositoryMock
            .Setup(x => x.GetByNameAsync(command.ProductName.Trim().ToLower()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(product => savedProduct = product)
            .ReturnsAsync((Product product) => product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(200, result.StatusCode);

        var productDto = result.Value;
        Assert.Equal(command.ProductName, productDto.ProductName);
        Assert.Equal(command.Price, productDto.Price);
        Assert.NotEqual(Guid.Empty, productDto.Id);
        Assert.NotEqual(default, productDto.CreatedAt);
        Assert.True(DateTime.UtcNow.AddMinutes(-1) <= productDto.CreatedAt
            && productDto.CreatedAt <= DateTime.UtcNow);

        // Verify repository calls
        _productRepositoryMock.Verify(x => x.GetByNameAsync(command.ProductName.Trim().ToLower()), Times.Once);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);
        _productRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        // Verify saved product
        Assert.NotNull(savedProduct);
        Assert.Equal(command.ProductName, savedProduct.ProductName);
        Assert.Equal(command.Price, savedProduct.Price);
        Assert.Equal(productDto.Id, savedProduct.Id);
        Assert.Equal(productDto.CreatedAt, savedProduct.CreatedAt);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductNameAlreadyExists()
    {
        // Arrange
        var productName = "Existing Product";
        var command = new CreateProductCommand(productName, 99.99m);

        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            ProductName = productName,
            Price = 149.99m,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _productRepositoryMock
            .Setup(x => x.GetByNameAsync(command.ProductName.Trim().ToLower()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal($"Product with name '{productName}' already exists", result.Error);

        // Verify repository calls
        _productRepositoryMock.Verify(x => x.GetByNameAsync(command.ProductName.Trim().ToLower()), Times.Once);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Never);
        _productRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrows()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", 99.99m);
        var expectedError = new Exception("Database connection error");

        _productRepositoryMock
            .Setup(x => x.GetByNameAsync(command.ProductName.Trim().ToLower()))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .ThrowsAsync(expectedError);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal($"Unexpected error: {expectedError.Message}", result.Error);

        // Verify repository calls
        _productRepositoryMock.Verify(x => x.GetByNameAsync(command.ProductName.Trim().ToLower()), Times.Once);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);
        _productRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public async Task Handle_ShouldReturnFailure_WhenPriceIsInvalid(decimal price)
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", price);

        _productRepositoryMock
            .Setup(x => x.GetByNameAsync(command.ProductName.Trim().ToLower()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Product price must be greater than zero", result.Error);

        // Verify no repository calls were made after validation
        _productRepositoryMock.Verify(x => x.GetByNameAsync(command.ProductName.Trim().ToLower()), Times.Never);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Never);
        _productRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData("  Test Product  ", "test product")]
    [InlineData("TEST PRODUCT", "test product")]
    [InlineData("Test  Product", "test product")]
    public async Task Handle_ShouldNormalizeProductName_WhenCreatingProduct(string inputName, string normalizedName)
    {
        // Arrange
        var command = new CreateProductCommand(inputName, 99.99m);
        Product? savedProduct = null;

        _productRepositoryMock
            .Setup(x => x.GetByNameAsync(normalizedName))
            .ReturnsAsync((Product?)null);

        _productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(product => savedProduct = product)
            .ReturnsAsync((Product product) => product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(savedProduct);

        // Verify the product name was normalized correctly
        Assert.Equal(inputName.Trim(), savedProduct.ProductName); // Original name is preserved
        _productRepositoryMock.Verify(x => x.GetByNameAsync(normalizedName), Times.Once); // Normalized for lookup

        // Verify repository calls
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);
        _productRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
