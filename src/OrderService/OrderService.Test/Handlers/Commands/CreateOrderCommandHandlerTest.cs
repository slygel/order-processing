using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.UseCases.Commands.CreateOrder;
using OrderService.Application.UseCases.Commands.Handlers;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using OrderService.Domain.Enums;
using SharedEvent.Events;

namespace OrderService.Test.Handlers.Commands;

public class CreateOrderCommandHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTest()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productServiceMock = new Mock<IProductService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();

        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _productServiceMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WhenCommandIsValid()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();

        var product1Dto = new ProductDto
        {
            Id = product1Id,
            ProductName = "Product 1",
            Price = 100,
            CreatedAt = DateTime.UtcNow
        };

        var product2Dto = new ProductDto
        {
            Id = product2Id,
            ProductName = "Product 2",
            Price = 200,
            CreatedAt = DateTime.UtcNow
        };

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(product1Id, 2),
                new CreateOrderItemCommand(product2Id, 1)
            }
        };

        _productServiceMock
            .Setup(x => x.GetProductAsync(product1Id))
            .ReturnsAsync(product1Dto);
        _productServiceMock
            .Setup(x => x.GetProductAsync(product2Id))
            .ReturnsAsync(product2Dto);

        Order? savedOrder = null;
        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => savedOrder = order)
            .ReturnsAsync((Order order) => order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        
        var orderDto = result.Value;
        Assert.Equal(OrderStatus.Pending, orderDto.Status);
        Assert.Equal(2, orderDto.Items.Count);

        // Verify order items
        var item1 = orderDto.Items.First(i => i.ProductId == product1Id);
        Assert.Equal(2, item1.Quantity);
        Assert.Equal(100, item1.UnitPrice);
        Assert.Equal("Product 1", item1.ProductName);

        var item2 = orderDto.Items.First(i => i.ProductId == product2Id);
        Assert.Equal(1, item2.Quantity);
        Assert.Equal(200, item2.UnitPrice);
        Assert.Equal("Product 2", item2.ProductName);

        // Verify total amount calculation
        var expectedTotal = (2 * 100) + (1 * 200); // (2 * product1.Price) + (1 * product2.Price)
        Assert.Equal(expectedTotal, orderDto.Items.Sum(i => i.Quantity * i.UnitPrice));

        // Verify repository calls
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        // Verify event publication
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<OrderCreatedEvent>(e =>
                    e.OrderId == orderDto.Id &&
                    e.Items.Count == 2 &&
                    e.TotalAmount == expectedTotal
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        // Arrange
        var invalidProductId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(invalidProductId, 1)
            }
        };

        _productServiceMock
            .Setup(x => x.GetProductAsync(invalidProductId))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal($"Product with ID {invalidProductId} not found.", result.Error);

        // Verify no order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuantityIsZero()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            ProductName = "Test Product",
            Price = 100,
            CreatedAt = DateTime.UtcNow
        };

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(productId, 0)
            }
        };

        _productServiceMock
            .Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(productDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("Quantity must be greater than zero.", result.Error);

        // Verify no order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenQuantityIsNegative()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            ProductName = "Test Product",
            Price = 100,
            CreatedAt = DateTime.UtcNow
        };

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(productId, -1)
            }
        };

        _productServiceMock
            .Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(productDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("Quantity must be greater than zero.", result.Error);

        // Verify no order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoItemsInCommand()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("Order must contain at least one item.", result.Error);

        // Verify no order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenItemsIsNull()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = null!
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("Order must contain at least one item.", result.Error);

        // Verify no order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrowsException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            ProductName = "Test Product",
            Price = 100,
            CreatedAt = DateTime.UtcNow
        };

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(productId, 1)
            }
        };

        _productServiceMock
            .Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(productDto);

        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("Unexpected error: Database connection failed", result.Error);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderWithSingleItem_WhenCommandIsValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            ProductName = "Single Product",
            Price = 250.50m,
            CreatedAt = DateTime.UtcNow
        };

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(productId, 3)
            }
        };

        _productServiceMock
            .Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(productDto);

        Order? savedOrder = null;
        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => savedOrder = order)
            .ReturnsAsync((Order order) => order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        
        var orderDto = result.Value;
        Assert.Equal(OrderStatus.Pending, orderDto.Status);
        Assert.Single(orderDto.Items);

        var item = orderDto.Items.First();
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(250.50m, item.UnitPrice);
        Assert.Equal("Single Product", item.ProductName);

        // Verify total amount
        var expectedTotal = 3 * 250.50m;
        Assert.Equal(expectedTotal, orderDto.Items.Sum(i => i.Quantity * i.UnitPrice));

        // Verify repository calls
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        // Verify event publication
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<OrderCreatedEvent>(e =>
                    e.OrderId == orderDto.Id &&
                    e.Items.Count == 1 &&
                    e.TotalAmount == expectedTotal
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldHandleDuplicateProductIds_WhenCommandIsValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            ProductName = "Duplicate Product",
            Price = 100,
            CreatedAt = DateTime.UtcNow
        };

        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(productId, 2),
                new CreateOrderItemCommand(productId, 1) // Same product ID
            }
        };

        _productServiceMock
            .Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(productDto);

        Order? savedOrder = null;
        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => savedOrder = order)
            .ReturnsAsync((Order order) => order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        
        var orderDto = result.Value;
        Assert.Equal(OrderStatus.Pending, orderDto.Status);
        Assert.Equal(2, orderDto.Items.Count); // Should create two separate order items

        // Verify both items have the same product
        Assert.All(orderDto.Items, item => 
        {
            Assert.Equal(productId, item.ProductId);
            Assert.Equal("Duplicate Product", item.ProductName);
            Assert.Equal(100, item.UnitPrice);
        });

        // One item should have quantity 2, the other quantity 1
        var quantities = orderDto.Items.Select(i => i.Quantity).OrderBy(q => q).ToList();
        Assert.Equal(new[] { 1, 2 }, quantities);

        // Verify product service was called only once (due to distinct product IDs)
        _productServiceMock.Verify(x => x.GetProductAsync(productId), Times.Once);
    }
}
