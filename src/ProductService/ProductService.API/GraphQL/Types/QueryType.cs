using System;
using ProductService.API.GraphQL.Queries;

namespace ProductService.API.GraphQL.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor.Name("Query");

        descriptor.Field(f => f.GetProducts(default!))
            .Description("Gets all products")
            .ResolveWith<Query>(q => q.GetProducts(default!));

        descriptor.Field(f => f.GetProductById(default!, default!))
            .Description("Gets a product by ID")
            .Argument("id", a => a.Type<NonNullType<UuidType>>())
            .ResolveWith<Query>(q => q.GetProductById(default!, default!));
    }
}
