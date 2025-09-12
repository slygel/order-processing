using Moq;
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
        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            ProductName = "Product 1",
            Price = 100,
            CreatedAt = DateTime.UtcNow
        };

        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            ProductName = "Product 2",
            Price = 200,
            CreatedAt = DateTime.UtcNow
        };

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product1.Id,
                        Product = product1,
                        Quantity = 2,
                        UnitPrice = product1.Price
                    }
                }
            },
            new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Paid,
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product1.Id,
                        Product = product1,
                        Quantity = 1,
                        UnitPrice = product1.Price
                    },
                    new OrderItem
                    {
                        ProductId = product2.Id,
                        Product = product2,
                        Quantity = 3,
                        UnitPrice = product2.Price
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

        // Verify first order
        var order1 = orders[0];
        var dto1 = orderDtos[0];
        Assert.Equal(order1.Id, dto1.Id);
        Assert.Equal(order1.Status, dto1.Status);
        Assert.Equal(order1.CreatedAt, dto1.CreatedAt);
        Assert.Single(dto1.Items);

        var item1 = dto1.Items[0];
        Assert.Equal(product1.Id, item1.ProductId);
        Assert.Equal(product1.ProductName, item1.ProductName);
        Assert.Equal(2, item1.Quantity);
        Assert.Equal(product1.Price, item1.UnitPrice);

        // Verify second order
        var order2 = orders[1];
        var dto2 = orderDtos[1];
        Assert.Equal(order2.Id, dto2.Id);
        Assert.Equal(order2.Status, dto2.Status);
        Assert.Equal(order2.CreatedAt, dto2.CreatedAt);
        Assert.Equal(2, dto2.Items.Count);

        var item2_1 = dto2.Items[0];
        Assert.Equal(product1.Id, item2_1.ProductId);
        Assert.Equal(product1.ProductName, item2_1.ProductName);
        Assert.Equal(1, item2_1.Quantity);
        Assert.Equal(product1.Price, item2_1.UnitPrice);

        var item2_2 = dto2.Items[1];
        Assert.Equal(product2.Id, item2_2.ProductId);
        Assert.Equal(product2.ProductName, item2_2.ProductName);
        Assert.Equal(3, item2_2.Quantity);
        Assert.Equal(product2.Price, item2_2.UnitPrice);
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
    }
}
