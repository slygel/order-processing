using System;
using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.UseCases.Queries.GetOrderById;

public class GetOrderByIdQuery : IRequest<OrderDto?>
{
    public Guid OrderId { get; set; }
}

