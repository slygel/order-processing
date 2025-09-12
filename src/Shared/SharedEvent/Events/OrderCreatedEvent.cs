using System;

namespace SharedEvent.Events;

public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemEvent> Items { get; set; } = new();
}
