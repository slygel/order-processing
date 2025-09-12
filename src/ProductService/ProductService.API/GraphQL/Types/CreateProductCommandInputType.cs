using ProductService.Application.UseCases.Commands;

namespace ProductService.API.GraphQL.Types;

public class CreateProductCommandInputType : InputObjectType<CreateProductCommand>
{
    protected override void Configure(IInputObjectTypeDescriptor<CreateProductCommand> descriptor)
    {
        descriptor.Name("CreateProductCommandInput");

        descriptor.Field(f => f.ProductName)
            .Type<NonNullType<StringType>>()
            .Description("Name of the product");

        descriptor.Field(f => f.Price)
            .Type<NonNullType<DecimalType>>()
            .Description("Price of the product");
    }
}