FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0.414 AS build
WORKDIR /src

COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]

RUN dotnet restore "ApiGateway/ApiGateway.csproj"
COPY . .
WORKDIR "/src/ApiGateway"

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
