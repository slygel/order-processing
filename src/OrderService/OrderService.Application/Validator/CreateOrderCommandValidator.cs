using System;
using FluentValidation;
using OrderService.Application.UseCases.Commands.CreateOrder;

namespace OrderService.Application.Validator;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .ForEach(item =>
            {
                item.SetValidator(new CreateOrderItemCommandValidator());
            });
    }
}

