using System;
using MediatR;
using OrderService.Application.Common;
using OrderService.Application.DTOs;

namespace OrderService.Application.UseCases.Commands.CreateOrder;

public record CreateOrderItemCommand(Guid ProductId, int Quantity);

public class CreateOrderCommand : IRequest<Results<OrderDto>>
{
    public List<CreateOrderItemCommand> Items { get; set; } = new();
}

