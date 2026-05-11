using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.Api.Ecommerce;
using OpenTelemetry.Metrics;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder ConfigureApi(this IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ApiNotifierFilter>();
        });

        builder.Services.AddCors();
        builder.Services.AddRabbitPubSub(config);
        ConfigureMongoDb(builder.Services, config);
        
        // Register metrics
        builder.Services.AddSingleton<PedidoMetrics>();
        
        builder.Services.AddSender(typeof(Program).Assembly);
        ConfigureTelemetry(builder, config);
        builder.Services.AddNotificationLog();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument("K1.Atlas.Ecommerce.Api", "v1");

        return builder;
    }

    public static IServiceCollection ConfigureMongoDb(IServiceCollection services, IConfiguration config)
    {
        services.InitializeMongoDbConfiguration(config, mongoDbSection: "MongoDB");
        
        services.RegisterMongoDbCollection<Cliente>("Cliente", cm =>
        {
            cm.AutoMap();
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
        })
        .RegisterMongoDbCollection<Produto>("Produto", cm =>
        {
            cm.AutoMap();
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
        })
        .RegisterMongoDbCollection<Pedido>("Pedido", cm =>
        {
            cm.AutoMap();
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
        });
        
        return services;
    }

    public static IHostApplicationBuilder ConfigureTelemetry(IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddTelemetryOtlp("K1.Atlas.Ecommerce.Api", "1.0.0");
        
        // Register sample-specific meters
        builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => 
            metrics.AddMeter("K1.Atlas.Ecommerce.Api.Pedidos"));
        
        builder.Logging.AddTelemetryOtlp();

        return builder;
    }
}
