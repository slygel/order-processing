using System;
using PaymentService.API.gRPC.Clients;
using PaymentService.API.Interfaces;
using SharedEvent.Events;

namespace PaymentService.API.Services;

public class PaymentProcessor : IPaymentProcessor
{
    private readonly OrderServiceClient _orderServiceClient;
    private readonly ILogger<PaymentProcessor> _logger;

    public PaymentProcessor(OrderServiceClient orderServiceClient, ILogger<PaymentProcessor> logger)
    {
        _orderServiceClient = orderServiceClient;
        _logger = logger;
    }

    public async Task ProcessPaymentAsync(OrderCreatedEvent orderCreatedEvent)
    {
        _logger.LogInformation("Simulating payment for OrderId {OrderId}", orderCreatedEvent.OrderId);

        await Task.Delay(TimeSpan.FromSeconds(5));

        // Call OrderService via gRPC
        var success = await _orderServiceClient.ConfirmPaymentAsync(orderCreatedEvent.OrderId);

        if (success)
        {
            _logger.LogInformation("Payment confirmed for OrderId {OrderId}", orderCreatedEvent.OrderId);
        }
        else
        {
            _logger.LogWarning("Payment confirmation failed for OrderId {OrderId}", orderCreatedEvent.OrderId);
        }
    }
}
