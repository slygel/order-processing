FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.414 AS build
WORKDIR /src

COPY ["PaymentService/PaymentService.API/PaymentService.API.csproj", "PaymentService/PaymentService.API/"]
COPY ["Shared/SharedEvent/SharedEvent.csproj", "Shared/SharedEvent/"]

RUN dotnet restore "PaymentService/PaymentService.API/PaymentService.API.csproj"

COPY . .
WORKDIR "/src/PaymentService/PaymentService.API"

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5104
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
