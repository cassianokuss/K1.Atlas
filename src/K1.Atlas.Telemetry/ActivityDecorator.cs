using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using MediatR;

namespace K1.Atlas.Telemetry;

public static class ActivityDecorator
{
    public sealed class RequestHandler<TRequest>(
        IRequestHandler<TRequest> inner,
        ActivitySource activitySource,
        ITraceMap<TRequest>? traceMap = null
        )
        : IRequestHandler<TRequest> where TRequest : IRequest
    {
        public async Task HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            using var activity = CreateAndConfigureActivity(request, activitySource, traceMap);

            try
            {
                await inner.HandleAsync(request, cancellationToken);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                HandleActivityException(activity, ex);
                throw;
            }
        }
    }

    public sealed class RequestHandler<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> inner,
        ActivitySource activitySource,
        ITraceMap<TRequest>? traceMap = null)
        : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            using var activity = CreateAndConfigureActivity(request, activitySource, traceMap);

            try
            {
                var response = await inner.HandleAsync(request, cancellationToken);
                activity?.SetStatus(ActivityStatusCode.Ok);
                return response;
            }
            catch (Exception ex)
            {
                HandleActivityException(activity, ex);
                throw;
            }
        }
    }

    private static Activity? CreateAndConfigureActivity<TRequest>(
        TRequest request,
        ActivitySource activitySource,
        ITraceMap<TRequest>? traceMap)
        where TRequest : IBaseRequest
    {
        var activityName = typeof(TRequest).Name;
        var activity = activitySource.StartActivity(activityName);

        if (activity == null)
            return null;

        activity.SetTag("ActivityId", activity.Id);

        if (traceMap != null)
        {
            ConfigureActivityWithTraceMap(activity, request, traceMap);
        }

        return activity;
    }

    private static void ConfigureActivityWithTraceMap<TRequest>(
        Activity activity,
        TRequest request,
        ITraceMap<TRequest> traceMap)
        where TRequest : IBaseRequest
    {
        foreach (PropertyInfo prop in traceMap.Properties)
        {
            object? value = prop.GetValue(request);
            activity.SetTag(prop.Name, value);
        }

        activity.SetTag("Request", JsonSerializer.Serialize(request));
    }

    private static void HandleActivityException(Activity? activity, Exception ex)
    {
        if (activity == null)
            return;

        activity.AddException(ex);
        activity.SetStatus(ActivityStatusCode.Error, ex.Message);
    }
}
