using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;

namespace K1.Atlas.IntegrationTest;

public class CustomWebApplicationFactory(MongoDbContainer mongoContainer, RabbitMqContainer rabbitMqContainer) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((ctx, config) =>
        {
            config.Sources.Clear();

            config.AddJsonFile("appsettings.json", optional: false)
                         .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true);

            var settings = new Dictionary<string, string?>
            {
                ["RabbitMQ:Host"] = rabbitMqContainer.Hostname,
                ["RabbitMQ:Port"] = rabbitMqContainer.GetMappedPublicPort(5672).ToString(),
                ["RabbitMQ:VirtualHost"] = "/",
                ["RabbitMQ:DefaultExchange"] = "Teste",
                ["RabbitMQ:DurableQueues"] = true.ToString(),
                ["RabbitMQ:PersistentDelivery"] = true.ToString(),
                ["RabbitMQ:LazyQueues"] = true.ToString(),
                ["RabbitMQ:User"] = "guest",
                ["RabbitMQ:Password"] = "guest",
                ["MongoDB:Host"] = mongoContainer.Hostname,
                ["MongoDB:Port"] = mongoContainer.GetMappedPublicPort(27017).ToString(),
                ["MongoDB:Database"] = "teste123",
                ["otlp:Host"] = "http://localhost",
                ["otlp:Port"] = "4317"
            };

            config.AddInMemoryCollection(settings);
        });
    }
}
