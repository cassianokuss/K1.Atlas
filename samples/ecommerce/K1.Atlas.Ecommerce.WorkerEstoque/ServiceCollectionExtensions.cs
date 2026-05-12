using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.WorkerEstoque;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.WorkerEstoque.Features.ReservarEstoque;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder ConfigureWorker(this IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddRabbitPubSub(config);
        ConfigureMongoDb(builder.Services, config);
        builder.Services.AddNotificationLog();
        
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
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.SetIgnoreExtraElements(true);
            })
            .RegisterMongoDbCollection<Produto>("Produto", cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.SetIgnoreExtraElements(true);
            })
            .RegisterMongoDbCollection<ReservaEstoque>("ReservaEstoque", cm =>
            {
                cm.AutoMap();
                cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                cm.SetIgnoreExtraElements(true);
            });

        return services;
    }

    public static IHostApplicationBuilder ConfigureTelemetry(IHostApplicationBuilder builder, IConfiguration config)
    {
        builder.Services.AddTelemetryOtlp("K1.Atlas.Ecommerce.WorkerEstoque", "1.0.0");
        
        // Configurar AlwaysOnSampler para Workers garantir que todos os traces sejam coletados
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracerBuilder =>
        {
            tracerBuilder.SetSampler(new AlwaysOnSampler());
        });
        
        builder.Logging.AddTelemetryOtlp();

        builder.Services.RegisterTraceClass(builder =>
        {
            builder.TraceClass<ReservarEstoque>(ci =>
            {
                ci.AutoMap();
                ci.AddProperty(e => e.PedidoId);
            });
        });

        return builder;
    }
}
