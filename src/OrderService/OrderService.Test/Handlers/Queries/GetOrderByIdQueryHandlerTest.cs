using Moq;
using OrderService.Application.UseCases.Queries.GetOrderById;
using OrderService.Application.UseCases.Queries.Handlers;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

namespace OrderService.Test.Handlers.Queries;

public class GetOrderByIdQueryHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTest()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrderByIdQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = productId1,
                    Product = new Product
                    {
                        Id = productId1,
                        ProductName = "Product 1",
                        Price = 100
                    },
                    Quantity = 2,
                    UnitPrice = 100
                },
                new OrderItem
                {
                    ProductId = productId2,
                    Product = new Product
                    {
                        Id = productId2,
                        ProductName = "Product 2",
                        Price = 200
                    },
                    Quantity = 1,
                    UnitPrice = 200
                }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        var query = new GetOrderByIdQuery { OrderId = orderId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        Assert.Equal(OrderStatus.Pending, result.Status);
        Assert.Equal(order.CreatedAt, result.CreatedAt);

        // Verify order items
        Assert.Equal(2, result.Items.Count);

        // Verify first item
        var item1 = result.Items[0];
        Assert.Equal(productId1, item1.ProductId);
        Assert.Equal("Product 1", item1.ProductName);
        Assert.Equal(2, item1.Quantity);
        Assert.Equal(100, item1.UnitPrice);

        // Verify second item
        var item2 = result.Items[1];
        Assert.Equal(productId2, item2.ProductId);
        Assert.Equal("Product 2", item2.ProductName);
        Assert.Equal(1, item2.Quantity);
        Assert.Equal(200, item2.UnitPrice);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        var query = new GetOrderByIdQuery { OrderId = orderId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
