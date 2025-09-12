using System;
using MediatR;

namespace OrderService.Application.UseCases.Commands.ConfirmPayment;

public class ConfirmPaymentCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }

    public ConfirmPaymentCommand() {}
    
    public ConfirmPaymentCommand(Guid orderId)
    {
        OrderId = orderId;
    }
}
