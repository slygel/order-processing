using MediatR;
using ProductService.Application.DTOs;

namespace ProductService.Application.UseCases.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
