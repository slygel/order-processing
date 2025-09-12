using System;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.UseCases.Commands.ConfirmPayment;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.UseCases.Commands.Handlers;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<ConfirmPaymentCommandHandler> _logger;

    public ConfirmPaymentCommandHandler(
        IOrderRepository orderRepository,
        ILogger<ConfirmPaymentCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }
    public async Task<bool> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);

        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found when confirming payment", request.OrderId);
            return false;
        }

        order.Status = OrderStatus.Paid;

        await _orderRepository.SaveChangesAsync();
        _logger.LogInformation("Order {OrderId} is {OrderStatus}", request.OrderId, order.Status);

        return true;
    }
}
