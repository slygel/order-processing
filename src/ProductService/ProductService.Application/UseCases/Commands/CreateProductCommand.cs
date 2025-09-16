using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.UseCases.Commands;

public record CreateProductCommand(string ProductName, decimal Price) : IRequest<Results<ProductDto>>;
