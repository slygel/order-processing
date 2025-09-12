using System;
using MediatR;
using OrderService.Application.DTOs;
using OrderService.Application.UseCases.Queries.GetOrderById;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.UseCases.Queries.Handlers;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return null;
        }

        return new OrderDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            
            Status = order.Status,
            Items = order.OrderItems.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }
}