FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Clientes.Api/Clientes.Api.csproj src/Clientes.Api/
COPY src/Clientes.Application/Clientes.Application.csproj src/Clientes.Application/
COPY src/Clientes.Domain/Clientes.Domain.csproj src/Clientes.Domain/
COPY src/Clientes.Infrastructure/Clientes.Infrastructure.csproj src/Clientes.Infrastructure/
COPY src/Clientes.Contracts/Clientes.Contracts.csproj src/Clientes.Contracts/
RUN dotnet restore src/Clientes.Api/Clientes.Api.csproj

COPY src/ src/
RUN dotnet publish src/Clientes.Api/Clientes.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "Clientes.Api.dll"]
