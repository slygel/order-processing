using Grpc.Net.Client;
using MassTransit;
using PaymentService.API.Consumers;
using PaymentService.API.gRPC.Clients;
using PaymentService.API.Interfaces;
using PaymentService.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services
builder.Services.AddScoped<OrderServiceClient>();

builder.Services.AddScoped<IPaymentProcessor, PaymentProcessor>();

builder.Services.AddGrpcClient<SharedEvent.Protos.OrderService.OrderServiceClient>(o =>
{
    o.Address = new Uri("http://order-service:5203");
}).ConfigureChannel(options =>
{
    options.HttpHandler = new SocketsHttpHandler
    {
        EnableMultipleHttp2Connections = true,
        AllowAutoRedirect = true
    };
});

// MassTransit
builder.Services.AddMassTransit(x =>
{
    // Add consumers
    x.AddConsumer<OrderCreatedConsumer>(cfg =>
    {
        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Configure consumer endpoint
        cfg.ReceiveEndpoint("order-created-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);

            // Optional: Move failed messages to dead-letter queue
            e.UseMessageRetry(r => r.Interval(3, 5));   // retry 3 times
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
