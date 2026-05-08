# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["K1.Atlas.sln", "./"]
COPY ["src/K1.Atlas.Domain/K1.Atlas.Domain.csproj", "src/K1.Atlas.Domain/"]
COPY ["src/K1.Atlas.Host/K1.Atlas.Host.csproj", "src/K1.Atlas.Host/"]
COPY ["src/K1.Atlas.MongoDB/K1.Atlas.MongoDB.csproj", "src/K1.Atlas.MongoDB/"]
COPY ["src/K1.Atlas.PubSub/K1.Atlas.PubSub.csproj", "src/K1.Atlas.PubSub/"]
COPY ["src/K1.Atlas.PubSub.RabbitMQ/K1.Atlas.PubSub.RabbitMQ.csproj", "src/K1.Atlas.PubSub.RabbitMQ/"]
COPY ["src/K1.Atlas.Telemetry/K1.Atlas.Telemetry.csproj", "src/K1.Atlas.Telemetry/"]
COPY ["teste/K1.Atlas.WorkerFiscal/K1.Atlas.WorkerFiscal.csproj", "teste/K1.Atlas.WorkerFiscal/"]

# Restore dependencies
RUN dotnet restore "teste/K1.Atlas.WorkerFiscal/K1.Atlas.WorkerFiscal.csproj"

# Copy source code
COPY ["src/", "src/"]
COPY ["teste/K1.Atlas.WorkerFiscal/", "teste/K1.Atlas.WorkerFiscal/"]

# Build application
WORKDIR "/src/teste/K1.Atlas.WorkerFiscal"
RUN dotnet build "K1.Atlas.WorkerFiscal.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "K1.Atlas.WorkerFiscal.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "K1.Atlas.WorkerFiscal.dll"]
