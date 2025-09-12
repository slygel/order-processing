using MediatR;
using ProductService.Application.DTOs;

namespace ProductService.Application.UseCases.Queries;

public record GetProductsQuery() : IRequest<IEnumerable<ProductDto>>;
