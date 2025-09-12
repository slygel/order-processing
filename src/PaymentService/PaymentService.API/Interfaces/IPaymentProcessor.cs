using System;
using SharedEvent.Events;

namespace PaymentService.API.Interfaces;

public interface IPaymentProcessor
{
    Task ProcessPaymentAsync(OrderCreatedEvent orderCreatedEvent);
}
