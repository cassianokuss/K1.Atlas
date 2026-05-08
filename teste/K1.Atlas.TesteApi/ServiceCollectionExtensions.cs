using K1.Atlas.Telemetry.Logging;
using K1.Atlas.TesteApi.Cadastros;
using K1.Atlas.TesteApi.NotasFiscais;
using K1.Atlas.TesteApi.Ecommerce;

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
        builder.Services.AddSender(typeof(Program).Assembly);
        ConfigureTelemetry(builder, config);
        builder.Services.AddNotificationLog();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument("K1.Atlas.TesteApi", "v1");

        return builder;
    }

    public static IServiceCollection ConfigureMongoDb(IServiceCollection services, IConfiguration config)
    {
        services.InitializeMongoDbConfiguration(config, mongoDbSection: "MongoDB");

        services.RegisterMongoDbCollection<Contribuinte>("Contribuinte", cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            })
            .RegisterMongoDbCollection<NFSe>("NFSe", cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            })
            .RegisterMongoDbCollection<Cliente>("Cliente", cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            })
            .RegisterMongoDbCollection<Produto>("Produto", cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            })
            .RegisterMongoDbCollection<Pedido>("Pedido", cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        return services;
    }

    public static IHostApplicationBuilder ConfigureTelemetry(IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddTelemetryOtlp("K1.Atlas.TesteApi", "1.0.0");
        builder.Logging.AddTelemetryOtlp();

        builder.Services.RegisterTraceClass(builder =>
        {
            builder.TraceClass<CriarContribuinte>(ci =>
            {
                ci.AutoMap();
            });
            builder.TraceClass<CriarNFSe>(ci =>
            {
                ci.AutoMap();
                ci.AddProperty(e => e.Contribuinte);
            });
        });

        return builder;
    }
}
