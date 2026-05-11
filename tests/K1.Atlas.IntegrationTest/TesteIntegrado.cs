using System.Net;
using System.Net.Http.Json;
using DotNet.Testcontainers.Builders;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;

namespace K1.Atlas.IntegrationTest;

public class TesteIntegrado : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:4.4")
      .WithPortBinding(0, 27017)
      .WithUsername(string.Empty)
      .WithPassword(string.Empty)
      .WithCleanUp(true)
      .Build();
    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:4.1-management-alpine")
      .WithPortBinding(0, 5672)
      .WithPortBinding(0, 15672)
      .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
      .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
      .Build();

    private CustomWebApplicationFactory? _factory;

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        _factory = new CustomWebApplicationFactory(_mongoContainer, _rabbitMqContainer);
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}
