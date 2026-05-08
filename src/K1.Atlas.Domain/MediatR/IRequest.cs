namespace MediatR;

public interface IRequest : IBaseRequest;
public interface IRequest<out TResponse> : IBaseRequest;

public interface IBaseRequest { };

public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    public Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
