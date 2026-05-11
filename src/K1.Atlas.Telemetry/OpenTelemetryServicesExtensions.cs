using System.Diagnostics;
using K1.Atlas.Telemetry;
using MediatR;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenTelemetryServicesExtensions
    {
        public static IServiceCollection AddTelemetryOtlp(this IServiceCollection services, string serviceName, string serviceVersion)
        {
            var activitySource = new ActivitySource(serviceName, serviceVersion);
            services.AddSingleton(activitySource);
            services.AddSingleton<ITelemetryBuilder>(new TelemetryBuilder(services));

            services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter((exporterOptions, metricReaderOptions) =>
                {
                    metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
                }))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddSource(serviceName)
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                    .AddHttpClientInstrumentation(option =>
                    {
                        option.RecordException = true;
                        option.FilterHttpRequestMessage = context =>
                        {
                            string uri = context.RequestUri?.AbsoluteUri.ToLower() ?? string.Empty;
                            return context.RequestUri?.Port != 5341 && !uri.Contains("health");
                        };
                        option.FilterHttpWebRequest = context =>
                        {
                            string uri = context.RequestUri.AbsoluteUri.ToLower();
                            return context.RequestUri.Port != 5341 && !uri.Contains("health");
                        };
                    })
                    .AddAspNetCoreInstrumentation(option =>
                    {
                        option.Filter = _ => true;
                        option.RecordException = true;
                        option.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            if (httpResponse.HttpContext.Request.QueryString.HasValue)
                                activity.AddTag("QueryString", httpResponse.HttpContext.Request.QueryString);

                            activity.AddTag("Headers", string.Join(", ", httpResponse.HttpContext.Request.Headers.Select(e => $"{e.Key}={e.Value}")));
                        };
                    })
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
                    .AddOtlpExporter();
            });

            services.TryDecorate(typeof(IRequestHandler<,>), typeof(ActivityDecorator.RequestHandler<,>));
            services.TryDecorate(typeof(IRequestHandler<>), typeof(ActivityDecorator.RequestHandler<>));

            return services;
        }

        public static ILoggingBuilder AddTelemetryOtlp(this ILoggingBuilder logging)
        {
            logging.AddJsonConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                options.UseUtcTimestamp = true;
            });

            logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeScopes = true;
                logging.IncludeFormattedMessage = true;

                logging.AddOtlpExporter();
            });

            return logging;
        }

        public static IServiceCollection RegisterTraceClass(this IServiceCollection services,
            Action<ITelemetryBuilder> builderAction)
        {
            var telemetryBuilder = new TelemetryBuilder(services);
            builderAction(telemetryBuilder);

            services.AddSingleton<ITelemetryBuilder>(telemetryBuilder);

            return services;
        }
    }
}
