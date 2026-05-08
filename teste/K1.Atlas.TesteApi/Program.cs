using MediatR;
using Microsoft.AspNetCore.Mvc;
using K1.Atlas.TesteApi.Cadastros;
using K1.Atlas.TesteApi.NotasFiscais;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.TesteApi.Ecommerce;
using K1.Atlas.TesteApi.Ecommerce.Commands;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureApi(builder.Configuration);

builder.Services.AddAsyncConsumer<Contribuinte, EmitirNFSeSubscription>(
    builder => builder.ForRoutingKeys(
        "ContribuiteCriado"
        )
    .WithQueueName("EmitirNFSeQueue")
    .ForExchange("Teste")
    .WithManualAck()
);

var app = builder.Build();

app.UseCors(policyBuilder =>
{
    policyBuilder.AllowAnyOrigin();
    policyBuilder.AllowAnyHeader();
    policyBuilder.AllowAnyMethod();
});

app.MapControllers();

app.MapGet("/", async ([FromServices] ISender sender, [FromServices] IRepository<NFSe> nfses, CancellationToken cancellationToken) =>
{
    var contribuinte = await sender.SendAsync(new CriarContribuinte() { Numero = 1, NumeroTexto = "1", Nome = $"Nome-{DateTime.Now}", Endereco = "Endereço Teste" }, cancellationToken);

    var nfse = await nfses.FirstOrDefaultAsync(builder => builder.Where(c => c.Contribuinte.NumDocReceita == contribuinte.NumDocReceita), cancellationToken);
    return nfse;
});

app.MapPost("/pedidos", async (
    [FromBody] CriarPedido command,
    [FromServices] ISender sender,
    CancellationToken cancellationToken) =>
{
    var pedido = await sender.SendAsync(command, cancellationToken);
    return Results.Created($"/pedidos/{pedido.Id}", pedido);
})
.WithName("CriarPedido");

app.MapGet("/seed", async (
    [FromServices] IRepository<Cliente> clienteRepo,
    [FromServices] IRepository<Produto> produtoRepo,
    CancellationToken cancellationToken) =>
{
    var clientes = SeedData.GetClientes();
    var produtos = SeedData.GetProdutos();

    foreach (var cliente in clientes)
    {
        await clienteRepo.SaveOrUpdateAsync(cliente, c => c.CpfCnpj == cliente.CpfCnpj, cancellationToken);
    }

    foreach (var produto in produtos)
    {
        await produtoRepo.SaveOrUpdateAsync(produto, p => p.Codigo == produto.Codigo, cancellationToken);
    }

    return Results.Ok(new { Clientes = clientes.Count, Produtos = produtos.Count });
})
.WithName("SeedData");

app.UseOpenApiDocument("swagger");

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}