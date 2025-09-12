using System;
using MassTransit;
using PaymentService.API.Interfaces;
using SharedEvent.Events;

namespace PaymentService.API.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        IPaymentProcessor paymentProcessor,
        ILogger<OrderCreatedConsumer> logger)
    {
        _paymentProcessor = paymentProcessor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var orderCreatedEvent = context.Message;

        _logger.LogInformation("Payment Service received OrderCreated for OrderId: {OrderId}", orderCreatedEvent.OrderId);

        try
        {
            await _paymentProcessor.ProcessPaymentAsync(orderCreatedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment for OrderId: {OrderId}", 
                orderCreatedEvent.OrderId);
            throw;
        }
    }
}
