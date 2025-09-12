using OrderService.API.GraphQL.Queries;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.API.gRPC.Services;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Repositories;
using OrderService.API.Exceptions;
using OrderService.API.GraphQL.Types;
using Consul;
using OrderService.Application;
using OrderService.Application.Interfaces;
using FluentValidation;
using MediatR;
using OrderService.Application.Behaviors;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"))
);

// Add Service
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductService, GrpcProductService>();

// Configure gRPC client
builder.Services.Configure<GrpcSettings>(builder.Configuration.GetSection("GrpcSettings"));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Anchor).Assembly);
});
builder.Services.AddValidatorsFromAssembly(typeof(Anchor).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Add GraphQL
builder.Services.AddGraphQLServer()
    .AddQueryType<QueryType>()
    .AddMutationType<MutationType>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true);

// Add gRPC
builder.Services.AddGrpc();

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

// 1. Add CORS service
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(options =>
{
    // REST
    options.ListenAnyIP(5103, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
    });
    // gRPC
    options.ListenAnyIP(5203, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var app = builder.Build();

var consulClient = new ConsulClient(cfg =>
{
    cfg.Address = new Uri("http://localhost:8500");
});

var registration = new AgentServiceRegistration()
{
    ID = $"order-service-{Guid.NewGuid()}",
    Name = "order-service",
    Address = "localhost",
    Port = 5103,
};

await consulClient.Agent.ServiceDeregister(registration.ID);
await consulClient.Agent.ServiceRegister(registration);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet(
    "/",
    context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    }
);

// Exception handler
app.ConfigureExceptionHandler();

app.MapControllers();
app.MapGrpcService<PaymentConfirmationService>();
app.MapGraphQL();
app.UseCors("AllowFrontend");

app.Run();