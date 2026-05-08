using K1.Atlas.Telemetry;
using K1.Atlas.Telemetry.Logging;
using MediatR;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class LogConfiguration
{
    public static IServiceCollection AddNotificationLog(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<INotifier, Notifier>();
        services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.ResponseBody | HttpLoggingFields.Request;
        });

        services.TryDecorate(typeof(IRequestHandler<,>), typeof(LoggingDecorator.RequestHandler<,>));
        services.TryDecorate(typeof(IRequestHandler<>), typeof(LoggingDecorator.RequestHandler<>));

        return services;
    }

}