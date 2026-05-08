using K1.Atlas.Telemetry.Logging;
using K1.Atlas.WorkerValidacao.Ecommerce;
using K1.Atlas.WorkerValidacao.Ecommerce.Commands;
using K1.Atlas.WorkerValidacao.Ecommerce.Services;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder ConfigureWorker(this IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddRabbitPubSub(config);
        ConfigureMongoDb(builder.Services, config);
        builder.Services.AddNotificationLog();
        
        // Register credit bureau service
        builder.Services.AddSingleton<IBureauCreditoService, BureauCreditoSimulator>();
        
        // IMPORTANTE: AddSender deve vir ANTES de ConfigureTelemetry
        // para que os decorators de Activity possam ser aplicados
        builder.Services.AddSender(typeof(Program).Assembly);
        ConfigureTelemetry(builder, config);

        return builder;
    }

    public static IServiceCollection ConfigureMongoDb(IServiceCollection services, IConfiguration config)
    {
        services.InitializeMongoDbConfiguration(config, mongoDbSection: "MongoDB");

        services.RegisterMongoDbCollection<Pedido>("Pedido", cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            })
            .RegisterMongoDbCollection<Cliente>("Cliente", cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });

        return services;
    }

    public static IHostApplicationBuilder ConfigureTelemetry(IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddTelemetryOtlp("K1.Atlas.WorkerValidacao", "1.0.0");
        
        // Configurar AlwaysOnSampler para Workers garantir que todos os traces sejam coletados
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracerBuilder =>
        {
            tracerBuilder.SetSampler(new AlwaysOnSampler());
        });
        
        builder.Logging.AddTelemetryOtlp();

        builder.Services.RegisterTraceClass(builder =>
        {
            builder.TraceClass<ValidarCredito>(ci =>
            {
                ci.AutoMap();
                ci.AddProperty(e => e.Pedido);
            });
        });

        return builder;
    }
}
