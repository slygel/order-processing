using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Queries.GetOrders;
using OrderService.Application.UseCases.Queries.Handlers;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

namespace OrderService.Test.Handlers.Queries;

public class GetOrdersQueryHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTest()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrdersQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllOrders_WhenOrdersExist()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var order1Id = Guid.NewGuid();
        var order2Id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var orders = new List<Order>
        {
            new Order
            {
                Id = order1Id,
                Status = OrderStatus.Pending,
                CreatedAt = now.AddDays(-1),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order1Id,
                        ProductId = productId1,
                        ProductName = "Product 1",
                        Quantity = 2,
                        UnitPrice = 100
                    }
                }
            },
            new Order
            {
                Id = order2Id,
                Status = OrderStatus.Paid,
                CreatedAt = now,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order2Id,
                        ProductId = productId1,
                        ProductName = "Product 1",
                        Quantity = 1,
                        UnitPrice = 100
                    },
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order2Id,
                        ProductId = productId2,
                        ProductName = "Product 2",
                        Quantity = 3,
                        UnitPrice = 200
                    }
                }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        // Assert
        var orderDtos = result.ToList();
        Assert.Equal(2, orderDtos.Count);

        // Verify first order (Pending order with one item)
        var order1 = orders[0];
        var dto1 = orderDtos[0];
        Assert.Equal(order1.Id, dto1.Id);
        Assert.Equal(OrderStatus.Pending, dto1.Status);
        Assert.Equal(now.AddDays(-1), dto1.CreatedAt);
        Assert.Single(dto1.Items);

        var item1 = dto1.Items[0];
        Assert.Equal(productId1, item1.ProductId);
        Assert.Equal("Product 1", item1.ProductName);
        Assert.Equal(2, item1.Quantity);
        Assert.Equal(100, item1.UnitPrice);
        Assert.Equal(200, item1.Quantity * item1.UnitPrice); // Subtotal

        // Verify second order (Paid order with two items)
        var order2 = orders[1];
        var dto2 = orderDtos[1];
        Assert.Equal(order2.Id, dto2.Id);
        Assert.Equal(OrderStatus.Paid, dto2.Status);
        Assert.Equal(now, dto2.CreatedAt);
        Assert.Equal(2, dto2.Items.Count);

        // Verify first item of second order
        var item2_1 = dto2.Items[0];
        Assert.Equal(productId1, item2_1.ProductId);
        Assert.Equal("Product 1", item2_1.ProductName);
        Assert.Equal(1, item2_1.Quantity);
        Assert.Equal(100, item2_1.UnitPrice);
        Assert.Equal(100, item2_1.Quantity * item2_1.UnitPrice); // Subtotal

        // Verify second item of second order
        var item2_2 = dto2.Items[1];
        Assert.Equal(productId2, item2_2.ProductId);
        Assert.Equal("Product 2", item2_2.ProductName);
        Assert.Equal(3, item2_2.Quantity);
        Assert.Equal(200, item2_2.UnitPrice);
        Assert.Equal(600, item2_2.Quantity * item2_2.UnitPrice); // Subtotal

        // Verify order totals
        Assert.Equal(200, dto1.Items.Sum(i => i.Quantity * i.UnitPrice));
        Assert.Equal(700, dto2.Items.Sum(i => i.Quantity * i.UnitPrice));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersExist()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Order>());

        // Act
        var result = await _handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result);
        _orderRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryThrows()
    {
        // Arrange
        var expectedError = new Exception("Database connection error");

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ThrowsAsync(expectedError);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _handler.Handle(new GetOrdersQuery(), CancellationToken.None)
        );

        Assert.Equal(expectedError.Message, exception.Message);
        _orderRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrder_WithEmptyItemsList_WhenOrderHasNoItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var orders = new List<Order>
        {
            new Order
            {
                Id = orderId,
                Status = OrderStatus.Pending,
                CreatedAt = now,
                OrderItems = new List<OrderItem>()
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        // Assert
        var orderDtos = result.ToList();
        Assert.Single(orderDtos);

        var dto = orderDtos[0];
        Assert.Equal(orderId, dto.Id);
        Assert.Equal(OrderStatus.Pending, dto.Status);
        Assert.Equal(now, dto.CreatedAt);
        Assert.Empty(dto.Items);
        Assert.Equal(0, dto.Items.Sum(i => i.Quantity * i.UnitPrice));
    }
}
