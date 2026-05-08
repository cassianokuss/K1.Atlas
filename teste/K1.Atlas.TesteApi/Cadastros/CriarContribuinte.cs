using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using MediatR;

namespace K1.Atlas.TesteApi.Cadastros;

public class CriarContribuinte : IRequest<Contribuinte>
{
    public int Numero { get; set; } = default!;
    public string NumeroTexto { get; set; } = default!;
    public string Nome { get; set; } = default!;
    public string Endereco { get; set; } = default!;
}

public class CriarContribuinteHandler(IRepository<Contribuinte> repo, INotifier notifier, IMessageProducer messageProducer) : IRequestHandler<CriarContribuinte, Contribuinte>
{
    public async Task<Contribuinte> HandleAsync(CriarContribuinte request, CancellationToken cancellationToken)
    {
        var contribuinte = new Contribuinte
        {
            Nome = request.Nome,
            DataCadastro = DateTime.Now,
            Endereco = request.Endereco,
            NumDocReceita = "123456",
            Valor = DateTime.Now.Millisecond
        };

        await repo.SaveOrUpdateAsync(contribuinte, e => e.NumDocReceita == contribuinte.NumDocReceita);
        await messageProducer.Publish(contribuinte, PublishOptions.RoutingTo("ContribuiteCriado").ToExchange("Teste"));

        notifier.NotifyWarning("Ta ruim!");
        notifier.NotifyError("E piorou!");

        return contribuinte;
    }
}