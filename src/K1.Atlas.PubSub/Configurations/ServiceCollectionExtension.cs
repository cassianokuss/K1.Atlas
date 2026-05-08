using System.Diagnostics;
using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.HostedServices;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Add an Async PubSub consumer hosted service.
    /// The service will be added as Transient.
    /// </summary>
    /// <typeparam name="TObj">Type of the serialized object</typeparam>
    /// <typeparam name="TService">Type of the processing service</typeparam>
    /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to register with.</param>
    /// <param name="builder">An action to configure the subscription behavior.</param>
    /// <returns>The original Microsoft.Extensions.DependencyInjection.IServiceCollection.</returns>
    public static IServiceCollection AddAsyncConsumer<TObj, TService>(this IServiceCollection services, Action<IAsyncConsumerOptionsBuilder<TObj>>? builder = null)
        where TService : class, IBackgroundConsumer<TObj>
    {
        services.AddScoped<TService>();
        services.AddTransient<IHostedService>(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

            return new AsyncConsumerService<TObj, TService>(
                        provider.GetRequiredService<IMessageConsumer>(),
                        scopeFactory, // Pass the scope factory instead of the service directly
                        builder,
                        provider.GetRequiredService<ActivitySource>()
                    );
        });

        return services;
    }
}
