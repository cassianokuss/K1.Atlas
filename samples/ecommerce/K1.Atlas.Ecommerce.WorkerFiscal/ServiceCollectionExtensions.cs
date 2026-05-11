using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce;
using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Commands;
using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Services;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder ConfigureWorker(this IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddRabbitPubSub(config);
        ConfigureMongoDb(builder.Services, config);
        builder.Services.AddNotificationLog();
        
        // Register metrics
        builder.Services.AddSingleton<NotaFiscalMetrics>();
        
        // Register SEFAZ service
        builder.Services.AddSingleton<ISefazService, SefazServiceSimulator>();
        
        // IMPORTANTE: AddSender deve vir ANTES de ConfigureTelemetry
        // para que os decorators de Activity possam ser aplicados
        builder.Services.AddSender(typeof(Program).Assembly);
        ConfigureTelemetry(builder, config);

        return builder;
    }

    public static IServiceCollection ConfigureMongoDb(IServiceCollection services, IConfiguration config)
    {
        services.InitializeMongoDbConfiguration(config, mongoDbSection: "MongoDB");

        services.RegisterMongoDbCollection<NotaFiscal>("NotaFiscal", cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.SetIgnoreExtraElements(true);
            })
            .RegisterMongoDbCollection<Pedido>("Pedido", cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.SetIgnoreExtraElements(true);
            })
            .RegisterMongoDbCollection<Cliente>("Cliente", cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.SetIgnoreExtraElements(true);
            });

        return services;
    }

    public static IHostApplicationBuilder ConfigureTelemetry(IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddTelemetryOtlp("K1.Atlas.Ecommerce.WorkerFiscal", "1.0.0");
        
        // Register sample-specific meters
        builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => 
            metrics.AddMeter("K1.Atlas.Ecommerce.WorkerFiscal.NotasFiscais"));
        
        // Configurar AlwaysOnSampler para Workers garantir que todos os traces sejam coletados
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracerBuilder =>
        {
            tracerBuilder.SetSampler(new AlwaysOnSampler());
        });
        
        builder.Logging.AddTelemetryOtlp();

        builder.Services.RegisterTraceClass(builder =>
        {
            builder.TraceClass<EmitirNotaFiscal>(ci =>
            {
                ci.AutoMap();
                ci.AddProperty(e => e.PedidoId);
            });
        });

        return builder;
    }
}
