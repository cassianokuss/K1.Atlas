using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce;
using K1.Atlas.Ecommerce.Contracts.Entities;
using FluentValidation;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureWorker(builder.Configuration);

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register PedidoCriadoSubscription to listen for "PedidoCriado" messages
builder.Services.AddAsyncConsumer<Pedido, PedidoCriadoSubscription>(
    builder => builder.ForRoutingKeys("PedidoCriado")
        .WithQueueName("ValidacaoCreditoQueue")
        .ForExchange("Pedidos")
        .WithManualAck()
);

var host = builder.Build();
host.Run();

public partial class Program
{
    protected Program() { }
}
