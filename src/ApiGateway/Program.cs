using Yarp.ReverseProxy.Configuration;
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add HealthChecks
builder.Services.AddHealthChecks();

// Configure YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// GraphQL Gateway
builder.Services
    .AddGraphQLServer()
    .AddRemoteSchema("order_service")
    .AddRemoteSchema("product_service");

builder.Services.AddHttpClient("order_service",
    c => c.BaseAddress = new Uri("http://order-service:5103/graphql")); 

builder.Services.AddHttpClient("product_service",
    c => c.BaseAddress = new Uri("http://product-service:5041/graphql"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
    });
}

app.MapGet(
    "/",
    context =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    }
);

app.UseCors("AllowAll");

// Map YARP routes
app.MapReverseProxy();
app.MapGraphQL("/graphql");
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();