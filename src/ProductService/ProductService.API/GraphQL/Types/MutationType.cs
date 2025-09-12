using ProductService.API.GraphQL.Mutations;

namespace ProductService.API.GraphQL.Types;

public class MutationType : ObjectType<Mutation>
{
    // Used to describe the shape of objects returned by the API
    protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor)
    {
        descriptor.Name("Mutation");

        descriptor.Field(f => f.CreateProduct(default!, default!))
            .Argument("command", a => a.Type<NonNullType<CreateProductCommandInputType>>())
            .Description("Creates a new product.");
    }
}