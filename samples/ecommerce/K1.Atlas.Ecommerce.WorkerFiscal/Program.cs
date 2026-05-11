using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureWorker(builder.Configuration);

builder.Services.AddAsyncConsumer<ReservaEstoque, EstoqueReservadoSubscription>(
    builder => builder.ForRoutingKeys(
        "EstoqueReservado"
        )
    .WithQueueName("EmissaoNotaFiscalQueue")
    .ForExchange("Pedidos")
    .WithManualAck()
);

var host = builder.Build();
host.Run();

public partial class Program
{
    protected Program() { }
}
