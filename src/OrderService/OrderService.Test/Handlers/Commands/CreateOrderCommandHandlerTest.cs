using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.UseCases.Commands.CreateOrder;
using OrderService.Application.UseCases.Commands.Handlers;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using OrderService.Domain.Enums;
using SharedEvent.Events;
using ProductService.Domain.Interfaces;
using ProductService.Domain.Entities;

namespace OrderService.Test.Handlers.Commands;

public class CreateOrderCommandHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTest()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();

        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object,
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

        var product1 = new Product
        {
            Id = product1Id,
            ProductName = "Product 1",
            Price = 100,
            CreatedAt = DateTime.UtcNow
        };

        var product2 = new Product
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

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product1Id))
            .ReturnsAsync(product1);
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(product2Id))
            .ReturnsAsync(product2);

        Order? savedOrder = null;
        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => savedOrder = order)
            .ReturnsAsync((Order order) => order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal(2, result.Items.Count);

        // Verify order items
        var item1 = result.Items.First(i => i.ProductId == product1Id);
        Assert.Equal(2, item1.Quantity);
        Assert.Equal(100, item1.UnitPrice);
        Assert.Equal("Product 1", item1.ProductName);

        var item2 = result.Items.First(i => i.ProductId == product2Id);
        Assert.Equal(1, item2.Quantity);
        Assert.Equal(200, item2.UnitPrice);
        Assert.Equal("Product 2", item2.ProductName);

        // Verify total amount calculation
        var expectedTotal = (2 * 100) + (1 * 200); // (2 * product1.Price) + (1 * product2.Price)
        Assert.Equal(expectedTotal, result.Items.Sum(i => i.Quantity * i.UnitPrice));

        // Verify repository calls
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

        // Verify event publication
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<OrderCreatedEvent>(e =>
                    e.OrderId == result.Id &&
                    e.Items.Count == 2 &&
                    e.TotalAmount == expectedTotal
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductDoesNotExist()
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

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidProductId))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        Assert.Equal($"Product with ID {invalidProductId} not found.", exception.Message);

        // Verify no order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenQuantityIsZero()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
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

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        Assert.Equal("Quantity must be greater than zero.", exception.Message);

        // Verify no order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenQuantityIsNegative()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
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

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        Assert.Equal("Quantity must be greater than zero.", exception.Message);

        // Verify no order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNoItemsInCommand()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        Assert.Equal("Order must contain at least one item.", exception.Message);

        // Verify no order was created
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
