using System;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.UseCases.Commands.CreateOrder;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Interfaces;
using SharedEvent.Events;

namespace OrderService.Application.UseCases.Commands.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductService _productService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductService productService,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _productService = productService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
        {
            throw new InvalidOperationException("Order must contain at least one item.");
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = new Dictionary<Guid, ProductDto>();

        foreach (var productId in productIds)
        {
            var product = await _productService.GetProductAsync(productId)
                ?? throw new InvalidOperationException($"Product with ID {productId} not found.");
            products[productId] = product;
        }

        // Create new order
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>()
        };

        // Create order items
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException($"Quantity must be greater than zero.");
            }

            var productDto = products[item.ProductId];
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = productDto.Id,
                Quantity = item.Quantity,
                UnitPrice = productDto.Price,
                ProductName = productDto.ProductName
            };
            order.OrderItems.Add(orderItem);
        }

        var createdOrder = await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();
        // Publish event
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = createdOrder.Id,
            CreatedAt = createdOrder.CreatedAt,
            TotalAmount = createdOrder.TotalAmount,
            Items = createdOrder.OrderItems.Select(item => new OrderItemEvent
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        await _publishEndpoint.Publish(orderCreatedEvent, cancellationToken);

        _logger.LogInformation(
            "Published OrderCreated event for OrderId: {OrderId}, Total Amount: {TotalAmount}",
            createdOrder.Id,
            createdOrder.TotalAmount
        );

        return new OrderDto
        {
            Id = createdOrder.Id,
            CreatedAt = createdOrder.CreatedAt,
            Status = createdOrder.Status,
            Items = createdOrder.OrderItems.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }
}
