using FluentValidation.TestHelper;
using OrderService.Application.UseCases.Commands.CreateOrder;
using OrderService.Application.Validator;

namespace OrderService.Test.Validators;

public class CreateOrderCommandValidatorTest
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTest()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(Guid.NewGuid(), 2),
                new CreateOrderItemCommand(Guid.NewGuid(), 1)
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenItemsIsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Order must contain at least one item.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenItemsIsNull()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = null!
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Order must contain at least one item.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenProductIdIsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(Guid.Empty, 1)
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].ProductId")
            .WithErrorMessage("Product ID is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldFail_WhenQuantityIsInvalid(int quantity)
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(Guid.NewGuid(), quantity)
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public void Validate_ShouldFail_WithMultipleErrors_WhenMultipleValidationsFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            Items = new List<CreateOrderItemCommand>
            {
                new CreateOrderItemCommand(Guid.Empty, 0),
                new CreateOrderItemCommand(Guid.Empty, -1)
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].ProductId")
            .WithErrorMessage("Product ID is required.");
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Quantity must be greater than zero.");
        result.ShouldHaveValidationErrorFor("Items[1].ProductId")
            .WithErrorMessage("Product ID is required.");
        result.ShouldHaveValidationErrorFor("Items[1].Quantity")
            .WithErrorMessage("Quantity must be greater than zero.");
    }
}
