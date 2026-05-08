using K1.Atlas.MongoDB;
using K1.Atlas.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class MongoDbConfiguration
{
    public static IServiceCollection InitializeMongoDbConfiguration(this IServiceCollection services, IConfiguration config, string mongoDbSection = "MongoDB")
    {
        BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
        BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

        services.Configure<DbConnectionConfig>(config.GetSection(mongoDbSection));

        services.AddSingleton<MongoDbConnection>();

        services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    public static IServiceCollection RegisterMongoDbCollection<T>(this IServiceCollection services, string collectionName, Action<BsonClassMap<T>> classMapInitializer)
        where T : class
    {
        BsonClassMap.RegisterClassMap<T>(classMapInitializer);
        services.AddSingleton(provider =>
        {
            var connection = provider.GetRequiredService<MongoDbConnection>();
            return connection.Database.GetCollection<T>(collectionName);
        });

        return services;
    }
}
