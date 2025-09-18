FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Expose both REST API and gRPC ports
EXPOSE 5103
EXPOSE 5203

FROM mcr.microsoft.com/dotnet/sdk:8.0.414 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY ["OrderService/OrderService.API/OrderService.API.csproj", "OrderService/OrderService.API/"]
COPY ["OrderService/OrderService.Application/OrderService.Application.csproj", "OrderService/OrderService.Application/"]
COPY ["OrderService/OrderService.Domain/OrderService.Domain.csproj", "OrderService/OrderService.Domain/"]
COPY ["OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService/OrderService.Infrastructure/"]
COPY ["Shared/SharedEvent/SharedEvent.csproj", "Shared/SharedEvent/"]

# Restore as distinct layers
RUN dotnet restore "OrderService/OrderService.API/OrderService.API.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/OrderService/OrderService.API"

# Build and publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set ASP.NET Core environment
ENV ASPNETCORE_URLS=http://+:5103;http://+:5203
ENV ASPNETCORE_ENVIRONMENT=Development
ENV RabbitMQ__Host=rabbitmq
ENV RabbitMQ__Username=guest

ENTRYPOINT ["dotnet", "OrderService.API.dll"]
