# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["K1.Atlas.slnx", "./"]
COPY ["src/K1.Atlas.Domain/K1.Atlas.Domain.csproj", "src/K1.Atlas.Domain/"]
COPY ["src/K1.Atlas.Host/K1.Atlas.Host.csproj", "src/K1.Atlas.Host/"]
COPY ["src/K1.Atlas.MongoDB/K1.Atlas.MongoDB.csproj", "src/K1.Atlas.MongoDB/"]
COPY ["src/K1.Atlas.PubSub/K1.Atlas.PubSub.csproj", "src/K1.Atlas.PubSub/"]
COPY ["src/K1.Atlas.PubSub.RabbitMQ/K1.Atlas.PubSub.RabbitMQ.csproj", "src/K1.Atlas.PubSub.RabbitMQ/"]
COPY ["src/K1.Atlas.Telemetry/K1.Atlas.Telemetry.csproj", "src/K1.Atlas.Telemetry/"]
COPY ["samples/ecommerce/K1.Atlas.Ecommerce.WorkerValidacao/K1.Atlas.Ecommerce.WorkerValidacao.csproj", "samples/ecommerce/K1.Atlas.Ecommerce.WorkerValidacao/"]

# Restore dependencies
RUN dotnet restore "samples/ecommerce/K1.Atlas.Ecommerce.WorkerValidacao/K1.Atlas.Ecommerce.WorkerValidacao.csproj"

# Copy source code
COPY ["src/", "src/"]
COPY ["samples/ecommerce/K1.Atlas.Ecommerce.WorkerValidacao/", "samples/ecommerce/K1.Atlas.Ecommerce.WorkerValidacao/"]

# Build application
WORKDIR "/src/samples/ecommerce/K1.Atlas.Ecommerce.WorkerValidacao"
RUN dotnet build "K1.Atlas.Ecommerce.WorkerValidacao.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "K1.Atlas.Ecommerce.WorkerValidacao.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "K1.Atlas.Ecommerce.WorkerValidacao.dll"]
