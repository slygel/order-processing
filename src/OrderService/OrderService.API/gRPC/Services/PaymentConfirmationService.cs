using System;
using Grpc.Core;
using MediatR;
using OrderService.Application.UseCases.Commands.ConfirmPayment;

namespace OrderService.API.gRPC.Services;

using SharedEvent.Protos;

public class PaymentConfirmationService : OrderService.OrderServiceBase
{
    private readonly IMediator _mediator;

    public PaymentConfirmationService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<ConfirmPaymentResponse> ConfirmPayment(ConfirmPaymentRequest request, ServerCallContext context)
    {
        var Commands = new ConfirmPaymentCommand(Guid.Parse(request.OrderId));

        var result = await _mediator.Send(Commands);

        return new ConfirmPaymentResponse
        {
            Success = result
        };
    }
}
