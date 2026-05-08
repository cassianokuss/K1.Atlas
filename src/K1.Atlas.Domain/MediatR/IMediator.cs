using Microsoft.Extensions.DependencyInjection;

namespace MediatR;

public interface ISender
{
    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

public interface INotification
{
}

public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}

public interface IPublisher
{
    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}

public interface IMediator : ISender, IPublisher
{
}

public class Mediator(IServiceProvider serviceProvider) : IMediator, ISender, IPublisher
{
    public Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest
    {
        var handlerType = typeof(IRequestHandler<TRequest>);
        var service = (IRequestHandler<TRequest>)serviceProvider.GetRequiredService(handlerType);
        return service.HandleAsync(request, cancellationToken);
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var service = (dynamic)serviceProvider.GetRequiredService(handlerType);
        return service.HandleAsync((dynamic)request, cancellationToken);
    }

    public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var handlerType = typeof(INotificationHandler<TNotification>);
        var handlers = serviceProvider.GetServices(handlerType);

        var tasks = handlers
            .Cast<INotificationHandler<TNotification>>()
            .Select(handler => handler.HandleAsync(notification, cancellationToken));

        await Task.WhenAll(tasks);
    }
}