using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentService.API.Consumers;
using PaymentService.API.Interfaces;
using SharedEvent.Events;

namespace PaymentService.Test.Consumers;

public class OrderCreatedConsumerTest
{
    private readonly Mock<IPaymentProcessor> _paymentProcessorMock;
    private readonly Mock<ILogger<OrderCreatedConsumer>> _loggerMock;
    private readonly OrderCreatedConsumer _consumer;
    private readonly Mock<ConsumeContext<OrderCreatedEvent>> _consumeContextMock;

    public OrderCreatedConsumerTest()
    {
        _paymentProcessorMock = new Mock<IPaymentProcessor>();
        _loggerMock = new Mock<ILogger<OrderCreatedConsumer>>();
        _consumer = new OrderCreatedConsumer(_paymentProcessorMock.Object, _loggerMock.Object);
        _consumeContextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();
    }

    [Fact]
    public async Task Consume_ShouldProcessPayment_WhenMessageIsValid()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var totalAmount = 300m;
        var productId = Guid.NewGuid();
        var quantity = 2;
        var unitPrice = 150;
        var now = DateTime.UtcNow;

        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = orderId,
            CreatedAt = now,
            TotalAmount = totalAmount,
            Items = new List<OrderItemEvent>
            {
                new OrderItemEvent
                {
                    ProductId = productId,
                    ProductName = "Test Product",
                    Quantity = quantity,
                    UnitPrice = unitPrice
                }
            }
        };

        _consumeContextMock
            .Setup(x => x.Message)
            .Returns(orderCreatedEvent);

        var expectedReceivedMessage = $"Payment Service received OrderCreated for OrderId: {orderId}";

        var sequence = new MockSequence();
        _loggerMock.InSequence(sequence)
            .Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedReceivedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _paymentProcessorMock
            .Setup(x => x.ProcessPaymentAsync(orderCreatedEvent))
            .Returns(Task.CompletedTask);

        // Act
        await _consumer.Consume(_consumeContextMock.Object);

        // Assert
        // Verify message was received
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedReceivedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Should log message received"
        );

        // Verify payment was processed
        _paymentProcessorMock.Verify(
            x => x.ProcessPaymentAsync(orderCreatedEvent),
            Times.Once,
            "Should process payment exactly once"
        );

        // Verify no error logs
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "No error logs should be generated for successful processing"
        );
    }

    [Fact]
    public async Task Consume_ShouldLogErrorAndRethrow_WhenProcessingFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = 300,
            Items = new List<OrderItemEvent>
            {
                new OrderItemEvent
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 2,
                    UnitPrice = 150
                }
            }
        };

        _consumeContextMock
            .Setup(x => x.Message)
            .Returns(orderCreatedEvent);

        var expectedException = new Exception("Payment processing failed");
        _paymentProcessorMock
            .Setup(x => x.ProcessPaymentAsync(orderCreatedEvent))
            .ThrowsAsync(expectedException);

        var expectedErrorMessage = $"Failed to process payment for OrderId: {orderId}";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _consumer.Consume(_consumeContextMock.Object)
        );

        // Verify the exception was the one we threw
        Assert.Same(expectedException, exception);

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedErrorMessage)),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Should log error when payment processing fails"
        );
    }
}
