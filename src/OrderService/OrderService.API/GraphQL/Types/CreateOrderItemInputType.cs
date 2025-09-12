using OrderService.Application.UseCases.Commands.CreateOrder;

namespace OrderService.API.GraphQL.Types;

public class CreateOrderItemInputType : InputObjectType<CreateOrderItemCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateOrderItemCommand> descriptor)
    {
        descriptor.Name("CreateOrderItemInput");

        descriptor.Field(f => f.ProductId)
            .Type<NonNullType<UuidType>>()
            .Description("The ID of the product to order");
        
        descriptor.Field(f => f.Quantity)
            .Type<NonNullType<IntType>>()
            .Description("The quantity of the product to order");
    }
}