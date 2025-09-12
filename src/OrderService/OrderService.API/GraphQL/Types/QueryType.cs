using OrderService.API.GraphQL.Queries;

namespace OrderService.API.GraphQL.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor.Name("Query");

        descriptor.Field(f => f.GetOrders(default!))
            .Description("Gets all orders")
            .ResolveWith<Query>(q => q.GetOrders(default!));

        descriptor.Field(f => f.GetOrderById(default!, default!))
            .Description("Gets an order by ID")
            .Argument("id", a => a.Type<NonNullType<UuidType>>())
            .ResolveWith<Query>(q => q.GetOrderById(default!, default!));
    }
}
