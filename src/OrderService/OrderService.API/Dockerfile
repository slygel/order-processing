FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.414 AS build
WORKDIR /src

COPY ["OrderService/OrderService.API/OrderService.API.csproj", "OrderService/OrderService.API/"]
COPY ["OrderService/OrderService.Application/OrderService.Application.csproj", "OrderService/OrderService.Application/"]
COPY ["OrderService/OrderService.Domain/OrderService.Domain.csproj", "OrderService/OrderService.Domain/"]
COPY ["OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService/OrderService.Infrastructure/"]
COPY ["Shared/SharedEvent/SharedEvent.csproj", "Shared/SharedEvent/"]

RUN dotnet restore "OrderService/OrderService.API/OrderService.API.csproj"

COPY . .
WORKDIR "/src/OrderService/OrderService.API"

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5103
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "OrderService.API.dll"]
