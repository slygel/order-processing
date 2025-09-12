using System;
using SharedEvent.Protos;

namespace PaymentService.API.gRPC.Clients;

public class OrderServiceClient
{
    private readonly OrderService.OrderServiceClient _client;
    private readonly ILogger<OrderServiceClient> _logger;

    public OrderServiceClient(OrderService.OrderServiceClient client, ILogger<OrderServiceClient> logger)
    {
        _client = client;
        _logger = logger;
    }
    public async Task<bool> ConfirmPaymentAsync(Guid orderId)
    {
        _logger.LogInformation("Calling OrderService gRPC ConfirmPayment for {OrderId}", orderId);

        var response = await _client.ConfirmPaymentAsync(new ConfirmPaymentRequest
        {
            OrderId = orderId.ToString()
        });

        _logger.LogInformation("Response {response}", response);

        return response.Success;
    }
}
