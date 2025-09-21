FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

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

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5041
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "ProductService.API.dll"]
