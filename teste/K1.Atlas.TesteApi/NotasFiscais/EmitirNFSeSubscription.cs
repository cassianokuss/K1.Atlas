using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.HostedServices;
using MediatR;
using K1.Atlas.TesteApi.Cadastros;

namespace K1.Atlas.TesteApi.NotasFiscais;

public class EmitirNFSeSubscription : IBackgroundConsumer<Contribuinte>
{
    private readonly ISender _sender;
    public EmitirNFSeSubscription(ISender sender)
    {
        _sender = sender;
    }
    public async Task ConsumeAsync(Contribuinte obj, IMessageContext context, CancellationToken cancellationToken)
    {
        await _sender.SendAsync(new CriarNFSe(obj), cancellationToken);
        await context.AckAsync(cancellationToken);
    }
}