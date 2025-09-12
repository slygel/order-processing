using System;
using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.UseCases.Queries.GetOrders;

public class GetOrdersQuery : IRequest<IEnumerable<OrderDto>> {}
