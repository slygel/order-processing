using OrderService.Application.UseCases.Commands.CreateOrder;

namespace OrderService.API.GraphQL.Types;

public class CreateOrderCommandInputType : InputObjectType<CreateOrderCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateOrderCommand> descriptor)
    {
        descriptor.Name("CreateOrderCommandInput");
        
        descriptor.Field(f => f.Items)
            .Type<NonNullType<ListType<NonNullType<CreateOrderItemInputType>>>>()
            .Description("List of items to order");
    }
}
