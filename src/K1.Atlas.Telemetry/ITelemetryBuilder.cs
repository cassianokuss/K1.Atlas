using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace K1.Atlas.Telemetry;

public interface ITelemetryBuilder
{
    ITelemetryBuilder TraceClass<T>(Action<ITraceMap<T>> classInitializer) where T : IBaseRequest;
}

public class TelemetryBuilder : ITelemetryBuilder
{
    private readonly IServiceCollection _services;

    public TelemetryBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public ITelemetryBuilder TraceClass<T>(Action<ITraceMap<T>> classInitializer) where T : IBaseRequest
    {
        var traceMap = new TraceMap<T>();
        classInitializer(traceMap);
        _services.AddSingleton<ITraceMap<T>>(traceMap);

        return this;
    }
}
