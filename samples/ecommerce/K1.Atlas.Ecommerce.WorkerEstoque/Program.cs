using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureWorker(builder.Configuration);

builder.Services.AddAsyncConsumer<Pedido, ReservarEstoqueSubscription>(
    builder => builder.ForRoutingKeys(
        "PedidoAprovado"
        )
    .WithQueueName("ReservaEstoqueQueue")
    .ForExchange("Pedidos")
    .WithManualAck()
);

builder.Services.AddAsyncConsumer<Pedido, PedidoRejeitadoSubscription>(
    builder => builder.ForRoutingKeys(
        "PedidoRejeitado"
        )
    .WithQueueName("LiberarEstoqueQueue")
    .ForExchange("Pedidos")
    .WithManualAck()
);

var host = builder.Build();
host.Run();

public partial class Program
{
    protected Program() { }
}
