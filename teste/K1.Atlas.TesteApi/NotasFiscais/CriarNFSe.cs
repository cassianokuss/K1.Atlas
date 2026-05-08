using System.Text.Json;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Domain.Repositories;
using MediatR;
using K1.Atlas.TesteApi.Cadastros;

namespace K1.Atlas.TesteApi.NotasFiscais;

public class CriarNFSe : IRequest<NFSe>
{
    public Contribuinte Contribuinte { get; set; }

    public CriarNFSe(Contribuinte contribuinte)
    {
        Contribuinte = contribuinte;
    }
}

public class CriarNFSeHandler(IRepository<NFSe> repo, INotifier notifier) : IRequestHandler<CriarNFSe, NFSe>
{
    public async Task<NFSe> HandleAsync(CriarNFSe request, CancellationToken cancellationToken)
    {
        var nfse = new NFSe
        {
            Numero = DateTime.Now.Millisecond,
            Contribuinte = request.Contribuinte,
            Valor = DateTime.Now.Millisecond
        };

        await repo.SaveOrUpdateAsync(nfse, e => e.Numero == nfse.Numero);

        notifier.NotifyInformation("NFS-e criada. {Numero} {nfse}", nfse.Numero, JsonSerializer.Serialize(nfse));
        return nfse;
    }
}
