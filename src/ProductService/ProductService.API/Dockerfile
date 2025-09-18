FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Expose both REST API and gRPC ports
EXPOSE 5041
EXPOSE 5241

FROM mcr.microsoft.com/dotnet/sdk:8.0.414 AS build
WORKDIR /src

COPY ["ProductService/ProductService.API/ProductService.API.csproj", "ProductService/ProductService.API/"]
COPY ["ProductService/ProductService.Application/ProductService.Application.csproj", "ProductService/ProductService.Application/"]
COPY ["ProductService/ProductService.Domain/ProductService.Domain.csproj", "ProductService/ProductService.Domain/"]
COPY ["ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "ProductService/ProductService.Infrastructure/"]
COPY ["Shared/SharedEvent/SharedEvent.csproj", "Shared/SharedEvent/"]

RUN dotnet restore "ProductService/ProductService.API/ProductService.API.csproj"
COPY . .
WORKDIR "/src/ProductService/ProductService.API"

# Build and publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5041;http://+:5241
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "ProductService.API.dll"]
