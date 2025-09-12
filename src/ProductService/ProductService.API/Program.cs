using Consul;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.API.GraphQL.Types;
using ProductService.API.gRPC.Services;
using ProductService.Application;
using ProductService.Application.Behaviors;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure;
using ProductService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"))
);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Anchor).Assembly);
});
builder.Services.AddValidatorsFromAssembly(typeof(Anchor).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Add Service
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Add GraphQL
builder.Services.AddGraphQLServer()
    .AddQueryType<QueryType>()
    .AddMutationType<MutationType>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

// Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add gRPC service
builder.Services.AddGrpc();

// Configure Kestrel to listen on both HTTP1 and HTTP2
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP1 for REST/GraphQL
    options.ListenAnyIP(5041, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
    // HTTP2 for gRPC
    options.ListenAnyIP(5241, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var consulClient = new ConsulClient(cfg =>
{
    cfg.Address = new Uri("http://localhost:8500");
});

var registration = new AgentServiceRegistration()
{
    ID = $"product-service-{Guid.NewGuid()}",
    Name = "product-service",
    Address = "localhost",
    Port = 5041,
};

await consulClient.Agent.ServiceDeregister(registration.ID);
await consulClient.Agent.ServiceRegister(registration);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapGraphQL();

// Map gRPC service
app.MapGrpcService<ProductGrpcService>();

app.Run();