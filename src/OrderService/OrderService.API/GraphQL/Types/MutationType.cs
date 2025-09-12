using OrderService.API.GraphQL.Mutations;

namespace OrderService.API.GraphQL.Types;

public class MutationType : ObjectType<Mutation>
{
    protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor)
    {
        descriptor.Name("Mutation");
        
        descriptor.Field(f => f.CreateOrder(default!, default!))
            .Argument("command", a => a.Type<NonNullType<CreateOrderCommandInputType>>())
            .Description("Creates a new order.");
    }
}
