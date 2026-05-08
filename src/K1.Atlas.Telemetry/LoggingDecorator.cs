using MediatR;
using Microsoft.Extensions.Logging;

namespace K1.Atlas.Telemetry;

public static class LoggingDecorator
{
    public sealed class RequestHandler<TRequest>(
        IRequestHandler<TRequest> inner,
        ILogger<RequestHandler<TRequest>> logger)
        : IRequestHandler<TRequest> where TRequest : IRequest
    {
        public async Task HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing command {CommandName}", typeof(TRequest).Name);

            try
            {
                await inner.HandleAsync(request, cancellationToken);
                logger.LogInformation("Completed command {CommandName}", typeof(TRequest).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing command {CommandName}", typeof(TRequest).Name);
                throw;
            }
        }
    }

    public sealed class RequestHandler<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> inner,
        ILogger<RequestHandler<TRequest, TResponse>> logger)
        : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing command {CommandName}", typeof(TRequest).Name);

            try
            {
                var result = await inner.HandleAsync(request, cancellationToken);
                logger.LogInformation("Completed command {CommandName}", typeof(TRequest).Name);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing command {CommandName}", typeof(TRequest).Name);
                throw;
            }
        }
    }
}
