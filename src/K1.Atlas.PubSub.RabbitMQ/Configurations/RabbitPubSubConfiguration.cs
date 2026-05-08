using Microsoft.Extensions.Configuration;
using K1.Atlas.PubSub.Rabbit;
using K1.Atlas.PubSub.Rabbit.Serializers;
using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.Producer;
using K1.Atlas.PubSub.RabbitMQ;
using MessagePack.Resolvers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitPubSubConfiguration
    {
        /// <summary>
        /// Add RabbitMQ PubSub essential services.
        /// </summary>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to register with.</param>
        /// <returns>The original Microsoft.Extensions.DependencyInjection.IServiceCollection.</returns>
        public static IServiceCollection AddRabbitPubSub(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ConfigRabbitMQ>(config.GetSection("RabbitMQ"));

            MessagePack.MessagePackSerializer.DefaultOptions = MessagePack.MessagePackSerializerOptions.Standard
                .WithResolver(CompositeResolver.Create(
                    TypelessObjectResolver.Instance,
                    ContractlessStandardResolver.Instance
                ));

            services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
            services.AddSingleton<IMessageConsumer, MessageConsumer>();
            services.AddSingleton<ISerializationManager, SerializationManagerImpl>();
            services.AddSingleton<ISerializer, MessagePackSerializer>();
            services.AddSingleton<IMessageProducer, MessageProducer>();

            return services;
        }
    }
}
