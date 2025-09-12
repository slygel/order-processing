using System;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public List<OrderItem> OrderItems { get; set; } = new();
    public decimal TotalAmount => OrderItems.Sum(item => item.TotalPrice);
}

