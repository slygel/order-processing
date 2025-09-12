using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.UseCases.Commands.ConfirmPayment;
using OrderService.Application.UseCases.Commands.Handlers;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

namespace OrderService.Test.Handlers.Commands;

public class ConfirmPaymentCommandHandlerTest
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ILogger<ConfirmPaymentCommandHandler>> _loggerMock;
    private readonly ConfirmPaymentCommandHandler _handler;

    public ConfirmPaymentCommandHandlerTest()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<ConfirmPaymentCommandHandler>>();

        _handler = new ConfirmPaymentCommandHandler(
            _orderRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldConfirmPayment_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var command = new ConfirmPaymentCommand { OrderId = orderId };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(OrderStatus.Paid, order.Status);

        // Verify the order was saved
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ConfirmPaymentCommand { OrderId = orderId };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);

        // Verify no changes were saved
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenOrderIsAlreadyPaid()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Paid,
            CreatedAt = DateTime.UtcNow
        };

        var command = new ConfirmPaymentCommand { OrderId = orderId };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(OrderStatus.Paid, order.Status);

        // Verify no changes were saved
        _orderRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
