using K1.Atlas.Ecommerce.WorkerFiscal;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Infrastructure;
using FluentValidation;

var builder = Host.CreateApplicationBuilder(args);

builder.ConfigureWorker(builder.Configuration);

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register domain services
builder.Services.AddScoped<ISefazRetryPolicy, SefazRetryPolicy>();

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
